﻿// <copyright file="BlockBuilder.cs" company="BitcoderCZ">
// Copyright (c) BitcoderCZ. All rights reserved.
// </copyright>

using FancadeLoaderLib.Editing.Scripting.Terminals;
using FancadeLoaderLib.Editing.Scripting.TerminalStores;
using FancadeLoaderLib.Editing.Utils;
using MathUtils.Vectors;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace FancadeLoaderLib.Editing.Scripting.Builders;

public abstract class BlockBuilder
{
	protected List<BlockSegment> segments = [];
	protected List<Block> highlightedBlocks = [];
	protected List<ConnectionRecord> connections = [];
	protected List<ValueRecord> values = [];

	public virtual void AddBlockSegments(IEnumerable<Block> blocks)
	{
		BlockSegment segment = new BlockSegment(blocks);

		segments.Add(segment);
	}

	public virtual void AddHighlightedBlock(Block block)
		=> highlightedBlocks.Add(block);

	public virtual void Connect(ITerminal from, ITerminal to)
		=> connections.Add(new ConnectionRecord(from, to));

	public void Connect(ITerminalStore from, ITerminalStore to)
	{
		if (from is NopTerminalStore || to is NopTerminalStore)
		{
			return;
		}

		if (to.In is null)
		{
			return;
		}

		foreach (var target in from.Out)
		{
			Connect(target, to.In);
		}
	}

	public virtual void SetBlockValue(Block block, int valueIndex, object value)
		=> values.Add(new ValueRecord(block, valueIndex, value));

	public abstract object Build(int3 posToBuildAt);

	public virtual void Clear()
	{
		segments.Clear();
		connections.Clear();
		values.Clear();
	}

	protected Block[] PreBuild(int3 posToBuildAt, bool sortByPos)
	{
		if (posToBuildAt.X < 0 || posToBuildAt.Y < 0 || posToBuildAt.Z < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(posToBuildAt), $"{nameof(posToBuildAt)} must be >= 0");
		}
		else if (segments.Count == 0)
		{
			return [];
		}

		int totalBlockCount = highlightedBlocks.Count;
		int3[] segmentSizes = new int3[segments.Count];

		for (int i = 0; i < segments.Count; i++)
		{
			totalBlockCount += segments[i].Blocks.Length;
			segmentSizes[i] = segments[i].Size + new int3(2, 1, 2); // margin
		}

		int3[] segmentPositions = BinPacker.Compute(segmentSizes);

		Block[] blocks = new Block[totalBlockCount];

		int3 highlightedPos = posToBuildAt;
		for (int i = 0; i < highlightedBlocks.Count; i++)
		{
			highlightedBlocks[i].Position = highlightedPos;
			highlightedPos.X += 3;
		}

		highlightedBlocks.CopyTo(blocks);

		int index = highlightedBlocks.Count;
		int3 off = highlightedBlocks.Count > 0 ? new int3(0, 0, 4) : int3.Zero;

		for (int i = 0; i < segments.Count; i++)
		{
			BlockSegment segment = segments[i];

			segment.Move((segmentPositions[i] + posToBuildAt + off) - segment.MinPos);

			segment.Blocks.CopyTo(blocks, index);
			index += segment.Blocks.Length;
		}

		if (sortByPos)
		{
			Array.Sort(blocks, (a, b) =>
			{
				int comp = a.Position.Z.CompareTo(b.Position.Z);
				return comp == 0 ? a.Position.X.CompareTo(b.Position.X) : comp;
			});
		}

		return blocks;
	}

	protected virtual int3 ChooseSubPos(int3 pos)
		=> new int3(7, 3, 3);

	protected readonly record struct ConnectionRecord(ITerminal From, ITerminal To);

	protected readonly record struct ValueRecord(Block Block, int ValueIndex, object Value);

	protected class BlockSegment
	{
		public readonly ImmutableArray<Block> Blocks;

		public BlockSegment(IEnumerable<Block> blocks)
		{
			if (blocks is null)
			{
				throw new ArgumentNullException(nameof(blocks));
			}

			Blocks = blocks.ToImmutableArray();
			if (Blocks.Length == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(blocks), $"{nameof(blocks)} cannot be empty.");
			}

			CalculateMinMax();
		}

		public int3 MinPos { get; private set; }

		public int3 MaxPos { get; private set; }

		public int3 Size => (MaxPos - MinPos) + int3.One;

		public void Move(int3 move)
		{
			if (move == int3.Zero)
			{
				return;
			}

			for (int i = 0; i < Blocks.Length; i++)
			{
				Blocks[i].Position += move;
			}

			MinPos += move;
			MaxPos += move;
		}

		private void CalculateMinMax()
		{
			int3 min = new int3(int.MaxValue, int.MaxValue, int.MaxValue);
			int3 max = new int3(int.MinValue, int.MinValue, int.MinValue);

			for (int i = 0; i < Blocks.Length; i++)
			{
				BlockDef type = Blocks[i].Type;

				min = int3.Min(Blocks[i].Position, min);
				max = int3.Max(Blocks[i].Position + type.Size, max);
			}

			MinPos = min;
			MaxPos = max - int3.One;
		}
	}
}