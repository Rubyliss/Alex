using System.Drawing;
using System.Numerics;
using Alex.Engine;
using Alex.Engine.Graphics.Sprites;
using Alex.Gamestates;
using Alex.Graphics;
using Alex.Utils;
using SharpDX.Direct3D11;
using Veldrid;
using Veldrid.Sdl2;
using Rectangle = Veldrid.Rectangle;

namespace Alex.Rendering.UI
{
    public class Button : UIComponent
    {
        public delegate void OnClickedDelegate();

        public event OnClickedDelegate OnButtonClick;

        private Rectangle ButtonRectangle { get; set; }
        private Texture ButtonTexture { get; set; }
        private Texture HoverTexture { get; set; }
        private bool Hovering { get; set; }
        public string Text { get; set; }
        public Button(string text)
        {
            Text = text;
            Hovering = false;
            Size = new Vector2(400, 40);

          //  PrevMouseState = Mouse.GetState();
        }

        public override void Render(RenderArgs args)
        {
	        if (HoverTexture == null || ButtonTexture == null)
	        {
		        HoverTexture = TextureUtils.ImageToTexture2D(args.GraphicsDevice, Resources.ButtonState2);
		        ButtonTexture = TextureUtils.ImageToTexture2D(args.GraphicsDevice, Resources.ButtonState1);
			}

            args.SpriteBatch.Begin(ref args.Commands, SpriteSortMode.Deferred);

			ButtonRectangle = new Rectangle((int) Location.X,(int) Location.Y, (int) Size.X, (int) Size.Y);
            args.SpriteBatch.Draw(Hovering ? HoverTexture : ButtonTexture, ButtonRectangle, Color.Cornsilk);

            var measureString = Alex.Font.MeasureString(Text);

            args.SpriteBatch.DrawString(Alex.Font, Text,
                new Vector2(Location.X + Size.X / 2 - (measureString.X / 2),
                    Location.Y + Size.Y / 2 - (measureString.Y / 2)), Color.White);

            args.SpriteBatch.End();
        }

        private MouseState PrevMouseState { get; set; }
        public override void Update(GameTime time)
        {
           /*var ms = Mouse.GetState();
            var mouseRec = new Rectangle(ms.X, ms.Y, 1, 1);

            if (mouseRec.Intersects(ButtonRectangle))
            {
                Hovering = true;
                if (ms != PrevMouseState)
                {
                    if (ms.LeftButton == ButtonState.Released && PrevMouseState.LeftButton == ButtonState.Pressed && OnButtonClick != null)
                    {
                        OnButtonClick.Invoke();
                    }
                }
            }
            else
            {
                Hovering = false;
            }
            PrevMouseState = ms;*/
        }
    }
}
