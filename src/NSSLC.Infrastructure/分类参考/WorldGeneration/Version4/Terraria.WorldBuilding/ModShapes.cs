using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace Terraria.WorldBuilding;

public static class ModShapes
{
	public class All : GenModShape
	{
		public All(ShapeData data)
			: base(data)
		{
		}

		public override bool Perform(Point origin, GenAction action){
  return new bool ();
}	}

	public class OuterOutline : GenModShape
	{
		private static readonly int[] POINT_OFFSETS = new int[16]
		{
			1, 0, -1, 0, 0, 1, 0, -1, 1, 1,
			1, -1, -1, 1, -1, -1
		};

		private bool _useDiagonals;

		private bool _useInterior;

		public OuterOutline(ShapeData data, bool useDiagonals = true, bool useInterior = false)
			: base(data)
		{
			_useDiagonals = useDiagonals;
			_useInterior = useInterior;
		}

		public override bool Perform(Point origin, GenAction action){
  return new bool ();
}	}

	public class InnerOutline : GenModShape
	{
		private static readonly int[] POINT_OFFSETS = new int[16]
		{
			1, 0, -1, 0, 0, 1, 0, -1, 1, 1,
			1, -1, -1, 1, -1, -1
		};

		private bool _useDiagonals;

		public InnerOutline(ShapeData data, bool useDiagonals = true)
			: base(data)
		{
			_useDiagonals = useDiagonals;
		}

		public override bool Perform(Point origin, GenAction action){
  return new bool ();
}	}
}
