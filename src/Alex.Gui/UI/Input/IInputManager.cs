using Alex.Engine.UI.Input.Listeners;

namespace Alex.Engine.UI.Input
{
    public interface IInputManager
    {

        IMouseListener MouseListener { get; }

        void Update(GameTime gameTime);
    }
}
