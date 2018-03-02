using System;
using System.Collections.Generic;
using System.Text;
using MonoGame.Extended.Gui;

namespace Alex.CoreRT.Gamestates
{
    public class MainMenuScreen : GuiScreen
    {
        private Alex _alex;

        public MainMenuScreen(Alex alex, GuiSkin skin)
            : base(skin)
        {
            _alex = alex;
            
        }
    }
}
