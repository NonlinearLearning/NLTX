using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Biomes.CaveHouse;

public static class HouseUtils
{
	private static readonly bool[] BlacklistedTiles = TileID.Sets.Factory.CreateBoolSet(true, 225, 41, 43, 44, 226, 203, 112, 25, 151, 21, 467);

	private static readonly bool[] BeelistedTiles = TileID.Sets.Factory.CreateBoolSet(true, 41, 43, 44, 226, 203, 112, 25, 151, 21, 467);
	public static int GetMaxPossibleRoomsInABigAbandonedHouse()
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Biomes.CaveHouse.HouseUtils.GetMaxPossibleRoomsInABigAbandonedHouse"))
	{
		if (WorldGen.SecretSeed.errorWorld.Enabled)
		{
			return 30;
		}
		return 7;
	}
	}
}
