using System;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Biomes;

public class TerrainPass : GenPass
{

	private class SurfaceHistory
	{
		private readonly double[] _heights;

		private int _index;

		public double this[int index]
		{
			get
			{
				return _heights[(index + _index) % _heights.Length];
			}
			set
			{
				_heights[(index + _index) % _heights.Length] = value;
			}
		}

		public SurfaceHistory(int size)
		{
			_heights = new double[size];
		}
}

	public TerrainPass()
		: base(GenPassNameID.Terrain, 449.3721923828125)
	{
	}

	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration){}
}
