﻿using System;

namespace Alex.Engine.UI.Input.Listeners
{
	public interface IMouseListener
	{
		event EventHandler<MouseEventArgs> MouseDown;
		event EventHandler<MouseEventArgs> MouseUp;
		event EventHandler<MouseEventArgs> MouseMove;
		event EventHandler<MouseEventArgs> MouseScroll;

		void Update(GameTime gameTime);
	}
}