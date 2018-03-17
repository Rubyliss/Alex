using System;
using System.Diagnostics;
using Alex.Engine.Content;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Alex.Engine
{
	public class Game : IDisposable
	{
		private const double DesiredFrameLengthSeconds = 1.0 / 60.0;

		public readonly Sdl2Window Window;
		public readonly GraphicsDevice GraphicsDevice;
		public readonly ContentManager Content;

		

		private Viewport _viewPort;
		public Viewport Viewport => _viewPort;

		public bool IsFixedTimeStep { get; set; } = true;

		private bool _shouldExit;
		private bool _suppressDraw;
		private bool _initialized = false;

		protected Game()
		{
			if (Window == null)
			{
				WindowCreateInfo windowCi = new WindowCreateInfo
				{
					X = 50,
					Y = 50,
					WindowWidth = 960,
					WindowHeight = 540,
					WindowInitialState = WindowState.Normal,
					WindowTitle = "Alex - Engine"
				};

				GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions(false, null, false)
				{
					HasMainSwapchain = true,
#if DEBUG
					Debug = true
#endif
				};

				Window = VeldridStartup.CreateWindow(ref windowCi);
				GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Window, gdOptions, GraphicsBackend.OpenGL);


				_viewPort = new Viewport(Window.X, Window.Y, Window.Width, Window.Height, 0, 1);
				Window.Resized += () =>
				{
					_viewPort.X = Window.X;
					_viewPort.Y = Window.Y;
					_viewPort.Width = Window.Width;
					_viewPort.Height = Window.Height;

					GraphicsDevice.ResizeMainWindow((uint)Window.Width, (uint)Window.Height);
				};

				Window.Moved += point =>
				{
					_viewPort.X = point.X;
					_viewPort.Y = point.Y;
					//GraphicsDevice.
				};

				Content = new ContentManager(this, GraphicsDevice, "");
			}
		}

		~Game()
		{
			Dispose(false);
		}

		protected virtual void Initialize()
		{
		}

		protected virtual void LoadContent()
		{
		}

		protected virtual void UnloadContent()
		{
		}

		protected virtual void Update(GameTime gameTime)
		{
		}

		protected virtual void Draw(GameTime gameTime)
		{
		}

		private void DoInitialize()
		{
			Initialize();
			LoadContent();

			_initialized = true;
		}

		public void Run()
		{
			if (!_initialized)
			{
				DoInitialize();
			}

			GameTime gameTime = new GameTime();
			long previousFrameTicks = 0;
			Stopwatch sw = new Stopwatch();
			sw.Start();
			while (Window.Exists)
			{
				long currentFrameTicks = sw.ElapsedTicks;
				double deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;

				while (IsFixedTimeStep && deltaSeconds < DesiredFrameLengthSeconds)
				{
					currentFrameTicks = sw.ElapsedTicks;
					deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
				}

				previousFrameTicks = currentFrameTicks;

				InputSnapshot snapshot = null;
				snapshot = Window.PumpEvents();

				gameTime.TotalGameTime += TimeSpan.FromTicks(currentFrameTicks);
				gameTime.ElapsedGameTime = TimeSpan.FromSeconds(deltaSeconds);

				Update(gameTime);
				if (!Window.Exists)
				{
					break;
				}

				Draw(gameTime);
			}
		}

		public void Exit()
		{
			_shouldExit = true;
			_suppressDraw = true;

			UnloadContent();
		}

		private bool _isDisposed;
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					
				}
				_isDisposed = true;
			}
		}
	}
}
