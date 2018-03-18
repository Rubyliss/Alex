using System.Drawing;
using Alex.API.World;
using Veldrid;
using Rectangle = Veldrid.Rectangle;

namespace Alex.Gamestates
{
    public class LoadingWorldState : GameState
    {
		private Texture Background { get; }
	    public LoadingWorldState(Alex alex, Texture background) : base(alex)
	    {
		    Background = background;
	    }

	    private int Progress { get; set; } = 0;
	    private string DisplayString { get; set; } = "Loading world...";
	    public void UpdateProgress(LoadingState state, int percentage)
	    {
		    Progress = percentage;
		    switch (state)
		    {
				case LoadingState.ConnectingToServer:
					DisplayString = "Connecting to server...";
					break;
				case LoadingState.LoadingChunks:
					DisplayString = $"Loading chunks: {percentage}%";
					break;
				case LoadingState.GeneratingVertices:
					DisplayString = $"Initiating world: {percentage}%";
					break;
				case LoadingState.Spawning:
					DisplayString = $"Getting ready...";
					break;
		    }
	    }

	    protected override void OnDraw2D(RenderArgs args)
	    {
			args.SpriteBatch.Begin(ref args.Commands);
		   var bounds = Alex.Window.Bounds;
			//Start draw background
			var retval = new Rectangle(
			   bounds.X,
			  bounds.Y,
			  bounds.Width,
			  bounds.Height);
		    args.SpriteBatch.Draw(Background, retval, Color.White);
		    //End draw backgroun

		    // string displayString = Generating ? $"Initializing world: {Progress}%" : $"Loading world: {Progress}%";
		    var size = Alex.Font.MeasureString(DisplayString);
		    args.SpriteBatch.DrawString(Alex.Font, DisplayString, CenterScreen - (size / 2), Color.White);

		    args.SpriteBatch.End();
		}
    }
}
