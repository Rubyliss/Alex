using System;
using Alex.Engine.UI.Layout;

namespace Alex.Engine.UI.Controls.Menu
{
	public class UiMenu : UiStackPanel
	{

		public void AddMenuItem(string text, Action action = null)
		{
			AddChild(new UiMenuItem(text, action));
		}

	}
}
