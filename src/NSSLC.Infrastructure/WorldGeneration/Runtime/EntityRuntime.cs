using System;
using System.Linq;
using NSSLC.WorldGeneration.Geometry;

namespace NSSLC.WorldGeneration;

public partial class Player {
  public bool ghost;
  public bool fireWalk;
  public Rectangle Hitbox => new Rectangle((int)position.X, (int)position.Y, width, height);
  public int whoAmI;
  public int chest = -1;
  public string name = "";
  public int selectedItem;
  public int statLife = 100;
  public int statLifeMax2 = 100;
  public Item[] inventory = Enumerable.Range(0, 59).Select(index => new Item()).ToArray();
  public bool HasItem(int type) => inventory.Any(item => item.type == type && item.stack > 0);
  public void InterruptItemUsageIfOverTile(int tileType) { }
}

public partial class NPC {
  public float[] ai = new float[4];
  internal static int GetHousingCategoryForType(int npcType) {
    // Version4 assigns category 1 to exactly the town-pet NPC types.
    return npcType >= 0 && npcType < NPCID.Count && NPCID.Sets.IsTownPet[npcType] ? 1 : 0;
  }

  public string TypeName => "NPC." + type;
  public Vector2 Bottom {
    get => position + new Vector2(width / 2f, height);
    set => position = value - new Vector2(width / 2f, height);
  }
  public bool isLikeATownNPC => townNPC;
  public static bool TooWindyForButterflies => Math.Abs(Main.windSpeedTarget) >= 0.4;

  public static void setFireFlyChance() {
    if (Main.rand == null) {
      Main.rand = new Utilities.UnifiedRandom((int)DateTime.Now.Ticks);
    }

    if (WorldGen.genRand.Next(9) == 0) {
      fireFlyChance = Main.rand.Next(5, 10);
      fireFlyFriendly = Main.rand.Next(1, 4);
      fireFlyMultiple = Main.rand.Next(3, 8);
    } else if (WorldGen.genRand.Next(3) == 0) {
      fireFlyChance = 999999;
      fireFlyFriendly = 999999;
      fireFlyMultiple = 999999;
    } else {
      fireFlyChance = Main.rand.Next(10, 60);
      fireFlyFriendly = Main.rand.Next(2, 15);
      fireFlyMultiple = Main.rand.Next(6, 30);
    }

    if (Main.rand.Next(3) == 0) {
      butterflyChance = 999999;
      stinkBugChance = Main.rand.Next(1, 14);
      if (WorldGen.genRand.Next(5) == 0) {
        stinkBugChance = 999999;
      }
    } else {
      stinkBugChance = 999999;
      butterflyChance = Main.rand.Next(1, 21);
      if (WorldGen.genRand.Next(5) == 0) {
        butterflyChance = 999999;
      }
    }

    if (Main.tenthAnniversaryWorld) {
      fireFlyChance = Math.Max(1, (int)(fireFlyChance * 0.5f));
      butterflyChance = Math.Max(1, (int)(butterflyChance * 0.5f));
    }
  }

  public static void SetWorldSpecificMonstersByWorldID() {
    var unifiedRandom = new Utilities.UnifiedRandom(Main.worldID);
    int firstCavernGroup = unifiedRandom.Next(3);
    int secondCavernGroup = unifiedRandom.Next(3);
    while (firstCavernGroup == secondCavernGroup) {
      firstCavernGroup = unifiedRandom.Next(3);
      secondCavernGroup = unifiedRandom.Next(3);
    }

    for (int version = 0; version < 2; version++) {
      int cavernGroup = version == 0 ? firstCavernGroup : secondCavernGroup;
      for (int index = 0; index < 3; index++) {
        cavernMonsterType[version, index] = cavernGroup switch {
          0 => unifiedRandom.Next(494, 496),
          1 => unifiedRandom.Next(496, 498),
          _ => unifiedRandom.Next(498, 507)
        };
      }
    }
  }

  public static bool AnyDanger(bool quick = false, bool ignorePillars = false) => false;

  public static int NewNPC(DataStructures.IEntitySource source, int X, int Y, int Type,
                           int Start = 0, float ai0 = 0, float ai1 = 0, float ai2 = 0,
                           float ai3 = 0, int Target = 255) {
    for (int index = Start; index < Main.npc.Length; index++) {
      NPC npc = Main.npc[index];
      if (!npc.active) {
        npc.active = true;
        npc.type = Type;
        npc.housingCategory = GetHousingCategoryForType(Type);
        npc.width = 18;
        npc.height = 40;
        npc.life = 250;
        npc.townNPC = true;
        npc.homeless = true;
        npc.position = new Vector2(X - npc.width / 2, Y - npc.height);
        npc.ai[0] = ai0;
        npc.ai[1] = ai1;
        npc.ai[2] = ai2;
        npc.ai[3] = ai3;
        npc.GivenName = TypeNameFor(Type);
        return index;
      }
    }
    throw new InvalidOperationException("The generated world's NPC capacity was exceeded.");
  }

  private static string TypeNameFor(int type) => "NPC." + type;
}

public partial class Netplay {
  public static int GetSectionX(int x) => x / 200;
  public static int GetSectionY(int y) => y / 150;
  public static dynamic[] Clients = new dynamic[256];
}

public partial class AchievementsHelper {
  public static void NotifyTileDestroyed(params object[] arguments) { }
}

public partial class SoundID {
  public const int LiquidsWaterLava = 19;
  public const int LiquidsHoneyWater = 19;
  public const int LiquidsHoneyLava = 19;
}
