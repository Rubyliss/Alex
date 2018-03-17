using System;
using System.Collections.Generic;
using System.IO;
using Alex.Engine.Graphics.Effects;
using Alex.Engine.Graphics.Sprites;
using Alex.Engine.Utils;
using SixLabors.Fonts;
using Veldrid;
using Font = SixLabors.Fonts.Font;

namespace Alex.Engine.Content
{
	public sealed class ContentManager : DisposableBase
	{
		private readonly Dictionary<Type, ContentLoader> _contentLoaders;

		private readonly Dictionary<string, object> _cachedObjects;

		private readonly Dictionary<FontKey, Font> _cachedFonts;

		private readonly Dictionary<uint, DeviceBuffer> _cachedNullStructuredBuffers;

		public GraphicsDevice GraphicsDevice { get; }

		public EffectLibrary EffectLibrary { get; }

		public Sampler LinearClampSampler { get; }
		public Sampler PointClampSampler { get; }

		public Texture NullTexture { get; }
		public Texture SolidWhiteTexture { get; }

		internal SpriteFontCache SpriteFontCache { get; }
		private Game Game { get; }

		public string RootDirectory;

		public ContentManager(Game game, GraphicsDevice graphicsDevice,
			string basePath)
		{
			RootDirectory = basePath;
			Game = game;
			GraphicsDevice = graphicsDevice;
			SpriteFontCache = new SpriteFontCache(this);

			_contentLoaders = new Dictionary<Type, ContentLoader>
			{
				
			};

			_cachedObjects = new Dictionary<string, object>();

			EffectLibrary = AddDisposable(new EffectLibrary(graphicsDevice));

			_cachedFonts = new Dictionary<FontKey, Font>();

			var linearClampSamplerDescription = SamplerDescription.Linear;
			linearClampSamplerDescription.AddressModeU = SamplerAddressMode.Clamp;
			linearClampSamplerDescription.AddressModeV = SamplerAddressMode.Clamp;
			linearClampSamplerDescription.AddressModeW = SamplerAddressMode.Clamp;
			LinearClampSampler = AddDisposable(
				graphicsDevice.ResourceFactory.CreateSampler(ref linearClampSamplerDescription));

			var pointClampSamplerDescription = SamplerDescription.Point;
			pointClampSamplerDescription.AddressModeU = SamplerAddressMode.Clamp;
			pointClampSamplerDescription.AddressModeV = SamplerAddressMode.Clamp;
			pointClampSamplerDescription.AddressModeW = SamplerAddressMode.Clamp;
			PointClampSampler = AddDisposable(
				graphicsDevice.ResourceFactory.CreateSampler(ref pointClampSamplerDescription));

			NullTexture = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled)));
			SolidWhiteTexture = AddDisposable(graphicsDevice.CreateStaticTexture2D(
				1,
				1,
				new TextureMipMapData[] { new TextureMipMapData(
					new byte[] { 255, 255, 255, 255 },
					4, 4, 1, 1),},
				PixelFormat.R8_G8_B8_A8_UNorm));

			_cachedNullStructuredBuffers = new Dictionary<uint, DeviceBuffer>();
		}

		internal DeviceBuffer GetNullStructuredBuffer(uint size)
		{
			if (!_cachedNullStructuredBuffers.TryGetValue(size, out var result))
			{
				_cachedNullStructuredBuffers.Add(size, result = AddDisposable(GraphicsDevice.ResourceFactory.CreateBuffer(
					new BufferDescription(
						size,
						BufferUsage.StructuredBufferReadOnly,
						size))));
			}
			return result;
		}

		public void Unload()
		{
			foreach (var cachedObject in _cachedObjects.Values)
			{
				if (cachedObject is IDisposable d)
				{
					RemoveAndDispose(ref d);
				}
			}
			_cachedObjects.Clear();
		}

		public T Load<T>(
			string[] filePaths,
			LoadOptions options = null,
			bool fallbackToPlaceholder = true)
			where T : class
		{
			for (var i = 0; i < filePaths.Length; i++)
			{
				var actuallyFallbackToPlaceholder = fallbackToPlaceholder && i == filePaths.Length - 1;

				var result = Load<T>(filePaths[i], options, actuallyFallbackToPlaceholder);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		public T Load<T>(
			string filePath,
			LoadOptions options = null,
			bool fallbackToPlaceholder = true)
			where T : class
		{
			if (_cachedObjects.TryGetValue(filePath, out var asset))
			{
				return (T)asset;
			}

			var type = typeof(T);

			if (!_contentLoaders.TryGetValue(type, out var contentLoader))
			{
				throw new Exception($"Could not finder content loader for type '{type.FullName}'");
			}

			string entry = null;
			foreach (var testFilePath in contentLoader.GetPossibleFilePaths(filePath))
			{
				var testEntry = Path.Combine(RootDirectory, testFilePath);
				if (File.Exists(testEntry))
				{
					entry = testEntry;
					break;
				}
			}

			if (entry != null)
			{
				asset = contentLoader.Load(entry, this, Game, options);

				if (asset is IDisposable d)
				{
					AddDisposable(d);
				}

				_cachedObjects.Add(filePath, asset);
			}
			else if (fallbackToPlaceholder)
			{
				asset = contentLoader.PlaceholderValue;
			}

			return (T)asset;
		}

		public Font GetOrCreateFont(string fontName, float fontSize, FontStyle fontStyle)
		{
			var key = new FontKey
			{
				FontName = fontName,
				FontSize = fontSize,
				FontWeight = fontStyle
			};

			if (!_cachedFonts.TryGetValue(key, out var font))
			{
				if (!SystemFonts.TryFind(fontName, out var fontFamily))
				{
					fontName = "Arial";
				}


				_cachedFonts.Add(key, font = SystemFonts.CreateFont(
					fontName,
					fontSize,
					fontStyle));
			}

			return font;
		}

		private struct FontKey
		{
			public string FontName;
			public float FontSize;
			public FontStyle FontWeight;
		}

		public class LoadOptions
		{
			// TODO: Refactor this, it's not a good API.
			public bool CacheAsset { get; set; } = true;
		}

	}
}
