using System.IO;
using NSSLC.WorldGeneration.Geometry;
using NSSLC.WorldGeneration.Utilities;

namespace NSSLC.WorldGeneration.GameContent;

public class TreeTopsInfo
{
	public class AreaId
	{
		public const int Forest1 = 0;

		public const int Forest2 = 1;

		public const int Forest3 = 2;

		public const int Forest4 = 3;

		public const int Corruption = 4;

		public const int Jungle = 5;

		public const int Snow = 6;

		public const int Hallow = 7;

		public const int Crimson = 8;

		public const int Desert = 9;

		public const int Ocean = 10;

		public const int GlowingMushroom = 11;

		public const int Underworld = 12;

		public static readonly int Count = 13;
	}

	private int[] _variations = new int[AreaId.Count];

	public int GetTreeStyle(int areaId)
{
	
		return _variations[areaId];
	
	}
	public void CopyExistingWorldInfoForWorldGeneration()
{
	
		CopyExistingWorldInfo();
	
	}
	private void CopyExistingWorldInfo()
{
	
		_variations[0] = Main.treeStyle[0];
		_variations[1] = Main.treeStyle[1];
		_variations[2] = Main.treeStyle[2];
		_variations[3] = Main.treeStyle[3];
		_variations[4] = WorldGen.corruptBG;
		_variations[5] = WorldGen.jungleBG;
		_variations[6] = WorldGen.snowBG;
		_variations[7] = WorldGen.hallowBG;
		_variations[8] = WorldGen.crimsonBG;
		_variations[9] = WorldGen.desertBG;
		_variations[10] = WorldGen.oceanBG;
		_variations[11] = WorldGen.mushroomBG;
		_variations[12] = WorldGen.underworldBG;
	
	}
}
