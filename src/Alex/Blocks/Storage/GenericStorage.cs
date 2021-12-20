using System;
using Alex.Blocks.Storage.Palette;
using Alex.Networking.Java.Util;
using NLog;

namespace Alex.Blocks.Storage;

public abstract class GenericStorage<TValue> : IDisposable where TValue : class, IHasKey
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();
	private IStorage Storage { get; set; }

	private int _bits;
	private readonly int _size;
	protected IPallete<TValue> Pallette { get; set; }
        
	private object _lock = new object();
	protected int MaxBitsPerEntry = 8;
	protected int SmallestValue = 4;
	public GenericStorage(TValue defaultValue, int bitsPerBlock = 8, int size = 4096)
	{
		_bits = bitsPerBlock;
		_size = size;
            
		Storage = new FlexibleStorage(_bits, size);
		Pallette = new IntIdentityHashBiMap<TValue>((1 << _bits));
            
		Pallette.Add(defaultValue);
	}

	protected int X = 16, Y = 16, Z = 16;

	protected abstract DirectPallete<TValue> GetDirectPalette();
	
	public void Set(int x, int y, int z, TValue state)
	{
		var idx = GetIndex(x, y, z);
		Set(idx, state);
	}

	private void Set(int idx, TValue state)
	{
		//  lock (_lock)
		{
			uint i = IdFor(state);

			Storage[idx] = i;
		}
	}

	private uint IdFor(TValue state)
	{
		uint i = Pallette.GetId(state);

		if (i == uint.MaxValue)
		{
			i = Pallette.Add(state);

			if (i >= (1 << this._bits))
			{
				var newBits = _bits + 1;
				if (newBits < MaxBitsPerEntry)
				{
					var old = Storage;

					try
					{
						Storage = new FlexibleStorage(newBits, _size);
						_bits = newBits;
						
						for (int s = 0; s < _size; s++)
						{
							Storage[s] = old[s];
						}
					}
					finally
					{
						old?.Dispose();
					}
				}
				else
				{
					_bits = CalculateDirectPaletteSize();
					var oldPalette = Pallette;
					var oldStorage = Storage;

					try
					{
						Pallette = GetDirectPalette();
						Storage = new FlexibleStorage(_bits, _size);

						for (int s = 0; s < _size; s++)
						{
							var oldValue = oldStorage[s];
							var newValue = oldPalette.GetId(oldPalette.Get(oldValue));
							Storage[s] = newValue;
							//data.set(i, newValue);
						}
					}
					finally
					{
						oldPalette?.Dispose();
						oldStorage?.Dispose();
					}

					return Pallette.GetId(state);
				}
				//return Resize(_bits + 1, state);
			}
		}

		return i;
	}

	public TValue Get(int x, int y, int z)
	{
		return Get(GetIndex(x, y, z));
	}

	private TValue Get(int index)
	{
		// lock (_lock)
		{
			var result = Pallette.Get(Storage[index]);

			if (result == null)
				return GetDefault();

			return result;
		}
	}

	protected abstract TValue GetDefault();

	protected abstract int GetIndex(int x, int y, int z);

	protected abstract int CalculateDirectPaletteSize();
	
	public void Read(MinecraftStream ms)
	{
		int bitsPerEntry = (byte) ms.ReadUnsignedByte();

		int palleteLength = ms.ReadVarInt();; // = ms.ReadVarInt();

		if (bitsPerEntry <= SmallestValue)
			bitsPerEntry = SmallestValue;

		var oldStorage = Storage;
		var oldPalette = Pallette;
            
		try
		{
			if (bitsPerEntry > 0 && bitsPerEntry <= MaxBitsPerEntry)
			{
				_bits = bitsPerEntry;
				
				uint[] pallette = new uint[palleteLength];
				for (int i = 0; i < palleteLength; i++)
				{
					pallette[i] = (uint)ms.ReadVarInt();
				}

				var directPalette = GetDirectPalette();
				Pallette = new IntIdentityHashBiMap<TValue>(palleteLength + 1);
				Pallette.Add(GetDefault());

				for (uint id = 0; id < palleteLength; id++)
				{
					uint stateId = pallette[id];
					TValue state = directPalette.Get(stateId);// _globalLookup(stateId);
					Pallette.Put(state, id);
				}
			}
			else
			{
				_bits = bitsPerEntry = CalculateDirectPaletteSize();
				Pallette = GetDirectPalette();
			}

			Storage = ReadStorage(ms, bitsPerEntry);
		}
		finally
		{
			oldPalette?.Dispose();
			oldStorage?.Dispose();
		}
	}

	private IStorage ReadStorage(MinecraftStream ms, int bitsPerEntry)
	{
		int length = ms.ReadVarInt();

		long[] dataArray = new long[length];

		for (int i = 0; i < dataArray.Length; i++)
		{
			dataArray[i] = ms.ReadLong();
		}

		var storage = new FlexibleStorage(bitsPerEntry, _size);
		var valueMask = (uint)((1L << bitsPerEntry) - 1);

		int bitOffset = 0;

		for (int y = 0; y < Y; y++)
		{
			for (int z = 0; z < Z; z++)
			{
				for (int x = 0; x < X; x++)
				{
					if (64 - (bitOffset % 64) < bitsPerEntry)
					{
						bitOffset += 64 - (bitOffset % 64);
					}

					int startLongIndex = bitOffset / 64;
					int end_long_index = startLongIndex;
					int startOffset = bitOffset % 64;
					bitOffset += bitsPerEntry;

					if (startLongIndex >= dataArray.Length || end_long_index >= dataArray.Length)
						continue;

					uint rawId;

					if (startLongIndex == end_long_index)
					{
						rawId = (uint)(dataArray[startLongIndex] >> startOffset);
					}
					else
					{
						int endOffset = 64 - startOffset;

						rawId = (uint)(dataArray[startLongIndex] >> startOffset
						               | dataArray[end_long_index] << endOffset);
					}

					rawId &= valueMask;

					storage[GetIndex(x, y, z)] = rawId;
				}
			}
		}

		return storage;
	}

	public void Dispose()
	{
		Storage?.Dispose();
		Pallette?.Dispose();
	}
}