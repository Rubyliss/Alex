﻿using System.Drawing;
using System.Numerics;
using Alex.Engine;
using Alex.Gamestates;
using Alex.Graphics;
using Alex.Utils;
using Veldrid;
using Veldrid.Sdl2;
using Rectangle = Veldrid.Rectangle;

namespace Alex.Rendering.UI
{
    public class TrackBar : UIComponent
    {
        private Rectangle ButtonRectangle { get; set; }
        private Rectangle TrackerRectangle { get; set; }
	    private Texture ButtonTexture { get; set; } = null;
	    private Texture TrackerTexture { get; set; } = null;

        public string Text { get; set; }
        private bool Focus { get; set; }
        public int Value { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }

        public TrackBar()
        {
            Text = "";

            Value = 50;
            MaxValue = 100;
            MinValue = 0;
            Size = new Vector2(400, 40);
            
            Focus = false;
        }

        public override void Render(RenderArgs args)
        {
	        if (ButtonTexture == null || TrackerTexture == null)
	        {
		        ButtonTexture = TextureUtils.ImageToTexture2D(args.GraphicsDevice, Resources.ButtonState0);
		        TrackerTexture = TextureUtils.ImageToTexture2D(args.GraphicsDevice, Resources.ButtonState1);
			}

            ButtonRectangle = new Rectangle((int)Location.X, (int)Location.Y, (int)Size.X, (int)Size.Y);
            var x = (int) ((int) Size.X - Size.X/(MaxValue)*Value);

            if (x + 13 > (Location.X + Size.X)) x = (int) (Location.X + Size.X) - 13;
            if (x < Location.X) x = (int) (Location.X + 3);

            TrackerRectangle = new Rectangle(x, (int)Location.Y, 10, (int)Size.Y);

            Color color = Color.Gray;

            var s = Text;

            var measureString = Alex.Font.MeasureString(s);
            while (measureString.X > ButtonRectangle.Width - 13)
            {
                s = s.Remove(0, 1);
                measureString = Alex.Font.MeasureString(s);
            }

            args.SpriteBatch.Begin(args.Commands);

            args.SpriteBatch.Draw(ButtonTexture, ButtonRectangle, Color.Cornsilk);
            args.SpriteBatch.DrawString(Alex.Font, s,
                new Vector2(Location.X + Size.X / 2 - measureString.X / 2, Location.Y + measureString.Y / 2 - 3), color);

            args.SpriteBatch.Draw(TrackerTexture, TrackerRectangle, Color.Cornsilk);

            args.SpriteBatch.End();
        }

        public override void Update(GameTime time)
        {
            /*var ms = Mouse.GetState();
            var mouseRec = new Rectangle(ms.X, ms.Y, 1, 1);

            if (ms.LeftButton == ButtonState.Pressed)
            {
                if (mouseRec.Intersects(ButtonRectangle))
                {
                    //var a = (ms.X - (ButtonRectangle.X)) / (ButtonRectangle.Width / MaxValue);
                    var a = (ButtonRectangle.X + ButtonRectangle.Width - ms.X) / (ButtonRectangle.Width / MaxValue);
                    if (a > MaxValue) a = MaxValue;
                    if (a < MinValue) a = MinValue;

                    Value = (int) a;
                }
            }*/
        }
    }
}
