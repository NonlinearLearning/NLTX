using System;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation.Dungeon.Features;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon.Rooms;

public class BiomeStructuredDungeonRoom : BiomeDungeonRoom
{
	public const int VARIANT_DOUBLEDIAMOND = 0;

	public const int VARIANT_ROUNDED = 1;

	public const int VARIANT_CANDY = 2;

	public const int VARIANT_WIGGLED = 3;

	public const int MAX_VARIANTS = 4;

	public Vector2 Position;

	public int RoomInnerSize;

	public int RoomOuterSize;

	public int WallDepth;

	public BiomeStructuredDungeonRoom(DungeonRoomSettings settings)
		: base(settings)
	{
		_innerShapeData = new ShapeData();
		_outerShapeData = new ShapeData();
	}

	public override void CalculateRoom(DungeonData data){}
	public override bool GenerateRoom(DungeonData data){
  return new bool ();
}
	public override void GenerateEarlyDungeonFeaturesInRoom(DungeonData data){}
}
