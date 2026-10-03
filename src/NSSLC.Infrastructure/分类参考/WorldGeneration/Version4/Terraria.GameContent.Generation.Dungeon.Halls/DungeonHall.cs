using System;
using System.Collections.Generic;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Rooms;

namespace Terraria.GameContent.Generation.Dungeon.Halls;

public abstract class DungeonHall
{
	public DungeonHallSettings settings;

	public bool calculated;

	public bool generated;

	public DungeonBounds Bounds = new DungeonBounds();

	public Vector2D StartPosition;

	public Vector2D EndPosition;

	public Vector2D StartDirection;

	public Vector2D EndDirection;

	public bool CrackedBrick;

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

	public DungeonHall(DungeonHallSettings settings)
	{
		this.settings = settings;
	}

	public abstract void CalculateHall(DungeonData data, Vector2D startPoint, Vector2D endPoint);

	public abstract void CalculatePlatformsAndDoors(DungeonData data);

	public abstract void GenerateHall(DungeonData data);

	public virtual int GetFurnitureCount(int defaultCount){
  return new int ();
}
	public virtual bool CanPlaceTileAt(DungeonData data, Tile tile, int tileType, int tileCrackedType){
  return new bool ();
}
	public virtual bool CanRemoveTileAt(DungeonData data, Tile tile, int tileCrackedType){
  return new bool ();
}
}
