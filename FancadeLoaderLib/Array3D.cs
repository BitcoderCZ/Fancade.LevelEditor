﻿// <copyright file="Array3D.cs" company="BitcoderCZ">
// Copyright (c) BitcoderCZ. All rights reserved.
// </copyright>

using MathUtils.Vectors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FancadeLoaderLib;

public class Array3D<T> : IEnumerable<T>
{
	private int _layerSize;

	public Array3D(int sizeX, int sizeY, int sizeZ)
	{
		LengthX = sizeX;
		LengthY = sizeY;
		LengthZ = sizeZ;
		_layerSize = sizeX * sizeY;

		Array = new T[sizeX * sizeY * sizeZ];
	}

	public Array3D(IEnumerable<T> collection, int sizeX, int sizeY, int sizeZ)
	{
		LengthX = sizeX;
		LengthY = sizeY;
		LengthZ = sizeZ;
		_layerSize = sizeX * sizeY;

		Array = collection.ToArray();
		if (Length != LengthX * LengthY * LengthZ)
		{
			throw new ArgumentException($"{nameof(collection)} length must be equal to: sizeX * sizeY * sizeZ ({LengthX * LengthY * LengthZ}), but it is: {Length}", "collection");
		}
	}

	public Array3D(T[] array, int sizeX, int sizeY, int sizeZ)
	{
		LengthX = sizeX;
		LengthY = sizeY;
		LengthZ = sizeZ;
		_layerSize = sizeX * sizeY;

		Array = array;
		if (Length != LengthX * LengthY * LengthZ)
		{
			throw new ArgumentException($"{nameof(array)}.Length must be equal to: sizeX * sizeY * sizeZ ({LengthX * LengthY * LengthZ}), but it is: {Length}", "collection");
		}
	}

	public Array3D(Array3D<T> array)
	{
		LengthX = array.LengthX;
		LengthY = array.LengthY;
		LengthZ = array.LengthZ;
		_layerSize = array._layerSize;

		Array = (T[])array.Array.Clone();
	}

	public T[] Array { get; private set; }

	public int LengthX { get; private set; }

	public int LengthY { get; private set; }

	public int LengthZ { get; private set; }

	public int Length => Array.Length;

	public int Count => Array.Length;

	public T this[int i]
	{
		get => Array[i];
		set => Array[i] = value;
	}

	public T this[int x, int y, int z]
	{
		get => Get(x, y, z);
		set => Set(x, y, z, value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool InBounds(int3 pos)
		=> InBounds(pos.X, pos.Y, pos.Z);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool InBounds(int x, int y, int z)
		=> x >= 0 && x < LengthX && y >= 0 && y < LengthY && z >= 0 && z < LengthZ;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Index(int3 pos)
		=> Index(pos.X, pos.Y, pos.Z);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Index(int x, int y, int z)
		=> x + (y * LengthX) + (z * _layerSize);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int3 Index(int index)
	{
		int remaining = index % _layerSize;
		return new int3(remaining % LengthX, remaining / LengthX, index / _layerSize);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Get(int x, int y, int z)
		=> Array[Index(x, y, z)];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Set(int x, int y, int z, T value)
		=> Array[Index(x, y, z)] = value;

	public void Resize(int newX, int newY, int newZ)
	{
		if (newX < 0 || newY < 0 || newZ < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		else if (newX == 0 || newY == 0 || newZ == 0)
		{
			Array = [];

			LengthX = 0;
			LengthY = 0;
			LengthZ = 0;
			_layerSize = 0;

			return;
		}
		else if (newX == LengthX && newY == LengthY && newZ == LengthZ)
		{
			return; // same length
		}

		T[] newArray = new T[newX * newY * newZ];

		int newSize2 = newX * newY;

		int minX = Math.Min(LengthX, newX);

		for (int z = 0; z < newZ; z++)
		{
			for (int y = 0; y < newY; y++)
			{
				if (InBounds(0, y, z) && InBoundsNew(y, z))
				{
					System.Array.Copy(Array, Index(0, y, z), newArray, IndexNew(0, y, z), minX);
				}
			}
		}

		Array = newArray;

		LengthX = newX;
		LengthY = newY;
		LengthZ = newZ;
		_layerSize = LengthX * LengthY;

		int IndexNew(int x, int y, int z)
		{
			return x + (y * newX) + (z * newSize2);
		}

		bool InBoundsNew(int y, int z)
		{
			return y >= 0 && y < newY && z >= 0 && z < newZ;
		}
	}

	public Array3D<T> Clone()
		=> new Array3D<T>(this);

	public void CopyTo(Array array, int index)
		=> array.CopyTo(array, index);

	public IEnumerator GetEnumerator()
		=> Array.GetEnumerator();

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
		=> ((IEnumerable<T>)Array).GetEnumerator();
}
