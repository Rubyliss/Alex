using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using Alex.Engine.Content;
using Alex.Engine.Graphics.Effects;
using Alex.Engine.Utils;
using Alex.Engine.Vertices;
using Veldrid;
using Rectangle = System.Drawing.Rectangle;

namespace Alex.Engine.Graphics.Sprites
{
	/// <summary>
	/// Helper class for drawing text strings and sprites in one or more optimized batches.
	/// </summary>
	public class SpriteBatch : DisposableBase
	{
		private GraphicsDevice GraphicsDevice;
		private readonly SpriteMaterial _material;
		private readonly ConstantBuffer<SpriteMaterial.MaterialConstantsVS> _materialConstantsVSBuffer;

		private readonly DeviceBuffer _vertexBuffer;
		private readonly SpriteVertex[] _vertices;
		private readonly DeviceBuffer _indexBuffer;

		private const int InitialBatchSize = 256;
		private SpriteBatchItem[] _batchItems;
		private int _currentBatchIndex;

		private ContentManager ContentManager;
		private bool _begun = false;

		private Game Game { get; }
		/// <summary>
		/// Constructs a <see cref="SpriteBatch"/>.
		/// </summary>
		/// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>, which will be used for sprite rendering.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="graphicsDevice"/> is null.</exception>
		public SpriteBatch(Game game, GraphicsDevice graphicsDevice, ContentManager contentManager)
		{
			if (graphicsDevice == null)
			{
				throw new ArgumentNullException("graphicsDevice");
			}

			this.Game = game;
			this.GraphicsDevice = graphicsDevice;
			this.ContentManager = contentManager;

			OutputDescription desc = graphicsDevice.SwapchainFramebuffer.OutputDescription;
			_material = AddDisposable(new SpriteMaterial(
				contentManager,
				contentManager.EffectLibrary.Sprite, in desc));

			_materialConstantsVSBuffer = AddDisposable(new ConstantBuffer<SpriteMaterial.MaterialConstantsVS>(contentManager.GraphicsDevice));

			_material.SetMaterialConstantsVS(_materialConstantsVSBuffer.Buffer);

			_vertexBuffer = AddDisposable(GraphicsDevice.ResourceFactory.CreateBuffer(
				new BufferDescription(SpriteVertex.VertexDescriptor.Stride * 4, BufferUsage.VertexBuffer | BufferUsage.Dynamic)));

			_vertices = new SpriteVertex[4]; // Order is TL, TR, BL, BR

			_indexBuffer = AddDisposable(GraphicsDevice.CreateStaticBuffer(
				new ushort[] { 0, 1, 2, 2, 1, 3 },
				BufferUsage.IndexBuffer));

			_batchItems = new SpriteBatchItem[InitialBatchSize];
		}

		private CommandList _commandList;

		private int CallingThread = -1;
		public void Begin(CommandList commandList, SpriteSortMode sortMode = SpriteSortMode.Deferred)
		{
			if (CallingThread > 0) return;
			if (_begun) throw new Exception("Cannot call Begin before end has been called!");
			_begun = true;
			CallingThread = Thread.CurrentThread.ManagedThreadId;

			this._commandList = commandList;

			_material.Effect.Begin(commandList);
			//_material.SetSampler(GraphicsDevice.PointSampl
			_materialConstantsVSBuffer.Value.Projection =
				Matrix4x4.CreateOrthographicOffCenter(0, Game.Viewport.Width, Game.Viewport.Height, 0, 0, 1);

			_materialConstantsVSBuffer.Update(commandList);
			_material.SetMaterialConstantsVS(_materialConstantsVSBuffer.Buffer);

			_currentBatchIndex = 0;
			
		}

		public void End()
		{
			if (Thread.CurrentThread.ManagedThreadId != CallingThread) return;

			for (var i = 0; i < _currentBatchIndex; i++)
			{
				ref var batchItem = ref _batchItems[i];

				_vertices[0] = batchItem.VertexTL;
				_vertices[1] = batchItem.VertexTR;
				_vertices[2] = batchItem.VertexBL;
				_vertices[3] = batchItem.VertexBR;
				
				_commandList.UpdateBuffer(_vertexBuffer, 0, _vertices);
				_commandList.SetVertexBuffer(0, _vertexBuffer);
				//_commandList.u

				_material.SetTexture(batchItem.Texture);

				_material.ApplyPipelineState();
				_material.ApplyProperties();

				_material.Effect.ApplyPipelineState(_commandList);
				_material.Effect.ApplyParameters(_commandList);

				_commandList.SetPipeline(_material.Effect._pipelineState);
				var indexCount = batchItem.ItemType == SpriteBatchItemType.Quad ? 6u : 3u;
				 
				_commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

				_commandList.DrawIndexed(indexCount);
			}

			_begun = false;
			//Push to GPU

			//	GraphicsDevice.
		}

		private ref SpriteBatchItem CreateBatchItem()
		{
			if (_currentBatchIndex >= _batchItems.Length)
			{
				System.Array.Resize(ref _batchItems, _batchItems.Length * 2);
			}

			return ref _batchItems[_currentBatchIndex++];
		}

		public void DrawImage(
			Texture image,
			Rectangle? sourceRect,
			RectangleF destinationRect,
			RgbaFloat color)
		{
			ref var batchItem = ref CreateBatchItem();

			batchItem.Texture = image;

			var sourceRectangle = sourceRect ?? new Rectangle(0, 0, (int)image.Width, (int)image.Height);

			var texCoordTL = new Vector2(
				sourceRectangle.Left,
				sourceRectangle.Top);

			var texCoordBR = new Vector2(
				sourceRectangle.Right,
				sourceRectangle.Bottom );

			batchItem.Set(
				destinationRect.X,
				destinationRect.Y,
				destinationRect.Width,
				destinationRect.Height,
				in color,
				in texCoordTL,
				in texCoordBR,
				0);
		}

		public void DrawImage(
			Texture image,
			Rectangle? sourceRect,
			Vector2 position,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			RgbaFloat color)
		{
			ref var batchItem = ref CreateBatchItem();

			batchItem.Texture = image;

			var sourceRectangle = sourceRect ?? new Rectangle(0, 0, (int)image.Width, (int)image.Height);

			var texCoordTL = new Vector2(
				sourceRectangle.Left / (float)image.Width,
				sourceRectangle.Top / (float)image.Height);

			var texCoordBR = new Vector2(
				sourceRectangle.Right / (float)image.Width,
				sourceRectangle.Bottom / (float)image.Height);

			var width = sourceRectangle.Width * scale.X;
			var height = sourceRectangle.Height * scale.Y;

			origin *= scale;

			batchItem.Set(
				position.X,
				position.Y,
				-origin.X,
				-origin.Y,
				width,
				height,
				(float)Math.Sin(rotation),
				(float)Math.Cos(rotation),
				in color,
				in texCoordTL,
				in texCoordBR,
				0);
		}

		/*public void DrawImage(
			Texture texture,
			in Triangle2D sourceTriangle,
			in Triangle2D destinationTriangle,
			in ColorRgbaF tintColor)
		{
			ref var batchItem = ref CreateBatchItem();

			batchItem.Texture = texture;

			var textureCoordinates = new Triangle2D
			{
				V0 = new Vector2(sourceTriangle.V0.X / texture.Width, sourceTriangle.V0.Y / texture.Height),
				V1 = new Vector2(sourceTriangle.V1.X / texture.Width, sourceTriangle.V1.Y / texture.Height),
				V2 = new Vector2(sourceTriangle.V2.X / texture.Width, sourceTriangle.V2.Y / texture.Height)
			};

			batchItem.Set(
				destinationTriangle,
				textureCoordinates,
				tintColor,
				0);
		}*/


		public void Draw(Texture texture, Vector2 position)
		{
			DrawImage(texture, new Rectangle(0, 0, (int) texture.Width, (int) texture.Height), new RectangleF(position.X, position.Y, texture.Width, texture.Height),  RgbaFloat.White);
		}

		public void Draw(Texture texture, Vector2 position, Color color)
		{
			DrawImage(texture, new Rectangle(0, 0, (int)texture.Width, (int)texture.Height), new RectangleF(position.X, position.Y, texture.Width, texture.Height), new RgbaFloat(1f / 255f * (color.R), 1f / 255f * (color.G), 1f / 255f * (color.B), 1f / 255f * (color.A)));
		}

		public void Draw(Texture texture, Rectangle rectangle, Color color)
		{
			DrawImage(texture, new Rectangle(0, 0, (int)texture.Width, (int)texture.Height), new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height), new RgbaFloat(1f / 255f * (color.R), 1f / 255f * (color.G), 1f / 255f * (color.B), 1f / 255f * (color.A)));
		}

		public void Draw(Texture texture, Veldrid.Rectangle rectangle, Color color)
		{
			Draw(texture, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height), color);
		}

		public void Draw(Texture texture, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale)
		{
			
		}

		public void Draw(Texture texture, Veldrid.Rectangle sourceRectangle, Veldrid.Rectangle destinationRectangle, Color color)
		{
			DrawImage(texture, new Rectangle(sourceRectangle.X, sourceRectangle.Y, sourceRectangle.Width, sourceRectangle.Height), new RectangleF(destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height), 
				new RgbaFloat(1f / 255f * (color.R), 1f / 255f * (color.G), 1f / 255f * (color.B), 1f / 255f * (color.A)));
		}

		public void DrawString(SpriteFont font, string text, Vector2 position, Color color)
		{
			
		}

		public void DrawString(SpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale)
		{
			
		}

		/// <summary>
		/// Draw a line between the two supplied points.
		/// </summary>
		/// <param name="start">Starting point.</param>
		/// <param name="end">End point.</param>
		/// <param name="color">The draw color.</param>
		public void DrawLine(Vector2 start, Vector2 end, Color color)
		{
			float length = (end - start).Length();
			float rotation = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
			Draw(ContentManager.SolidWhiteTexture, start, color, rotation, Vector2.Zero, new Vector2(length, 1));
		}

		/// <summary>
		/// Draw a rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle to draw.</param>
		/// <param name="color">The draw color.</param>
		public void DrawRectangle(Rectangle rectangle, Color color)
		{
			Draw(ContentManager.SolidWhiteTexture, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, 1), color);
			Draw(ContentManager.SolidWhiteTexture, new Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, 1), color);
			Draw(ContentManager.SolidWhiteTexture, new Rectangle(rectangle.Left, rectangle.Top, 1, rectangle.Height), color);
			Draw(ContentManager.SolidWhiteTexture, new Rectangle(rectangle.Right, rectangle.Top, 1, rectangle.Height + 1), color);
		}

		/// <summary>
		/// Fill a rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle to fill.</param>
		/// <param name="color">The fill color.</param>
		public void FillRectangle(Rectangle rectangle, Color color)
		{
			Draw(ContentManager.SolidWhiteTexture, rectangle, color);
		}

		public void FillRectangle(Veldrid.Rectangle rectangle, Color color)
		{
			FillRectangle(new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height), color);
		}

		private struct SpriteBatchItem
		{
			public Texture Texture;

			public SpriteBatchItemType ItemType;

			public SpriteVertex VertexTL;
			public SpriteVertex VertexTR;
			public SpriteVertex VertexBL;

			// Not used for a triangle item.
			public SpriteVertex VertexBR;

			public void Set(float x, float y, float dx, float dy, float w, float h, float sin, float cos, in RgbaFloat color, in Vector2 texCoordTL, in Vector2 texCoordBR, float depth)
			{
				ItemType = SpriteBatchItemType.Quad;

				VertexTL.Position.X = x + dx * cos - dy * sin;
				VertexTL.Position.Y = y + dx * sin + dy * cos;
				VertexTL.Position.Z = depth;
				VertexTL.Color = color;
				VertexTL.UV.X = texCoordTL.X;
				VertexTL.UV.Y = texCoordTL.Y;

				VertexTR.Position.X = x + (dx + w) * cos - dy * sin;
				VertexTR.Position.Y = y + (dx + w) * sin + dy * cos;
				VertexTR.Position.Z = depth;
				VertexTR.Color = color;
				VertexTR.UV.X = texCoordBR.X;
				VertexTR.UV.Y = texCoordTL.Y;

				VertexBL.Position.X = x + dx * cos - (dy + h) * sin;
				VertexBL.Position.Y = y + dx * sin + (dy + h) * cos;
				VertexBL.Position.Z = depth;
				VertexBL.Color = color;
				VertexBL.UV.X = texCoordTL.X;
				VertexBL.UV.Y = texCoordBR.Y;

				VertexBR.Position.X = x + (dx + w) * cos - (dy + h) * sin;
				VertexBR.Position.Y = y + (dx + w) * sin + (dy + h) * cos;
				VertexBR.Position.Z = depth;
				VertexBR.Color = color;
				VertexBR.UV.X = texCoordBR.X;
				VertexBR.UV.Y = texCoordBR.Y;
			}

			public void Set(float x, float y, float w, float h, in RgbaFloat color, in Vector2 texCoordTL, in Vector2 texCoordBR, float depth)
			{
				ItemType = SpriteBatchItemType.Quad;

				VertexTL.Position.X = x;
				VertexTL.Position.Y = y;
				VertexTL.Position.Z = depth;
				VertexTL.Color = color;
				VertexTL.UV.X = texCoordTL.X;
				VertexTL.UV.Y = texCoordTL.Y;

				VertexTR.Position.X = x + w;
				VertexTR.Position.Y = y;
				VertexTR.Position.Z = depth;
				VertexTR.Color = color;
				VertexTR.UV.X = texCoordBR.X;
				VertexTR.UV.Y = texCoordTL.Y;

				VertexBL.Position.X = x;
				VertexBL.Position.Y = y + h;
				VertexBL.Position.Z = depth;
				VertexBL.Color = color;
				VertexBL.UV.X = texCoordTL.X;
				VertexBL.UV.Y = texCoordBR.Y;

				VertexBR.Position.X = x + w;
				VertexBR.Position.Y = y + h;
				VertexBR.Position.Z = depth;
				VertexBR.Color = color;
				VertexBR.UV.X = texCoordBR.X;
				VertexBR.UV.Y = texCoordBR.Y;
			}

			/*public void Set(in Triangle2D triangle, in Triangle2D texCoords, in RgbaFloat color, float depth)
			{
				ItemType = SpriteBatchItemType.Triangle;

				VertexTL.Position.X = triangle.V0.X;
				VertexTL.Position.Y = triangle.V0.Y;
				VertexTL.Position.Z = depth;
				VertexTL.Color = color;
				VertexTL.UV.X = texCoords.V0.X;
				VertexTL.UV.Y = texCoords.V0.Y;

				VertexTR.Position.X = triangle.V1.X;
				VertexTR.Position.Y = triangle.V1.Y;
				VertexTR.Position.Z = depth;
				VertexTR.Color = color;
				VertexTR.UV.X = texCoords.V1.X;
				VertexTR.UV.Y = texCoords.V1.Y;

				VertexBL.Position.X = triangle.V2.X;
				VertexBL.Position.Y = triangle.V2.Y;
				VertexBL.Position.Z = depth;
				VertexBL.Color = color;
				VertexBL.UV.X = texCoords.V2.X;
				VertexBL.UV.Y = texCoords.V2.Y;
			}*/
		}

		private enum SpriteBatchItemType
		{
			Quad,
			Triangle
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SpriteVertex
	{
		public Vector3 Position;
		public Vector2 UV;
		public RgbaFloat Color;

		public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
			new VertexElementDescription("POSITION", VertexElementSemantic.Position, VertexElementFormat.Float3),
			new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
			new VertexElementDescription("COLOR", VertexElementSemantic.Color, VertexElementFormat.Float4));
	}

	public enum SpriteSortMode
	{
		/// <summary>
		/// All sprites are drawing when <see cref="SpriteBatch.End"/> invokes, in order of draw call sequence. Depth is ignored.
		/// </summary>
		Deferred,
		/// <summary>
		/// Each sprite is drawing at individual draw call, instead of <see cref="SpriteBatch.End"/>. Depth is ignored.
		/// </summary>
		Immediate,
		/// <summary>
		/// Same as <see cref="SpriteSortMode.Deferred"/>, except sprites are sorted by texture prior to drawing. Depth is ignored.
		/// </summary>
		Texture,
		/// <summary>
		/// Same as <see cref="SpriteSortMode.Deferred"/>, except sprites are sorted by depth in back-to-front order prior to drawing.
		/// </summary>
		BackToFront,
		/// <summary>
		/// Same as <see cref="SpriteSortMode.Deferred"/>, except sprites are sorted by depth in front-to-back order prior to drawing.
		/// </summary>
		FrontToBack
	}

	/// <summary>
	/// Defines sprite visual options for mirroring.
	/// </summary>
	[Flags]
	public enum SpriteEffects
	{
		/// <summary>
		/// No options specified.
		/// </summary>
		None = 0,
		/// <summary>
		/// Render the sprite reversed along the X axis.
		/// </summary>
		FlipHorizontally = 1,
		/// <summary>
		/// Render the sprite reversed along the Y axis.
		/// </summary>
		FlipVertically = 2
	}
}
