namespace MinimalImplementation.Combat.Npcs;

internal static class NpcSlimeAiRules {
  // NPC.AI_001_Slimes and AI_001_Slimes_GenerateItemInsideBody, v1.4.5.6/319.
  // This is the ordinary type 1, netID 1, server, non-balloon branch.
  internal static int RollBody(
      NpcView npc, NpcSlimeEnvironment environment, INpcAiRandom random) {
    bool aboveCenter = npc.Position.Y + npc.Height / 2 < environment.WorldSurfaceTiles * 16d;
    int attempts = environment.SlimeRain ? 3 : 1;
    for (int attempt = 0; attempt < attempts; attempt++) {
      if (environment.GenuineParty && aboveCenter) {
        return random.Next(0, 2) == 0 ? random.Next(3736, 3739) : 1345;
      }
      if (random.Next(0, 20) == 0) {
        return RollGenericBody(random);
      }
      if (random.Next(0, 40) == 0) {
        if (npc.Position.Y / 16f <= environment.WorldSurfaceTiles) {
          return 751;
        }
        if (!environment.InRockLayer) {
          return random.Next(0, 3) switch { 1 => 3, 2 => 9, _ => 2 };
        }
        if (random.Next(0, 10) == 0) {
          return 3609;
        }
        return random.Next(0, 4) switch { 1 => 150, 2 => 3086, 3 => 3081, _ => 3 };
      }
      if (!aboveCenter && random.Next(0, 500) == 0) {
        return 539;
      }
    }
    return -1;
  }

  internal static bool SupportsBody(float item) {
    return item is -1f or 0f or 2f or 3f or 9f or 751f or 3086f or 3081f or 3609f or
        3736f or 3737f or 3738f or 1345f or 290f or 292f or 296f or 2322f or 2997f or
        2350f or 8f or 965f or 166f or 58f or 11f or 12f or 13f or 14f or 699f or
        700f or 701f or 702f or 71f or 72f or 73f;
  }

  internal static NpcCombatStats BodyStats(NpcDefinition definition, float item) {
    int defenseBonus = item switch {
      9f or 3086f or 3081f => 16,
      3609f => 8,
      _ => 0
    };
    return new NpcCombatStats(
        definition.Damage + (item == 3609f ? 6 : 0), definition.Defense + defenseBonus,
        item is 3086f or 3081f ? 0f : definition.KnockbackResist);
  }

  internal static (float Gravity, float MaximumFallSpeed) Gravity(
      float positionY, NpcSlimeEnvironment environment) {
    return NpcGravityRules.Calculate(positionY, environment.WorldSurfaceTiles,
        environment.MaximumTilesX, environment.WasWet, environment.WasHoneyWet,
        environment.WasShimmerWet);
  }

  private static int RollGenericBody(INpcAiRandom random) {
    switch (random.Next(0, 4)) {
      case 0:
        return random.Next(0, 7) switch {
          0 => 290,
          1 => 292,
          2 => 296,
          3 => 2322,
          _ => random.Next(0, 2) == 0 ? 2997 : 2350
        };
      case 1:
        return random.Next(0, 4) switch { 0 => 8, 1 => 965, 2 => 166, _ => 58 };
      case 2:
        return random.Next(0, 2) == 0 ? random.Next(11, 15) : random.Next(699, 703);
      default:
        return random.Next(0, 3) switch { 0 => 71, 1 => 72, _ => 73 };
    }
  }
}
