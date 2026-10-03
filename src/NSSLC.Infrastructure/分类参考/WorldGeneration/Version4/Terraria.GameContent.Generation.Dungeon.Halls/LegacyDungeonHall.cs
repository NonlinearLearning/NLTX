using System;
using System.Collections.Generic;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Rooms;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon.Halls;

public class LegacyDungeonHall : DungeonHall
{
	public Vector2D LastHall;

	public int Strength;

	public int Steps;

	protected Vector2D OverrideStartPosition;

	protected Vector2D OverrideEndPosition;

	public LegacyDungeonHall(DungeonHallSettings settings)
		: base(settings)
	{
	}

	public override void CalculatePlatformsAndDoors(DungeonData data){}
	public override void CalculateHall(DungeonData data, Vector2D startPoint, Vector2D endPoint){}
	public override void GenerateHall(DungeonData data){}
	public bool GenerateHall(DungeonData data, int x, int y)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.Halls.LegacyDungeonHall.GenerateHall"))
	{
		generated = false;
		LegacyHall(data, x, y, generating: true);
		generated = true;
		return true;
	}
	}
	public virtual void LegacyHall(DungeonData dungeonData, int i, int j, bool generating = false){}
}
