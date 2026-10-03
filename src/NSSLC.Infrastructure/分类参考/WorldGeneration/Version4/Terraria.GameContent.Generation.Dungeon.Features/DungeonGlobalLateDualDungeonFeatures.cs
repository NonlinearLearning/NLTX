using System.Collections.Generic;
using Terraria.GameContent.Generation.Dungeon.Rooms;

namespace Terraria.GameContent.Generation.Dungeon.Features;

public class DungeonGlobalLateDualDungeonFeatures : GlobalDungeonFeature
{
	public DungeonGlobalLateDualDungeonFeatures(DungeonFeatureSettings settings)
		: base(settings)
	{
		DungeonCrawler.CurrentDungeonData.dungeonFeatures.Add(this);
	}

	public override bool GenerateFeature(DungeonData data){
  return new bool ();
}
}
