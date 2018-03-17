using System;
using System.Drawing;
using System.Numerics;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks;
using Alex.Utils;
using Alex.ResourcePackLib.Json;
using Veldrid.Utilities;

namespace Alex.Graphics.Models
{
    public class BlockModel : Model
	{
        public BlockModel()
        {

        }

        public virtual VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, Block baseBlock)
        {
            return new VertexPositionNormalTextureColor[0];
        }

	    public virtual MiNET.Utils.BoundingBox GetBoundingBox(Vector3 position, Block requestingBlock)
	    {
			return new MiNET.Utils.BoundingBox(position, position + Vector3.One);
	    }

		protected VertexPositionNormalTextureColor[] GetFaceVertices(BlockFace blockFace, Vector3 startPosition, Vector3 endPosition, UVMap uvmap, int rotation = 0)
		{
			Color faceColor = Color.White;
			Vector3 normal = Vector3.Zero;
			Vector3 textureTopLeft = Vector3.Zero, textureBottomLeft = Vector3.Zero, textureBottomRight = Vector3.Zero, textureTopRight = Vector3.Zero;
			switch (blockFace)
			{
				case BlockFace.Up: //Positive Y
					textureTopLeft = new Vector3(startPosition.X, endPosition.Y, endPosition.Z);
					textureTopRight = new Vector3(endPosition.X, endPosition.Y, endPosition.Z);

					textureBottomLeft = new Vector3(startPosition.X, endPosition.Y, startPosition.Z);
					textureBottomRight = new Vector3(endPosition.X, endPosition.Y, startPosition.Z);

					normal = Vector3.UnitY;
					faceColor = uvmap.ColorTop;
					break;
				case BlockFace.Down: //Negative Y
					textureTopLeft = new Vector3(startPosition.X, startPosition.Y, endPosition.Z);
					textureTopRight = new Vector3(endPosition.X, startPosition.Y, endPosition.Z);

					textureBottomLeft = new Vector3(startPosition.X, startPosition.Y, startPosition.Z);
					textureBottomRight = new Vector3(endPosition.X, startPosition.Y, startPosition.Z);

					normal = -Vector3.UnitY;
					faceColor = uvmap.ColorBottom;
					break;
				case BlockFace.West: //Negative X
					textureTopLeft = new Vector3(startPosition.X, endPosition.Y, startPosition.Z);
					textureTopRight = new Vector3(startPosition.X, endPosition.Y, endPosition.Z);

					textureBottomLeft = new Vector3(startPosition.X, startPosition.Y, startPosition.Z);
					textureBottomRight = new Vector3(startPosition.X, startPosition.Y, endPosition.Z);

					normal = -Vector3.UnitX;
					faceColor = uvmap.ColorLeft;
					break;
				case BlockFace.East: //Positive X
					textureTopLeft = new Vector3(endPosition.X, endPosition.Y, startPosition.Z);
					textureTopRight = new Vector3(endPosition.X, endPosition.Y, endPosition.Z);

					textureBottomLeft = new Vector3(endPosition.X, startPosition.Y, startPosition.Z);
					textureBottomRight = new Vector3(endPosition.X, startPosition.Y, endPosition.Z);

					normal = Vector3.UnitX;
					faceColor = uvmap.ColorRight; 
					break;
				case BlockFace.South: //Positive Z
					textureTopLeft = new Vector3(startPosition.X, endPosition.Y, startPosition.Z);
					textureTopRight = new Vector3(endPosition.X, endPosition.Y, startPosition.Z);

					textureBottomLeft = new Vector3(startPosition.X, startPosition.Y, startPosition.Z);
					textureBottomRight = new Vector3(endPosition.X, startPosition.Y, startPosition.Z);

					normal = Vector3.UnitZ;
					faceColor = uvmap.ColorFront;
					break;
				case BlockFace.North: //Negative Z
					textureTopLeft = new Vector3(startPosition.X, endPosition.Y, endPosition.Z);
					textureTopRight = new Vector3(endPosition.X, endPosition.Y, endPosition.Z);

					textureBottomLeft = new Vector3(startPosition.X, startPosition.Y, endPosition.Z);
					textureBottomRight = new Vector3(endPosition.X, startPosition.Y, endPosition.Z);

					normal = -Vector3.UnitZ;
					faceColor = uvmap.ColorBack;
					break;
				case BlockFace.None:
					break;
			}

			Vector2 uvTopLeft = uvmap.TopLeft;
			Vector2 uvTopRight = uvmap.TopRight;
			Vector2 uvBottomLeft = uvmap.BottomLeft;
			Vector2 uvBottomRight = uvmap.BottomRight;

			var faceRotation = rotation;

			if (faceRotation == 90)
			{
				uvTopLeft = uvmap.BottomLeft;
				uvTopRight = uvmap.TopLeft;

				uvBottomLeft = uvmap.BottomRight;
				uvBottomRight = uvmap.TopRight;
			}
			else if (faceRotation == 180)
			{
				uvTopLeft = uvmap.BottomRight;
				uvTopRight = uvmap.BottomLeft;

				uvBottomLeft = uvmap.TopRight;
				uvBottomRight = uvmap.TopLeft;
			}
			else if (faceRotation == 270)
			{
				uvTopLeft = uvmap.BottomLeft;
				uvTopRight = uvmap.TopLeft;

				uvBottomLeft = uvmap.BottomRight;
				uvBottomRight = uvmap.TopRight;
			}


			var topLeft = new VertexPositionNormalTextureColor(textureTopLeft, normal, uvTopLeft, faceColor);
			var topRight = new VertexPositionNormalTextureColor(textureTopRight, normal, uvTopRight, faceColor);
			var bottomLeft = new VertexPositionNormalTextureColor(textureBottomLeft, normal, uvBottomLeft,
				faceColor);
			var bottomRight = new VertexPositionNormalTextureColor(textureBottomRight, normal, uvBottomRight,
				faceColor);

			switch (blockFace)
			{
				case BlockFace.Up:
					return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});
				case BlockFace.Down:
					return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});
				case BlockFace.North:
					return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});
				case BlockFace.East:
					return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});
				case BlockFace.South:
					return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});
				case BlockFace.West:
					return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});
			}

			return new VertexPositionNormalTextureColor[0];
		}

		protected byte GetLight(IWorld world, Vector3 position)
	    {
			Vector3 lightOffset = Vector3.Zero;

		    bool initial = true;
		    byte blockLight = 0;
		    byte skyLight = 0;

		    byte highestBlocklight = 0;
		    byte highestSkylight = 0;
		    bool lightFound = false;
		    for(int i = 0; i < 6; i++)
		    {
			    switch (i)
			    {
					case 0:
						lightOffset = Vector3.Zero;
						break;
					case 1:
						initial = false;
						lightOffset = Vector3.UnitY;
						break;
					case 2:
						lightOffset = Vector3.UnitZ;
						break;
					case 3:
						lightOffset = -Vector3.UnitZ;
						break;
					case 4:
						lightOffset = -Vector3.UnitX;
						break;
					case 5:
						lightOffset = Vector3.UnitX;
						break;
					case 6:
						lightOffset = -Vector3.UnitY;
						break;
			    }

			    skyLight = world.GetSkyLight(position + lightOffset);
			    blockLight = world.GetBlockLight(position + lightOffset);
			    if (initial && (blockLight > 0 || skyLight > 0))
			    {
				    lightFound = true;

					break;
			    }
				else if (skyLight > 0 || blockLight > 0)
			    {
				    if (skyLight > 0)
				    {
					    lightFound = true;
						break;
				    }
				    else if (blockLight > highestBlocklight)
				    {
					    highestBlocklight = blockLight;
					    highestSkylight = skyLight;
				    }
			    }
			}

		    if (!lightFound)
		    {
			    skyLight = highestSkylight;
			    if (highestBlocklight > 0)
			    {
				    blockLight = (byte)(highestBlocklight - 1);
				}
			    else
			    {
				    blockLight = 0;
			    }
		    }

		    var result = (byte)Math.Min(blockLight + skyLight, 15);

		    return result;
	    }

	    protected bool CanRender(IWorld world, Block me, Vector3 pos)
	    {
		    if (pos.Y >= 256) return true;

		    var cX = (int)pos.X & 0xf;
		    var cZ = (int)pos.Z & 0xf;

		    if (cX < 0 || cX > 16)
			    return false;

		    if (cZ < 0 || cZ > 16)
			    return false;

		   // var blockStateId = world.GetBlockStateId(pos);
			//BlockFactory.
		    var block = world.GetBlock(pos);

		    if (me.Solid && block.Transparent) return true;
		    if (me.Transparent && block.Transparent && !block.Solid) return false;
			if (me.Transparent) return true;
			if (!me.Transparent && block.Transparent) return true;
		    if (block.Solid && !block.Transparent) return false;

		    return true;
	    }

	    protected UVMap GetTextureUVMap(ResourceManager resources, string texture, float x1, float x2, float y1, float y2)
	    {
			if (resources == null)
		    {
			    x1 = 0;
			    x2 = 1 / 32f;
			    y1 = 0;
			    y2 = 1 / 32f;

			    return new UVMap(new Vector2(x1, y1),
				   new Vector2(x2, y1), new Vector2(x1, y2),
				  new  Vector2(x2, y2), Color.White, Color.White, Color.White);
		    }

		    var textureInfo = resources.Atlas.GetAtlasLocation(texture.Replace("blocks/", ""));
		    var textureLocation = textureInfo.Position;

		    var uvSize = resources.Atlas.AtlasSize;

		    var pixelSizeX = (1f / uvSize.X); //0.0625
		    var pixelSizeY = (1f / uvSize.Y);

		    textureLocation.X /= uvSize.X;
		    textureLocation.Y /= uvSize.Y;

		    x1 = textureLocation.X + (x1 * pixelSizeX);
		    x2 = textureLocation.X + (x2 * pixelSizeX);
		    y1 = textureLocation.Y + (y1 * pixelSizeY);
		    y2 = textureLocation.Y + (y2 * pixelSizeY);


		    return new UVMap(new Vector2(x1, y1),
			    new Vector2(x2, y1), new Vector2(x1, y2),
			    new Vector2(x2, y2), Color.White, Color.White, Color.White);
		}
    }
}
