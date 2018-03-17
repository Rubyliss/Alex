using System;
using Veldrid;
using Veldrid.Sdl2;

namespace Alex.Engine.UI.Input.Listeners
{
    public class MouseEventArgs : EventArgs
    {
        public MouseState PreviousState { get; }
        public MouseState CurrentState { get; }

        public MouseButton Button { get; }
        public Point Position { get; }

        public int ScrollWheelValue { get; }
        public int ScrollWheelDelta { get; }

        public MouseEventArgs(MouseState previousState, MouseState currentState, Point position, MouseButton buttons)
        {
            PreviousState = previousState;
            CurrentState = currentState;
            Button = buttons;

            Position = position;
          //  ScrollWheelValue = currentState.ScrollWheelValue;
         //   ScrollWheelDelta = previousState.ScrollWheelValue - currentState.ScrollWheelValue;
        }

    }
    
}
