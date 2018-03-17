﻿using Alex.Engine.UI.Input.Listeners;

namespace Alex.Engine.UI.Controls
{
    public class UiToggleButton : UiControl
    {
        private bool _active;

        public bool Active
        {
            get => _active;
            set
            {
                if (value == _active) return;
                _active = value;
                OnPropertyChanged();
            }
        }

        public UiToggleButton()
        {
        }

        protected override void OnMouseUp(MouseEventArgs args)
        {
            base.OnMouseUp(args);
            Active = !Active;
        }
    }
}
