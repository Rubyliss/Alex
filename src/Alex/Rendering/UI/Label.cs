﻿using System.Drawing;
using Alex.Engine.Graphics.Sprites;
using Alex.Gamestates;
using Alex.Graphics;

namespace Alex.Rendering.UI
{
    public class Label : UIComponent
    {
        public SpriteFont Font { get; set; }
        public Color Color { get; set; }
        public string Text { get; set; }
        public Label(string text)
        {
            Text = text;
            Color = Color.White;
            Font = Alex.Font;
        }

        public override void Render(RenderArgs args)
        {
            args.SpriteBatch.Begin(args.Commands, SpriteSortMode.Deferred);
            try
            {
                foreach (var line in Text.Split('\n'))
                {
                    args.SpriteBatch.DrawString(Font, line, Location, Color);
                }

                //args.SpriteBatch.End();
            }
            catch
            {
            }
            finally
            {
                args.SpriteBatch.End();
            }
        }
    }
}
