using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameContent.Generation.Dungeon.Features;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon.Entrances;

public class TowerDungeonEntrance : DungeonEntrance
{
	public TowerDungeonEntrance(DungeonEntranceSettings settings)
		: base(settings)
	{
	}

	public override void CalculateEntrance(DungeonData data, int x, int y){}
	public override bool GenerateEntrance(DungeonData data, int x, int y){
  return new bool ();
}
	public override bool CanGenerateFeatureAt(DungeonData data, IDungeonFeature feature, int x, int y){
  return new bool ();
}
}
