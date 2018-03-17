using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Alex.API.Graphics;
using Alex.Engine;
using Alex.Engine.Graphics.Sprites;
using Alex.Engine.UI;
using Alex.Graphics;
using Alex.Rendering.UI;


using Veldrid;

namespace Alex.Gamestates
{
	public class GameState
	{
		public Dictionary<string, UIComponent> Controls { get; set; }

		public UiContainer Gui { get; private set; }

		protected GraphicsDevice Graphics { get; }

		protected Alex Alex { get; }

		public GameState(Alex alex)
		{
			Alex = alex;
			Graphics = alex.GraphicsDevice;
			Controls = new Dictionary<string, UIComponent>();
		}

		public Viewport Viewport => new Viewport(Alex.Window.X, Alex.Window.Y, Alex.Window.Width, Alex.Window.Height, 0, 1);

		public Vector2 CenterScreen
		{
			get
			{
				return new Vector2((Viewport.Width / 2f),
					(Viewport.Height / 2f));
			}
		}

		public void Load(RenderArgs args)
		{
			Gui = new UiContainer
			{
				ClassName = "GuiRoot"
			};

			OnLoad(args);

			Gui.UpdateLayout();
		}

		public void Unload()
		{
			OnUnload();
		}

		public void Draw2D(RenderArgs args)
		{
			OnDraw2D(args);

			foreach (var control in Controls.Values.ToArray())
			{
				control.Render(args);
			}
		}

		public void Draw3D(RenderArgs args)
		{
			OnDraw3D(args);
		}

		public void Update(GameTime gameTime)
		{
			OnUpdate(gameTime);

			foreach (var control in Controls.Values.ToArray())
			{
				control.Update(gameTime);
			}
		}

		public void Show()
		{
			OnShow();
			Alex.UiManager.Root.AddChild(Gui);
			Gui.UpdateLayout();
		}

		public void Hide()
		{
			OnHide();
			Alex.UiManager.Root.RemoveChild(Gui);
			Gui.UpdateLayout();
		}
		
		protected virtual void OnShow() { }
		protected virtual void OnHide() { }

		protected virtual void OnLoad(RenderArgs args) { }
		protected virtual void OnUnload() { }

		protected virtual void OnUpdate(GameTime gameTime) { }

		protected virtual void OnDraw2D(RenderArgs args) { }
		protected virtual void OnDraw3D(RenderArgs args) { }
	}

	public class RenderArgs : IRenderArgs
	{
		public GameTime GameTime { get; set; }
		public GraphicsDevice GraphicsDevice { get; set; }
		public SpriteBatch SpriteBatch { get; set; }
		public CommandList Commands { get; set; }
	}

	public class UpdateArgs : IUpdateArgs
	{
		public GameTime GameTime { get; set; }
		public GraphicsDevice GraphicsDevice { get; set; }
	}
}
