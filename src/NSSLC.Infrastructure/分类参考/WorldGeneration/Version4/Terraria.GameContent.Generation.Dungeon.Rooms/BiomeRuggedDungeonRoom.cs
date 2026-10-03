using System;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation.Dungeon.Features;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon.Rooms;

public class BiomeRuggedDungeonRoom : BiomeDungeonRoom
{
	public Vector2 Position;

	public int RoomInnerSize;

	public int RoomOuterSize;

	public int WallDepth;

	public BiomeRuggedDungeonRoom(DungeonRoomSettings settings)
		: base(settings)
	{
		_innerShapeData = new ShapeData();
		_outerShapeData = new ShapeData();
	}

	public override void CalculateRoom(DungeonData data){}
	public override bool GenerateRoom(DungeonData data){
  return new bool ();
}
	public override bool CanGenerateFeatureAt(DungeonData data, IDungeonFeature feature, int x, int y){
  return new bool ();
}
	public override void GenerateEarlyDungeonFeaturesInRoom(DungeonData data){}
}
