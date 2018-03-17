using System.Numerics;
using Alex.Engine.UI.Layout;
using Alex.Engine.UI.Themes;
using Veldrid;

namespace Alex.Engine.UI.Rendering
{
    public class UiElementRenderArgs
    {

        public UiElement Element { get; }

        public Rectangle LayoutBounds { get; }
        public Rectangle Bounds { get; }
        public Rectangle ContentBounds { get; }

        public Vector2 Position { get; }

        public UiElementStyle Style { get; }
        public UiElementLayoutParameters Layout { get; }

        public UiElementRenderArgs(UiElement element)
        {
            Element = element;

            Bounds = Element.LayoutParameters.Bounds;
            ContentBounds = Element.LayoutParameters.InnerBounds;
            LayoutBounds = Element.LayoutParameters.OuterBounds;

            Position = new Vector2(ContentBounds.X, ContentBounds.Y);

            Style = Element.Style;
            Layout = Element.LayoutParameters;
        }

    }
}
