namespace MinimalImplementation.Combat.Npcs;

/// <summary>Source identifiers for G2 to translate into P07 Bestiary and Banner calls.</summary>
public sealed record NpcKillCredit(
    short NpcNetId, string PersistentNpcId, short BannerId, int KillsRequired) {
  internal static NpcKillCredit ForCoreType(short type) {
    // ContentSamples uses NPCID.Search names. BannerSystem.NPCtoBanner maps these
    // three types; their banner items (1683/1639/1701) use the default threshold 50.
    return type switch {
      1 => new NpcKillCredit(1, "BlueSlime", 69, 50),
      2 => new NpcKillCredit(2, "DemonEye", 25, 50),
      3 => new NpcKillCredit(3, "Zombie", 87, 50),
      _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
  }
}
