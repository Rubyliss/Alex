﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;

namespace Alex.Engine.Utils
{
	internal static class ResourceFactoryExtensions
	{
		public static DeviceBuffer CreateStaticBuffer<T>(
			this GraphicsDevice graphicsDevice,
			T data,
			BufferUsage usage,
			uint structureByteStride = 0u)
			where T : struct
		{
			return graphicsDevice.CreateStaticBuffer(
				new[] {data},
				usage,
				structureByteStride);
		}

		public static DeviceBuffer CreateStaticBuffer<T>(
			this GraphicsDevice graphicsDevice,
			T[] data,
			BufferUsage usage,
			uint structureByteStride = 0u)
			where T : struct
		{
			var bufferSize = (uint) (data.Length * Marshal.SizeOf<T>());

			var staging = graphicsDevice.ResourceFactory.CreateBuffer(
				new BufferDescription(bufferSize, BufferUsage.Staging, 0));

			var result = graphicsDevice.ResourceFactory.CreateBuffer(
				new BufferDescription(bufferSize, usage, structureByteStride));

			var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
			commandList.Begin();

			var map = graphicsDevice.Map<T>(staging, MapMode.Write, 0);

			for (var i = 0; i < data.Length; i++)
			{
				map[i] = data[i];
			}

			graphicsDevice.Unmap(staging, 0);

			commandList.CopyBuffer(staging, 0, result, 0, bufferSize);

			commandList.End();

			graphicsDevice.SubmitCommands(commandList);

			graphicsDevice.DisposeWhenIdle(commandList);
			graphicsDevice.DisposeWhenIdle(staging);

			return result;
		}

		public static DeviceBuffer CreateStaticStructuredBuffer<T>(
			this GraphicsDevice graphicsDevice,
			T[] data)
			where T : struct
		{
			return graphicsDevice.CreateStaticBuffer(
				data,
				BufferUsage.StructuredBufferReadOnly,
				(uint) Marshal.SizeOf<T>());
		}

		public unsafe static Texture CreateStaticTexture2D(
			this GraphicsDevice graphicsDevice,
			uint width, uint height,
			in TextureMipMapData mipMapData,
			PixelFormat pixelFormat)
		{
			return graphicsDevice.CreateStaticTexture2D(
				width, height,
				new[]
				{
					mipMapData
				},
				pixelFormat);
		}

		public unsafe static Texture CreateStaticTexture2D(
			this GraphicsDevice graphicsDevice,
			uint width, uint height,
			TextureMipMapData[] mipMapData,
			PixelFormat pixelFormat)
		{
			var mipMapLevels = (uint) mipMapData.Length;

			var staging = graphicsDevice.ResourceFactory.CreateTexture(
				TextureDescription.Texture2D(
					width,
					height,
					mipMapLevels,
					1,
					pixelFormat,
					TextureUsage.Staging));

			var result = graphicsDevice.ResourceFactory.CreateTexture(
				TextureDescription.Texture2D(
					width,
					height,
					mipMapLevels,
					1,
					pixelFormat,
					TextureUsage.Sampled));

			var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
			commandList.Begin();

			for (var level = 0u; level < mipMapLevels; level++)
			{
				var mipMap = mipMapData[level];

				fixed (void* pin = mipMap.Data)
				{
					graphicsDevice.UpdateTexture(
						staging,
						new IntPtr(pin),
						(uint) mipMap.Data.Length,
						0, 0, 0,
						mipMap.Width,
						mipMap.Height,
						1,
						level,
						0);

					commandList.CopyTexture(
						staging, 0, 0, 0, level, 0,
						result, 0, 0, 0, level, 0,
						mipMap.Width,
						mipMap.Height,
						1, 1);
				}
			}

			commandList.End();

			graphicsDevice.SubmitCommands(commandList);

			graphicsDevice.DisposeWhenIdle(commandList);
			graphicsDevice.DisposeWhenIdle(staging);

			return result;
		}
	}

	public readonly struct TextureMipMapData
	{
		public readonly byte[] Data;

		public readonly uint RowPitch;
		public readonly uint SlicePitch;

		public readonly uint Width;
		public readonly uint Height;

		internal TextureMipMapData(
			byte[] data,
			uint rowPitch,
			uint slicePitch,
			uint width,
			uint height)
		{
			Data = data;

			RowPitch = rowPitch;
			SlicePitch = slicePitch;

			Width = width;
			Height = height;
		}
	}
}
