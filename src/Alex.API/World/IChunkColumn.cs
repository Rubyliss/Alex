﻿using System;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.World
{
	public interface IChunkColumn : IDisposable
	{
		int X { get; }
		int Z { get; }

		IBlockState GetBlockState(int x, int y, int z);
		void SetBlockState(int x, int y, int z, IBlockState state);
		IBlock GetBlock(int bx, int by, int bz);
		void SetBlock(int bx, int by, int bz, IBlock block);
		void SetHeight(int bx, int bz, short h);
		byte GetHeight(int bx, int bz);
		void SetBiome(int bx, int bz, int biome);
		int GetBiome(int bx, int bz);
		byte GetBlocklight(int bx, int by, int bz);
		void SetBlocklight(int bx, int by, int bz, byte data);
		byte GetSkylight(int bx, int by, int bz);
		void SetSkyLight(int bx, int by, int bz, byte data);
		void GenerateMeshes(IWorld world, out ChunkMesh mesh);

		VertexBuffer VertexBuffer { get; set; }
		VertexBuffer TransparentVertexBuffer { get; set; }

		object VertexLock { get; set; }
		object UpdateLock { get; set; }
		bool IsDirty { get; set; }
		bool SkyLightDirty { get; set; }
		ScheduleType Scheduled { get; set; }
		int GetHeighest();
		//void SetBlockState(int x, int y, int z, IBlockState blockState);
		bool IsTransparent(int x, int y, int z);
		bool IsSolid(int bx, int by, int bz);

		void GetBlockData(int bx, int by, int bz, out bool transparent, out bool solid);
    }

	[Flags]
	public enum ScheduleType
	{
		Unscheduled = 1,
		Full = 2,
		Border = 4,
		Scheduled = 8,
		Skylight = 16
	}
}