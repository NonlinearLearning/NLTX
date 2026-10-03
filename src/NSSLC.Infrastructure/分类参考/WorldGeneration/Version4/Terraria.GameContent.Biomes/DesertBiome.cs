using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria.GameContent.Biomes.Desert;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Biomes;

public class DesertBiome : MicroBiome
{
	[JsonProperty("ChanceOfEntrance")]
	public double ChanceOfEntrance = 0.3333;

	public override bool Place(Point origin, StructureMap structures, GenerationProgress progress){
  return new bool ();
}
}
