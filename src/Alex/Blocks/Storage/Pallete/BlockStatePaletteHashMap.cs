using Alex.API.Blocks.State;
using Alex.API.World;
using Alex.Blocks.State;

namespace Alex.Blocks.Storage.Pallete
{
	public class BlockStatePaletteHashMap : IBlockStatePalette
	{
		private IntIdentityHashBiMap<IBlockState> _statePaletteMap;
		private IBlockStatePaletteResizer _paletteResizer;
		private int _bits;

		public BlockStatePaletteHashMap(int bitsIn, IBlockStatePaletteResizer paletteResizerIn)
		{
			this._bits = bitsIn;
			this._paletteResizer = paletteResizerIn;
			this._statePaletteMap = new IntIdentityHashBiMap<IBlockState>(1 << bitsIn);
		}

		public uint IdFor(IBlockState state)
		{
			uint i = this._statePaletteMap.GetId(state);

			if (i == uint.MaxValue)
			{
				i = this._statePaletteMap.Add(state);

				if (i >= 1 << this._bits)
				{
					i = this._paletteResizer.OnResize(this._bits + 1, state);
				}
			}

			return i;
		}

		public IBlockState GetBlockState(uint indexKey)
		{
			return _statePaletteMap.Get(indexKey);
		}

		public int GetSerializedSize()
		{
			int i = BlockState.GetVarIntSize(this._statePaletteMap.Size());

			for (uint j = 0; j < this._statePaletteMap.Size(); ++j)
			{
				i += BlockState.GetVarIntSize(
					BlockFactory.GetBlockStateId(_statePaletteMap.Get(j)));
			}

			return i;
		}
	}
}