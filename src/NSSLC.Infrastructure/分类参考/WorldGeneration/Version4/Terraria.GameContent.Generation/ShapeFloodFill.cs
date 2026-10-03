using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation;

public class ShapeFloodFill : GenShape
{
	private int _maximumActions;

	public ShapeFloodFill(int maximumActions = 100)
	{
		_maximumActions = maximumActions;
	}

	public override bool Perform(Point origin, GenAction action){
  return new bool ();
}
}
