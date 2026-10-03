using Terraria.Utilities;

namespace Terraria.WorldBuilding;

public class GenBase
{
	public delegate bool CustomPerUnitAction(int x, int y, params object[] args);

	protected static Tile[,] _tiles => Main.tile;
}
