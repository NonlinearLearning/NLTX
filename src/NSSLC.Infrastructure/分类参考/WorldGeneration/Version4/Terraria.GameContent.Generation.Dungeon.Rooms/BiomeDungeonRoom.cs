using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Features;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon.Rooms;

public abstract class BiomeDungeonRoom : DungeonRoom
{
	protected const int BIOMEROOM_INNER_SIZE_BASE = 32;

	protected const int BIOMEROOM_INNER_SIZE_BASE_TEMPLE = 50;

	protected const int BIOMEROOM_WALL_DEPTH = 8;

	protected ShapeData _innerShapeData;

	protected ShapeData _outerShapeData;

	public BiomeDungeonRoom(DungeonRoomSettings settings)
		: base(settings)
	{
	}

	public override bool CanGenerateFeatureAt(DungeonData data, IDungeonFeature feature, int x, int y){
  return new bool ();
}
	public override void GeneratePreHallwaysDungeonFeaturesInRoom(DungeonData data){}
	public override void GenerateEarlyDungeonFeaturesInRoom(DungeonData data){}
	public override void CalculatePlatformsAndDoors(DungeonData data){}
	public override ConnectionPointQuality GetHallwayConnectionPoint(Vector2D otherRoomPos, out Vector2D
  connectionPoint){
  connectionPoint = new Vector2D();
  return default;
}
	public override ProtectionType GetProtectionTypeFromPoint(int x, int y){
  return default;
}
	public override bool IsInsideRoom(int x, int y){
  return new bool ();
}
}
