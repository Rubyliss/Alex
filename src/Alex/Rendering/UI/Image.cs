using System.Drawing;
using System.Numerics;
using Alex.Engine.Graphics.Sprites;
using Alex.Gamestates;
using Alex.Graphics;
using SharpDX.Direct3D11;
using Veldrid;

namespace Alex.Rendering.UI
{
    public class Image : UIComponent
    {
        public Texture Texture { get; set; }
        public Image(Texture texture)
        {
            Texture = texture;
            Size = new Vector2(texture.Width, texture.Height);
        }

        public override void Render(RenderArgs args)
        {
            args.SpriteBatch.Begin(args.Commands, SpriteSortMode.Deferred);

            args.SpriteBatch.Draw(Texture, Location, Color.White);

            args.SpriteBatch.End();
        }
    }
}
