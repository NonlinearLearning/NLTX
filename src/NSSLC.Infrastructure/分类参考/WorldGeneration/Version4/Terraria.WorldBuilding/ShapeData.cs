using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace Terraria.WorldBuilding;

public class ShapeData
{
	private HashSet<Point16> _points;

	public int Count => _points.Count;

	public ShapeData()
	{
		_points = new HashSet<Point16>();
	}

	public ShapeData(ShapeData original)
	{
		_points = new HashSet<Point16>(original._points);
	}
	public void Clear()
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.ShapeData.Clear"))
	{
		_points.Clear();
	}
	}
}
