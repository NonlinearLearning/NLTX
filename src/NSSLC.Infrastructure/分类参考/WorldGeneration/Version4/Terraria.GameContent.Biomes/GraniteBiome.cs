using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Biomes;

public class GraniteBiome : MicroBiome
{
	private struct Magma
	{
		public readonly double Pressure;

		public readonly double Resistance;

		public readonly bool IsActive;

		private Magma(double pressure, double resistance, bool active)
		{
			Pressure = pressure;
			Resistance = resistance;
			IsActive = active;
		}
}

	private const int MAX_MAGMA_ITERATIONS = 300;

	private Magma[,] _sourceMagmaMap = new Magma[200, 200];

	private Magma[,] _targetMagmaMap = new Magma[200, 200];

	private static Vector2D[] _normalisedVectors = new Vector2D[9]
	{
		Vector2D.Normalize(new Vector2D(-1.0, -1.0)),
		Vector2D.Normalize(new Vector2D(-1.0, 0.0)),
		Vector2D.Normalize(new Vector2D(-1.0, 1.0)),
		Vector2D.Normalize(new Vector2D(0.0, -1.0)),
		new Vector2D(0.0, 0.0),
		Vector2D.Normalize(new Vector2D(0.0, 1.0)),
		Vector2D.Normalize(new Vector2D(1.0, -1.0)),
		Vector2D.Normalize(new Vector2D(1.0, 0.0)),
		Vector2D.Normalize(new Vector2D(1.0, 1.0))
	};

	public static bool CanPlace(Point origin, StructureMap structures)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Biomes.GraniteBiome.CanPlace"))
	{
		if (WorldGen.BiomeTileCheck(origin.X, origin.Y))
		{
			return false;
		}
		return !GenBase._tiles[origin.X, origin.Y].active();
	}
	}
	public override bool Place(Point origin, StructureMap structures, GenerationProgress progress){
  return new bool ();
}
}
