using System;
using NSSLC.WorldGeneration.Geometry;
using NSSLC.WorldGeneration.DataStructures;

namespace NSSLC.WorldGeneration;

/// <summary>Inventory state required by generated containers; no game simulation behavior.</summary>
public sealed partial class Item {
  public int type;
  public int stack;
  public byte prefix;
  public int rare;
  public int dye;
  public int whoAmI;
  public int fishingPole;
  public int axe;
  public int createTile = -1;
  public int placeStyle;
  public bool accessory;
  public bool melee;
  public bool vanity;
  public int damage;
  public int useAnimation;
  public int useTime;
  public int reuseDelay;
  public int mana;
  public int crit;
  public int bonusTagDamage;
  public int armorPenetration;
  public int value;
  public float knockBack;
  public float scale;
  public float shootSpeed;
  public static int[] bodyType = new int[7000];
  public static int[] headType = new int[7000];
  public static int[] legType = new int[7000];
  public bool IsAir => type <= 0 || stack <= 0;

  public void SetDefaults(int itemType, bool noMatCheck = false) {
    if (itemType < 0 || itemType >= ItemID.Count) {
      throw new ArgumentOutOfRangeException(nameof(itemType));
    }
    type = itemType;
    stack = itemType == 0 ? 0 : 1;
    prefix = 0;
    bonusTagDamage = 0;
    armorPenetration = 0;
    ItemCatalog.Apply(this);
    createTile = ItemID.Sets.DerivedPlacementDetails[itemType].tileType;
    placeStyle = ItemID.Sets.DerivedPlacementDetails[itemType].tileStyle;
  }

  public void netDefaults(int itemType) {
    SetDefaults(itemType);
  }

  public void TurnToAir() {
    type = 0;
    stack = 0;
    prefix = 0;
  }



  public Item Clone() => (Item)MemberwiseClone();
  public Item DeepClone() => Clone();

  public void OverrideWith(Item other) {
    type = other.type;
    stack = other.stack;
    prefix = other.prefix;
  }

  public static int NewItem(IEntitySource source, Vector2 position, Vector2 randomBox,
                           int Type, int Stack = 1, bool noBroadcast = false,
                           int prefixGiven = 0, bool noGrabDelay = false) {
    for (int index = 0; index < Main.item.Length; index++) {
      if (!Main.item[index].active) {
        WorldItem worldItem = Main.item[index];
        worldItem.inner.SetDefaults(Type);
        worldItem.inner.stack = Stack;
        worldItem.inner.prefix = (byte)prefixGiven;
        worldItem.type = Type;
        worldItem.stack = Stack;
        worldItem.active = true;
        worldItem.position = position;
        return index;
      }
    }
    throw new InvalidOperationException("The generated world's item capacity was exceeded.");
  }

  public static int NewItem(IEntitySource source, int X, int Y, int Width, int Height,
                           int Type, int Stack = 1, bool noBroadcast = false, int pfix = 0,
                           bool noGrabDelay = false) {
    return NewItem(source, new Vector2(X, Y), new Vector2(Width, Height), Type, Stack,
                   noBroadcast, pfix, noGrabDelay);
  }

	public static int GetRandomVoiceItem()
{
	
		return WorldGen.genRand.Next(14) switch
		{
			1 => 5500, 
			2 => 5501, 
			3 => 5502, 
			4 => 5503, 
			5 => 5504, 
			6 => 5505, 
			7 => 5506, 
			8 => 5507, 
			9 => 5508, 
			10 => 5509, 
			11 => 5484, 
			12 => 5485, 
			13 => 5534, 
			_ => 5499, 
		};
	
	}
}
