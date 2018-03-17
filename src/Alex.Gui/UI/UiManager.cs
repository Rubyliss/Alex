using Alex.Engine.Graphics.Sprites;
using Alex.Engine.UI.Input;
using Alex.Engine.UI.Rendering;
using Alex.Engine.UI.Themes;
using Veldrid;

namespace Alex.Engine.UI
{
	public class UiManager
	{
		private Game Game { get; }

		public UiTheme Theme { get; set; } = new UiTheme();

		public UiScaledResolution ScaledResolution { get; private set; }

		public UiRenderer Renderer { get; private set; }

		public UiRoot Root { get; private set; }

		private bool _doResize = false;

		private IInputManager Input { get; }

		public UiManager(Game game)
		{
			Game = game;

			Input = new UiInputManager(this);

		}

		private void OnScaleChanged(object sender, UiScaleEventArgs e)
		{
			Renderer.SetVirtualSize(e.ScaledWidth, e.ScaledHeight, e.ScaleFactor);
			
			_doResize = true;
		}

		public Point PointToScreen(Point point)
		{
			return Renderer?.PointToScreen(point) ?? point;
		}

		public void Init(GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			Renderer = new UiRenderer(this, graphics, spriteBatch);

			ScaledResolution              =  new UiScaledResolution(Game);
			ScaledResolution.ScaleChanged += OnScaleChanged;

			Root = new UiRoot(Renderer);
			
			_doResize = true;

			Root.Activate(Input);
		}
		
		public void Update(GameTime gameTime)
		{
			ScaledResolution.Update();

			if (_doResize)
			{
				Root.UpdateLayoutInternal();

				_doResize = false;
			}

			Input.Update(gameTime);
			Root.Update(gameTime);
		}

		public void Draw(GameTime gameTime, CommandList commandList)
		{
			Renderer.BeginDraw(commandList);
			Root.Draw(gameTime, Renderer);
			Renderer.EndDraw();
		}
	}
}
