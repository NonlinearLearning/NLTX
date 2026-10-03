using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon;

namespace Terraria.GameContent.Biomes;

public class DungeonControlLine
{
	public int Index;

	public DungeonControlLine Next;

	public DungeonControlLine Prev;

	public Vector2D Start;

	public Vector2D End;

	public Vector2D StartTangent;

	public Vector2D EndTangent;

	public Vector2D StartNormal;

	public Vector2D EndNormal;

	public double CrossTangent;

	public double StartRadius;

	public double EndRadius;

	public static double NormalizedDistanceSafeFromDither;

	private const double StyleTransitionDitherWidth = 0.5;

	private const int BorderWidth = 4;

	public Vector2D NormalizedLineDirection;

	public double LineLength;

	public DungeonGenerationStyleData Style;

	public int ProgressionStage;

	public bool CurveLine;

	public Vector2D Center => (End + Start) / 2.0;

	[JsonConstructor]
	private DungeonControlLine()
	{
	}

	public DungeonControlLine(Vector2D start, Vector2D end, double startRadius, double endRadius, int progressionStage, DungeonGenerationStyleData style)
	{
		Start = start;
		End = end;
		StartRadius = startRadius;
		EndRadius = endRadius;
		ProgressionStage = progressionStage;
		Style = style;
		Vector2D v = End - Start;
		LineLength = v.Length();
		NormalizedLineDirection = v.SafeNormalize(Vector2D.UnitX);
	}
	public bool CanPaint(int x, int y, out double distance, out double normalizedLineProgress)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Biomes.DungeonControlLine.CanPaint"))
	{
		distance = 0.0;
		normalizedLineProgress = 0.0;
		Vector2D vector2D = new Vector2D(x, y);
		Vector2D value = vector2D - Start;
		double num = Vector2D.Dot(value, StartTangent);
		if (num < 0.0)
		{
			if (Prev != null)
			{
				return false;
			}
			normalizedLineProgress = 0.0;
			distance = value.Length();
			return true;
		}
		Vector2D value2 = vector2D - End;
		double num2 = Vector2D.Dot(value2, EndTangent);
		if (num2 < 0.0)
		{
			if (Next != null)
			{
				return false;
			}
			normalizedLineProgress = 1.0;
			distance = value2.Length();
			return true;
		}
		double num3 = Vector2D.Dot(value, StartNormal);
		double num4 = Vector2D.Dot(value2, EndNormal);
		double num5 = (num + num2) / 2.0;
		num *= num;
		num2 *= num2;
		double num6 = num / (num + num2);
		double value3 = num3 * (1.0 - num6) + num4 * num6 - num5 * CrossTangent * num6 * (1.0 - num6);
		distance = Math.Abs(value3);
		normalizedLineProgress = num6;
		return true;
	}
	}
}
