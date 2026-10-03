using System;
using System.Collections.Generic;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Rooms;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon.Halls;

public class RegularDungeonHall : DungeonHall
{
	public RegularDungeonHall(DungeonHallSettings settings)
		: base(settings)
	{
	}

	public override void CalculatePlatformsAndDoors(DungeonData data){}
	public override void CalculateHall(DungeonData data, Vector2D startPoint, Vector2D endPoint){}
	public override void GenerateHall(DungeonData data){}
}
