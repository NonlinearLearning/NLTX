using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon.Entrances;

public class LegacyDungeonEntrance : DungeonEntrance
{
	public LegacyDungeonEntrance(DungeonEntranceSettings settings)
		: base(settings)
	{
	}

	public override void CalculateEntrance(DungeonData data, int x, int y){}
	public override bool GenerateEntrance(DungeonData data, int x, int y){
  return new bool ();
}
}
