using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon;

namespace Terraria.GameContent.Biomes;

public class DitherSnake : List<DungeonControlLine>
{
	private static readonly Vector2D[] CircleTestPoints = (from i in Enumerable.Range(0, 12)
		select Vector2D.UnitX.RotatedBy(Math.PI * 2.0 * (double)i / 12.0)).ToArray();

	private static readonly double ExtraBuffer = 1.0 / Math.Cos(Math.PI / 6.0);
	public DungeonControlLine GetClosestLineTo(Vector2D pos)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Biomes.DitherSnake.GetClosestLineTo"))
	{
		DungeonControlLine result = null;
		double num = double.MaxValue;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			DungeonControlLine current = enumerator.Current;
			double num2 = current.Center.Distance(pos);
			if (num2 < num)
			{
				result = current;
				num = num2;
			}
		}
		return result;
	}
	}
	public DungeonControlLine GetLineContaining(Vector2D pos, DungeonControlLine initialGuess = null, int depth = 0)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Biomes.DitherSnake.GetLineContaining"))
	{
		if (initialGuess == null)
		{
			initialGuess = GetClosestLineTo(pos);
		}
		if (depth == 3)
		{
			return null;
		}
		if (Vector2D.Dot(pos - initialGuess.Start, initialGuess.StartTangent) < 0.0 && initialGuess.Prev != null)
		{
			return GetLineContaining(pos, initialGuess.Prev, depth + 1);
		}
		if (Vector2D.Dot(pos - initialGuess.End, initialGuess.EndTangent) < 0.0 && initialGuess.Next != null)
		{
			return GetLineContaining(pos, initialGuess.Next, depth + 1);
		}
		return initialGuess;
	}
	}
	public double GetPositionAlongSnake(Vector2D pos)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Biomes.DitherSnake.GetPositionAlongSnake"))
	{
		DungeonControlLine lineContaining = GetLineContaining(pos);
		if (!lineContaining.CanPaint((int)pos.X, (int)pos.Y, out var _, out var normalizedLineProgress))
		{
			normalizedLineProgress = 0.5;
		}
		return (double)lineContaining.Index + normalizedLineProgress;
	}
	}
}
