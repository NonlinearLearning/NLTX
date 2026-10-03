using System;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Features;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon.Rooms;

public class LegacyDungeonRoom : DungeonRoom
{
	private ShapeData _innerShapeData = new ShapeData();

	private ShapeData _outerShapeData = new ShapeData();

	public Vector2D StartPosition;

	public Vector2D EndPosition;

	public int Strength;

	public LegacyDungeonRoom(DungeonRoomSettings settings)
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
	public override bool TryGenerateChestInRoom(DungeonData data, DungeonGlobalBasicChests feature){
  return new bool ();
}
	public override bool DualDungeons_TryGenerateBiomeChestInRoom(DungeonData data, DungeonGlobalBiomeChests
  feature){
  return new bool ();
}
}
