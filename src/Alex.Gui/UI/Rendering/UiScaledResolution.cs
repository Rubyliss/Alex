﻿using System;
using Veldrid;

namespace Alex.Engine.UI.Rendering
{
	public class UiScaleEventArgs : EventArgs
	{
		public int ScaledWidth  { get; }
		public int ScaledHeight { get; }
		public int ScaleFactor  { get; }

		public UiScaleEventArgs(int scaledWidth, int scaledHeight, int scaleFactor)
		{
			ScaledWidth  = scaledWidth;
			ScaledHeight = scaledHeight;
			ScaleFactor  = scaleFactor;
		}
	}

	public class UiScaledResolution
	{
		public event EventHandler<UiScaleEventArgs> ScaleChanged;

		public double ScaledWidthD  { get; private set; }
		public double ScaledHeightD { get; private set; }

		public int ScaledWidth  { get; private set; }
		public int ScaledHeight { get; private set; }

		public int ScaleFactor { get; private set; }

		private int _targetWidth = 320;

		public int TargetWidth
		{
			get => _targetWidth;
			set
			{
				_targetWidth = value;
				Update();
			}
		}

		private int _targetHeight = 240;

		public int TargetHeight
		{
			get => _targetHeight;
			set
			{
				_targetHeight = value;
				Update();
			}
		}

		private int _guiScale = 1000;

		public int GuiScale
		{
			get => _guiScale;
			set
			{
				_guiScale = Math.Max(0, value);
				_guiScale = _guiScale == 0 ? 1000 : _guiScale;
				Update();
			}
		}

		private GraphicsDevice Graphics { get; }
		private Game GameInstance { get; }
	//	private Viewport       Viewport => Graphics.Viewport;

		public UiScaledResolution(Game game)
		{
			GameInstance = game;
			Graphics = game.GraphicsDevice;

		//	Graphics.DeviceReset          += (sender, args) => Update();
			game.Window.Resized += Update;
			game.Window.FocusGained += Update;
			game.Window.Shown += Update;
		//	game.Activated                += (sender, args) => Update();

			Update();
		}

		public void Update()
		{
			var viewportWidth  = GameInstance.Window.Width;
			var viewportHeight = GameInstance.Window.Height;

			var scaleFactor = 1;

			while (scaleFactor < GuiScale && viewportWidth / (scaleFactor + 1) >= TargetWidth &&
			       viewportHeight                          / (scaleFactor + 1) >= TargetHeight)
			{
				++scaleFactor;
			}


			ScaledWidthD  = (double) viewportWidth  / (double) scaleFactor;
			ScaledHeightD = (double) viewportHeight / (double) scaleFactor;
			var scaledWidth  = (int) Math.Ceiling(ScaledWidthD);
			var scaledHeight = (int) Math.Ceiling(ScaledHeightD);

			if (scaledWidth != ScaledWidth || scaledHeight != ScaledHeight || ScaleFactor != scaleFactor)
			{
				ScaleFactor  = scaleFactor;
				ScaledWidth  = scaledWidth;
				ScaledHeight = scaledHeight;

				ScaleChanged?.Invoke(this, new UiScaleEventArgs(ScaledWidth, ScaledHeight, ScaleFactor));
			}
		}
	}
}