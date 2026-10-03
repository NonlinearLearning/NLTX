using NSSLC.WorldGeneration.DataStructures;
using NSSLC.WorldGeneration.GameContent;
using NSSLC.WorldGeneration.ID;
using NSSLC.WorldGeneration.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using NSSLC.WorldGeneration.Geometry;
using NSSLC.WorldGeneration.Runtime;

namespace NSSLC.WorldGeneration.Utilities;

public static class TileSnapshot
{
	[StructLayout(LayoutKind.Explicit)]
	public struct TileStruct
	{
		[FieldOffset(0)]
		private ushort _type;

		[FieldOffset(2)]
		private ushort _wall_bTileHeader3_packed;

		[FieldOffset(4)]
		private ushort _sTileHeader;

		[FieldOffset(6)]
		private short _frameX;

		[FieldOffset(8)]
		private short _frameY;

		[FieldOffset(10)]
		private byte _liquid;

		[FieldOffset(11)]
		private byte _bTileHeader;

		[FieldOffset(0)]
		private int _i0;

		[FieldOffset(4)]
		private int _i1;

		[FieldOffset(8)]
		private int _i2;

		private static string[] _liquidNames = new string[4] { "water", "lava", "honey", "shimmer" };

		public static TileStruct From(Tile tile)
{
			TileStruct result = new TileStruct
			{
				_type = tile.type
			};
			ushort wall = tile.wall;
			result._sTileHeader = tile.sTileHeader;
			result._liquid = tile.liquid;
			result._bTileHeader = tile.bTileHeader;
			result._wall_bTileHeader3_packed = (ushort)(wall | ((tile.bTileHeader3 & 0xE0) << 8));
			if ((result._sTileHeader & 0x20) == 0)
			{
				result._type = 0;
				result._sTileHeader &= 35808;
			}
			else
			{
				if (Main.tileFrameImportant[result._type])
				{
					result._frameX = tile.frameX;
					result._frameY = tile.frameY;
				}
				if ((result._sTileHeader & 0x7400) != 0 && !TileID.Sets.SaveSlopes[result._type])
				{
					result._sTileHeader &= 35839;
				}
			}
			if (wall == 0)
			{
				result._bTileHeader &= 224;
			}
			if (result._liquid == 0)
			{
				result._bTileHeader &= 159;
			}
			return result;
		}
		public static bool operator ==(TileStruct lhs, TileStruct rhs)
		{
			if (lhs._i0 == rhs._i0 && lhs._i1 == rhs._i1)
			{
				return lhs._i2 == rhs._i2;
			}
			return false;
		}

		public static bool operator !=(TileStruct lhs, TileStruct rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj){
  return new bool ();
}
		public override int GetHashCode(){
  return new int ();
}
		public override string ToString(){
  return default;
}
		public void Write(BinaryWriter writer)
{
			writer.Write(_i0);
			writer.Write(_i1);
			writer.Write(_i2);
		}
		public static TileStruct Read(BinaryReader reader)
{
			return new TileStruct
			{
				_i0 = reader.ReadInt32(),
				_i1 = reader.ReadInt32(),
				_i2 = reader.ReadInt32()
			};
		}	}

	private static WorldFileData _worldFile;

	private static TileStruct[] _tiles;

	private static List<TileEntity> _tileEntities;

	private static List<Chest> _chests;

	private static MemoryStream _tempStream = new MemoryStream();

	private static BinaryWriter _tempWriter = new BinaryWriter(_tempStream);

	private static BinaryReader _tempReader = new BinaryReader(_tempStream);

	public static object Context { get; private set; }

	public static void Create(object context = null)
{
		Context = context;
		_worldFile = Main.ActiveWorldFileData;
		SaveTiles();
		SaveTileEntities(clone: true);
		SaveChests(clone: true);
	}
	private static void SaveTiles(){}
	private static void SaveTileEntities(bool clone){}
	private static void SaveChests(bool clone){}
	public static void Restore()
{
		RestoreTiles();
		RestoreTileEntities(_tileEntities, clone: true);
		RestoreChests(_chests, clone: true);
		if (Main.dedServ)
		{
			NetMessage.ResyncTiles(new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY));
		}
	}
	private static void RestoreTiles(){}
	private static void RestoreTileEntities(List<TileEntity> entities, bool clone){}
	private static void RestoreChests(List<Chest> chests, bool clone){}
	public static void Save(BinaryWriter writer)
{
		writer.Write(Marshal.SizeOf(typeof(TileStruct)));
		TileStruct[] tiles = _tiles;
		foreach (TileStruct tileStruct in tiles)
		{
			tileStruct.Write(writer);
		}
		writer.Write(_tileEntities.Count);
		foreach (TileEntity tileEntity in _tileEntities)
		{
			TileEntity.Write(writer, tileEntity);
		}
		writer.Write(_chests.Count);
		foreach (Chest chest in _chests)
		{
			writer.Write(chest.index);
			writer.Write(chest.x);
			writer.Write(chest.y);
			writer.Write(chest.maxItems);
			writer.Write(chest.name);
			for (int j = 0; j < chest.maxItems; j++)
			{
				Item item = chest.item[j];
				if (item.IsAir)
				{
					writer.Write((ushort)0);
					continue;
				}
				writer.Write((ushort)item.type);
				writer.Write((ushort)item.stack);
				writer.Write(item.prefix);
			}
		}
	}
	public static void Load(BinaryReader reader, object context = null)
{
		if (reader.ReadInt32() != Marshal.SizeOf(typeof(TileStruct)))
		{
			throw new Exception("TileSnapshot was saved with a different value of #define SNAPSHOT_RUNTIME_DATA");
		}
		Context = context;
		_worldFile = Main.ActiveWorldFileData;
		Array.Resize(ref _tiles, Main.maxTilesX * Main.maxTilesY);
		for (int i = 0; i < _tiles.Length; i++)
		{
			_tiles[i] = TileStruct.Read(reader);
		}
		if (_tileEntities == null)
		{
			_tileEntities = new List<TileEntity>();
		}
		_tileEntities.Clear();
		int num = reader.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			_tileEntities.Add(TileEntity.Read(reader, 319));
		}
		if (_chests == null)
		{
			_chests = new List<Chest>();
		}
		_chests.Clear();
		num = reader.ReadInt32();
		for (int k = 0; k < num; k++)
		{
			Chest chest = Chest.CreateOutOfArray(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
			chest.name = reader.ReadString();
			chest.FillWithEmptyInstances();
			for (int l = 0; l < chest.maxItems; l++)
			{
				int num2 = reader.ReadUInt16();
				if (num2 != 0)
				{
					Item obj = chest.item[l];
					obj.SetDefaults(num2);
					obj.stack = reader.ReadUInt16();
					obj.Prefix(reader.ReadByte());
				}
			}
			_chests.Add(chest);
		}
	}
}
