using System;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Biomes;

public class MarbleBiome : MicroBiome
{
	private delegate bool SlabState(int x, int y, int scale);

	private static class SlabStates
	{
		public static bool Empty(int x, int y, int scale)
{
		using (new global::Terraria.CallTracker("Terraria.GameContent.Biomes.MarbleBiome.SlabStates.Empty"))
		{
			return false;
		}
		}
}

	private const int SCALE = 3;
	public override bool Place(Point origin, StructureMap structures, GenerationProgress progress){
  return new bool ();
}
}
