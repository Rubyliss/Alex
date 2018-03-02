using System;
using System.Collections.Generic;
using System.Text;
using Alex.CoreRT.Gui.Screens;

namespace Alex.CoreRT.Gamestates.Menu
{
    public class MainMenu : MenuScreen
    {
        private Alex _alex;

        public MainMenu(Alex alex) : base(alex.Services)
        {
            _alex = alex;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            AddMenuItem("Multiplayer", () => { });
            AddMenuItem("Debug World", () => { });
            AddMenuItem("Options", () => { });
            AddMenuItem("Exit", _alex.Exit);
        }
    }
    
}
