using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Generation.Dungeon;

namespace Terraria.WorldBuilding;

public static class Actions
{
	public class ContinueWrapper : GenAction
	{
		private GenAction _action;

		public ContinueWrapper(GenAction action)
		{
			_action = action;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class Count : GenAction
	{
		private Ref<int> _count;

		public Count(Ref<int> count)
		{
			_count = count;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class Scanner : GenAction
	{
		private Ref<int> _count;

		public Scanner(Ref<int> count)
		{
			_count = count;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class TileScanner : GenAction
	{
		private ushort[] _tileIds;

		private Dictionary<ushort, int> _tileCounts;

		public TileScanner(params ushort[] tiles)
		{
			_tileIds = tiles;
			_tileCounts = new Dictionary<ushort, int>();
			for (int i = 0; i < tiles.Length; i++)
			{
				_tileCounts[_tileIds[i]] = 0;
			}
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}
}

	public class Blank : GenAction
	{
		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class Custom : GenAction
	{
		private CustomPerUnitAction _perUnit;

		public Custom(CustomPerUnitAction perUnit)
		{
			_perUnit = perUnit;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class ClearMetadata : GenAction
	{
		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class Clear : GenAction
	{
		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class ClearTile : GenAction
	{
		private bool _frameNeighbors;

		public ClearTile(bool frameNeighbors = false)
		{
			_frameNeighbors = frameNeighbors;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class ClearWall : GenAction
	{
		private bool _frameNeighbors;

		public ClearWall(bool frameNeighbors = false)
		{
			_frameNeighbors = frameNeighbors;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class HalfBlock : GenAction
	{
		private bool _value;

		public HalfBlock(bool value = true)
		{
			_value = value;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SetTile : GenAction
	{
		private ushort _type;

		private bool _doFraming;

		private bool _doNeighborFraming;

		private bool _clearTile;

		public SetTile(ushort type, bool setSelfFrames = false, bool setNeighborFrames = true, bool clearTile = true)
		{
			_type = type;
			_doFraming = setSelfFrames;
			_doNeighborFraming = setNeighborFrames;
			_clearTile = clearTile;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SetWall : GenAction
	{
		private ushort _type;

		private bool _doFraming;

		private bool _doNeighborFraming;

		private bool _clearTile;

		public SetWall(ushort type, bool setSelfFrames = false, bool setNeighborFrames = true, bool clearTile = true)
		{
			_type = type;
			_doFraming = setSelfFrames;
			_doNeighborFraming = setNeighborFrames;
			_clearTile = clearTile;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SetTileKeepWall : GenAction
	{
		private ushort _type;

		private bool _doFraming;

		private bool _doNeighborFraming;

		public SetTileKeepWall(ushort type, bool setSelfFrames = false, bool setNeighborFrames = true)
		{
			_type = type;
			_doFraming = setSelfFrames;
			_doNeighborFraming = setNeighborFrames;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class UpdateBounds : GenAction
	{
		private DungeonBounds _bounds;

		public UpdateBounds(DungeonBounds bounds)
		{
			_bounds = bounds;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class DebugDraw : GenAction
	{
		private Color _color;

		private SpriteBatch _spriteBatch;

		public DebugDraw(SpriteBatch spriteBatch, Color color = default(Color))
		{
			_spriteBatch = spriteBatch;
			_color = color;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SetSlope : GenAction
	{
		private int _slope;

		public SetSlope(int slope)
		{
			_slope = slope;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SetHalfTile : GenAction
	{
		private bool _halfTile;

		public SetHalfTile(bool halfTile)
		{
			_halfTile = halfTile;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SetTilePaint : GenAction
	{
		private byte paintID;

		public SetTilePaint(byte paintID)
		{
			this.paintID = paintID;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class ClearTilePaint : GenAction
	{
		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SetWallPaint : GenAction
	{
		private byte paintID;

		public SetWallPaint(byte paintID)
		{
			this.paintID = paintID;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class ClearWallPaint : GenAction
	{
		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SetTileAndWallPaint : GenAction
	{
		private byte paintID;

		public SetTileAndWallPaint(byte paintID)
		{
			this.paintID = paintID;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class ClearTileAndWallPaint : GenAction
	{
		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SetTileAndWallRainbowPaint : GenAction
	{
		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class PlaceTile : GenAction
	{
		private ushort _type;

		private int _style;

		public PlaceTile(ushort type, int style = 0)
		{
			_type = type;
			_style = style;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class RemoveWall : GenAction
	{
		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class PlaceWall : GenAction
	{
		private ushort _type;

		private bool _neighbors;

		public PlaceWall(ushort type, bool neighbors = true)
		{
			_type = type;
			_neighbors = neighbors;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SetLiquid : GenAction
	{
		private int _type;

		private byte _value;

		public SetLiquid(int type = 0, byte value = byte.MaxValue)
		{
			_value = value;
			_type = type;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SwapSolidTile : GenAction
	{
		private ushort _type;

		public SwapSolidTile(ushort type)
		{
			_type = type;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class SetFrames : GenAction
	{
		private bool _frameNeighbors;

		public SetFrames(bool frameNeighbors = false)
		{
			_frameNeighbors = frameNeighbors;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public class Smooth : GenAction
	{
		private bool _applyToNeighbors;

		public Smooth(bool applyToNeighbors = false)
		{
			_applyToNeighbors = applyToNeighbors;
		}

		public override bool Apply(Point origin, int x, int y, params object[] args){
  return new bool ();
}	}

	public static GenAction Chain(params GenAction[] actions)
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.Actions.Chain"))
	{
		for (int i = 0; i < actions.Length - 1; i++)
		{
			actions[i].NextAction = actions[i + 1];
		}
		return actions[0];
	}
	}
}
