﻿// <copyright file="BlockData.cs" company="BitcoderCZ">
// Copyright (c) BitcoderCZ. All rights reserved.
// </copyright>

using FancadeLoaderLib.Partial;
using MathUtils.Vectors;
using System;
using System.Runtime.CompilerServices;

namespace FancadeLoaderLib;

public class BlockData
{
	public readonly Array3D<ushort> Array;

	private const int BlockSize = 8;

	public BlockData()
	{
		Array = new Array3D<ushort>(BlockSize, BlockSize, BlockSize);
	}

	public BlockData(Array3D<ushort> blocks)
	{
		Array = blocks;

		Size = new int3(Array.LengthX, Array.LengthY, Array.LengthZ);
		Trim();
	}

	public BlockData(BlockData data)
	{
		Array = data.Array.Clone();
		Size = data.Size;
	}

	public int3 Size { get; private set; }

	public int Length => Array.Length;

	public ushort this[int index]
	{
		get => Array[index];
		set => Array[index] = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool InBounds(int3 pos)
		=> Array.InBounds(pos.X, pos.Y, pos.Z);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool InBounds(int x, int y, int z)
		=> Array.InBounds(x, y, z);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int3 Index(int i)
		=> Array.Index(i);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Index(int3 pos)
		=> Array.Index(pos);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Index(int x, int y, int z)
		=> Array.Index(x, y, z);

	#region SetGroup
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetGroup(int3 pos, PrefabGroup group)
		=> SetGroup(pos.X, pos.Y, pos.Z, group);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetGroup(int x, int y, int z, PrefabGroup group)
	{
		ushort id = group.Id;
		byte3 size = group.Size;

		EnsureSize(x + (size.X - 1), y + (size.Y - 1), z + (size.Z - 1));

		for (byte zIndex = 0; zIndex < size.Z; zIndex++)
		{
			for (byte yIndex = 0; yIndex < size.Y; yIndex++)
			{
				for (byte xIndex = 0; xIndex < size.X; xIndex++)
				{
					byte3 pos = new byte3(xIndex, yIndex, zIndex);
					if (!group.ContainsKey(pos))
					{
						continue;
					}

					SetBlockInternal(x + xIndex, y + yIndex, z + zIndex, id);
					id++;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetGroup(int3 pos, PartialPrefabGroup group)
		=> SetGroup(pos.X, pos.Y, pos.Z, group);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetGroup(int x, int y, int z, PartialPrefabGroup group)
	{
		ushort id = group.Id;
		byte3 size = group.Size;

		EnsureSize(x + (size.X - 1), y + (size.Y - 1), z + (size.Z - 1));

		for (byte zIndex = 0; zIndex < size.Z; zIndex++)
		{
			for (byte yIndex = 0; yIndex < size.Y; yIndex++)
			{
				for (byte xIndex = 0; xIndex < size.X; xIndex++)
				{
					byte3 pos = new byte3(xIndex, yIndex, zIndex);
					if (!group.ContainsKey(pos))
					{
						continue;
					}

					SetBlockInternal(x + xIndex, y + yIndex, z + zIndex, id);
					id++;
				}
			}
		}
	}
	#endregion

	#region SetBlock
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetBlock(int3 pos, ushort id)
		=> SetBlock(pos.X, pos.Y, pos.Z, id);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetBlock(int index, ushort id)
	{
		int3 pos = Index(index);
		SetBlock(pos.X, pos.Y, pos.Z, id);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetBlock(int x, int y, int z, ushort id)
	{
		if (id != 0)
		{
			EnsureSize(x, y, z); // not placing "air"
		}

		SetBlockInternal(x, y, z, id);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetBlockUnchecked(int3 pos, ushort id)
		=> SetBlockUnchecked(pos.X, pos.Y, pos.Z, id);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetBlockUnchecked(int index, ushort id)
	{
		int3 pos = Index(index);
		SetBlockUnchecked(pos.X, pos.Y, pos.Z, id);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetBlockUnchecked(int x, int y, int z, ushort id)
		=> SetBlockInternal(x, y, z, id);
	#endregion

	#region GetBlock
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ushort GetBlock(int3 pos)
		=> Array[pos.X, pos.Y, pos.Z];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ushort GetBlock(int x, int y, int z)
		=> Array[x, y, z];
	#endregion

	public void Trim()
	{
		if (Size == int3.Zero)
		{
			return;
		}

		int maxX = int.MaxValue;
		int maxY = int.MaxValue;
		int maxZ = int.MaxValue;

		int3 scanPos = Size - int3.One;

		while (true)
		{
			if (maxX == int.MaxValue)
			{
				for (int y = 0; y <= scanPos.Y; y++)
				{
					for (int z = 0; z <= scanPos.Z; z++)
					{
						if (Array[Index(scanPos.X, y, z)] != 0)
						{
							maxX = scanPos.X;
							goto endX;
						}
					}
				}
			}

		endX:
			if (maxY == int.MaxValue)
			{
				for (int x = 0; x <= scanPos.X; x++)
				{
					for (int z = 0; z <= scanPos.Z; z++)
					{
						if (Array[Index(x, scanPos.Y, z)] != 0)
						{
							maxY = scanPos.Y;
							goto endY;
						}
					}
				}
			}

		endY:
			if (maxZ == int.MaxValue)
			{
				for (int x = 0; x <= scanPos.X; x++)
				{
					for (int y = 0; y <= scanPos.Y; y++)
					{
						if (Array[Index(x, y, scanPos.Z)] != 0)
						{
							maxZ = scanPos.Z;
							goto endZ;
						}
					}
				}
			}

		endZ:
			if (maxX != int.MaxValue && maxY != int.MaxValue && maxZ != int.MaxValue)
			{
				Resize(new int3(maxX + 1, maxY + 1, maxZ + 1), false);
				return;
			}
			else if (scanPos.X == 1 && scanPos.Y == 1 && scanPos.Z == 1)
			{
				// no blocks
				Resize(int3.Zero, false);
				return;
			}

			scanPos = new int3(Math.Max(1, scanPos.X - 1), Math.Max(1, scanPos.Y - 1), Math.Max(1, scanPos.Z - 1));
		}
	}

	public void EnsureSize(int3 pos)
		=> EnsureSize(pos.X, pos.Y, pos.Z);

	public void EnsureSize(int posX, int posY, int posZ)
	{
		int3 size = Size;
		if (posX >= size.X)
		{
			size.X = posX + 1;
		}

		if (posY >= size.Y)
		{
			size.Y = posY + 1;
		}

		if (posZ >= size.Z)
		{
			size.Z = posZ + 1;
		}

		if (size != Size)
		{
			// only resize if actually needed
			if (size.X > Array.LengthX || size.Y > Array.LengthY || size.Z > Array.LengthZ)
			{
				Resize(size);
			}
			else
			{
				Size = size;
			}
		}
	}

	public BlockData Clone()
		=> new BlockData(this);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetBlockInternal(int x, int y, int z, ushort id)
		=> Array[x, y, z] = id;

	#region Utils
	private void Resize(int3 size, bool useBlock = true)
	{
		if (useBlock)
		{
			Array.Resize(
				CeilToMultiple(size.X, BlockSize),
				CeilToMultiple(size.Y, BlockSize),
				CeilToMultiple(size.Z, BlockSize));
		}
		else
		{
			Array.Resize(size.X, size.Y, size.Z);
		}

		Size = size;
	}

	private int CeilToMultiple(int numb, int blockSize)
	{
		int mod = numb % blockSize;
		return Math.Max(mod == 0 ? numb : numb + (blockSize - mod), blockSize);
	}
	#endregion
}
