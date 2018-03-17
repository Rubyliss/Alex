﻿using System.Numerics;
using Alex.Engine;
using Alex.Gamestates;
using Alex.Graphics;


namespace Alex.Rendering.UI
{
	public class UIComponent
	{
		public Vector2 Location;
		public Vector2 Size;
		public UIComponent()
		{
			Location = Vector2.Zero;
			Size = Vector2.Zero;
		}

		public virtual void Render(RenderArgs args)
		{

		}

		public virtual void Update(GameTime time)
		{

		}

		public virtual void OnWindowResize()
		{

		}
	}
}
