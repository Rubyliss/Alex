using System.Drawing;
using System.Numerics;
using Alex.Engine;
using Alex.Engine.Graphics.Sprites;
using Alex.Graphics;
using Alex.Rendering.UI;
using Veldrid;
using Rectangle = Veldrid.Rectangle;

namespace Alex.Gamestates.Playing
{
	public class InGameMenuState : GameState
	{
		private PlayingState State { get; }
		public InGameMenuState(Alex alex, PlayingState playingState, InputSnapshot state) : base(alex)
		{
			State = playingState;
			PreviousKeyboardState = state;

			Button disconnectButton = new Button("Disconnect");
			disconnectButton.OnButtonClick += DisconnectButtonOnOnButtonClick;

			Button returnButton = new Button("Return to game");
			returnButton.OnButtonClick += ReturnButtonOnOnButtonClick;

			Controls.Add("disconnectBtn", disconnectButton);
			Controls.Add("returnBtn", returnButton);
			Controls.Add("info", new Info());

			Alex.Window.CursorVisible = true;
			//Alex.IsMouseVisible = true;
		}

		private void ReturnButtonOnOnButtonClick()
		{

			Alex.Window.CursorVisible = false;
			//Alex.IsMouseVisible = false;
			Alex.GameStateManager.SetActiveState(State);
		}

		private void DisconnectButtonOnOnButtonClick()
		{
			// State.Disconnect();
			Alex.GameStateManager.SetActiveState("title");
			Alex.GameStateManager.RemoveState("serverMenu");
			Alex.GameStateManager.RemoveState("play");
		}

		protected override void OnDraw3D(RenderArgs args)
		{
			State.Draw3D(args);
		}

		protected override void OnDraw2D(RenderArgs args)
		{
			Viewport viewPort = Viewport;
			SpriteBatch sb = args.SpriteBatch;

			sb.Begin(ref args.Commands);

			sb.FillRectangle(new Rectangle(0, 0, (int) viewPort.Width, (int)viewPort.Height), Color.FromArgb(128, Color.Black));// new Color(Color.Black, 0.5f));

			sb.End();
		}

		private InputSnapshot PreviousKeyboardState { get; set; }
		protected override void OnUpdate(GameTime gameTime)
		{
			Controls["returnBtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y - 30);
			Controls["disconnectBtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 20);

			if (Alex.Window.Focused)
			{
				//State.SendPositionUpdate(gameTime);

				InputSnapshot currentKeyboardState = Alex.Window.PumpEvents();// Keyboard.GetState();
				if (currentKeyboardState != PreviousKeyboardState)
				{
					if (currentKeyboardState.IsKeyDown(KeyBinds.Menu))
					{
						ReturnButtonOnOnButtonClick();
					}
				}
				PreviousKeyboardState = currentKeyboardState;
			}
		}
	}
}
