using System;
using System.Collections.Generic;
using NSSLC.WorldGeneration.Geometry;
using NSSLC.WorldGeneration.DataStructures;
using NSSLC.WorldGeneration.ID;
using NSSLC.WorldGeneration.ObjectData;
namespace NSSLC.WorldGeneration;
public class Chest {
  public Item[] item;
  public int maxItems;
  public int x;
  public int y;
  public int index;
  public bool bankChest;
  public string name;
  private bool _itemsGotSet;
  private static Dictionary<Point, Chest> _chestsByCoords = new();
  public static int[] chestTypeToIcon = new int[52];
  public static int[] chestTypeToIcon2 = new int[38];
  public Chest(int index = -1, int x = 0, int y = 0, bool bank = false, int maxItems = 40) {
    this.index = index;
    this.x = x;
    this.y = y;
    this.maxItems = maxItems;
    bankChest = bank;
    item = new Item[maxItems];
    FillWithEmptyInstances();
  }
  public static Chest Create(int x, int y) {
    int index = CreateChest(x, y);
    return index >= 0 ? Main.chest[index] : null;
  }
  public static void SetupTravelShop() { }
	public static void Clear()
{
	
		Array.Clear(Main.chest, 0, Main.chest.Length);
		_chestsByCoords.Clear();
	
	}
	public static Chest CreateWorldChest(int index, int x, int y)
{
	
		Chest chest = new Chest(index, x, y);
		chest.FillWithEmptyInstances();
		Assign(chest);
		return chest;
	
	}
	public static void Assign(Chest chest)
{
	
		Main.chest[chest.index] = chest;
		_chestsByCoords[new Point(chest.x, chest.y)] = chest;
	
	}
	public void Resize(int newSize)
{
	
		int num = maxItems;
		maxItems = newSize;
		Array.Resize(ref item, newSize);
		if (_itemsGotSet)
		{
			for (int i = num; i < newSize; i++)
			{
				item[i] = new Item();
			}
		}
	
	}
	public static void RemoveChest(int chestIndex)
{
	
		Chest chest = Main.chest[chestIndex];
		if (chest != null)
		{
			_chestsByCoords.Remove(new Point(chest.x, chest.y));
		}
		Main.chest[chestIndex] = null;
	
	}
	public static Chest CreateOutOfArray(int index, int x, int y, int maxItems)
{
	
		return new Chest(index, x, y, bank: false, maxItems);
	
	}
	public void FillWithEmptyInstances()
{
	
		for (int i = 0; i < maxItems; i++)
		{
			item[i] = new Item();
		}
		_itemsGotSet = true;
	
	}
	public static int FindChest(int X, int Y)
{
	
		if (_chestsByCoords.TryGetValue(new Point(X, Y), out var value))
		{
			return value.index;
		}
		return -1;
	
	}
	public static int FindEmptyChest(int x, int y, int type = 21, int style = 0, int direction = 1, int alternate = 0)
{
	
		int num = -1;
		for (int i = 0; i < 8000; i++)
		{
			Chest chest = Main.chest[i];
			if (chest != null)
			{
				if (chest.x == x && chest.y == y)
				{
					return -1;
				}
			}
			else if (num == -1)
			{
				num = i;
			}
		}
		return num;
	
	}
	public static bool NearOtherChests(int x, int y)
{
	
		for (int i = x - 25; i < x + 25; i++)
		{
			for (int j = y - 8; j < y + 8; j++)
			{
				Tile tileSafely = Framing.GetTileSafely(i, j);
				if (tileSafely.active() && TileID.Sets.BasicChest[tileSafely.type])
				{
					return true;
				}
			}
		}
		return false;
	
	}
	public static int AfterPlacement_Hook(int x, int y, int type = 21, int style = 0, int direction = 1, int alternate = 0)
{
	
		Point16 baseCoords = new Point16(x, y);
		TileObjectData.OriginToTopLeft(type, style, ref baseCoords);
		int num = FindEmptyChest(baseCoords.X, baseCoords.Y);
		if (num == -1)
		{
			return -1;
		}
		if (Main.netMode != 1)
		{
			CreateWorldChest(num, baseCoords.X, baseCoords.Y);
		}
		else
		{
			switch (type)
			{
			case 21:
				NetMessage.SendData(34, -1, -1, null, 0, x, y, style);
				break;
			case 467:
				NetMessage.SendData(34, -1, -1, null, 4, x, y, style);
				break;
			default:
				NetMessage.SendData(34, -1, -1, null, 2, x, y, style);
				break;
			}
		}
		return num;
	
	}
	public static int CreateChest(int X, int Y, int id = -1)
{
	
		int num = id;
		if (num == -1)
		{
			num = FindEmptyChest(X, Y);
			if (num == -1)
			{
				return -1;
			}
			if (Main.netMode == 1)
			{
				return num;
			}
		}
		CreateWorldChest(num, X, Y);
		return num;
	
	}
	public static bool CanDestroyChest(int X, int Y)
{
	
		if (!_chestsByCoords.TryGetValue(new Point(X, Y), out var value))
		{
			return true;
		}
		for (int i = 0; i < value.maxItems; i++)
		{
			if (value.item[i] != null && value.item[i].type > 0 && value.item[i].stack > 0)
			{
				return false;
			}
		}
		return true;
	
	}
	public static bool DestroyChest(int X, int Y)
{
	
		if (!_chestsByCoords.TryGetValue(new Point(X, Y), out var value))
		{
			return true;
		}
		for (int i = 0; i < value.maxItems; i++)
		{
			if (value.item[i] != null && value.item[i].type > 0 && value.item[i].stack > 0)
			{
				return false;
			}
		}
		int num = value.index;
		RemoveChest(num);
		if (Main.player[Main.myPlayer].chest == num)
		{
			Main.player[Main.myPlayer].chest = -1;
		}
		return true;
	
	}
	public static bool IsLocked(int x, int y)
{
	
		return IsLocked(Main.tile[x, y]);
	
	}
	public static bool IsLocked(Tile t)
{
	
		if (t == null)
		{
			return true;
		}
		if (t.type == 21 && ((t.frameX >= 72 && t.frameX <= 106) || (t.frameX >= 144 && t.frameX <= 178) || (t.frameX >= 828 && t.frameX <= 1006) || (t.frameX >= 1296 && t.frameX <= 1330) || (t.frameX >= 1368 && t.frameX <= 1402) || (t.frameX >= 1440 && t.frameX <= 1474)))
		{
			return true;
		}
		if (t.type == 467)
		{
			return t.frameX / 36 == 13;
		}
		return false;
	
	}
}
