﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Utils;
using Alex.Worlds.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;
using NLog;
using UniversalThreadManagement;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;

//using OpenTK.Graphics;

namespace Alex.Worlds
{
    public class ChunkManager : IDisposable
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkManager));

		private GraphicsDevice Graphics { get; }
        private IWorld World { get; }
	    private Alex Game { get; }

        private int _chunkUpdates = 0;
	    public int ChunkUpdates => Enqueued.Count;
	    public int LowPriortiyUpdates => LowPriority.Count;
	    public int ChunkCount => Chunks.Count;

	    public AlphaTestEffect TransparentEffect { get; }
		public BasicEffect OpaqueEffect { get; }

	    public int Vertices { get; private set; }
	    public int RenderedChunks { get; private set; } = 0;

	    public SkylightCalculations SkylightCalculator { get; private set; }
		public ChunkManager(Alex alex, GraphicsDevice graphics, IWorld world)
		{
			Game = alex;
            Graphics = graphics;
            World = world;
            Chunks = new ConcurrentDictionary<ChunkCoordinates, IChunkColumn>();

			SkylightCalculator = new SkylightCalculations(world, coordinates =>
			{
				if (Chunks.TryGetValue(coordinates, out IChunkColumn column))
				{
					if (IsWithinView(coordinates, CameraBoundingFrustum))
					{
						var distance = new ChunkCoordinates(CameraPosition).DistanceTo(coordinates);
						if (Math.Abs(distance) < Game.GameSettings.RenderDistance / 2d)
						//if (column.SkyLightDirty) //Initial
						{
							return SkylightCalculations.CheckResult.HighPriority;
						}
						else
						{
							return SkylightCalculations.CheckResult.MediumPriority;
						}
					}
					else
					{
						return SkylightCalculations.CheckResult.LowPriority;
					}
				}

				return SkylightCalculations.CheckResult.Cancel;
			});

				//	var distance = (float)Math.Pow(alex.GameSettings.RenderDistance, 2) * 0.8f;
			var fogStart = 0;//distance - (distance * 0.35f);
			TransparentEffect = new AlphaTestEffect(Graphics)
			{
				Texture = alex.Resources.Atlas.GetAtlas(),
				VertexColorEnabled = true,
				World = Matrix.Identity,
				AlphaFunction = CompareFunction.Greater,
				ReferenceAlpha = 127,
				//FogEnd = distance,
				FogStart = fogStart,
				FogEnabled = false
			};
			//TransparentEffect.FogColor = new Vector3(0.5f, 0.5f, 0.5f);

			//TransparentEffect.FogEnd = distance;
			//	TransparentEffect.FogStart = distance - (distance * 0.55f);
			//	TransparentEffect.FogEnabled = true;

			OpaqueEffect = new BasicEffect(Graphics)
			{
				TextureEnabled = true,
				Texture = alex.Resources.Atlas.GetAtlas(),
			//	FogEnd = distance,
				FogStart = fogStart,
				VertexColorEnabled = true,
				LightingEnabled = true,
				FogEnabled = false
			};

			Updater = new Thread(ChunkUpdateThread)
            {IsBackground = true};

            //HighPriority = new ConcurrentQueue<ChunkCoordinates>();
            //LowPriority = new ConcurrentQueue<ChunkCoordinates>();
            LowPriority = new ThreadSafeList<ChunkCoordinates>();
            HighestPriority = new ConcurrentQueue<ChunkCoordinates>();

			SkylightThread = new Thread(SkyLightUpdater)
			{
				IsBackground = true
			};
		}

	    public void GetPendingLightingUpdates(out int low, out int mid, out int high)
	    {
		    low = SkylightCalculator.LowPriorityPending;
		    mid = SkylightCalculator.MidPriorityPending;
		    high = SkylightCalculator.HighPriorityPending;
	    }

	    public void Start()
	    {
		    Updater.Start();
			SkylightThread.Start();
		}

		//private ThreadSafeList<Entity> Entities { get; private set; } 

		private ConcurrentQueue<ChunkCoordinates> HighestPriority { get; set; }
	   // private ConcurrentQueue<ChunkCoordinates> HighPriority { get; set; }
	    private ThreadSafeList<ChunkCoordinates> LowPriority { get; set; }
		private ThreadSafeList<ChunkCoordinates> Enqueued { get; } = new ThreadSafeList<ChunkCoordinates>();
		private ConcurrentDictionary<ChunkCoordinates, IChunkColumn> Chunks { get; }

	    private readonly ThreadSafeList<ChunkCoordinates> _renderedChunks = new ThreadSafeList<ChunkCoordinates>();

		private Thread Updater { get; }
		private Thread SkylightThread { get; }

		// private readonly AutoResetEvent _updateResetEvent = new AutoResetEvent(false);
		private CancellationTokenSource CancelationToken { get; set; } = new CancellationTokenSource();

		private Vector3 CameraPosition = Vector3.Zero;
	    private BoundingFrustum CameraBoundingFrustum = new BoundingFrustum(Matrix.Identity);

	    private void SkyLightUpdater()
	    {
            while (!CancelationToken.IsCancellationRequested)
		    {
			    var cameraPos = new ChunkCoordinates(CameraPosition);

			    if (SkylightCalculator.HasPending())
			    {
				    if (SkylightCalculator.TryLightNext(cameraPos, out var coords))
				    {
					    if (Chunks.TryGetValue(coords, out IChunkColumn column))
					    {
						    //column.SkyLightDirty = false;
						    ScheduleChunkUpdate(coords, ScheduleType.Full);
					    }
				    }
			    }
		    }
	    }

		private ConcurrentDictionary<ChunkCoordinates, IWorkItemResult> _workItems = new ConcurrentDictionary<ChunkCoordinates, IWorkItemResult>();
        private long _threadsRunning = 0;
        private void ChunkUpdateThread()
		{
			int maxThreads = Game.GameSettings.ChunkThreads; //Environment.ProcessorCount / 2;
			SmartThreadPool taskScheduler = new SmartThreadPool();
			taskScheduler.MaxThreads = maxThreads;

			Stopwatch sw = new Stopwatch();

            while (!CancelationToken.IsCancellationRequested)
            {
               // SpinWait.SpinUntil(() => Interlocked.Read(ref _threadsRunning) < maxThreads);
                SpinWait.SpinUntil(() => taskScheduler.InUseThreads < maxThreads);

                bool nonInView = false;
                double radiusSquared = Math.Pow(Game.GameSettings.RenderDistance, 2);
				try
				{
					if (HighestPriority.TryDequeue(out var coords))
					{
						var task = taskScheduler.QueueWorkItem(() =>
						{
							if (Chunks.TryGetValue(coords, out var val))
							{
								UpdateChunk(val);
							}

							_workItems.TryRemove(coords, out _);
                        }, WorkItemPriority.Highest);
						_workItems.TryAdd(coords, task);

					}
				   else if (Enqueued.Count > 0)
					{
						//var cc = new ChunkCoordinates(CameraPosition);

                        coords = Enqueued.MinBy(x => Math.Abs(x.DistanceTo(new ChunkCoordinates(CameraPosition))));
                        if (!IsWithinView(coords, CameraBoundingFrustum))
                        {
	                        if (!_workItems.ContainsKey(coords))
	                        {
		                        LowPriority.TryAdd(coords);
		                        Enqueued.Remove(coords);
	                        }
                        }
                        else
                        {
	                        Enqueued.Remove(coords);

	                        var task = taskScheduler.QueueWorkItem(() =>
	                        {
		                        if (Chunks.TryGetValue(coords, out var val))
		                        {
			                        UpdateChunk(val);
		                        }
		                        _workItems.TryRemove(coords, out _);
                            }, WorkItemPriority.Normal);
	                        _workItems.TryAdd(coords, task);
                        }
					}
					else if (LowPriority.Count > 0)
					{
						//var cc = new ChunkCoordinates(CameraPosition);

						coords = LowPriority.MinBy(x => Math.Abs(x.DistanceTo(new ChunkCoordinates(CameraPosition))));

						LowPriority.Remove(coords);

						var task = taskScheduler.QueueWorkItem(() =>
						{
							if (Chunks.TryGetValue(coords, out var val))
							{
								UpdateChunk(val);
							}

							_workItems.TryRemove(coords, out _);
						}, WorkItemPriority.Normal);

						_workItems.TryAdd(coords, task);
                    }
				}
				catch (OperationCanceledException)
				{
					break;
				}
			}

			if (!CancelationToken.IsCancellationRequested)
				Log.Warn($"Chunk update loop has unexpectedly ended!");


			taskScheduler.Dispose();

        }

	    private bool IsWithinView(IChunkColumn chunk, BoundingFrustum frustum)
	    {
		    var chunkPos = new Vector3(chunk.X * ChunkColumn.ChunkWidth, 0, chunk.Z * ChunkColumn.ChunkDepth);
		    return frustum.Intersects(new Microsoft.Xna.Framework.BoundingBox(chunkPos,
			    chunkPos + new Vector3(ChunkColumn.ChunkWidth, 16 * ((chunk.GetHeighest() >> 4) + 1),
				    ChunkColumn.ChunkDepth)));

	    }

	    private bool IsWithinView(ChunkCoordinates chunk, BoundingFrustum frustum)
	    {
		    var chunkPos = new Vector3(chunk.X * ChunkColumn.ChunkWidth, 0, chunk.Z * ChunkColumn.ChunkDepth);
		    return frustum.Intersects(new Microsoft.Xna.Framework.BoundingBox(chunkPos,
			    chunkPos + new Vector3(ChunkColumn.ChunkWidth, CameraPosition.Y + 10,
				    ChunkColumn.ChunkDepth)));

	    }

        internal bool UpdateChunk(IChunkColumn chunk)
	    {
			if (!Monitor.TryEnter(chunk.UpdateLock))
			{
				Interlocked.Decrement(ref _chunkUpdates);
				return false; //Another thread is already updating this chunk, return.
			}

			try
			{

			//	if (chunk.Scheduled.HasFlag(ScheduleType.Full))
				{
					chunk.GenerateMeshes(World, out var mesh);

					VertexBuffer opaqueVertexBuffer = chunk.VertexBuffer;
					if (opaqueVertexBuffer == null ||
					    mesh.SolidVertices.Length != opaqueVertexBuffer.VertexCount)
					{
						opaqueVertexBuffer = RenewVertexBuffer(mesh.SolidVertices);

						VertexBuffer oldBuffer;
						lock (chunk.VertexLock)
						{
							oldBuffer = chunk.VertexBuffer;
							chunk.VertexBuffer = opaqueVertexBuffer;
						}

						oldBuffer?.Dispose();
					}
					else if (mesh.SolidVertices.Length > 0)
					{
						chunk.VertexBuffer.SetData(mesh.SolidVertices);
					}

					VertexBuffer transparentVertexBuffer = chunk.TransparentVertexBuffer;
					if (transparentVertexBuffer == null ||
					    mesh.TransparentVertices.Length != transparentVertexBuffer.VertexCount)
					{
						transparentVertexBuffer = RenewVertexBuffer(mesh.TransparentVertices);

						VertexBuffer oldBuffer;
						lock (chunk.VertexLock)
						{
							oldBuffer = chunk.TransparentVertexBuffer;
							chunk.TransparentVertexBuffer = transparentVertexBuffer;
						}

						oldBuffer?.Dispose();
					}
					else if (mesh.TransparentVertices.Length > 0)
					{
						chunk.TransparentVertexBuffer.SetData(mesh.TransparentVertices);
					}
				}

				chunk.IsDirty = false;
				chunk.Scheduled = ScheduleType.Unscheduled;

				return true;
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Exception while updating chunk: {ex.ToString()}");
			}
			finally
			{
				//Enqueued.Remove(new ChunkCoordinates(chunk.X, chunk.Z));
                Interlocked.Decrement(ref _chunkUpdates);
				Monitor.Exit(chunk.UpdateLock);
			}

		    return false;
	    }

	    private VertexBuffer RenewVertexBuffer(VertexPositionNormalTextureColor[] vertices)
	    {
		    VertexBuffer buffer = new VertexBuffer(Graphics,
			    VertexPositionNormalTextureColor.VertexDeclaration,
			    vertices.Length,
			    BufferUsage.WriteOnly);

		    if (vertices.Length > 0)
		    {
			    buffer.SetData(vertices);
		    }

		    return buffer;
	    }

	    public void Draw(IRenderArgs args)
	    {
		    var device = args.GraphicsDevice;
		    var camera = args.Camera;

		    Stopwatch sw = Stopwatch.StartNew();
			
		    TransparentEffect.View = camera.ViewMatrix;
		    TransparentEffect.Projection = camera.ProjectionMatrix;

		    OpaqueEffect.View = camera.ViewMatrix;
		    OpaqueEffect.Projection = camera.ProjectionMatrix;

			var r = _renderedChunks.ToArray();
		    var chunks = new KeyValuePair<ChunkCoordinates, IChunkColumn>[r.Length];
		    for (var index = 0; index < r.Length; index++)
		    {
			    var c = r[index];
			    if (Chunks.TryGetValue(c, out IChunkColumn chunk))
			    {
				    chunks[index] = new KeyValuePair<ChunkCoordinates, IChunkColumn>(c, chunk);
			    }
			    else
			    {
				    chunks[index] = new KeyValuePair<ChunkCoordinates, IChunkColumn>(c, null);
			    }
		    }

		    VertexBuffer[] opaqueBuffers = new VertexBuffer[chunks.Length];
		    VertexBuffer[] transparentBuffers = new VertexBuffer[chunks.Length];

		    var tempVertices = 0;
		    int tempChunks = 0;
		    int tempFailed = 0;
		    int opaqueFramesFailed = 0;
		    int transparentFramesFailed = 0;

		    for (var index = 0; index < chunks.Length; index++)
		    {
			    var c = chunks[index];
			    var chunk = c.Value;
			    if (chunk == null) continue;

			    VertexBuffer buffer = null;
			    VertexBuffer transparentBuffer = null;


			    buffer = chunk.VertexBuffer;
			    transparentBuffer = chunk.TransparentVertexBuffer;


			    if (buffer != null)
			    {
				    opaqueBuffers[index] = buffer;
			    }

			    if (transparentBuffer != null)
			    {
				    transparentBuffers[index] = transparentBuffer;
			    }

			    if ((chunk.IsDirty || (buffer == null || transparentBuffer == null)) &&
			        chunk.Scheduled == ScheduleType.Unscheduled)
			    {
				    //	ScheduleChunkUpdate(c.Key, ScheduleType.Full);
				    if (buffer == null && transparentBuffer == null)
				    {
					    tempFailed++;
				    }

				    continue;
			    }

			    if (transparentBuffer != null && buffer != null)
				    tempChunks++;
		    }

		    //Render Solid
		    device.DepthStencilState = DepthStencilState.Default;
		    device.BlendState = BlendState.AlphaBlend;

		    foreach (var b in opaqueBuffers)
		    {
			    if (b == null)
			    {
				    opaqueFramesFailed++;
				    continue;
			    }

			    if (b.VertexCount == 0) continue;

			    device.SetVertexBuffer(b);

			    foreach (var pass in OpaqueEffect.CurrentTechnique.Passes)
			    {
				    pass.Apply();
				    //device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount /3);
			    }

			    device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount / 3);

			    tempVertices += b.VertexCount;
		    }

			//Render transparent blocks
		    foreach (var b in transparentBuffers)
		    {
			    if (b == null)
			    {
				    transparentFramesFailed++;
				    continue;
			    }

			    if (b.VertexCount == 0) continue;

			    device.SetVertexBuffer(b);

			    foreach (var pass in TransparentEffect.CurrentTechnique.Passes)
			    {
				    pass.Apply();
			    }

			    device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount / 3);

			    tempVertices += b.VertexCount;
		    }

		    Vertices = tempVertices;
		    RenderedChunks = tempChunks;

		    sw.Stop();
		    if (tempFailed > 0)
		    {
			    /*	Log.Debug(
					    $"Frame time: {sw.Elapsed.TotalMilliseconds}ms\n\t\tTransparent: {transparentFramesFailed} / {transparentBuffers.Length} chunks\n\t\tOpaque: {opaqueFramesFailed} / {opaqueBuffers.Length} chunks\n\t\t" +
					    $"Full chunks: {tempChunks} / {chunks.Length}\n\t\t" +
					    $"Missed frames: {tempFailed}");*/
		    }
	    }

	    public Vector3 FogColor
	    {
		    get { return TransparentEffect.FogColor; }
		    set
		    {
				TransparentEffect.FogColor = value;
			    OpaqueEffect.FogColor = value;
		    }
	    }

	    public float FogDistance
	    {
		    get { return TransparentEffect.FogEnd; }
		    set
		    {
			    TransparentEffect.FogEnd = value;
			    OpaqueEffect.FogEnd = value;
		    }
	    }

	    public Vector3 AmbientLightColor
		{
		    get { return TransparentEffect.DiffuseColor; }
		    set
		    {
			    TransparentEffect.DiffuseColor = value;
			    OpaqueEffect.AmbientLightColor = value;
		    }
	    }

	    private Vector3 CamDir = Vector3.Zero;
		public void Update(IUpdateArgs args)
	    {
		  //  TransparentEffect.FogColor = skyRenderer.WorldFogColor.ToVector3();
		 //   OpaqueEffect.FogColor = skyRenderer.WorldFogColor.ToVector3();

		    //   OpaqueEffect.AmbientLightColor = TransparentEffect.DiffuseColor =
			//    Color.White.ToVector3() * new Vector3((skyRenderer.BrightnessModifier));
			
		    var camera = args.Camera;
		    CameraBoundingFrustum = camera.BoundingFrustum;
		    CameraPosition = camera.Position;
		    CamDir = camera.Target;

			var renderedChunks = Chunks.ToArray().Where(x =>
		    {
			    var chunkPos = new Vector3(x.Key.X * ChunkColumn.ChunkWidth, 0, x.Key.Z * ChunkColumn.ChunkDepth);
			    return camera.BoundingFrustum.Intersects(new Microsoft.Xna.Framework.BoundingBox(chunkPos,
				    chunkPos + new Vector3(ChunkColumn.ChunkWidth, 16 * ((x.Value.GetHeighest() >> 4) + 1),
					    ChunkColumn.ChunkDepth)));
		    }).ToArray();

			foreach (var c in renderedChunks)
			{
				if (_renderedChunks.TryAdd(c.Key))
				{
					if (c.Value.SkyLightDirty)
						SkylightCalculator.CalculateLighting(c.Value, true, true);
                }
			}

		    foreach (var c in _renderedChunks.ToArray())
		    {
			    if (!renderedChunks.Any(x => x.Key.Equals(c)))
			    {
				    _renderedChunks.Remove(c);
			    }
		    }
		}

        public void AddChunk(IChunkColumn chunk, ChunkCoordinates position, bool doUpdates = false)
        {
            Chunks.AddOrUpdate(position, coordinates =>
            {
	           
                return chunk;
            }, (vector3, chunk1) =>
            {
	            if (!ReferenceEquals(chunk1, chunk))
	            {
		            chunk1.Dispose();
	            }

	            Log.Warn($"Replaced/Updated chunk at {position}");
                return chunk;
            });

	        //SkylightCalculator.CalculateLighting(chunk, true, true, false);

            if (doUpdates)
			{
				/*if (new ChunkCoordinates(CameraPosition).DistanceTo(position) < 6)
				{
					ScheduleChunkUpdate(position, ScheduleType.Full | ScheduleType.Skylight);
					/*ScheduleChunkUpdate(new ChunkCoordinates(position.X + 1, position.Z), ScheduleType.Border | ScheduleType.Skylight);
					ScheduleChunkUpdate(new ChunkCoordinates(position.X - 1, position.Z), ScheduleType.Border | ScheduleType.Skylight);
					ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z + 1), ScheduleType.Border | ScheduleType.Skylight);
					ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z - 1), ScheduleType.Border | ScheduleType.Skylight);
                }
				else
				{*/
			    //SkylightCalculator.CalculateLighting(chunk, true, false);
					ScheduleChunkUpdate(position, ScheduleType.Full);
				//}

				ScheduleChunkUpdate(new ChunkCoordinates(position.X + 1, position.Z), ScheduleType.Border);
				ScheduleChunkUpdate(new ChunkCoordinates(position.X - 1, position.Z), ScheduleType.Border);
				ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z + 1), ScheduleType.Border);
				ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z - 1), ScheduleType.Border);
            }
		}

	    public void ScheduleChunkUpdate(ChunkCoordinates position, ScheduleType type, bool prioritize = false)
	    {

		    if (Chunks.TryGetValue(position, out IChunkColumn chunk))
		    {
			    var currentSchedule = chunk.Scheduled;
			    if (prioritize)
			    {
				    chunk.Scheduled = type;

				    HighestPriority.Enqueue(position);
                    if (!Enqueued.Contains(position))
				    {
					    Enqueued.TryAdd(position);
                    }

				    //Interlocked.Increment(ref _chunkUpdates);
				//    _updateResetEvent.Set();

                    if (type.HasFlag(ScheduleType.Skylight))
				    {
					    SkylightCalculator.CalculateLighting(chunk, true, true);
				    }

                    return;
			    }

                if (type.HasFlag(ScheduleType.Skylight) && !currentSchedule.HasFlag(ScheduleType.Skylight))
                {
	                chunk.Scheduled = type;
                    SkylightCalculator.CalculateLighting(chunk, true, true);
                }
			    else
			    {
				    if (currentSchedule != ScheduleType.Unscheduled)
				    {
					    /*if (currentSchedule != ScheduleType.Full)
					    {
						    chunk.Scheduled = type;
					    }*/

					    return;
				    }

				    if (!Enqueued.Contains(position) && Enqueued.TryAdd(position))
				    {
					    chunk.Scheduled = type;
                        /*if (IsWithinView(chunk, CameraBoundingFrustum))
					    {
						    HighPriority.Enqueue(position);
					    }
					    else
					    {
						    LowPriority.Enqueue(position);
					    }*/

					    Interlocked.Increment(ref _chunkUpdates);
					//    _updateResetEvent.Set();
                    }    
			    }
		    }
	    }

	    public void RemoveChunk(ChunkCoordinates position, bool dispose = true)
        {
	        if (_workItems.TryGetValue(position, out var r))
	        {
		        r.Cancel();
	        }

            IChunkColumn chunk;
	        if (Chunks.TryRemove(position, out chunk))
	        {
		        if (dispose)
		        {
			        chunk?.Dispose();
		        }
	        }

	        LowPriority.Remove(position);

	        if (Enqueued.Remove(position))
	        {
		        Interlocked.Decrement(ref _chunkUpdates);
            }

			SkylightCalculator.Remove(position);
        }

        public void RebuildAll()
        {
            foreach (var i in Chunks)
            {
                ScheduleChunkUpdate(i.Key, ScheduleType.Full | ScheduleType.Skylight);
            }
        }

	    public KeyValuePair<ChunkCoordinates, IChunkColumn>[]GetAllChunks()
	    {
		    return Chunks.ToArray();
	    }

	    public void ClearChunks()
	    {
		    var chunks = Chunks.ToArray();
			Chunks.Clear();

		    foreach (var chunk in chunks)
		    {
				chunk.Value.Dispose();
		    }
			Enqueued.Clear();
	    }

	    public bool TryGetChunk(ChunkCoordinates coordinates, out IChunkColumn chunk)
	    {
		    return Chunks.TryGetValue(coordinates, out chunk);
	    } 

	    public void Dispose()
	    {
		    CancelationToken.Cancel();
			
			foreach (var chunk in Chunks.ToArray())
		    {
			    Chunks.TryRemove(chunk.Key, out IChunkColumn _);
			    Enqueued.Remove(chunk.Key);
                chunk.Value.Dispose();
		    }
	    }
    }
}