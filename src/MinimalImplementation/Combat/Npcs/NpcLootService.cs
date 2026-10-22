namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// Migrated type 1/2/3 loot with optional global item rules and common money/heal drops.
/// Death events and item creation effects are not a complete NPCLoot migration.
/// </summary>
public static class NpcLootService {
  private const int ItemTypeCount = 6147;

  public static IReadOnlyList<NpcDropIntent> RollCore(
      NpcView npc, NpcLootConditions conditions, bool anyInteractions, INpcLootRandom random) {
    return Roll(npc, conditions, anyInteractions, null, null, random);
  }

  /// <summary>Core items followed by money and common life/mana drops, in that order.</summary>
  public static IReadOnlyList<NpcDropIntent> RollCoreWithCommonLoot(
      NpcView npc, NpcLootConditions conditions, bool anyInteractions,
      NpcCommonLootFacts commonLoot, INpcLootRandom random) {
    ArgumentNullException.ThrowIfNull(commonLoot);
    return Roll(npc, conditions, anyInteractions, commonLoot, null, random);
  }

  /// <summary>Global items, type-specific items, money, then common life/mana.</summary>
  public static IReadOnlyList<NpcDropIntent> RollCoreWithGlobalLoot(
      NpcView npc, NpcLootConditions conditions, bool anyInteractions,
      NpcCommonLootFacts commonLoot, NpcGlobalLootFacts globalLoot, INpcLootRandom random) {
    ArgumentNullException.ThrowIfNull(commonLoot);
    ArgumentNullException.ThrowIfNull(globalLoot);
    return Roll(npc, conditions, anyInteractions, commonLoot, globalLoot, random);
  }

  private static IReadOnlyList<NpcDropIntent> Roll(
      NpcView npc, NpcLootConditions conditions, bool anyInteractions,
      NpcCommonLootFacts? commonLoot, NpcGlobalLootFacts? globalLoot, INpcLootRandom random) {
    ArgumentNullException.ThrowIfNull(npc);
    ArgumentNullException.ThrowIfNull(conditions);
    ArgumentNullException.ThrowIfNull(random);
    if (npc.Type is not (1 or 2 or 3) || npc.Life != 0) {
      throw new ArgumentException("Core loot requires a dead type 1, 2, or 3 NPC.", nameof(npc));
    }
    if (commonLoot is not null && (commonLoot.Entity != npc.Entity ||
        commonLoot.SourceNpcRevision != npc.Revision || !commonLoot.HasValidNumbers)) {
      throw new ArgumentException(
          "Common loot requires matching death-time facts within the numeric envelope.",
          nameof(commonLoot));
    }
    if (globalLoot is not null && (globalLoot.Entity != npc.Entity ||
        globalLoot.SourceNpcRevision != npc.Revision || !globalLoot.HasValidNumbers)) {
      throw new ArgumentException(
          "Global loot requires matching death-time facts with finite arithmetic bounds.",
          nameof(globalLoot));
    }

    // NPCLoot evaluates the statue roll before AnyInteractions, including when false.
    float statueRarity = npc.Type switch { 2 => 0.05f, 3 => 0.2f, _ => -1f };
    if (npc.SpawnedFromStatue && statueRarity != -1f &&
        (random.NextFloat() >= statueRarity || !anyInteractions)) {
      return Array.Empty<NpcDropIntent>();
    }

    var drops = new List<NpcDropIntent>();
    if (commonLoot is not null && globalLoot is not null) {
      RollMechBossSpawners(drops, commonLoot.Value, globalLoot, random);
    }
    // SlimeBodyItem is the second global rule, before the remaining global items
    // and all type-specific entries. The legacy entries keep their original order.
    if (npc.Type == 1 && npc.Ai[1] > 0f && npc.Ai[1] < ItemTypeCount) {
      int itemType = (int)npc.Ai[1];
      (int minimum, int maximum) = SlimeBodyStack(itemType);
      drops.Add(new NpcDropIntent(itemType, random.Next(minimum, maximum + 1)));
    }
    if (commonLoot is not null && globalLoot is not null) {
      RollGlobalItemsAfterSlimeBody(drops, npc, commonLoot.Value, globalLoot, random);
    }
    switch (npc.Type) {
      case 1:
        int gelMultiplier = conditions.ShouldDropExtraGel ? 2 : 1;
        TryCommon(drops, random, 23, 1, gelMultiplier, 2 * gelMultiplier);
        TryCommon(drops, random, 1309, conditions.ExpertMode ? 7000 : 10000);
        break;
      case 2:
        if (!TryCommon(drops, random, 236, 100)) {
          TryCommon(drops, random, 38, 3);
        }
        if (conditions.EyeOfCthulhuDefeatedAndNoAltarsInWorld) {
          TryCommon(drops, random, 43, 50);
        }
        break;
      case 3:
        TryCommon(drops, random, 216, 50);
        TryCommon(drops, random, 1304, 250);
        TryCommon(drops, random, 5332, 1500);
        if (conditions.SkyblockIsUpNoSickle) {
          TryCommon(drops, random, 1786, 15);
        }
        break;
    }
    if (commonLoot is not null) {
      RollMoney(drops, commonLoot, random);
      RollCommonLifeAndMana(drops, npc, commonLoot, random);
    }
    return drops.AsReadOnly();
  }

  private static void RollMechBossSpawners(
      List<NpcDropIntent> drops, float value, NpcGlobalLootFacts facts, INpcLootRandom random) {
    if (value <= 0f || !facts.HardMode) {
      return;
    }
    // Each undefeated boss gets a separate luck roll. The first success ends this
    // rule, with no fixed-stack RNG draw (unlike CommonDrop).
    if (!facts.DownedMechBoss1 && random.RollLuck(2500) == 0) {
      drops.Add(new NpcDropIntent(556, 1));
      return;
    }
    if (!facts.DownedMechBoss2 && random.RollLuck(2500) == 0) {
      drops.Add(new NpcDropIntent(544, 1));
      return;
    }
    if (!facts.DownedMechBoss3 && random.RollLuck(2500) == 0) {
      drops.Add(new NpcDropIntent(557, 1));
    }
  }

  private static void RollGlobalItemsAfterSlimeBody(
      List<NpcDropIntent> drops, NpcView npc, float value,
      NpcGlobalLootFacts facts, INpcLootRandom random) {
    // Halloween's fallback chains only from FailedRandomRoll, not failed conditions.
    if (facts.Halloween && value > 0f && value < 500f * facts.EnemyMoneyDropMultiplier &&
        npc.Damage < 40f * facts.EnemyDamageMultiplier && npc.Defense < 20 &&
        !TryCommon(drops, random, 1825, 2000)) {
      TryCommon(drops, random, 1827, 2000);
    }

    // DontDropDungeonKeysOrSouls contains only type 23 in this source version.
    // None of the supported type 1/2/3 NPCs is excluded from the key rules.
    if (value > 0f && facts.HardMode) {
      if (facts.ZoneJungle) {
        TryCommon(drops, random, 1533, 2500);
      }
      if (facts.ZoneCorrupt) {
        TryCommon(drops, random, 1534, 2500);
      }
      if (facts.ZoneCrimson) {
        TryCommon(drops, random, 1535, 2500);
      }
      if (facts.ZoneHallow) {
        TryCommon(drops, random, 1536, 2500);
      }
      if (facts.ZoneSnow) {
        TryCommon(drops, random, 1537, 2500);
      }
      if (facts.ZoneDesert && !facts.ZoneBeach) {
        TryCommon(drops, random, 4714, 2500);
      }
    }

    // The gift rules additionally exclude types 23/121, outside this entry's catalog.
    if (npc.MaximumLife > 1 && npc.Damage > 0 && !facts.Friendly && value > 0f) {
      if (facts.Halloween) {
        TryCommon(drops, random, 1774, 80);
      }
      if (facts.Christmas) {
        TryCommon(drops, random, 1869, 13);
      }
    }
    float topTiles = npc.Position.Y / 16f;
    if (npc.MaximumLife > 5 && value > 0f && !facts.Friendly && facts.HardMode &&
        topTiles > (float)facts.UnderworldLayerTiles) {
      TryCommon(drops, random, 2701, 50, 20, 50);
    }

    // BlueSlime is explicitly excluded from souls, independently of the ID set.
    if (!npc.IsBoss && npc.Type != 1 && facts.HardMode && npc.MaximumLife > 1 &&
        !facts.Friendly && value >= 1f &&
        (facts.RemixWorld || npc.Position.Y > facts.RockLayerTiles * 16d)) {
      if (facts.ZoneHallow) {
        TryCommon(drops, random, 520, 5);
      }
      if (facts.ZoneCorrupt || facts.ZoneCrimson) {
        TryCommon(drops, random, 521, 5);
      }
    }

    float centerTilesX = (npc.Position.X + npc.Width / 2f) / 16f;
    if (value > 0f && facts.HardMode && topTiles < facts.WorldSurfaceTiles + 10d &&
        (centerTilesX < 380f || centerTilesX > (float)(facts.MaximumTilesX - 380))) {
      TryCommon(drops, random, 1315, 100);
    }

    // RegisterYoyos appends these five global rules after PirateMap, before the
    // resolver appends any type-specific entries. HasPlayerTarget is target < 255.
    if (npc.Target < 255 && npc.MaximumLife > 5 && !facts.Friendly && value > 0f) {
      if (!facts.HardMode && topTiles > (float)(facts.MaximumTilesY - 350) &&
          facts.DownedBoss3) {
        TryCommon(drops, random, 3282, 400);
      }
      if (facts.HardMode && facts.ZoneSnow) {
        TryCommon(drops, random, 3289, 300);
      }
      if (facts.HardMode && facts.ZoneJungle && facts.DownedMechBossAny) {
        TryCommon(drops, random, 3286, 200);
      }
      if (facts.HardMode && facts.ZoneDungeon && facts.DownedPlantBoss) {
        TryCommon(drops, random, 3291, 400);
      }
      if (facts.HardMode && !facts.ZoneDungeon &&
          topTiles > (facts.RockLayerTiles + facts.MaximumTilesY * 2d) / 3d) {
        TryCommon(drops, random, 3290, 400);
      }
    }
  }

  private static void RollMoney(
      List<NpcDropIntent> drops, NpcCommonLootFacts facts, INpcLootRandom random) {
    float luck = facts.ClosestPlayerLuck;
    int attempts = random.NextFloat() < Math.Abs(luck) ? 2 : 1;
    float amount = 0f;
    for (int attempt = 0; attempt < attempts; attempt++) {
      float candidate = facts.Value;
      if (facts.Midas) {
        candidate *= 1f + random.Next(10, 51) * 0.01f;
      }
      candidate *= 1f + random.Next(-20, 76) * 0.01f;
      candidate = ApplyMoneyBonus(candidate, 2, 5, 10, random);
      candidate = ApplyMoneyBonus(candidate, 4, 10, 20, random);
      candidate = ApplyMoneyBonus(candidate, 8, 15, 30, random);
      candidate = ApplyMoneyBonus(candidate, 16, 20, 40, random);
      candidate = ApplyMoneyBonus(candidate, 32, 25, 50, random);
      candidate = ApplyMoneyBonus(candidate, 64, 50, 100, random);
      if (facts.BloodMoon) {
        candidate *= 1f + random.Next(0, 101) * 0.01f;
      }
      if (attempt == 0 || (luck < 0f ? candidate < amount : candidate > amount)) {
        amount = candidate;
      }
    }

    amount += facts.ExtraValue;
    while ((int)amount > 0) {
      // The source uses strict thresholds: exactly 100 copper remains a copper stack.
      int unit = amount > 1000000f ? 1000000 : amount > 10000f ? 10000 :
          amount > 100f ? 100 : 1;
      int itemType = unit switch { 1000000 => 74, 10000 => 73, 100 => 72, _ => 71 };
      int stack = (int)(amount / unit);
      if (stack > 50 && random.Next(0, 5) == 0) {
        stack /= random.Next(0, 3) + 1;
      }
      if (random.Next(0, 5) == 0) {
        stack /= random.Next(0, itemType == 71 ? 4 : 3) + 1;
      }
      if (itemType == 71 && stack < 1) {
        stack = 1;
      }

      int remainingStack = stack;
      if (itemType == 74) {
        while (remainingStack > 999) {
          drops.Add(new NpcDropIntent(itemType, 999));
          remainingStack -= 999;
        }
      }
      amount -= unit * stack;
      // Item.NewItem ignores nonpositive stacks. Preserve the source's next loop
      // and random draws when integer splitting temporarily produces zero coins.
      if (remainingStack > 0) {
        drops.Add(new NpcDropIntent(itemType, remainingStack));
      }
    }
  }

  private static float ApplyMoneyBonus(
      float amount, int denominator, int minimum, int maximum, INpcLootRandom random) {
    if (random.Next(0, denominator) == 0) {
      amount *= 1f + random.Next(minimum, maximum + 1) * 0.01f;
    }
    return amount;
  }

  private static void RollCommonLifeAndMana(
      List<NpcDropIntent> drops, NpcView npc, NpcCommonLootFacts facts, INpcLootRandom random) {
    // Keep the luck draws before the NPC checks, and the second Next(2) independent
    // when mana is already full. A missing mana drop can still lead to a heart.
    if (random.RollLuck(6) == 0 && npc.MaximumLife > 1 && npc.Damage > 0) {
      if (random.Next(0, 2) == 0 && facts.ClosestPlayerMana < facts.ClosestPlayerMaximumMana) {
        drops.Add(new NpcDropIntent(184, 1));
      } else if (random.Next(0, 2) == 0 &&
          facts.ClosestPlayerLife < facts.ClosestPlayerMaximumLife) {
        drops.Add(new NpcDropIntent(58, 1));
      }
    }
    if (random.RollLuck(2) == 0 && npc.MaximumLife > 1 && npc.Damage > 0 &&
        facts.ClosestPlayerMana < facts.ClosestPlayerMaximumMana) {
      drops.Add(new NpcDropIntent(184, 1));
    }
  }

  private static bool TryCommon(
      List<NpcDropIntent> drops, INpcLootRandom random, int itemType,
      int denominator, int minimum = 1, int maximum = 1) {
    if (random.RollLuck(denominator) >= 1) {
      return false;
    }
    // CommonDrop consumes a stack roll even for a fixed-size stack.
    drops.Add(new NpcDropIntent(itemType, random.Next(minimum, maximum + 1)));
    return true;
  }

  private static (int Minimum, int Maximum) SlimeBodyStack(int itemType) {
    return itemType switch {
      8 => (5, 10),
      166 => (2, 6),
      965 => (20, 45),
      11 or 12 or 13 or 14 or 174 or 364 or 365 or 366 or 699 or 700 or 701 or 702 or
          1104 or 1105 or 1106 or 3347 => (3, 13),
      71 => (50, 99),
      72 => (20, 99),
      73 => (1, 2),
      4343 or 4344 or 147 or 314 or 1124 or 1125 or 1345 or 3736 or 3737 or 3738 => (2, 5),
      2 or 3 or 9 or 150 or 593 or 751 or 1103 or 3081 or 3086 or 3609 or 3610 or
          5395 => (10, 25),
      _ => (1, 1)
    };
  }
}
