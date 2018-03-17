using System.Drawing;
using System.Numerics;
using Alex.Gamestates;


namespace Alex.Rendering.UI
{
    public class Info : UIComponent
    {
        public override void Render(RenderArgs args)
        {
            args.SpriteBatch.Begin(args.Commands);

            const string text = "Alex - Developed by Kennyvv";
            var size = Alex.Font.MeasureString(text);
            args.SpriteBatch.DrawString(Alex.Font, text, new Vector2(4, (Alex.Instance.Viewport.Height - size.Y) - 2), Color.White);

            args.SpriteBatch.End();
        }
    }
}
