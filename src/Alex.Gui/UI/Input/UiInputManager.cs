using Alex.Engine.UI.Input.Listeners;

namespace Alex.Engine.UI.Input
{
    public class UiInputManager : IInputManager
    {

        public IMouseListener MouseListener { get; private set; }

        public UiInputManager(UiManager uiManager)
        {
            MouseListener = new MouseListener(uiManager);
        }
        

        public void Update(GameTime gameTime)
        {
            MouseListener.Update(gameTime);
        }

    }
}
