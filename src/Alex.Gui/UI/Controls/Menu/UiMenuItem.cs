using System;
using Alex.Engine.UI.Abstractions;
using Alex.Engine.UI.Input.Listeners;

namespace Alex.Engine.UI.Controls.Menu
{
	public class UiMenuItem : UiControl, ITextElement
	{
		private string _text;

		public string Text
		{
			get { return _text; }
			set
			{
				if (_text == value) return;
				_text = value;
				OnPropertyChanged();
				MarkLayoutDirty();
			}
		}
		public Action Action { get; }

		public UiMenuItem(string text, Action action = null)
		{
			Text = text;
			Action = action;
		}
		
		protected override void OnMouseUp(MouseEventArgs args)
		{
			if (IsMouseOver)
			{
				Action?.Invoke();
			}
		}

	}
}
