using System;
using System.Collections.Generic;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Rooms;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon.Halls;

public class StairwellDungeonHall : DungeonHall
{
	public StairwellDungeonHall(StairwellDungeonHallSettings settings)
		: base(settings)
	{
	}

	public override void CalculatePlatformsAndDoors(DungeonData data){}
	public override void CalculateHall(DungeonData data, Vector2D startPoint, Vector2D endPoint){}
	public override void GenerateHall(DungeonData data){}
	public override bool CanPlaceTileAt(DungeonData data, Tile tile, int tileType, int
  tileCrackedType){
  return new bool ();
}
}
