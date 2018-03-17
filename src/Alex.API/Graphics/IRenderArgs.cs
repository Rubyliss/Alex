using Alex.Engine;
using Alex.Engine.Graphics.Sprites;
using Veldrid;

namespace Alex.API.Graphics
{
	public interface IRenderArgs
	{
		GameTime GameTime { get; }
		GraphicsDevice GraphicsDevice { get; }
		SpriteBatch SpriteBatch { get; }
	}

	public interface IUpdateArgs
	{
		GameTime GameTime { get; }
		GraphicsDevice GraphicsDevice { get; }
	}
}
