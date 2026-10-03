using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;

namespace Terraria.WorldBuilding;

public static class Modifiers
{
	public class ShapeScale : GenAction
	{
		private int _scale;

		public ShapeScale(int scale)
		{
			_scale = scale;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class Expand : GenAction
	{
		private int _xExpansion;

		private int _yExpansion;

		public Expand(int expansion)
		{
			_xExpansion = expansion;
			_yExpansion = expansion;
		}

		public Expand(int xExpansion, int yExpansion)
		{
			_xExpansion = xExpansion;
			_yExpansion = yExpansion;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class RadialDither : GenAction
	{
		private double _innerRadius;

		private double _outerRadius;

		public RadialDither(double innerRadius, double outerRadius)
		{
			_innerRadius = innerRadius;
			_outerRadius = outerRadius;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class Blotches : GenAction
	{
		private int _minX;

		private int _minY;

		private int _maxX;

		private int _maxY;

		private double _chance;

		public Blotches(int scale = 2, double chance = 0.3)
		{
			_minX = scale;
			_minY = scale;
			_maxX = scale;
			_maxY = scale;
			_chance = chance;
		}

		public Blotches(int xScale, int yScale, double chance = 0.3)
		{
			_minX = xScale;
			_maxX = xScale;
			_minY = yScale;
			_maxY = yScale;
			_chance = chance;
		}

		public Blotches(int leftScale, int upScale, int rightScale, int downScale, double chance = 0.3)
		{
			_minX = leftScale;
			_maxX = rightScale;
			_minY = upScale;
			_maxY = downScale;
			_chance = chance;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class InShape : GenAction
	{
		private readonly ShapeData _shapeData;

		public InShape(ShapeData shapeData)
		{
			_shapeData = shapeData;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class NotInShape : GenAction
	{
		private readonly ShapeData _shapeData;

		public NotInShape(ShapeData shapeData)
		{
			_shapeData = shapeData;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class Conditions : GenAction
	{
		private readonly GenCondition[] _conditions;

		public Conditions(params GenCondition[] conditions)
		{
			_conditions = conditions;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class OnlyWalls : GenAction
	{
		private ushort[] _types;

		public OnlyWalls(params ushort[] types)
		{
			_types = types;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class OnlyTiles : GenAction
	{
		private ushort[] _types;

		public OnlyTiles(params ushort[] types)
		{
			_types = types;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class Checkerboard : GenAction
	{
		private int _percentile;

		public Checkerboard(int percentile)
		{
			_percentile = percentile;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class IsTouching : GenAction
	{
		private static readonly int[] DIRECTIONS = new int[16]
		{
			0, -1, 1, 0, -1, 0, 0, 1, -1, -1,
			1, -1, -1, 1, 1, 1
		};

		private bool _useDiagonals;

		private ushort[] _tileIds;

		public IsTouching(bool useDiagonals, params ushort[] tileIds)
		{
			_useDiagonals = useDiagonals;
			_tileIds = tileIds;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class NotTouching : GenAction
	{
		private static readonly int[] DIRECTIONS = new int[16]
		{
			0, -1, 1, 0, -1, 0, 0, 1, -1, -1,
			1, -1, -1, 1, 1, 1
		};

		private bool _useDiagonals;

		private ushort[] _tileIds;

		public NotTouching(bool useDiagonals, params ushort[] tileIds)
		{
			_useDiagonals = useDiagonals;
			_tileIds = tileIds;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class IsTouchingAir : GenAction
	{
		private static readonly int[] DIRECTIONS = new int[16]
		{
			0, -1, 1, 0, -1, 0, 0, 1, -1, -1,
			1, -1, -1, 1, 1, 1
		};

		private bool _useDiagonals;

		public IsTouchingAir(bool useDiagonals = false)
		{
			_useDiagonals = useDiagonals;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SkipTiles : GenAction
	{
		private ushort[] _types;

		public SkipTiles(params ushort[] types)
		{
			_types = types;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class HasLiquid : GenAction
	{
		private int _liquidType;

		private int _liquidLevel;

		public HasLiquid(int liquidLevel = -1, int liquidType = -1)
		{
			_liquidType = liquidType;
			_liquidLevel = liquidLevel;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class NoLiquid : GenAction
	{
		private int _liquidType;

		public NoLiquid(int liquidType = -1)
		{
			_liquidType = liquidType;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SkipWalls : GenAction
	{
		private ushort[] _types;

		public SkipWalls(params ushort[] types)
		{
			_types = types;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SkipUnbreakableWalledTiles : GenAction
	{
		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class IsAboveHeight : GenAction
	{
		private int _y;

		private bool _inclusive;

		public IsAboveHeight(int y, bool inclusive = false)
		{
			_y = y;
			_inclusive = inclusive;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class IsBelowHeight : GenAction
	{
		private int _y;

		private bool _inclusive;

		public IsBelowHeight(int y, bool inclusive = false)
		{
			_y = y;
			_inclusive = inclusive;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class IsEmpty : GenAction
	{
		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class IsSolid : GenAction
	{
		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class IsNotSolid : GenAction
	{
		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class RectangleMask : GenAction
	{
		private int _xMin;

		private int _yMin;

		private int _xMax;

		private int _yMax;

		public RectangleMask(int xMin, int xMax, int yMin, int yMax)
		{
			_xMin = xMin;
			_yMin = yMin;
			_xMax = xMax;
			_yMax = yMax;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class Offset : GenAction
	{
		private int _xOffset;

		private int _yOffset;

		public Offset(int x, int y)
		{
			_xOffset = x;
			_yOffset = y;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class Dither : GenAction
	{
		private double _failureChance;

		public Dither(double failureChance = 0.5)
		{
			_failureChance = failureChance;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class Flip : GenAction
	{
		private bool _flipX;

		private bool _flipY;

		public Flip(bool flipX, bool flipY)
		{
			_flipX = flipX;
			_flipY = flipY;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}
}
