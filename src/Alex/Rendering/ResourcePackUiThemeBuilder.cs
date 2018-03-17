using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;
using Alex.Engine.Textures;
using Alex.Engine.UI;
using Alex.Engine.UI.Common;
using Alex.Engine.UI.Controls;
using Alex.Engine.UI.Controls.Menu;
using Alex.Engine.UI.Themes;
using Alex.Graphics;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Textures;
using Alex.Utils;

using Newtonsoft.Json;
using SharpDX.Direct3D11;
using Veldrid;

namespace Alex.Rendering
{
	public class ResourcePackUiThemeBuilder
	{
		private GraphicsDevice                       Graphics     { get; }
		private BedrockResourcePack                  ResourcePack { get; set; }
		private UiTheme                              Theme        { get; }
		private Dictionary<string, NinePatchTexture> Textures     { get; }

		public ResourcePackUiThemeBuilder(GraphicsDevice graphicsDevice)
		{
			Graphics = graphicsDevice;
			Theme    = new UiTheme();
			Textures = new Dictionary<string, NinePatchTexture>();
		}

		public void LoadResources(BedrockResourcePack resourcePack)
		{
			ResourcePack = resourcePack;
			Build();
		}

		public UiTheme GetTheme()
		{
			return Theme;
		}

		private void Build()
		{
			BuildBaseStyles();
			BuildPanelStyles();
			BuildButtonStyles();
		}

		private void BuildBaseStyles()
		{
			Theme.AddClass<UiRoot>(new UiElementStyle()
			{
				PositionAnchor             = Vector2.Zero,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				VerticalContentAlignment   = VerticalAlignment.Center,
				SizeAnchor                 = Vector2.One
			});
			Theme.AddClass("GuiRoot", new UiElementStyle()
			{
				SizeAnchor = Vector2.One
			});

			Theme.AddClass<UiElement>(new UiElementStyle()
			{
				TextFont  = Alex.Font,
				TextColor = Color.Black,
			});
		}

		private void BuildPanelStyles()
		{
			Theme.AddClass("TitleScreenRoot", new UiElementStyle()
			{
				StylePriority              = 100,
				Background                 = GetTexture("background"),
				BackgroundRepeat           = TextureRepeatMode.Tile,
				SizeAnchor                 = Vector2.One,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				VerticalContentAlignment   = VerticalAlignment.Center
			});

			Theme.AddClass("TitleScreenLogo", new UiElementStyle()
			{
				Background           = (NinePatchTexture) TextureUtils.ImageToTexture2D(Graphics, Resources.logo),
				PositionAnchor       = new Vector2(0.75f, 0.1f),
				PositionAnchorOrigin = new Vector2(0.5f,  0f),
				Width                = 150,
				Height               = 50
			});
			Theme.AddClass("TitleScreenMenuPanel", new UiElementStyle()
			{
				BackgroundColor = Color.FromArgb(25, Color.Black),// new Color(Color.Black, 0.2f),
				PositionAnchor  = new Vector2(0.15f, 0.0f),
				Width           = 120,
				SizeAnchor      = new Vector2(0f, 1f)
			});

			//Theme.AddClass<UiPanel>(i => string.IsNullOrWhiteSpace(i.ClassName), new UiElementStyle()
			//{
			//    BackgroundColor = new Color(Color.Black, 0.2f),
			//    PositionAnchor = new Vector2(0.15f, 0.0f)
			//});
		}


		private void BuildButtonStyles()
		{
			Theme.AddClass<UiMenuItem>(i => !i.IsMouseDown && !i.IsMouseOver, new UiElementStyle()
			{
				Background = GetTexture("beacon_button_default", true),
			});
			Theme.AddClass<UiMenuItem>(i => !i.IsMouseDown && i.IsMouseOver, new UiElementStyle()
			{
				Background = GetTexture("beacon_button_hover", true),
			});
			Theme.AddClass<UiMenuItem>(i => i.IsMouseDown, new UiElementStyle()
			{
				Background = GetTexture("beacon_button_pressed", true),
			});

			Theme.AddClass<UiButton>(i => !i.IsMouseDown && !i.IsMouseOver, new UiElementStyle()
			{
				Background = GetTexture("beacon_button_default", true),
			});
			Theme.AddClass<UiButton>(i => !i.IsMouseDown && i.IsMouseOver, new UiElementStyle()
			{
				Background = GetTexture("beacon_button_hover", true),
			});
			Theme.AddClass<UiButton>(i => i.IsMouseDown, new UiElementStyle()
			{
				Background = GetTexture("beacon_button_pressed", true),
			});

			Theme.AddClass<UiToggleButton>(new UiElementStyle()
			{
				MinWidth                   = 30,
				MinHeight                  = 16,
				HorizontalContentAlignment = HorizontalAlignment.Left
			});
			Theme.AddClass<UiToggleButton>(i => !i.IsMouseOver && !i.Active, new UiElementStyle()
			{
				Background = GetTexture("toggle_off", true),
			});
			Theme.AddClass<UiToggleButton>(i => i.IsMouseOver && !i.Active, new UiElementStyle()
			{
				Background = GetTexture("toggle_off_hover", true),
			});
			Theme.AddClass<UiToggleButton>(i => !i.IsMouseOver && i.Active, new UiElementStyle()
			{
				Background = GetTexture("toggle_on", true),
			});
			Theme.AddClass<UiToggleButton>(i => i.IsMouseOver && i.Active, new UiElementStyle()
			{
				Background = GetTexture("toggle_on_hover", true),
			});
			Theme.AddClass<UiButton>(new UiElementStyle()
			{
				MinWidth                   = 80,
				TextFont                   = Alex.Font,
				TextColor                  = Color.FromArgb(0x4C, 0x4C, 0x4C, 0xFF),
				TextSize                   = 0.5f,
				Padding                    = new Thickness(5, 10),
				Margin                     = new Thickness(5),
				PositionAnchor             = Vector2.Zero,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				VerticalContentAlignment   = VerticalAlignment.Center
			});

			Theme.AddClass<UiMenuItem>(new UiElementStyle()
			{
				MinWidth                   = 80,
				TextFont                   = Alex.Font,
				TextColor                  = Color.FromArgb(0x4C, 0x4C, 0x4C, 0xFF),
				TextSize                   = 0.5f,
				Padding                    = new Thickness(5, 10),
				Margin                     = new Thickness(5),
				PositionAnchor             = Vector2.Zero,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				VerticalContentAlignment   = VerticalAlignment.Center
			});
		}

		private NinePatchTexture GetTexture(string fileName, bool forceNineSlice = false)
		{
			NinePatchTexture texture;
			if (!Textures.TryGetValue(fileName, out texture))
			{
				var rawTexture = LoadTexture2D(fileName);
				var ninePatch  = GetTextureInfo(fileName, rawTexture, forceNineSlice);

				texture = new NinePatchTexture(rawTexture, ninePatch.Bounds, ninePatch.InnerBounds);
				Textures.Add(fileName, texture);
			}

			return texture;
		}

		private Texture LoadTexture2D(string fileName)
		{
			if (ResourcePack.TryGetTexture($"textures/ui/{fileName}", out var bitmap))
			{
				return TextureUtils.BitmapToTexture2D(Graphics, bitmap);
			}

			return null;
			/*var texture = new Texture(Graphics, 2, 2, false, SurfaceFormat.Color);
			texture.SetData(new[]
			{
				Color.Black,
				Color.Purple,
				Color.Purple,
				Color.Black
			});

			return texture;*/
		}

		private TextureInfoJson GetTextureInfo(string fileName, Texture texture, bool forceNineSlice = false)
		{
			if (ResourcePack.TryGetTextureJson($"textures/ui/{fileName}", out var info))
			{
				return info;
			}

			if (forceNineSlice)
			{
				var w = texture.Width;
				var h = texture.Height;

				var sliceSizeX = (int) Math.Floor(w / 3d);
				var sliceSizeY = (int) Math.Floor(h / 3d);

				return new TextureInfoJson()
				{
					BaseSize      = new int[] {(int) w, (int) h},
					NineSliceSize = new[] {sliceSizeX, sliceSizeY}
				};
			}

			return new TextureInfoJson();
		}
	}
}