using Alex.Engine.UI.Abstractions;

namespace Alex.Engine.UI.Controls
{
	public class UiLabel : UiElement, ITextElement
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

		public UiLabel(string text)
		{
			Text = text;
		}

	}
}
