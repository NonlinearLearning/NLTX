using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Biomes;

public class CaveHouseBiome : MicroBiome
{
	private readonly HouseBuilderContext _builderContext = new HouseBuilderContext();

	[JsonProperty]
	public double IceChestChance { get; set; }

	[JsonProperty]
	public double JungleChestChance { get; set; }

	[JsonProperty]
	public double GoldChestChance { get; set; }

	[JsonProperty]
	public double GraniteChestChance { get; set; }

	[JsonProperty]
	public double MarbleChestChance { get; set; }

	[JsonProperty]
	public double MushroomChestChance { get; set; }

	[JsonProperty]
	public double DesertChestChance { get; set; }

	public override bool Place(Point origin, StructureMap structures, GenerationProgress progress){
  return new bool ();
}
}
