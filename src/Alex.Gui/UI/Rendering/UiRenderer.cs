using System;
using System.Drawing;
using System.Numerics;
using Alex.Engine.Graphics.Sprites;
using Alex.Engine.Textures;
using Alex.Engine.UI.Abstractions;
using Alex.Engine.UI.Common;
using Alex.Engine.UI.Themes;
using Veldrid;
using Point = Veldrid.Point;
using Rectangle = Veldrid.Rectangle;

namespace Alex.Engine.UI.Rendering
{
	public class UiRenderer
	{
		public event EventHandler SizeChanged;

		public UiManager UiManager { get; }
		public UiTheme   Theme     => UiManager.Theme;

		private GraphicsDevice Graphics    { get; }
		private SpriteBatch SpriteBatch { get; }


		public int VirtualWidth  { get; private set; }
		public int VirtualHeight { get; private set; }
		public int ScaleFactor   { get; private set; }

		public Matrix4x4 ScaleMatrix         { get; private set; }
		public Matrix4x4 PointToScreenMatrix { get; private set; }

		public UiRenderer(UiManager uiManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			UiManager   = uiManager;
			SpriteBatch = spriteBatch;
			Graphics    = graphicsDevice;

			UpdateViewport();
		}

		public void SetVirtualSize(int width, int height, int scaleFactor)
		{
			var oldWidth = VirtualWidth;
			var oldHeight = VirtualHeight;
			var oldScaleFactor = ScaleFactor;

			VirtualWidth  = width;
			VirtualHeight = height;
			ScaleFactor   = scaleFactor;
			UpdateViewport();

			if (oldWidth != width || oldHeight != height || oldScaleFactor != scaleFactor)
			{
				SizeChanged?.Invoke(this, null);
			}
		}

		private void UpdateViewport()
		{
		/*	var viewport = Graphics.Viewport;
			var scaleX   = (float) viewport.Width  / VirtualWidth;
			var scaleY   = (float) viewport.Height / VirtualHeight;

			ScaleMatrix         = Matrix4x4.CreateScale(scaleX, scaleY, 1.0f);
			Matrix4x4.Invert(ScaleMatrix, out var p);
			PointToScreenMatrix = p;*/
		}

		public Point PointToScreen(Point point)
		{
			return Vector2.Transform(point.ToVector2(), PointToScreenMatrix).ToPoint();
		}

		public void BeginDraw(CommandList commandList)
		{
			SpriteBatch.Begin(ref commandList /*SpriteSortMode.Deferred, BlendStateDescription.SingleAlphaBlend, null, DepthStencilStateDescription.Disabled, RasterizerStateDescription.CullNone, null, ScaleMatrix*/);
		}

		public void EndDraw()
		{
			SpriteBatch.End();
		}


		public void DrawElement(UiElement element)
		{
			var args = new UiElementRenderArgs(element);

			DrawElementStyle(element, args);

			// Debug Bounding Boxes
			//DrawDebugBoundingBoxes(element, args);
		}

		private void DrawDebugBoundingBoxes(UiElement element, UiElementRenderArgs args)
		{
			DrawRectangle(args.LayoutBounds,  Color.Red);
			DrawRectangle(args.Bounds,        Color.Green);
			DrawRectangle(args.ContentBounds, Color.Blue);
		}

		private void DrawElementStyle(UiElement element, UiElementRenderArgs args)
		{
			var style = args.Style;

			if (style.BackgroundColor.HasValue)
			{
				FillRectangle(args.Bounds, style.BackgroundColor.Value);
			}

			if (style.Background != null)
			{
				DrawNinePatch(args.Bounds, style.Background, style.BackgroundRepeat ?? TextureRepeatMode.Stretch);
			}

			if (element is ITextElement textElement)
			{
				DrawTextElement(textElement, args);
			}
		}

		private void DrawTextElement(ITextElement element, UiElementRenderArgs args)
		{
			var style = args.Style;
			if (!(style.TextFont != null && style.TextColor.HasValue && !string.IsNullOrWhiteSpace(element.Text))) return;

			var font = style.TextFont;

			var size = args.Style.TextSize ?? 1.0f;

			var textScale = new Vector2(size, size);
			var textSize  = font.MeasureString(element.Text) * textScale;


			var pos = Vector2.Zero;
			if (style.HorizontalContentAlignment == HorizontalAlignment.Center)
			{
				pos.X = (args.ContentBounds.Width - textSize.X) / 2f;
			}
			else if (style.HorizontalContentAlignment == HorizontalAlignment.Right)
			{
				pos.X = (args.ContentBounds.Width - textSize.X);
			}

			if (style.VerticalContentAlignment == VerticalAlignment.Center)
			{
				pos.Y = (args.ContentBounds.Height - textSize.Y) / 2f;
			}
			else if (style.VerticalContentAlignment == VerticalAlignment.Bottom)
			{
				pos.Y = (args.ContentBounds.Height - textSize.Y);
			}

			pos += args.Position;

			if (style.TextShadowSize.HasValue && style.TextShadowSize.Value > 0 && style.TextShadowColor.HasValue)
			{
				var shadowColor = style.TextShadowColor.Value;

				for (int i = 0; i < style.TextShadowSize; i++)
				{
					SpriteBatch.DrawString(style.TextFont, element.Text, pos + new Vector2(i, i), shadowColor, 0, Vector2.Zero,
						textScale);
				}
			}

			SpriteBatch.DrawString(style.TextFont, element.Text, pos, style.TextColor.Value, 0, Vector2.Zero, textScale);
		}

		public void FillRectangle(Rectangle bounds, Color color)
		{
			//var texture = new Texture(Graphics, 1, 1, false, SurfaceFormat.Color);
			//texture.SetData(new Color[] {color});

			//FillRectangle(bounds, texture, TextureRepeatMode.Stretch);
		}

		public void DrawNinePatch(Rectangle bounds, NinePatchTexture ninePatchTexture, TextureRepeatMode repeatMode)
		{
			if (ninePatchTexture.Padding == Thickness.Zero)
			{
				FillRectangle(bounds, ninePatchTexture.Texture, repeatMode);
				return;
			}
			
			var sourceRegions = ninePatchTexture.SourceRegions;
			var destRegions   = ninePatchTexture.ProjectRegions(bounds);

			for (var i = 0; i < sourceRegions.Length; i++)
			{
				var srcPatch = sourceRegions[i];
				var dstPatch = destRegions[i];

				if(dstPatch.Width > 0 && dstPatch.Height > 0)
					SpriteBatch.Draw(ninePatchTexture.Texture,  srcPatch, dstPatch, color: Color.White);
			}
		}

		public void FillRectangle(Rectangle bounds, Texture texture, TextureRepeatMode repeatMode)
		{
			if (repeatMode == TextureRepeatMode.NoRepeat)
			{
				SpriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, (int) texture.Width, (int) texture.Height), new Rectangle(0, 0, (int) texture.Width, (int) texture.Height),
					Color.White);
			}
			else if (repeatMode == TextureRepeatMode.Stretch)
			{
				SpriteBatch.Draw(texture, bounds, Color.White);
			}
			else if (repeatMode == TextureRepeatMode.ScaleToFit)
			{
			}
			else if (repeatMode == TextureRepeatMode.Tile)
			{
				var repeatX = Math.Ceiling((float) bounds.Width  / texture.Width);
				var repeatY = Math.Ceiling((float) bounds.Height / texture.Height);

				for (int i = 0; i < repeatX; i++)
				{
					for (int j = 0; j < repeatY; j++)
					{
						var p = new Vector2(bounds.X, bounds.Y) + new Vector2(i * texture.Width, j * texture.Height);
					//	SpriteBatch.Draw(texture, p, new Rectangle(bounds.X, bounds.Y, (int)texture.Width, (int)texture.Height), Color.White);
					}
				}
			}
		}

		public void DrawRectangle(Rectangle bounds, Color color, int thickness = 1)
		{
			var texture = Graphics.ResourceFactory.CreateTexture(new TextureDescription(1, 1, 0, 1, 0, PixelFormat.R8_G8_B8_A8_SInt,
				TextureUsage.RenderTarget, TextureType.Texture2D));

			
			//Graphics.UpdateTexture(texture, );
		//	var texture = new Texture(Graphics, 1, 1, false, SurfaceFormat.Color);
			//texture.SetData(new Color[] {color});

			// Top
			SpriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);

			// Right
			SpriteBatch.Draw(texture, new Rectangle(bounds.X + bounds.Width - thickness, bounds.Y, thickness, bounds.Height),
				color);

			// Bottom
			SpriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y + bounds.Height - thickness, bounds.Width, thickness),
				color);

			// Left
			SpriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
		}

		public void DrawText(Rectangle bounds, string text, SpriteFont font, Color color)
		{
			if (font != null && !string.IsNullOrWhiteSpace(text))
			{
				SpriteBatch.DrawString(font, text, new Vector2(bounds.X, bounds.Y), color);
			}
		}
	}
}