using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using ReLogic.Utilities;
using Terraria.GameContent.Biomes.Desert;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Biomes;

public class DunesBiome : MicroBiome
{
	private class DunesDescription
	{

		private DunesDescription()
		{
		}
}

	[JsonProperty("SingleDunesWidth")]
	private WorldGenRange _singleDunesWidth = WorldGenRange.Empty;

	public int MaximumWidth => _singleDunesWidth.ScaledMaximum * 2;

	public override bool Place(Point origin, StructureMap structures, GenerationProgress progress){
  return new bool ();
}
}
