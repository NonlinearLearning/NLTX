using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
	
		return Volatile.Read(ref _variations)[areaId];
	
	}
	public void ApplyStyles(IReadOnlyList<int> styles)
	{
		ArgumentNullException.ThrowIfNull(styles);
		if (styles.Count != _variations.Length)
		{
			throw new ArgumentException(
				$"Tree tops state must contain exactly {_variations.Length} styles.",
				nameof(styles));
		}

		int[] stableStyles = new int[styles.Count];
		for (int areaId = 0; areaId < stableStyles.Length; areaId++)
		{
			stableStyles[areaId] = styles[areaId];
		}

		Volatile.Write(ref _variations, stableStyles);
	}
	public void Reset()
	{
		Volatile.Write(ref _variations, new int[AreaId.Count]);
	}
	public void CopyExistingWorldInfoForWorldGeneration()
{
	
		CopyExistingWorldInfo();
	
	}
private void CopyExistingWorldInfo()
{
		int[] variations = new int[AreaId.Count];
		variations[0] = Main.treeStyle[0];
		variations[1] = Main.treeStyle[1];
		variations[2] = Main.treeStyle[2];
		variations[3] = Main.treeStyle[3];
		variations[4] = WorldGen.corruptBG;
		variations[5] = WorldGen.jungleBG;
		variations[6] = WorldGen.snowBG;
		variations[7] = WorldGen.hallowBG;
		variations[8] = WorldGen.crimsonBG;
		variations[9] = WorldGen.desertBG;
		variations[10] = WorldGen.oceanBG;
		variations[11] = WorldGen.mushroomBG;
		variations[12] = WorldGen.underworldBG;
		Volatile.Write(ref _variations, variations);
	}
}
