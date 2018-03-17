using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using SharpDX.Direct3D11;
using SixLabors.ImageSharp;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.Utilities;
using Image = System.Drawing.Image;
using PixelFormat = Veldrid.PixelFormat;

namespace Alex.Utils
{
	public static class TextureUtils
	{
		internal static uint SizeInBytes<T>(this T[] array) where T : struct
		{
			return (uint)(array.Length * Unsafe.SizeOf<T>());
		}

		public static Texture BitmapToTexture2D(GraphicsDevice device, Bitmap bmp)
		{
			
			ImageSharpTexture t;
			using (MemoryStream ms = new MemoryStream())
			{
				bmp.Save(ms, ImageFormat.Png);
				ms.Position = 0;

				t = new ImageSharpTexture(SixLabors.ImageSharp.Image.Load(ms), false);
			}

			return t.CreateDeviceTexture(device, device.ResourceFactory);

			/*Rgba32[] imgData = new Rgba32[bmp.Width * bmp.Height];
			Texture texture = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint) bmp.Width, (uint) bmp.Height, 1, 1,
				PixelFormat.R32_G32_B32_A32_UInt,
				TextureUsage.RenderTarget | TextureUsage.Sampled |
				TextureUsage.Storage));
			unsafe
			{
				BitmapData origdata =
					bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

				uint* byteData = (uint*)origdata.Scan0;

				for (int i = 0; i < imgData.Length; i++)
				{
					var val = byteData[i];
					//imgData[i] = new RgbaByte((byte) ((val & 0x00FF0000) >> 16), (byte) (val & 0x0000FF00), (byte) ((val & 0x000000FF) << 16), (byte) (val & 0xFF000000));
					imgData[i] = new Rgba32((val & 0x000000FF) << 16 | (val & 0x0000FF00) | (val & 0x00FF0000) >> 16 | (val & 0xFF000000));
				}

				byteData = null;

				bmp.UnlockBits(origdata);


				fixed (Rgba32* pixelsPtr = imgData)
				{
					device.UpdateTexture(texture, (IntPtr)pixelsPtr, imgData.SizeInBytes(), 0, 0, 0, texture.Width, texture.Height, 0, 1, 1);
				}
			}

			return texture;*/
		}

	//	public static Texture ImageToTexture2D(GraphicsDevice device, Image bmp)
	//	{
	//		ImageSharpTexture tex = new ImageSharpTexture();
	//		return tex.CreateDeviceTexture(device, device.ResourceFactory);
			//var image = new Bitmap(new Bitmap(bmp));
		//	return BitmapToTexture2D(device, image);
	//	}

		public static Texture ImageToTexture2D(GraphicsDevice device, byte[] bmp)
		{
			ImageSharpTexture tex = new ImageSharpTexture(SixLabors.ImageSharp.Image.Load(bmp), false);
			return tex.CreateDeviceTexture(device, device.ResourceFactory);
			//using (MemoryStream s = new MemoryStream(bmp))
			//{
			//	var image = new Bitmap(new Bitmap(s));
			//	return BitmapToTexture2D(device, image);
			//}
		}
	}
}
