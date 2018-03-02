using System;
using System.Collections.Generic;
using System.Text;
using Alex.CoreRT.Gamestates.Menu;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;

namespace Alex.CoreRT.Gamestates
{
    public class CoolSceneManager
    {
        private GuiSystem _guiSystem;

        public ViewportAdapter ViewportAdapter { get; }
        

        public GuiSkin Skin { get; }
        private Alex _alex;

        public CoolSceneManager(Alex alex)
        {
            _alex = alex;
            Skin = new GuiSkin();

            ScreenGameComponent screenGameComponent;
            alex.Components.Add(screenGameComponent = new ScreenGameComponent(alex));

            screenGameComponent.Register(new MainMenu(alex));

            ViewportAdapter = new BoxingViewportAdapter(alex.Window, alex.GraphicsDevice, 800, 480);
            var guiRenderer = new GuiSpriteBatchRenderer(alex.GraphicsDevice, ViewportAdapter.GetScaleMatrix);


            _guiSystem = new GuiSystem(ViewportAdapter, guiRenderer)
            {
                Screens = { new MainMenuScreen(alex, Skin) }
            };
            
        }

        public void LoadScreen()
        {
            _guiSystem.Screens[0].Show();
        }

        public void Back()
        {
            if (_guiSystem.Screens[0].IsVisible)
                _alex.Exit();
        }

        public void Update(GameTime gameTime)
        {
            _guiSystem.Update(gameTime);
        }


        public void Draw(GameTime gameTime)
        {
            ActiveScreen.(gameTime);
            _guiSystem.Draw(gameTime);
        }
    }
}
