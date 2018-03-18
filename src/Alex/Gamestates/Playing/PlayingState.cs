﻿using System;
using System.Drawing;
using System.Numerics;
using Alex.API.World;
using Alex.Blocks;
using Alex.Engine;
using Alex.Graphics;
using Alex.Rendering.Camera;
using Alex.Rendering.UI;
using Alex.Utils;
using Alex.Worlds;

using MiNET;
using MiNET.Utils;
using Veldrid;
using Veldrid.Sdl2;
using Rectangle = Veldrid.Rectangle;

namespace Alex.Gamestates.Playing
{
	public class PlayingState : GameState
	{
		private World World { get; }
		private FirstPersonCamera Camera;
		private CameraComponent CamComponent { get; }

		private FpsMonitor FpsCounter { get; set; }
		private Texture CrosshairTexture { get; set; }

		private ChatComponent Chat { get; }

		public PlayingState(Alex alex, GraphicsDevice graphics, WorldProvider worldProvider) : base(alex)
		{
			//Alex = alex;
			Chat = new ChatComponent();

			Camera = new FirstPersonCamera(alex.GameSettings.RenderDistance, Vector3.Zero, Vector3.Zero);

			World = new World(alex, graphics, Camera, worldProvider);
			Camera.MoveTo(World.GetSpawnPoint(), Vector3.Zero);

			CamComponent = new CameraComponent(Camera, Graphics, World, alex.GameSettings);
		}

		protected override void OnLoad(RenderArgs args)
		{
			Controls.Add("chatComponent", Chat);

			FpsCounter = new FpsMonitor();
			CrosshairTexture = TextureUtils.ImageToTexture2D(args.GraphicsDevice, Resources.crosshair);

			Camera.MoveTo(World.GetSpawnPoint(), Vector3.Zero);
			base.OnLoad(args);
		}

		private float AspectRatio { get; set; }
		private bool RenderWireframe { get; set; } = false;
		private string MemoryUsageDisplay { get; set; } = "";

		private TimeSpan _previousMemUpdate = TimeSpan.Zero;
		protected override void OnUpdate(GameTime gameTime)
		{
			if (Alex.Window.Focused)
			{
				var newAspectRatio = Alex.Viewport.Width / Alex.Viewport.Height;// Graphics.Viewport.AspectRatio;
				if (AspectRatio != newAspectRatio)
				{
					Camera.UpdateAspectRatio(newAspectRatio);
					AspectRatio = newAspectRatio;
				}

				CamComponent.Update(gameTime, !Chat.RenderChatInput);

				UpdateRayTracer(Alex.GraphicsDevice, World);

				CheckInput(gameTime);

				World.Update(gameTime);

				var headBlock = World.GetBlock(Camera.Position);
				if (headBlock.BlockId == 8 || headBlock.BlockId == 9)
				{
					if (!_renderWaterOverlay)
					{
						_renderWaterOverlay = true;
					}
				}else if (_renderWaterOverlay)
				{
					_renderWaterOverlay = false;
				}


				if (RenderDebug)
				{
					if (gameTime.TotalGameTime - _previousMemUpdate > TimeSpan.FromSeconds(5))
					{
						_previousMemUpdate = gameTime.TotalGameTime;
						//Alex.Process.Refresh();
						MemoryUsageDisplay = $"Allocated memory: {GetBytesReadable(Environment.WorkingSet)}";
					}
				}
			}
			base.OnUpdate(gameTime);
		}

		private bool _renderWaterOverlay = false;

		private Vector3 _raytracedBlock;
		protected void UpdateRayTracer(GraphicsDevice graphics, World world)
		{
			_raytracedBlock = RayTracer.Raytrace(graphics, world, Camera);
			if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
			{
				SelBlock = (Block)World.GetBlock(_raytracedBlock.X, _raytracedBlock.Y, _raytracedBlock.Z);
				RayTraceBoundingBox = SelBlock.GetBoundingBox(_raytracedBlock);
			}
			else
			{
				SelBlock = new Air();
			}
		}

		private void ToggleWireframe()
		{
			RenderWireframe = !RenderWireframe;

			if (RenderWireframe)
			{
				//Graphics.RasterizerState.FillMode = FillMode.WireFrame;
			}
			else
			{
		//		Graphics.RasterizerState.FillMode = FillMode.Solid;
			}
		}

		private Block SelBlock { get; set; } = new Air();
		private BoundingBox RayTraceBoundingBox { get; set; }
		private bool RenderDebug { get; set; } = true;

		private InputSnapshot _oldKeyboardState;
		private MouseState _oldMouseState;
		protected void CheckInput(GameTime gameTime)
		{
			/*MouseState currentMouseState = Mouse.GetState();
			if (currentMouseState != _oldMouseState)
			{
				if (currentMouseState.LeftButton == ButtonState.Pressed)
				{
					if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
					{
						//World.SetBlock(_selectedBlock.X, _selectedBlock.Y, _selectedBlock.Z, new Air());
					}
				}

				if (currentMouseState.RightButton == ButtonState.Pressed)
				{
					if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
					{
						SelBlock.Interact(World,
							new BlockCoordinates(new PlayerLocation(_raytracedBlock.X, _raytracedBlock.Y, _raytracedBlock.Z)), BlockFace.None, null);
					}
				}
			}
			_oldMouseState = currentMouseState;*/

			var currentKeyboardState = Alex.Window.PumpEvents();
			
		//	KeyboardState currentKeyboardState = Keyboard.GetState();
			if (currentKeyboardState != _oldKeyboardState)
			{
				if (currentKeyboardState.IsKeyDown(KeyBinds.Menu))
				{
					if (Chat.RenderChatInput)
					{
						Chat.Dismiss();
					}
					else
					{
						Alex.GameStateManager.SetActiveState(new InGameMenuState(Alex, this, currentKeyboardState));
					}
				}

				if (!Chat.RenderChatInput)
				{
					if (currentKeyboardState.IsKeyDown(KeyBinds.DebugInfo))
					{
						RenderDebug = !RenderDebug;
					}

					if (currentKeyboardState.IsKeyDown(KeyBinds.ToggleWireframe)) 
					{
						ToggleWireframe();
					}

					if (currentKeyboardState.IsKeyDown(KeyBinds.ToggleFreeCam))
					{
						CamComponent.IsFreeCam = !CamComponent.IsFreeCam;
					}

					if (currentKeyboardState.IsKeyDown(KeyBinds.ReBuildChunks))
					{
						World.RebuildChunks();
					}
				}
			}
			_oldKeyboardState = currentKeyboardState;
		}

		protected override void OnDraw2D(RenderArgs args)
		{
			try
			{
				args.SpriteBatch.Begin(ref args.Commands);


				if (_renderWaterOverlay)
				{
					//Start draw background
				/*	var retval = new Rectangle(
						args.SpriteBatch.GraphicsDevice.Viewport.X,
						args.SpriteBatch.GraphicsDevice.Viewport.Y,
						args.SpriteBatch.GraphicsDevice.Viewport.Width,
						args.SpriteBatch.GraphicsDevice.Viewport.Height);
					args.SpriteBatch.FillRectangle(retval, new Color(Color.DarkBlue, 0.5f));*/
					//End draw backgroun
				}

				args.SpriteBatch.Draw(CrosshairTexture,
					new Vector2(CenterScreen.X - CrosshairTexture.Width / 2f, CenterScreen.Y - CrosshairTexture.Height / 2f));

				if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
				{
					args.SpriteBatch.RenderBoundingBox(
						new Veldrid.Utilities.BoundingBox(RayTraceBoundingBox.Min, RayTraceBoundingBox.Max), 
						Camera.ViewMatrix, Camera.ProjectionMatrix, Color.LightGray);
				}

				World.Render2D(args);

				if (RenderDebug)
				{
					RenderDebugScreen(args);
				}
			}
			finally
			{
				args.SpriteBatch.End();
			}

		//	ActiveOverlays.ForEach(x => x.Render(args));

			//base.Render2D(args);
		}

		private void RenderDebugScreen(RenderArgs args)
		{
			DebugLeft(args);
			DebugRight(args);
		}

		private void DebugRight(RenderArgs args)
		{
			var bounds = Alex.Window.Bounds;
		
			var screenWidth = bounds.Width;
			//var device = args.GraphicsDevice.Adapter.DeviceName;
			var positionString = "";
			var meisured = Vector2.Zero;
			int y = 0;

			var backColor = Color.FromArgb(64, Color.Black);

			positionString = Alex.DotnetRuntime;
			meisured = Alex.Font.MeasureString(positionString);

			args.SpriteBatch.FillRectangle(new Rectangle(screenWidth - (int)meisured.X, y, (int)meisured.X, (int)meisured.Y),
				backColor);
			args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(screenWidth - (int)meisured.X, y), Color.White);

			y += (int)meisured.Y;

			positionString = MemoryUsageDisplay;
			meisured = Alex.Font.MeasureString(positionString);

			args.SpriteBatch.FillRectangle(new Rectangle(screenWidth - (int)meisured.X, y, (int)meisured.X, (int)meisured.Y),
				backColor);
			args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(screenWidth - (int)meisured.X, y), Color.White);

			y += (int)meisured.Y;

			if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
			{
				positionString = "Looking at: " + _raytracedBlock;
				meisured = Alex.Font.MeasureString(positionString);

				args.SpriteBatch.FillRectangle(new Rectangle(screenWidth - (int) meisured.X, y, (int) meisured.X, (int) meisured.Y),
					backColor);
				args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(screenWidth - (int) meisured.X, y), Color.White);

				y += (int) meisured.Y;

				positionString = $"{SelBlock} ({SelBlock.BlockId}:{SelBlock.Metadata})";
				meisured = Alex.Font.MeasureString(positionString);

				args.SpriteBatch.FillRectangle(new Rectangle(screenWidth - (int) meisured.X, y, (int) meisured.X, (int) meisured.Y),
					backColor);
				args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(screenWidth - (int) meisured.X, y), Color.White);

				if (SelBlock.BlockState != null)
				{
					var dict = SelBlock.BlockState.ToDictionary();
					foreach (var kv in dict)
					{
						y += (int)meisured.Y;

						positionString = $"{kv.Key.Name}={kv.Value}";
						meisured = Alex.Font.MeasureString(positionString);

						args.SpriteBatch.FillRectangle(new Rectangle(screenWidth - (int)meisured.X, y, (int)meisured.X, (int)meisured.Y),
							backColor);
						args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(screenWidth - (int)meisured.X, y), Color.White);
					}
				}
			}
		}

		private void DebugLeft(RenderArgs args)
		{
			var fpsString = string.Format("Alex {0} ({1} FPS, {2} chunk updates)", Alex.Version,
					 Math.Round(FpsCounter.Value), World.ChunkUpdates);
			var meisured = Alex.Font.MeasureString(fpsString);

			var backColor = Color.FromArgb(64, Color.Black);

			args.SpriteBatch.FillRectangle(new Rectangle(0, 0, (int)meisured.X, (int)meisured.Y),
				backColor);
			args.SpriteBatch.DrawString(Alex.Font,
				fpsString, new Vector2(0, 0),
				Color.White);

			var y = (int)meisured.Y;
			var positionString = "Position: " + Camera.Position;
			meisured = Alex.Font.MeasureString(positionString);

			args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y),
				backColor);
			args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

			y += (int)meisured.Y;
			string facing = GetCardinalDirection(this.Camera);

			positionString = string.Format("Facing: {0}", facing);
			meisured = Alex.Font.MeasureString(positionString);

			args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y),
				backColor);
			args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

			y += (int)meisured.Y;


			positionString = "Vertices: " + World.Vertices;
			meisured = Alex.Font.MeasureString(positionString);

			args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y),
				backColor);
			args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

			y += (int)meisured.Y;

			positionString = "Chunks: " + World.ChunkCount + ", " + World.ChunkManager.RenderedChunks;
			meisured = Alex.Font.MeasureString(positionString);

			args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y),
				backColor);
			args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

			y += (int)meisured.Y;

			positionString = $"Entities: {World.EntityManager.EntityCount}, {World.EntityManager.EntitiesRendered}";
			meisured = Alex.Font.MeasureString(positionString);

			args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y),
				backColor);
			args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);
		}

		public static string GetCardinalDirection(FirstPersonCamera cam)
		{
			double rotation = (360 - cam.Yaw) % 360;
			if (rotation < 0)
			{
				rotation += 360.0;
			}

			return GetDirection(rotation);
		}

		private static string GetDirection(double rotation)
		{
			if (0 <= rotation && rotation < 22.5)
			{
				return "North";
			}
			else if (22.5 <= rotation && rotation < 67.5)
			{
				return "North East";
			}
			else if (67.5 <= rotation && rotation < 112.5)
			{
				return "East";
			}
			else if (112.5 <= rotation && rotation < 157.5)
			{
				return "South East";
			}
			else if (157.5 <= rotation && rotation < 202.5)
			{
				return "South";
			}
			else if (202.5 <= rotation && rotation < 247.5)
			{
				return "South West";
			}
			else if (247.5 <= rotation && rotation < 292.5)
			{
				return "West";
			}
			else if (292.5 <= rotation && rotation < 337.5)
			{
				return "North West";
			}
			else if (337.5 <= rotation && rotation < 360.0)
			{
				return "North";
			}
			else
			{
				return "N/A";
			}
		}

		private static string GetBytesReadable(long i)
		{
			// Get absolute value
			long absolute_i = (i < 0 ? -i : i);
			// Determine the suffix and readable value
			string suffix;
			double readable;
			if (absolute_i >= 0x1000000000000000) // Exabyte
			{
				suffix = "EB";
				readable = (i >> 50);
			}
			else if (absolute_i >= 0x4000000000000) // Petabyte
			{
				suffix = "PB";
				readable = (i >> 40);
			}
			else if (absolute_i >= 0x10000000000) // Terabyte
			{
				suffix = "TB";
				readable = (i >> 30);
			}
			else if (absolute_i >= 0x40000000) // Gigabyte
			{
				suffix = "GB";
				readable = (i >> 20);
			}
			else if (absolute_i >= 0x100000) // Megabyte
			{
				suffix = "MB";
				readable = (i >> 10);
			}
			else if (absolute_i >= 0x400) // Kilobyte
			{
				suffix = "KB";
				readable = i;
			}
			else
			{
				return i.ToString("0 B"); // Byte
			}
			// Divide by 1024 to get fractional value
			readable = (readable / 1024);
			// Return formatted number with suffix
			return readable.ToString("0.### ") + suffix;
		}

		protected override void OnDraw3D(RenderArgs args)
		{
			FpsCounter.Update();

			World.Render(args);

			base.OnDraw3D(args);
		}

		protected override void OnUnload()
		{
			World.Destroy();
		}
	}
}
