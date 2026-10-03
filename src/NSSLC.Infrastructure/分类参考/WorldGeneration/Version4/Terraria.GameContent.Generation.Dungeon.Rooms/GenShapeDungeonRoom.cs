using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Features;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon.Rooms;

public class GenShapeDungeonRoom : DungeonRoom
{
	private ShapeData _innerShapeData = new ShapeData();

	private ShapeData _outerShapeData = new ShapeData();

	public GenShapeDungeonRoom(DungeonRoomSettings settings)
		: base(settings)
	{
		_ = (GenShapeDungeonRoomSettings)settings;
	}

	public override void CalculateRoom(DungeonData data){}
	public override bool GenerateRoom(DungeonData data){
  return new bool ();
}
	public override bool CanGenerateFeatureAt(DungeonData data, IDungeonFeature feature, int x, int y){
  return new bool ();
}
	public override void GenerateEarlyDungeonFeaturesInRoom(DungeonData data){}
	public override Point GetRoomCenterForDungeonFeature(DungeonData data, DungeonFeature feature){
  return new Point();
}
	public override Point GetRoomCenterForHallway(Vector2D otherRoomPos){
  return new Point();
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
