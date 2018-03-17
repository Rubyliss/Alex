using System.Collections.Generic;
using Alex.Engine.Graphics.Sprites;
using Veldrid;

namespace Alex.Engine.Graphics.Effects
{
	public sealed class EffectLibrary : DisposableBase
	{
		private readonly GraphicsDevice _graphicsDevice;
		private readonly Dictionary<string, Effect> _effects;

		public Effect Sprite { get; }
		public EffectLibrary(GraphicsDevice graphicsDevice)
		{
			_graphicsDevice = graphicsDevice;
			_effects = new Dictionary<string, Effect>();

			Sprite = AddDisposable(new Effect(
				graphicsDevice,
				"sprite",
				SpriteVertex.VertexDescriptor,
				true));
		}

		public Effect GetEffect(
			string name,
			VertexLayoutDescription[] vertexDescriptors)
		{
			if (!_effects.TryGetValue(name, out var effect))
			{
				_effects[name] = effect = AddDisposable(new Effect(
					_graphicsDevice,
					name,
					vertexDescriptors));
			}
			return effect;
		}
	}
}
