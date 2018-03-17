using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using Alex.API.Graphics;
using Alex.Engine;
using Alex.Engine.Graphics;
using Alex.Rendering.Camera;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Veldrid;
using Veldrid.OpenGLBinding;
using Veldrid.Utilities;
using Vulkan;

namespace Alex.Graphics.Models.Entity
{
	public class EntityModelRenderer : Model
	{
		private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(EntityModelRenderer));
		private static ConcurrentDictionary<string, VertexPositionNormalTexture[]> ModelBonesCache { get; } = new ConcurrentDictionary<string, VertexPositionNormalTexture[]>();

		private EntityModel Model { get; }
		private IReadOnlyDictionary<string, ModelPart> Cubes { get; }
		private Texture Texture { get; set; }
		//private AlphaTestEffect Effect { get; set; }
		public EntityModelRenderer(EntityModel model, Texture texture)
		{
			Model = model;
			Texture = texture;

			var cubes = new Dictionary<string, ModelPart>();
			Cache(cubes);

			Cubes = cubes;
		}

		static EntityModelRenderer()
		{
			
		}

		private void Cache(Dictionary<string, ModelPart> cubes)
		{
		//	List<VertexPositionNormalTexture> textures = new List<VertexPositionNormalTexture>();
			foreach (var bone in Model.Bones)
			{
				if (bone == null) continue;
				if (bone.NeverRender) continue;

				if (bone.Cubes != null)
				{
					foreach (var cube in bone.Cubes)
					{
						if (cube == null)
						{
							Log.Warn("Cube was null!");
							continue;
						}

						if (cube.Uv == null)
						{
							Log.Warn("Cube.UV was null!");
							continue;
						}

						if (cube.Origin == null)
						{
							Log.Warn("Cube.Origin was null!");
							continue;
						}

						if (cube.Size == null)
						{
							Log.Warn("Cube.Size was null!");
							continue;
						}

						var size = cube.Size;
						var origin = cube.Origin;
						var pivot = bone.Pivot;
						var rotation = bone.Rotation;

						VertexPositionNormalTexture[] vertices = ModelBonesCache.GetOrAdd($"{Model.Name}:{bone.Name}", s =>
						{
							Cube built = new Cube(size, new Vector2(Texture.Width, Texture.Height), new Vector2(cube.Uv.X, cube.Uv.Y));

							return built.Front.Concat(built.Back).Concat(built.Top).Concat(built.Bottom).Concat(built.Left)
								.Concat(built.Right).ToArray();
						});

						if (!cubes.TryAdd(bone.Name, new ModelPart(vertices, 
							Texture,
							rotation, pivot, origin)))
						{
							Log.Warn($"Failed to add cube to list of bones: {Model.Name}:{bone.Name}");
						}
					}
				}
			}
		}

		public void Render(IRenderArgs args, Camera camera, Vector3 position, float yaw, float pitch)
		{
			foreach (var bone in Cubes)
			{
				if (bone.Value.IsDirty)
					continue;


				bone.Value.Render(args, camera, position, yaw, pitch);
			}
		}

		public void Update(GraphicsDevice device, GameTime gameTime, Vector3 position, float yaw, float pitch)
		{
			foreach (var bone in Cubes)
			{
				if (!bone.Value.IsDirty)
					continue;
				
				bone.Value.Update(device, gameTime);
			}
		}

		public override string ToString()
		{
			return Model.Name;
		}

		private class ModelPart : IDisposable
		{
			private Shader Effect { get; set; }
			private VertexBuffer Buffer { get; set; }
			private VertexPositionNormalTexture[] _vertices;

			public bool IsDirty { get; private set; }
			public Texture Texture { get; private set; }

			public Vector3 Rotation { get; private set; } = Vector3.Zero;
			public Vector3 Pivot { get; private set; } = Vector3.Zero;
			public Vector3 Origin { get; private set; }
			public ModelPart(VertexPositionNormalTexture[] textures, Texture texture, Vector3 rotation, Vector3 pivot, Vector3 origin)
			{
				_vertices = textures;
				Texture = texture;
				Rotation = rotation;
				Pivot = pivot;
				Origin = origin;

				Mod(ref _vertices, Origin, Pivot, Rotation);

				IsDirty = true;
			}

			public void Update(GraphicsDevice device, GameTime gameTime)
			{
				if (!IsDirty) return;

				//if (_vertices.Length > 0)
				//	Mod(ref _vertices, Origin, Pivot, Rotation);

				if (Effect == null)
				{
					return;
					//	Effect = new AlphaTestEffect(device);
					//	Effect.Texture = Texture;
				}

				DeviceBuffer currentBuffer = Buffer;
				if (_vertices.Length > 0 && (Buffer == null || currentBuffer.SizeInBytes != _vertices.Length * VertexPositionNormalTexture.SizeInBytes))
				{
					if (currentBuffer == null)
					{
						Buffer = new VertexBuffer(device, VertexPositionNormalTexture.SizeInBytes, _vertices.Length, BufferUsage.VertexBuffer);
						currentBuffer = Buffer;
						device.UpdateBuffer(currentBuffer, 0, _vertices);
						//currentBuffer.SetData(_vertices);
					}
					else if (_vertices.Length * VertexPositionNormalTexture.SizeInBytes > currentBuffer.SizeInBytes)
					{
						DeviceBuffer oldBuffer = currentBuffer;

						currentBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(VertexPositionNormalTexture.SizeInBytes * _vertices.Length),
							BufferUsage.VertexBuffer));

						device.UpdateBuffer(currentBuffer, 0, _vertices);

						//Buffer = ;
						oldBuffer.Dispose();
					}
					else
					{
						device.UpdateBuffer(currentBuffer, 0, _vertices);
					}
				}

				IsDirty = false;
			}

			public void Render(IRenderArgs args, Camera camera, Vector3 position, float yaw, float pitch)
			{
				if (_vertices == null || _vertices.Length == 0) return;

				if (Effect == null || Buffer == null) return;

				var buffer = Buffer;

				//Effect.World = Matrix4x4.CreateScale(1f / 16f) * Matrix4x4.CreateRotationY(MathUtils.ToRadians(yaw)) * Matrix4x4.CreateRotationX(MathUtils.ToRadians(pitch)) *
				//               Matrix4x4.CreateTranslation(position);

				//Effect.View = camera.ViewMatrix;
				//Effect.Projection = camera.ProjectionMatrix;

				//Effect.World = Matrix.CreateScale(1f / 16f) * Matrix.CreateTranslation(position);

				//args.GraphicsDevice.SetVertexBuffer(buffer);
			//	foreach (var pass in Effect.CurrentTechnique.Passes)
			//	{
			//		pass.Apply();
				//}

				//args.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertices.Length / 3);
			}

			private void Mod(ref VertexPositionNormalTexture[] data, Vector3 origin, Vector3 pivot, Vector3 rotation)
			{
				Matrix4x4 transform =
					Matrix4x4.CreateRotationX(rotation.X) *
					Matrix4x4.CreateRotationY(rotation.Y) *
					Matrix4x4.CreateRotationZ(rotation.Z) *
					Matrix4x4.CreateTranslation(pivot.X, pivot.Y, pivot.Z);

				for (int i = 0; i < data.Length; i++)
				{
					var pos = data[i].Position;

					pos = origin + pos;
					if (rotation != Vector3.Zero)
					{
						pos = Vector3.Transform(pos, transform);
					}

					data[i] = new VertexPositionNormalTexture(pos, data[i].Normal, data[i].TextureCoordinates);
				}
			}

			public void Dispose()
			{
				IsDirty = false;
				Effect?.Dispose();
				Buffer?.Dispose();
			}
		}
	}
}
