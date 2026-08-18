using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace Terraria;

public class Tile
{
	public ushort type;

	public ushort wall;

	public byte liquid;

	public ushort sTileHeader;

	public byte bTileHeader;

	public byte bTileHeader2;

	public byte bTileHeader3;

	public short frameX;

	public short frameY;

	private const int Bit0 = 1;

	private const int Bit1 = 2;

	private const int Bit2 = 4;

	private const int Bit3 = 8;

	private const int Bit4 = 16;

	private const int Bit5 = 32;

	private const int Bit6 = 64;

	private const int Bit7 = 128;

	private const ushort Bit15 = 32768;

	public const int Type_Solid = 0;

	public const int Type_Halfbrick = 1;

	public const int Type_SlopeDownRight = 2;

	public const int Type_SlopeDownLeft = 3;

	public const int Type_SlopeUpRight = 4;

	public const int Type_SlopeUpLeft = 5;

	public const int Liquid_Water = 0;

	public const int Liquid_Lava = 1;

	public const int Liquid_Honey = 2;

	public const int Liquid_Shimmer = 3;

	private const int NeitherLavaOrHoney = 159;

	private const int EitherLavaOrHoney = 96;

	public int collisionType
	{
		get
		{
			if (!active())
			{
				return 0;
			}
			if (halfBrick())
			{
				return 2;
			}
			if (slope() > 0)
			{
				return 2 + slope();
			}
			if (Main.tileSolid[type] && !Main.tileSolidTop[type])
			{
				return 1;
			}
			return -1;
		}
	}

	public Tile()
	{
		type = 0;
		wall = 0;
		liquid = 0;
		sTileHeader = 0;
		bTileHeader = 0;
		bTileHeader2 = 0;
		bTileHeader3 = 0;
		frameX = 0;
		frameY = 0;
	}

	public Tile(Tile copy)
	{
		if (copy == null)
		{
			type = 0;
			wall = 0;
			liquid = 0;
			sTileHeader = 0;
			bTileHeader = 0;
			bTileHeader2 = 0;
			bTileHeader3 = 0;
			frameX = 0;
			frameY = 0;
		}
		else
		{
			type = copy.type;
			wall = copy.wall;
			liquid = copy.liquid;
			sTileHeader = copy.sTileHeader;
			bTileHeader = copy.bTileHeader;
			bTileHeader2 = copy.bTileHeader2;
			bTileHeader3 = copy.bTileHeader3;
			frameX = copy.frameX;
			frameY = copy.frameY;
		}
	}

	public object Clone()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.Clone"))
	{
		return MemberwiseClone();
	}
	}
	public void ClearEverything()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.ClearEverything"))
	{
		type = 0;
		wall = 0;
		liquid = 0;
		sTileHeader = 0;
		bTileHeader = 0;
		bTileHeader2 = 0;
		bTileHeader3 = 0;
		frameX = 0;
		frameY = 0;
	}
	}
	public void ClearTile()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.ClearTile"))
	{
		ClearSlope();
		active(active: false);
		inActive(inActive: false);
	}
	}
	public void ClearSlope()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.ClearSlope"))
	{
		slope(0);
		halfBrick(halfBrick: false);
	}
	}
	public void ClearTileAndPaint()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.ClearTileAndPaint"))
	{
		ClearTile();
		ClearBlockPaintAndCoating();
	}
	}
	public void CopyFrom(Tile from)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.CopyFrom"))
	{
		type = from.type;
		wall = from.wall;
		liquid = from.liquid;
		sTileHeader = from.sTileHeader;
		bTileHeader = from.bTileHeader;
		bTileHeader2 = from.bTileHeader2;
		bTileHeader3 = from.bTileHeader3;
		frameX = from.frameX;
		frameY = from.frameY;
	}
	}
	public bool isTheSameAs(Tile compTile)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.isTheSameAs"))
	{
		if (compTile == null)
		{
			return false;
		}
		if (sTileHeader != compTile.sTileHeader)
		{
			return false;
		}
		if (active())
		{
			if (type != compTile.type)
			{
				return false;
			}
			if (Main.tileFrameImportant[type] && (frameX != compTile.frameX || frameY != compTile.frameY))
			{
				return false;
			}
		}
		if (wall != compTile.wall || liquid != compTile.liquid)
		{
			return false;
		}
		if (compTile.liquid == 0)
		{
			if (wallColor() != compTile.wallColor())
			{
				return false;
			}
			if (wire4() != compTile.wire4())
			{
				return false;
			}
		}
		else if (bTileHeader != compTile.bTileHeader)
		{
			return false;
		}
		if (invisibleBlock() != compTile.invisibleBlock() || invisibleWall() != compTile.invisibleWall() || fullbrightBlock() != compTile.fullbrightBlock() || fullbrightWall() != compTile.fullbrightWall())
		{
			return false;
		}
		return true;
	}
	}
	public int blockType()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.blockType"))
	{
		if (halfBrick())
		{
			return 1;
		}
		int num = slope();
		if (num > 0)
		{
			num++;
		}
		return num;
	}
	}
	public void liquidType(int liquidType)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.liquidType"))
	{
		switch (liquidType)
		{
		case 0:
			bTileHeader &= 159;
			break;
		case 1:
			lava(lava: true);
			break;
		case 2:
			honey(honey: true);
			break;
		case 3:
			shimmer(shimmer: true);
			break;
		}
	}
	}
	public byte liquidType()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.liquidType"))
	{
		return (byte)((bTileHeader & 0x60) >> 5);
	}
	}
	public bool nactive()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.nactive"))
	{
		if ((sTileHeader & 0x60) == 32)
		{
			return true;
		}
		return false;
	}
	}
	public void ResetToType(ushort type)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.ResetToType"))
	{
		liquid = 0;
		sTileHeader = 32;
		bTileHeader = 0;
		bTileHeader2 = 0;
		bTileHeader3 = 0;
		frameX = 0;
		frameY = 0;
		this.type = type;
	}
	}
	internal void ClearMetadata()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.ClearMetadata"))
	{
		liquid = 0;
		sTileHeader = 0;
		bTileHeader = 0;
		bTileHeader2 = 0;
		bTileHeader3 = 0;
		frameX = 0;
		frameY = 0;
	}
	}
	public Color actColor(Color oldColor)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.actColor"))
	{
		if (!inActive())
		{
			return oldColor;
		}
		double num = 0.4;
		return new Color((byte)(num * (double)(int)oldColor.R), (byte)(num * (double)(int)oldColor.G), (byte)(num * (double)(int)oldColor.B), oldColor.A);
	}
	}
	public void actColor(ref Vector3 oldColor)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.actColor"))
	{
		if (inActive())
		{
			oldColor *= 0.4f;
		}
	}
	}
	public bool topSlope()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.topSlope"))
	{
		byte b = slope();
		if (b != 1)
		{
			return b == 2;
		}
		return true;
	}
	}
	public bool bottomSlope()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.bottomSlope"))
	{
		byte b = slope();
		if (b != 3)
		{
			return b == 4;
		}
		return true;
	}
	}
	public bool leftSlope()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.leftSlope"))
	{
		byte b = slope();
		if (b != 2)
		{
			return b == 4;
		}
		return true;
	}
	}
	public bool rightSlope()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.rightSlope"))
	{
		byte b = slope();
		if (b != 1)
		{
			return b == 3;
		}
		return true;
	}
	}
	public bool HasSameSlope(Tile tile)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.HasSameSlope"))
	{
		return (sTileHeader & 0x7400) == (tile.sTileHeader & 0x7400);
	}
	}
	public byte wallColor()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wallColor"))
	{
		return (byte)(bTileHeader & 0x1F);
	}
	}
	public void wallColor(byte wallColor)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wallColor"))
	{
		bTileHeader = (byte)((bTileHeader & 0xE0) | wallColor);
	}
	}
	public bool lava()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.lava"))
	{
		return (bTileHeader & 0x60) == 32;
	}
	}
	public void lava(bool lava)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.lava"))
	{
		if (lava)
		{
			bTileHeader = (byte)((bTileHeader & 0x9F) | 0x20);
		}
		else
		{
			bTileHeader &= 223;
		}
	}
	}
	public bool honey()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.honey"))
	{
		return (bTileHeader & 0x60) == 64;
	}
	}
	public void honey(bool honey)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.honey"))
	{
		if (honey)
		{
			bTileHeader = (byte)((bTileHeader & 0x9F) | 0x40);
		}
		else
		{
			bTileHeader &= 191;
		}
	}
	}
	public bool shimmer()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.shimmer"))
	{
		return (bTileHeader & 0x60) == 96;
	}
	}
	public void shimmer(bool shimmer)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.shimmer"))
	{
		if (shimmer)
		{
			bTileHeader = (byte)((bTileHeader & 0x9F) | 0x60);
		}
		else
		{
			bTileHeader &= 159;
		}
	}
	}
	public bool water()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.water"))
	{
		return liquidType() == 0;
	}
	}
	public bool anyWater()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.anyWater"))
	{
		if (liquid > 0)
		{
			return water();
		}
		return false;
	}
	}
	public bool anyLava()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.anyLava"))
	{
		if (liquid > 0)
		{
			return lava();
		}
		return false;
	}
	}
	public bool anyHoney()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.anyHoney"))
	{
		if (liquid > 0)
		{
			return honey();
		}
		return false;
	}
	}
	public bool anyShimmer()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.anyShimmer"))
	{
		if (liquid > 0)
		{
			return shimmer();
		}
		return false;
	}
	}
	public bool wire4()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wire4"))
	{
		return (bTileHeader & 0x80) == 128;
	}
	}
	public void wire4(bool wire4)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wire4"))
	{
		if (wire4)
		{
			bTileHeader |= 128;
		}
		else
		{
			bTileHeader &= 127;
		}
	}
	}
	public int wallFrameX()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wallFrameX"))
	{
		return (bTileHeader2 & 0xF) * 36;
	}
	}
	public void wallFrameX(int wallFrameX)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wallFrameX"))
	{
		bTileHeader2 = (byte)((bTileHeader2 & 0xF0) | ((wallFrameX / 36) & 0xF));
	}
	}
	public byte frameNumber()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.frameNumber"))
	{
		return (byte)((bTileHeader2 & 0x30) >> 4);
	}
	}
	public void frameNumber(byte frameNumber)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.frameNumber"))
	{
		bTileHeader2 = (byte)((bTileHeader2 & 0xCF) | ((frameNumber & 3) << 4));
	}
	}
	public byte wallFrameNumber()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wallFrameNumber"))
	{
		return (byte)((bTileHeader2 & 0xC0) >> 6);
	}
	}
	public void wallFrameNumber(byte wallFrameNumber)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wallFrameNumber"))
	{
		bTileHeader2 = (byte)((bTileHeader2 & 0x3F) | ((wallFrameNumber & 3) << 6));
	}
	}
	public int wallFrameY()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wallFrameY"))
	{
		return (bTileHeader3 & 7) * 36;
	}
	}
	public void wallFrameY(int wallFrameY)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wallFrameY"))
	{
		bTileHeader3 = (byte)((bTileHeader3 & 0xF8) | ((wallFrameY / 36) & 7));
	}
	}
	public bool checkingLiquid()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.checkingLiquid"))
	{
		return (bTileHeader3 & 8) == 8;
	}
	}
	public void checkingLiquid(bool checkingLiquid)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.checkingLiquid"))
	{
		if (checkingLiquid)
		{
			bTileHeader3 |= 8;
		}
		else
		{
			bTileHeader3 &= 247;
		}
	}
	}
	public bool skipLiquid()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.skipLiquid"))
	{
		return (bTileHeader3 & 0x10) == 16;
	}
	}
	public void skipLiquid(bool skipLiquid)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.skipLiquid"))
	{
		if (skipLiquid)
		{
			bTileHeader3 |= 16;
		}
		else
		{
			bTileHeader3 &= 239;
		}
	}
	}
	public bool invisibleBlock()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.invisibleBlock"))
	{
		return (bTileHeader3 & 0x20) == 32;
	}
	}
	public void invisibleBlock(bool invisibleBlock)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.invisibleBlock"))
	{
		if (invisibleBlock)
		{
			bTileHeader3 |= 32;
		}
		else
		{
			bTileHeader3 = (byte)(bTileHeader3 & -33);
		}
	}
	}
	public bool invisibleWall()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.invisibleWall"))
	{
		return (bTileHeader3 & 0x40) == 64;
	}
	}
	public void invisibleWall(bool invisibleWall)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.invisibleWall"))
	{
		if (invisibleWall)
		{
			bTileHeader3 |= 64;
		}
		else
		{
			bTileHeader3 = (byte)(bTileHeader3 & -65);
		}
	}
	}
	public bool fullbrightBlock()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.fullbrightBlock"))
	{
		return (bTileHeader3 & 0x80) == 128;
	}
	}
	public void fullbrightBlock(bool fullbrightBlock)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.fullbrightBlock"))
	{
		if (fullbrightBlock)
		{
			bTileHeader3 |= 128;
		}
		else
		{
			bTileHeader3 = (byte)(bTileHeader3 & -129);
		}
	}
	}
	public byte color()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.color"))
	{
		return (byte)(sTileHeader & 0x1F);
	}
	}
	public void color(byte color)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.color"))
	{
		sTileHeader = (ushort)((sTileHeader & 0xFFE0) | color);
	}
	}
	public bool active()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.active"))
	{
		return (sTileHeader & 0x20) == 32;
	}
	}
	public void active(bool active)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.active"))
	{
		if (active)
		{
			sTileHeader |= 32;
		}
		else
		{
			sTileHeader &= 65503;
		}
	}
	}
	public bool inActive()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.inActive"))
	{
		return (sTileHeader & 0x40) == 64;
	}
	}
	public void inActive(bool inActive)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.inActive"))
	{
		if (inActive)
		{
			sTileHeader |= 64;
		}
		else
		{
			sTileHeader &= 65471;
		}
	}
	}
	public bool wire()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wire"))
	{
		return (sTileHeader & 0x80) == 128;
	}
	}
	public void wire(bool wire)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wire"))
	{
		if (wire)
		{
			sTileHeader |= 128;
		}
		else
		{
			sTileHeader &= 65407;
		}
	}
	}
	public bool wire2()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wire2"))
	{
		return (sTileHeader & 0x100) == 256;
	}
	}
	public void wire2(bool wire2)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wire2"))
	{
		if (wire2)
		{
			sTileHeader |= 256;
		}
		else
		{
			sTileHeader &= 65279;
		}
	}
	}
	public bool wire3()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wire3"))
	{
		return (sTileHeader & 0x200) == 512;
	}
	}
	public void wire3(bool wire3)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.wire3"))
	{
		if (wire3)
		{
			sTileHeader |= 512;
		}
		else
		{
			sTileHeader &= 65023;
		}
	}
	}
	public bool halfBrick()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.halfBrick"))
	{
		return (sTileHeader & 0x400) == 1024;
	}
	}
	public void halfBrick(bool halfBrick)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.halfBrick"))
	{
		if (halfBrick)
		{
			sTileHeader |= 1024;
		}
		else
		{
			sTileHeader &= 64511;
		}
	}
	}
	public bool actuator()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.actuator"))
	{
		return (sTileHeader & 0x800) == 2048;
	}
	}
	public void actuator(bool actuator)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.actuator"))
	{
		if (actuator)
		{
			sTileHeader |= 2048;
		}
		else
		{
			sTileHeader &= 63487;
		}
	}
	}
	public byte slope()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.slope"))
	{
		return (byte)((sTileHeader & 0x7000) >> 12);
	}
	}
	public void slope(byte slope)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.slope"))
	{
		sTileHeader = (ushort)((sTileHeader & 0x8FFF) | ((slope & 7) << 12));
	}
	}
	public bool fullbrightWall()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.fullbrightWall"))
	{
		return (sTileHeader & 0x8000) == 32768;
	}
	}
	public void fullbrightWall(bool fullbrightWall)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.fullbrightWall"))
	{
		if (fullbrightWall)
		{
			sTileHeader |= 32768;
		}
		else
		{
			sTileHeader = (ushort)(sTileHeader & -32769);
		}
	}
	}
	public bool anyWire()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.anyWire"))
	{
		if ((sTileHeader & 0x380) == 0)
		{
			return (bTileHeader & 0x80) != 0;
		}
		return true;
	}
	}
	public void Clear(TileDataType types)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.Clear"))
	{
		if ((types & TileDataType.Tile) != 0)
		{
			type = 0;
			active(active: false);
			frameX = 0;
			frameY = 0;
		}
		if ((types & TileDataType.Wall) != 0)
		{
			wall = 0;
			wallFrameX(0);
			wallFrameY(0);
		}
		if ((types & TileDataType.TilePaint) != 0)
		{
			ClearBlockPaintAndCoating();
		}
		if ((types & TileDataType.WallPaint) != 0)
		{
			ClearWallPaintAndCoating();
		}
		if ((types & TileDataType.Liquid) != 0)
		{
			liquid = 0;
			liquidType(0);
			checkingLiquid(checkingLiquid: false);
		}
		if ((types & TileDataType.Slope) != 0)
		{
			slope(0);
			halfBrick(halfBrick: false);
		}
		if ((types & TileDataType.Wiring) != 0)
		{
			wire(wire: false);
			wire2(wire2: false);
			wire3(wire3: false);
			wire4(wire4: false);
		}
		if ((types & TileDataType.Actuator) != 0)
		{
			actuator(actuator: false);
			inActive(inActive: false);
		}
	}
	}
	public static void SmoothSlope(int x, int y, bool applyToNeighbors = true, bool sync = false)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.SmoothSlope"))
	{
		if (applyToNeighbors)
		{
			SmoothSlope(x + 1, y, applyToNeighbors: false, sync);
			SmoothSlope(x - 1, y, applyToNeighbors: false, sync);
			SmoothSlope(x, y + 1, applyToNeighbors: false, sync);
			SmoothSlope(x, y - 1, applyToNeighbors: false, sync);
		}
		Tile tile = Main.tile[x, y];
		if (!WorldGen.CanPoundTile(x, y) || !WorldGen.SolidOrSlopedTile(x, y))
		{
			return;
		}
		bool flag = !WorldGen.TileEmpty(x, y - 1);
		bool flag2 = !WorldGen.SolidOrSlopedTile(x, y - 1) && flag;
		bool flag3 = WorldGen.SolidOrSlopedTile(x, y + 1);
		bool flag4 = WorldGen.SolidOrSlopedTile(x - 1, y);
		bool flag5 = WorldGen.SolidOrSlopedTile(x + 1, y);
		int num = ((flag ? 1 : 0) << 3) | ((flag3 ? 1 : 0) << 2) | ((flag4 ? 1 : 0) << 1) | (flag5 ? 1 : 0);
		bool flag6 = tile.halfBrick();
		int num2 = tile.slope();
		switch (num)
		{
		case 10:
			if (!flag2)
			{
				tile.halfBrick(halfBrick: false);
				tile.slope(3);
			}
			break;
		case 9:
			if (!flag2)
			{
				tile.halfBrick(halfBrick: false);
				tile.slope(4);
			}
			break;
		case 6:
			tile.halfBrick(halfBrick: false);
			tile.slope(1);
			break;
		case 5:
			tile.halfBrick(halfBrick: false);
			tile.slope(2);
			break;
		case 4:
			tile.slope(0);
			tile.halfBrick(halfBrick: true);
			break;
		default:
			tile.halfBrick(halfBrick: false);
			tile.slope(0);
			break;
		}
		if (sync)
		{
			int num3 = tile.slope();
			bool flag7 = flag6 != tile.halfBrick();
			bool flag8 = num2 != num3;
			if (flag7 && flag8)
			{
				NetMessage.SendData(17, -1, -1, null, 23, x, y, num3);
			}
			else if (flag7)
			{
				NetMessage.SendData(17, -1, -1, null, 7, x, y, 1f);
			}
			else if (flag8)
			{
				NetMessage.SendData(17, -1, -1, null, 14, x, y, num3);
			}
		}
	}
	}
	public void CopyPaintAndCoating(Tile other)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.CopyPaintAndCoating"))
	{
		color(other.color());
		invisibleBlock(other.invisibleBlock());
		fullbrightBlock(other.fullbrightBlock());
	}
	}
	public TileColorCache BlockColorAndCoating()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.BlockColorAndCoating"))
	{
		return new TileColorCache
		{
			Color = color(),
			FullBright = fullbrightBlock(),
			Invisible = invisibleBlock()
		};
	}
	}
	public TileColorCache WallColorAndCoating()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.WallColorAndCoating"))
	{
		return new TileColorCache
		{
			Color = wallColor(),
			FullBright = fullbrightWall(),
			Invisible = invisibleWall()
		};
	}
	}
	public void UseBlockColors(TileColorCache cache)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.UseBlockColors"))
	{
		cache.ApplyToBlock(this);
	}
	}
	public void UseWallColors(TileColorCache cache)
{
	using (new global::Terraria.CallTracker("Terraria.Tile.UseWallColors"))
	{
		cache.ApplyToWall(this);
	}
	}
	public void ClearBlockPaintAndCoating()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.ClearBlockPaintAndCoating"))
	{
		color(0);
		fullbrightBlock(fullbrightBlock: false);
		invisibleBlock(invisibleBlock: false);
	}
	}
	public void ClearWallPaintAndCoating()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.ClearWallPaintAndCoating"))
	{
		wallColor(0);
		fullbrightWall(fullbrightWall: false);
		invisibleWall(invisibleWall: false);
	}
	}
	public override string ToString()
{
	using (new global::Terraria.CallTracker("Terraria.Tile.ToString"))
	{
		return "Tile Type:" + type + " Active:" + active().ToString() + " Wall:" + wall + " Slope:" + slope() + " fX:" + frameX + " fY:" + frameY;
	}
	}}
