using Alex.Rendering.UI;

namespace Alex.Gamestates
{
	public class SplashScreen : GameState
	{
		public SplashScreen(Alex alex) : base(alex)
		{

		}

		protected override void OnLoad(RenderArgs args)
		{
			Controls.Add("logo", new Logo(Alex)
			{
				DrawMotd = false,
				Center = true
			});
		}
	}
}
