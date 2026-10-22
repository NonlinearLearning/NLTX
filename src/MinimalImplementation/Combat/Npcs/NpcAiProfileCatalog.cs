namespace MinimalImplementation.Combat.Npcs;

/// <summary>Maps only the dispatch families implemented by the owner-thread NPC tick.</summary>
public static class NpcAiProfileCatalog
{
  private static readonly IReadOnlyDictionary<int, NpcAiProfile> Profiles =
    new Dictionary<int, NpcAiProfile>
    {
      [1] = new(1, NpcAiProfileKind.Slime, true, true,
        "NPC.AI_001_Slimes ordinary unscaled type 1; Web/Dart, balloon state and " +
        "special-world rules remain unsupported"),
      [2] = new(2, NpcAiProfileKind.Floating, true, true,
        "NPC.AI_002_FloatingEye ordinary type 2 at scale 1; other type/style branches remain unsupported"),
      [7] = new(7, NpcAiProfileKind.Passive, false, true,
        "AI_007_TownEntities ordinary homeless Guide idle/walk/flee subset"),
      [3] = new(3, NpcAiProfileKind.Fighter, true, true,
        "NPC.AI_003_Fighters ordinary unscaled type 3; doors/gates and special worlds " +
        "remain unsupported")
    };

  public static NpcAiProfile Resolve(int aiStyle)
  {
    return Profiles.TryGetValue(aiStyle, out NpcAiProfile? profile)
      ? profile
      : new NpcAiProfile(
        aiStyle,
        NpcAiProfileKind.Unsupported,
        UsesTarget: false,
        UsesTileCollision: false,
        Evidence: "No MinimalImplementation profile has been migrated for this Version4 dispatch style.");
  }

  public static bool TryGet(int aiStyle, out NpcAiProfile profile)
  {
    return Profiles.TryGetValue(aiStyle, out profile!);
  }
}
