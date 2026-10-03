using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Features;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon.Rooms;

public abstract class DungeonRoom
{
	public DungeonRoomSettings settings;

	public bool calculated;

	public bool generated;

	public DungeonBounds InnerBounds = new DungeonBounds();

	public DungeonBounds OuterBounds = new DungeonBounds();

	public bool Processed
	{
		get
		{
			if (!calculated)
			{
				return generated;
			}
			return true;
		}
	}

	public Point Center => InnerBounds.Center;

	public DungeonRoom(DungeonRoomSettings settings)
	{
		this.settings = settings;
	}

	public virtual bool CanGenerateFeatureAt(DungeonData data, IDungeonFeature feature, int x, int y){
  return new bool ();
}
	public virtual void GeneratePreHallwaysDungeonFeaturesInRoom(DungeonData data){}
	public virtual void GenerateEarlyDungeonFeaturesInRoom(DungeonData data){}
	public virtual void GenerateLateDungeonFeaturesInRoom(DungeonData data){}
	public virtual Point GetRoomCenterForDungeonFeature(DungeonData data, DungeonFeature feature){
  return new Point();
}
	public virtual Point GetRoomCenterForHallway(Vector2D otherRoomPos){
  return new Point();
}
	public abstract void CalculateRoom(DungeonData data);

	public virtual void CalculatePlatformsAndDoors(DungeonData data){}
	public virtual ConnectionPointQuality GetHallwayConnectionPoint(Vector2D otherRoomPos, out Vector2D
  connectionPoint){
  connectionPoint = new Vector2D();
  return default;
}
	public abstract bool GenerateRoom(DungeonData data);

	public virtual bool TryGenerateChestInRoom(DungeonData data, DungeonGlobalBasicChests feature){
  return new bool ();
}
	public virtual bool DualDungeons_TryGenerateBiomeChestInRoom(DungeonData data, DungeonGlobalBiomeChests
  feature){
  return new bool ();
}
	public virtual ProtectionType GetProtectionTypeFromPoint(int x, int y){
  return default;
}
	public virtual bool IsInsideRoom(int x, int y){
  return new bool ();
}
	public virtual int GetFloodedRoomTileCount(){
  return new int ();
}
	public virtual void FloodRoom(byte liquidType){}
	public virtual int GetFurnitureCount(int defaultCount){
  return new int ();
}
}
