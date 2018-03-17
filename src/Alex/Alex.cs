using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using Alex.Engine;
using Alex.Engine.Graphics.Sprites;
using Alex.Engine.UI;
using Alex.Gamestates;
using Alex.Graphics;
using Alex.Rendering;
using Newtonsoft.Json;
using SharpDX.Direct3D11;
using Veldrid;
using BlendStateDescription = Veldrid.BlendStateDescription;
using CommandList = Veldrid.CommandList;
using DepthStencilStateDescription = Veldrid.DepthStencilStateDescription;
using RasterizerStateDescription = Veldrid.RasterizerStateDescription;
using Rectangle = Veldrid.Rectangle;

namespace Alex
{
	public partial class Alex : Game
	{
		private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(Alex));
		
		public static string DotnetRuntime { get; } =
			$"{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";

		public static string Version = "1.0";
		public static string Username { get; set; }
		public static IPEndPoint ServerEndPoint { get; set; }
		public static bool IsMultiplayer { get; set; } = false;

		public static SpriteFont Font;

		private SpriteBatch _spriteBatch;

		public static Alex Instance { get; private set; }
		public GameStateManager GameStateManager { get; private set; }
		public ResourceManager Resources { get; private set; }

		public UiManager UiManager { get; private set; }

		public Alex() : base()
		{
			Instance = this;

			//var graphics = new GraphicsDeviceManager(this)
		//	{
		//		PreferMultiSampling = false,
		//		SynchronizeWithVerticalRetrace = false,
		//	/	GraphicsProfile = GraphicsProfile.Reach
		//	};
			Content.RootDirectory = "assets";

			IsFixedTimeStep = false;
         //   _graphics.ToggleFullScreen();
			
			Username = "";
			
			//this.Window.AllowUserResizing = true;
			this.Window.Resized += () =>
			{
				/*if (graphics.PreferredBackBufferWidth != Window.ClientBounds.Width ||
					graphics.PreferredBackBufferHeight != Window.ClientBounds.Height)
				{
					graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
					graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
					graphics.ApplyChanges();
				}*/

			};
			UiManager = new UiManager(this);
		}

		public static EventHandler<KeyEvent> OnCharacterInput;

		private void Window_TextInput(KeyEvent e)
		{
			OnCharacterInput?.Invoke(this, e);
		}
		
		public void SaveSettings()
		{
			if (GameSettings.IsDirty)
			{
				Log.Info($"Saving settings...");
				File.WriteAllText("settings.json", JsonConvert.SerializeObject(GameSettings, Formatting.Indented));
			}
		}

		internal Settings GameSettings { get; private set; }

		protected override void Initialize()
		{
			//Window.Title = "Alex - " + Version;

			if (File.Exists("settings.json"))
			{
				try
				{
					GameSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
					Username = GameSettings.Username;
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Failed to load settings!");
				}
			}
			else
			{
				GameSettings = new Settings(string.Empty);
				GameSettings.IsDirty = true;
			}

			// InitCamera();
			this.Window.KeyDown += Window_TextInput;

			base.Initialize();
		}

		private CommandList _commandList;
		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(this, GraphicsDevice, Content);
			_commandList = GraphicsDevice.ResourceFactory.CreateCommandList();
		//	_commandList.SetPipeline(GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription(BlendStateDescription.SingleAlphaBlend, DepthStencilStateDescription.Disabled, RasterizerStateDescription.CullNone, PrimitiveTopology.TriangleStrip, )));

			UiManager.Init(GraphicsDevice, _spriteBatch);
			GameStateManager = new GameStateManager(GraphicsDevice, _spriteBatch, UiManager);

			GameStateManager.AddState("splash", new SplashScreen(this));
			GameStateManager.SetActiveState("splash");

			Log.Info($"Initializing Alex...");
			ThreadPool.QueueUserWorkItem(o => { InitializeGame(); });
		}

		protected override void UnloadContent()
		{
			SaveSettings();
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			UiManager.Update(gameTime);
			GameStateManager.Update(gameTime);

		}

		protected override void Draw(GameTime gameTime)
		{
			_commandList.Begin();

			_commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
			_commandList.SetViewport(0, Viewport);
			_commandList.SetFullViewports();

			_commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);

			_spriteBatch.Begin(_commandList);
			GameStateManager.Draw(gameTime, _commandList);

			base.Draw(gameTime);
			UiManager.Draw(gameTime, _commandList);

			_spriteBatch.FillRectangle(new Rectangle(50, 50, 50, 50), Color.Tomato);
			_spriteBatch.End();

			_commandList.End();
			GraphicsDevice.SubmitCommands(_commandList);
		
			GraphicsDevice.WaitForIdle();
			GraphicsDevice.SwapBuffers();
		}

		private void InitializeGame()
		{
			Extensions.Init(GraphicsDevice);

			if (!Directory.Exists("assets"))
			{
				Directory.CreateDirectory("assets");
			}

			if (!File.Exists(Path.Combine("assets", "Minecraftia.xnb")))
			{
				File.WriteAllBytes(Path.Combine("assets", "Minecraftia.xnb"), global::Alex.Resources.Minecraftia1);
			}

			//Font = 
			//var shader = Content.Load<EffectContent>(Path.Combine("shaders", "hlsl", "renderchunk.vertex"));
			
			Log.Info($"Loading blockstate metadata...");
			BlockFactory.Init();

			Log.Info($"Loading resources...");
			Resources = new ResourceManager(GraphicsDevice);
			if (!Resources.CheckResources(GraphicsDevice, GameSettings))
			{
				Exit();
				return;
			}

// 			Mouse.SetPosition(Viewport.Width / 2, Viewport.Height / 2);

			UiManager.Theme = Resources.UiThemeFactory.GetTheme();

			//GamestateManager.AddState("login", new LoginState(this));
			//GamestateManager.SetActiveState("login");

			GameStateManager.AddState("title", new TitleState(this)); 
			GameStateManager.AddState("options", new OptionsState(this));

			GameStateManager.SetActiveState("title");

			GameStateManager.RemoveState("splash");

			Log.Info($"Game initialized!");
		}
	}
}