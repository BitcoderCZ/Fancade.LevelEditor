﻿// <copyright file="IScopedCodePlacer.cs" company="BitcoderCZ">
// Copyright (c) BitcoderCZ. All rights reserved.
// </copyright>

using FancadeLoaderLib.Editing.Utils;
using System;

namespace FancadeLoaderLib.Editing.Scripting.Placers;

public interface IScopedCodePlacer : ICodePlacer
{
	int CurrentCodeBlockBlocks { get; }

	void EnterStatementBlock();

	void ExitStatementBlock();

	void EnterExpressionBlock();

	void ExitExpressionBlock();

	void EnterHighlight();

	void ExitHightlight();
}

public static class IScopedCodePlacerUtils
{
	public static IDisposable StatementBlock(this IScopedCodePlacer placer)
	{
		placer.EnterStatementBlock();
		return new Disposable(placer.ExitStatementBlock);
	}

	public static IDisposable ExpressionBlock(this IScopedCodePlacer placer)
	{
		placer.EnterExpressionBlock();
		return new Disposable(placer.ExitExpressionBlock);
	}

	public static IDisposable HighlightBlock(this IScopedCodePlacer placer)
	{
		placer.EnterHighlight();
		return new Disposable(placer.ExitHightlight);
	}
}