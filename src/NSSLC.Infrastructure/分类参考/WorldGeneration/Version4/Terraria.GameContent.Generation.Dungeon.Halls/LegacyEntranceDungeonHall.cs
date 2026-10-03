using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Rooms;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon.Halls;

public class LegacyEntranceDungeonHall : LegacyDungeonHall
{
	public int Direction;

	public LegacyEntranceDungeonHall(DungeonHallSettings settings)
		: base(settings)
	{
	}

	public override void CalculatePlatformsAndDoors(DungeonData data){}
	public override void LegacyHall(DungeonData dungeonData, int i, int j, bool generating = false){}
}
