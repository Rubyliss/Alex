using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.Utils;
using Alex.Worlds;


using MiNET.Utils;
using MiNET.Worlds;
using Axis = Alex.ResourcePackLib.Json.Axis;

namespace Alex.Graphics.Models
{
	public class CachedResourcePackModel : BlockModel
	{
		private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(CachedResourcePackModel));
		static CachedResourcePackModel()
		{
			
		}

		protected ResourceManager Resources { get; }
		public CachedResourcePackModel(ResourceManager resources, BlockStateModel[] variant)
		{
			Resources = resources;

			if (variant != null)
				Cache(variant); 
		}

		private class VariantCache
		{
			public int X, Y;

			public Dictionary<int, FaceCache> ElementCache = new Dictionary<int, FaceCache>();

			public class FaceCache
			{
				public BlockFace Face;
				public VertexPositionNormalTextureColor[] Vertices;
				public Vector3 CullFace;
				public int TintIndex = 0;
				public bool Shade = false;

				/*public VertexPositionNormalTextureColor[] Up;
				public VertexPositionNormalTextureColor[] Down;
				public VertexPositionNormalTextureColor[] West;
				public VertexPositionNormalTextureColor[] East;
				public VertexPositionNormalTextureColor[] South;
				public VertexPositionNormalTextureColor[] North;
				public VertexPositionNormalTextureColor[] None;*/
			}
		}

		private Dictionary<int, VariantCache> _variantCaches = new Dictionary<int, VariantCache>();

		protected bool Cached { get; private set; } = false;
		protected void Cache(BlockStateModel[] variants)
		{
			Cached = true;

			var c = new Vector3(8f, 8f, 8f);
			foreach (var var in variants)
			{
				VariantCache variantCache = new VariantCache();
				variantCache.X = var.X;
				variantCache.Y = var.Y;

				//	var c = new Vector3(8f, 8f, 8f);
				var modelRotationMatrix = Matrix4x4.CreateTranslation(-c) * Matrix4x4.CreateRotationX((float)MathUtils.ToRadians(360f - var.X)) *
				                          Matrix4x4.CreateRotationY((float)MathUtils.ToRadians(360f - var.Y)) * Matrix4x4.CreateTranslation(c);

				foreach (var element in var.Model.Elements)
				{
					var elementFrom = new Vector3((element.From.X), (element.From.Y),
						(element.From.Z));

					var elementTo = new Vector3((element.To.X), (element.To.Y),
						(element.To.Z));

					var width = elementTo.X - elementFrom.X;
					var depth = elementTo.Z - elementFrom.Z;

					var origin = new Vector3(((elementTo.X + elementFrom.X) / 2f) - 8,
						((elementTo.Y + elementFrom.Y) / 2f) - 8,
						((elementTo.Z + elementFrom.Z) / 2f) - 8);

					var elementRotation = element.Rotation;
					Matrix4x4 elementRotationMatrix = GetElementRotationMatrix(elementRotation, out float scalingFactor);

					foreach (var face in element.Faces)
					{
						VariantCache.FaceCache elementCache = new VariantCache.FaceCache();

						var uv = face.Value.UV;
						var uvmap = GetTextureUVMap(Resources, ResolveTexture(var, face.Value.Texture), uv.X1, uv.X2, uv.Y1, uv.Y2);

						var faceKey = face.Key;

						VertexPositionNormalTextureColor[] faceVertices;
						faceVertices = GetFaceVertices(faceKey, elementFrom, elementTo, uvmap, face.Value.Rotation);

						for (var index = 0; index < faceVertices.Length; index++)
						{
							var vert = faceVertices[index];

							//Apply element rotation
							if (elementRotation.Axis != Axis.Undefined)
							{
								var trans = new Vector3((width), 0, (depth));
								if (elementRotation.Axis == Axis.X)
								{
									trans = new Vector3(width , 0, 0);
								}
								else if (elementRotation.Axis == Axis.Z)
								{
									trans = new Vector3(0, 0, depth );
								}

								vert.Position = Vector3.Transform(vert.Position, Matrix4x4.CreateTranslation(trans) * elementRotationMatrix * Matrix4x4.CreateTranslation(-trans));

								//Scale the texture back to its correct size
								if (elementRotation.Rescale) 
								{
									if (elementRotation.Axis == Axis.X || elementRotation.Axis == Axis.Z)
									{
										vert.Position.Y *= scalingFactor;
									}

									if (elementRotation.Axis == Axis.Y || elementRotation.Axis == Axis.Z)
									{
										vert.Position.X *= scalingFactor;
									}

									if (elementRotation.Axis == Axis.Y || elementRotation.Axis == Axis.X)
									{
										vert.Position.Z *= scalingFactor;
									}
								}
							}

							//Apply model rotation
							vert.Position = Vector3.Transform(vert.Position, modelRotationMatrix);
							
							//Scale the position
							vert.Position = (vert.Position / 16f);

							if (vert.Position.X > Max.X)
							{
								Max.X = vert.Position.X;
							}
							else if (vert.Position.X < Min.X)
							{
								Min.X = vert.Position.X;
							}

							if (vert.Position.Y > Max.Y)
							{
								Max.Y = vert.Position.Y;
							}
							else if (vert.Position.Y < Min.Y)
							{
								Min.Y = vert.Position.Y;
							}

							if (vert.Position.Z > Max.Z)
							{
								Max.Z = vert.Position.Z;
							}
							else if (vert.Position.Z < Min.Z)
							{
								Min.Z = vert.Position.Z;
							}


							faceVertices[index] = vert;
						}

						GetFaceValues(face.Value.CullFace, face.Key, out var cull, out var cullFace);

						elementCache.TintIndex = face.Value.TintIndex;
						elementCache.Face = cull;
						elementCache.Vertices = faceVertices;
						elementCache.CullFace = cullFace;

						variantCache.ElementCache.Add(element.GetHashCode() ^ face.GetHashCode(), elementCache);
					}
				}

				_variantCaches.Add(variantCache.GetHashCode(), variantCache);
			}
		}

		protected Matrix4x4 GetElementRotationMatrix(BlockModelElementRotation elementRotation, out float rescale)
		{
			Matrix4x4 faceRotationMatrix = Matrix4x4.Identity;
			float ci = 0f;

			if (elementRotation.Axis != Axis.Undefined)
			{
				var elementRotationOrigin = new Vector3(elementRotation.Origin.X, elementRotation.Origin.Y, elementRotation.Origin.Z);

				//var elementAngle =
				//	MathUtils.ToRadians((float)(elementRotation.Axis == Axis.X ? -elementRotation.Angle : elementRotation.Angle));
				//elementAngle = elementRotation.Axis == Axis.Z ? elementAngle : -elementAngle;
				var elementAngle = MathUtils.ToRadians((float)elementRotation.Angle);
				ci = 1f / (float)Math.Cos(elementAngle);

				faceRotationMatrix = Matrix4x4.CreateTranslation(-elementRotationOrigin);
				if (elementRotation.Axis == Axis.X)
				{
					faceRotationMatrix *= Matrix4x4.CreateRotationX(elementAngle);
				}
				else if (elementRotation.Axis == Axis.Y)
				{
					faceRotationMatrix *= Matrix4x4.CreateRotationY(elementAngle);
				}
				else if (elementRotation.Axis == Axis.Z)
				{
					faceRotationMatrix *= Matrix4x4.CreateRotationZ(elementAngle);
				}

				faceRotationMatrix *= Matrix4x4.CreateTranslation(elementRotationOrigin);
			}

			rescale = ci;
			return faceRotationMatrix;
		}


		protected void GetFaceValues(string facename, BlockFace originalFace, out BlockFace face, out Vector3 offset)
		{
			Vector3 cullFace = Vector3.Zero;

			BlockFace cull;
			if (!Enum.TryParse(facename, out cull))
			{
				cull = originalFace;
			}
			switch (cull)
			{
				case BlockFace.Up:
					cullFace = Vector3.UnitY;
					break;
				case BlockFace.Down:
					cullFace = -Vector3.UnitY;
					break;
				case BlockFace.North:
					cullFace = -Vector3.UnitZ;
					break;
				case BlockFace.South:
					cullFace = Vector3.UnitZ;
					break;
				case BlockFace.West:
					cullFace = -Vector3.UnitX;
					break;
				case BlockFace.East:
					cullFace = Vector3.UnitX;
					break;
			}

			offset = cullFace;
			face = cull;
		}

		protected string ResolveTexture(BlockStateModel var, string texture)
		{
			string textureName = "no_texture";
			if (!var.Model.Textures.TryGetValue(texture.Replace("#", ""), out textureName))
			{
				textureName = texture;
			}

			if (textureName.StartsWith("#"))
			{
				if (!var.Model.Textures.TryGetValue(textureName.Replace("#", ""), out textureName))
				{
					textureName = "no_texture";
				}
			}

			return textureName;
		}

		public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, Block baseBlock)
		{
			var verts = new List<VertexPositionNormalTextureColor>();

			// MaxY = 0;
			Vector3 worldPosition = new Vector3(position.X, position.Y, position.Z);

			foreach (var variant in _variantCaches.Values)
			{
				var modelRotationMatrix = Matrix4x4.CreateRotationX((float)MathUtils.ToRadians(360f - variant.X)) *
				                          Matrix4x4.CreateRotationY((float)MathUtils.ToRadians(360f - variant.Y));

				foreach (var face in variant.ElementCache.Values)
				{
					var cullFace = Vector3.Transform(face.CullFace, modelRotationMatrix);

					if (cullFace != Vector3.Zero && !CanRender(world, baseBlock, worldPosition + cullFace))
						continue;

					VertexPositionNormalTextureColor[] faceVertices = face.Vertices;

					Color faceColor = faceVertices[0].Color;

					if (face.TintIndex >= 0)
					{
						int biomeId = world.GetBiome((int)worldPosition.X, 0, (int)worldPosition.Z);

						if (biomeId != -1)
						{
							var biome = BiomeUtils.GetBiomeById(biomeId);

							if (baseBlock.BlockId == 2)
							{
								faceColor = Resources.ResourcePack.GetGrassColor(biome.Temperature, biome.Downfall, (int)worldPosition.Y);
							}
							else
							{
								faceColor = Resources.ResourcePack.GetFoliageColor(biome.Temperature, biome.Downfall, (int)worldPosition.Y);
							}
						}
					}

					faceColor = UvMapHelp.AdjustColor(faceColor, face.Face, GetLight(world, worldPosition + cullFace), face.Shade);

					for (var index = 0; index < faceVertices.Length; index++)
					{
						var vert = faceVertices[index];
						vert.Color = faceColor;

						vert.Position = worldPosition + vert.Position;

						verts.Add(vert);
					}
				}
			}

			return verts.ToArray();
		}

		private Vector3 Min = Vector3.Zero;
		private Vector3 Max = Vector3.One;
		public override BoundingBox GetBoundingBox(Vector3 position, Block requestingBlock)
		{
			var min = Min / 16f;
			var max = Max / 16f;

			return new BoundingBox(position + min, position + max);
		}
	}
}