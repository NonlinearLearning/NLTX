using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon.Rooms;

public class WormlikeDungeonRoom : DungeonRoom
{
	private ShapeData _innerShapeData = new ShapeData();

	private ShapeData _outerShapeData = new ShapeData();

	public int InnerBoundsSizeMin;

	public int InnerBoundsSizeMax;

	public Vector2[] Positions;

	public WormlikeDungeonRoom(DungeonRoomSettings settings)
		: base(settings)
	{
	}

	public override void CalculateRoom(DungeonData data){}
	public override bool GenerateRoom(DungeonData data){
  return new bool ();
}
	public override int GetFloodedRoomTileCount(){
  return new int ();
}
	public override void FloodRoom(byte liquidType){}
	public override ProtectionType GetProtectionTypeFromPoint(int x, int y){
  return default;
}
	public override bool IsInsideRoom(int x, int y){
  return new bool ();
}
}
