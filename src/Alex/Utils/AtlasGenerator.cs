﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Alex.API.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using Color = System.Drawing.Color;

namespace Alex.Utils
{
    public class AtlasGenerator
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

	    private Dictionary<string, TextureInfo> _atlasLocations = new Dictionary<string, TextureInfo>();

	    private Texture2D _atlas;
	    public Vector2 AtlasSize { get; private set; }
		private Bitmap Atlas { get; set; } = null;
	    private GraphicsDevice Graphics { get; }
	    public AtlasGenerator(GraphicsDevice graphics)
	    {
		    Graphics = graphics;
	    }

	    public void GenerateAtlas(KeyValuePair<string, Bitmap>[] bitmaps, IProgressReceiver progressReceiver)
        {
	        Log.Info($"Generating texture atlas out of {bitmaps.Length} bitmaps...");

	        Bitmap no;
	        using (MemoryStream ms = new MemoryStream(Resources.no))
	        {
				no = new Bitmap(ms);
	        }

	        var regular = new[]
	        {
		        new KeyValuePair<string, Bitmap>("no_texture", no),
	        }.Concat(bitmaps.Where(x => x.Value.Height == 16 && x.Value.Width == 16)).ToArray();
		       
	        var others = bitmaps.Where(x => x.Value.Height != 16 && x.Value.Width != 16).ToArray();

			var bitmap = new Bitmap(AtlasWidth, AtlasHeight);

	        int total = regular.Length + others.Length;

			int xi = 0, yi = 0, offsetX = 0, yRemaining = 0;
	        int processedFiles = Process(ref bitmap, regular, ref xi, ref yi, ref offsetX, ref yRemaining, total, 0, progressReceiver);
	        yi += 16;
	        xi = 0;
			Process(ref bitmap, others, ref xi, ref yi, ref offsetX, ref yRemaining, total, processedFiles, progressReceiver);
		
			Atlas = bitmap;

			_atlas = TextureUtils.BitmapToTexture2D(Graphics, bitmap);
			AtlasSize = new Vector2(_atlas.Width, _atlas.Height);

           // bitmap.Save("assets\\terrain.png", ImageFormat.Png);
			Log.Info($"TextureAtlas generated! (Width:{_atlas.Width}px Height:{_atlas.Height}px)");
        }

	    private int Process(ref Bitmap bmp, KeyValuePair<string, Bitmap>[] data, ref int xi, ref int yi, ref int xOffset, ref int yRemaining, int total, int processed, IProgressReceiver progressReceiver)
	    {
		    int done = processed;
			var count = 0;

		    foreach (var bm in data.OrderByDescending(x => x.Value.Height))
		    {
			    string key = bm.Key;
			    count++;

			    double percentage = 100D * ((double)processed / (double)total);
			    progressReceiver.UpdateProgress((int)percentage, "Stitching textures...", key);

                var r = new System.Drawing.Rectangle(0, 0, bm.Value.Width, bm.Value.Height);
			    var destination = new System.Drawing.Rectangle(xi, yi, bm.Value.Width, bm.Value.Height);

			    TextureUtils.CopyRegionIntoImage(bm.Value, r, ref bmp, destination);

			    if (!_atlasLocations.ContainsKey(key))
			    {
				    _atlasLocations.Add(key, new TextureInfo(new Vector2(xi, yi), bm.Value.Width, bm.Value.Height));
			    }

			    if (bm.Value.Height > 16)
			    {
				    xOffset = xi;
				    yRemaining = bm.Value.Height;
			    }
			    xi += bm.Value.Width;

			    if (count == AtlasWidth / 16)
			    {
				    yi += 16;
				    xi = 0;
				    yRemaining -= 16;

				    count = 0;
			    }

			    done++;

			   
			}

		    return done;
	    }

	    private const int AtlasWidth = 512;
	    private const int AtlasHeight = 512;

	    public int TextureWidth { get; private set; } = 16;
	    public int TextureHeight { get; private set; }= 16;

		public void LoadResourcePackOnTop(KeyValuePair<string, Bitmap>[] bitmapsRaw, IProgressReceiver progressReceiver)
		{
			/*  string path = Path.Combine("assets", "minecraft", "textures", "blocks");

			  int textureWidth = 16, textureHeight = 16;
			  var bitmapsRaw = archive.Entries.Where(x =>
				  x.FullName.Replace('/', '\\').StartsWith(path, StringComparison.InvariantCultureIgnoreCase) &&
				  x.Name.EndsWith(".png")).ToArray();
			  Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
			  foreach (var entry in bitmapsRaw)
			  {
				  string name = entry.Name.Replace(".png", "");
				  if (!bitmaps.ContainsKey(name))
				  {
					  using (Stream stream = entry.Open())
					  {
						  Bitmap texture = new Bitmap(stream);
						  if (texture.Width > textureWidth && texture.Width % 16 == 0 && texture.Height > textureHeight && texture.Height % 16 == 0)
						  {
							  if (texture.Width == texture.Height)
							  {
								  textureWidth = texture.Width;
								  textureHeight = texture.Height;
							  }
						  }
						  bitmaps.Add(name, texture);
					  }
				  }
			  }*/

			int textureWidth = 16, textureHeight = 16;
			Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
			foreach (var bitmap in bitmapsRaw)
			{
				string name = Path.GetFullPath(bitmap.Key);

				if (!bitmaps.ContainsKey(name))
				{
					var texture = bitmap.Value;
					if (texture.Width > textureWidth && texture.Width % 16 == 0 && texture.Height > textureHeight &&
					    texture.Height % 16 == 0)
					{
						if (texture.Width == texture.Height)
						{
							textureWidth = texture.Width;
							textureHeight = texture.Height;
						}
					}

					bitmaps.Add(name, texture);
				}
			}


			Dictionary<string, TextureInfo> newLocations = new Dictionary<string, TextureInfo>();
			int cx = 0;
			int cy = 0;
			//Log.Debug($"Texture Height: {textureHeight} Width: {textureWidth}");
			Bitmap modifiedBitmap = new Bitmap(32 * textureWidth, 32 * textureHeight);

			foreach (var loc in _atlasLocations)
			{
				System.Drawing.Rectangle target = new System.Drawing.Rectangle(cx, cy, textureWidth, textureHeight);

				int w = textureWidth;
				int h = textureHeight;

				var atlasLocation = loc.Value;
				if (bitmaps.TryGetValue(loc.Key, out Bitmap texture))
				{
					TextureUtils.CopyRegionIntoImage(texture, new System.Drawing.Rectangle(0, 0, textureWidth, textureHeight), ref modifiedBitmap, target);
				}
				else
				{
					w = 16;
					h = 16;
					TextureUtils.CopyRegionIntoImage(Atlas, new System.Drawing.Rectangle((int)atlasLocation.Position.X, (int)atlasLocation.Position.Y, 16, 16), ref modifiedBitmap, target);
				}

				newLocations.Add(loc.Key, new TextureInfo(new Vector2(cx, cy), w, h));

				cx += textureWidth;
				if (cx == modifiedBitmap.Width)
				{
					cx = 0;
					cy += textureHeight;
				}
			}

			int noDataSection = textureWidth / 2;
			for (int xi = 0; xi < 2; xi++)
			{
				for (int yi = 0; yi < 2; yi++)
				{
					for (int x = xi * noDataSection; x < (xi * noDataSection) + noDataSection; x++)
					{
						for (int y = yi * noDataSection; y < (yi * noDataSection) + noDataSection; y++)
						{
							Color color;
							if ((xi == 0 && yi == 0) || (xi == 1 && yi == 1))
							{
								color = Color.Magenta;
							}
							else
							{
								color = Color.Black;
							}
							modifiedBitmap.SetPixel(x, y, color);
						}
					}
				}
			}

			Atlas = modifiedBitmap;
			_atlasLocations = newLocations;
			TextureWidth = textureWidth;
			TextureHeight = textureHeight;

			_atlas = TextureUtils.BitmapToTexture2D(Graphics, modifiedBitmap);
		    AtlasSize = new Vector2(_atlas.Width, _atlas.Height);

			//modifiedBitmap.Save("debug.png", ImageFormat.Png);
		}

	    public Texture2D GetAtlas()
	    { 
		    return _atlas;
	    }

	    public TextureInfo GetAtlasLocation(string file)
	    {
		    if (_atlasLocations.Count == 0) throw new Exception();

		    return _atlasLocations.ContainsKey(file) ? _atlasLocations[file] : new TextureInfo(Vector2.Zero, TextureWidth, TextureHeight);
	    }
	}
}
