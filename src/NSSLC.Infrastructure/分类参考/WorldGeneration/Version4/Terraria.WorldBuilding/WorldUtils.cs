using System;
using Microsoft.Xna.Framework;
using Terraria.Testing;

namespace Terraria.WorldBuilding;

public static class WorldUtils
{
	public static Rectangle ClampToWorld(Rectangle tileRectangle, int fluff = 0)
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldUtils.ClampToWorld"))
	{
		int num = Math.Max(fluff, Math.Min(tileRectangle.Left, Main.maxTilesX - fluff));
		int num2 = Math.Max(fluff, Math.Min(tileRectangle.Top, Main.maxTilesY - fluff));
		int num3 = Math.Max(fluff, Math.Min(tileRectangle.Right, Main.maxTilesX - fluff));
		int num4 = Math.Max(fluff, Math.Min(tileRectangle.Bottom, Main.maxTilesY - fluff));
		return new Rectangle(num, num2, num3 - num, num4 - num2);
	}
	}
	public static Rectangle GetWorldPlayArea()
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldUtils.GetWorldPlayArea"))
	{
		int num = 640;
		Point point = new Point((int)Main.leftWorld + num, (int)Main.topWorld + num);
		Point point2 = new Point((int)Main.rightWorld - num, (int)Main.bottomWorld - num);
		return new Rectangle(point.X, point.Y, point2.X - point.X, point2.Y - point.Y);
	}
	}
	public static Rectangle ClampToWorldBorders(Rectangle worldRect)
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldUtils.ClampToWorldBorders"))
	{
		if (DebugOptions.noLimits)
		{
			return worldRect;
		}
		return Utils.Clamp(worldRect, GetWorldPlayArea());
	}
	}
	public static bool Gen(Point origin, GenShape shape, GenAction action)
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldUtils.Gen"))
	{
		return shape.Perform(origin, action);
	}
	}
	public static bool Find(Point origin, GenSearch search, out Point result)
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldUtils.Find"))
	{
		result = search.Find(origin);
		if (result == GenSearch.NOT_FOUND)
		{
			return false;
		}
		return true;
	}
	}
	public static void WireLine(Point start, Point end)
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldUtils.WireLine"))
	{
		Point point = start;
		Point point2 = end;
		if (end.X < start.X)
		{
			Utils.Swap(ref end.X, ref start.X);
		}
		if (end.Y < start.Y)
		{
			Utils.Swap(ref end.Y, ref start.Y);
		}
		for (int i = start.X; i <= end.X; i++)
		{
			WorldGen.PlaceWire(i, point.Y);
		}
		for (int j = start.Y; j <= end.Y; j++)
		{
			WorldGen.PlaceWire(point2.X, j);
		}
	}
	}
}
