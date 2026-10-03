using System.IO;
using NSSLC.WorldGeneration.Geometry;


namespace NSSLC.WorldGeneration.GameContent;

public static class UnbreakableWallScan
{


	public static readonly int ScanDistance = 250;

	public static readonly Point[] Directions = new Point[8]
	{
		new Point(1, 0),
		new Point(1, 1),
		new Point(0, 1),
		new Point(-1, 1),
		new Point(-1, 0),
		new Point(-1, -1),
		new Point(0, -1),
		new Point(1, -1)
	};

	public static void Update(Player player)
{
	
		_ = Main.netMode;
		_ = 1;
	
	}
	public static bool InsideUnbreakableWalls(Point pt)
{
	
		int num = 0;
		for (int i = 0; i < Directions.Length; i++)
		{
			if (LineScan(pt, Directions[i]))
			{
				num |= 1 << i;
			}
		}
		for (int j = 0; j < Directions.Length; j++)
		{
			if ((num & 0x1F) == 0)
			{
				return false;
			}
			num = ((num << 1) & 0xFF) | (num >> 7);
		}
		return true;
	
	}
	public static bool LineScan(Point pt, Point dir)
{
	
		int num = 0;
		while (num < ScanDistance)
		{
			if (!WorldGen.InWorld(pt))
			{
				return false;
			}
			Tile tile = Main.tile[pt.X, pt.Y];
			if (tile == null)
			{
				return false;
			}
			if (tile.wall == 350)
			{
				return tile.wallColor() >= 16;
			}
			num++;
			pt.X += dir.X;
			pt.Y += dir.Y;
		}
		return false;
	
	}}
