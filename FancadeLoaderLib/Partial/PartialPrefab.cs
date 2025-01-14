﻿// <copyright file="PartialPrefab.cs" company="BitcoderCZ">
// Copyright (c) BitcoderCZ. All rights reserved.
// </copyright>

using FancadeLoaderLib.Raw;
using MathUtils.Vectors;
using System;

namespace FancadeLoaderLib.Partial;

/// <summary>
/// Only the name, type and group info of <see cref="Prefab"/>, used by <see cref="PartialPrefabGroup"/>.
/// </summary>
public class PartialPrefab : ICloneable
{
	public PrefabType Type;

	public ushort GroupId;
	public byte3 PosInGroup;

	private string _name;

	public PartialPrefab(PartialPrefab other)
		: this(other._name, other.Type, other.GroupId, other.PosInGroup)
	{
	}

	public PartialPrefab(Prefab other)
		: this(other.Name, other.Type, other.GroupId, other.PosInGroup)
	{
	}

	public PartialPrefab(RawPrefab other)
		: this(other.Name, other.HasTypeByte ? (PrefabType)other.TypeByte : PrefabType.Normal, other.GroupId, other.PosInGroup)
	{
	}

	public PartialPrefab(string name, PrefabType type)
		: this(name, type, ushort.MaxValue, default)
	{
	}

	public PartialPrefab(string name, PrefabType type, ushort groupid, byte3 posInGroup)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException(nameof(name));
		}

		_name = name;
		Type = type;
		GroupId = groupid;
		PosInGroup = posInGroup;
	}

	public string Name
	{
		get => _name;
		set
		{
			if (value is null)
			{
				throw new ArgumentNullException(nameof(value), $"{nameof(Name)} cannot be null.");
			}

			_name = value;
		}
	}

	public bool IsInGroup => GroupId != ushort.MaxValue;

	public static PartialPrefab Load(FcBinaryReader reader)
	{
		byte header = reader.ReadUInt8();

		bool hasTypeByte = ((header >> 0) & 1) == 1;
		bool nonDefaultName = ((header >> 1) & 1) == 1;
		bool isInGroup = ((header >> 2) & 1) == 1;

		PrefabType type = PrefabType.Normal;
		if (hasTypeByte)
		{
			type = (PrefabType)reader.ReadUInt8();
		}

		string name = "New Block";
		if (nonDefaultName)
		{
			name = reader.ReadString();
		}

		ushort groupId = ushort.MaxValue;
		byte3 posInGroup = default;
		if (isInGroup)
		{
			groupId = reader.ReadUInt16();
			posInGroup = reader.ReadVec3B();
		}

		return new PartialPrefab(name, type, groupId, posInGroup);
	}

	public void Save(FcBinaryWriter writer)
	{
		byte header = 0;

		if (Type != PrefabType.Normal)
		{
			header |= 0b1;
		}

		if (Name != "New Block")
		{
			header |= 0b10;
		}

		if (IsInGroup)
		{
			header |= 0b100;
		}

		writer.WriteUInt8(header);

		if (Type != PrefabType.Normal)
		{
			writer.WriteUInt8((byte)Type);
		}

		if (Name != "New Block")
		{
			writer.WriteString(Name);
		}

		if (IsInGroup)
		{
			writer.WriteUInt16(GroupId);
			writer.WriteVec3B(PosInGroup);
		}
	}

	public PartialPrefab Clone()
		=> new PartialPrefab(this);

	object ICloneable.Clone()
		=> new PartialPrefab(this);
}
