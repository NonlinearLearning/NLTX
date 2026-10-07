namespace Terraria.Npc;

public static class NpcAiProfileCoverageBaseline
{
  public static NpcAiProfileCoverageRegistry CreateCurrentSnapshot(
    IEnumerable<int> indexedStyles)
  {
    NpcAiProfileCoverageRegistry registry = new(indexedStyles);
    RegisterMappedProfile(registry, "Blue Slime", new(1, 1, 1), hasFiniteHandler: true);
    RegisterMappedProfile(registry, "Demon Eye", new(2, 2, 2), hasFiniteHandler: true);
    RegisterMappedProfile(registry, "Zombie", new(3, 3, 3), hasFiniteHandler: true);
    RegisterMappedProfile(registry, "Mother Slime", new(16, 16, 1), hasFiniteHandler: true);
    RegisterMappedProfile(registry, "Eye of Cthulhu", new(4, 4, 4), hasFiniteHandler: true);
    RegisterMappedProfile(registry, "Servant of Cthulhu", new(5, 5, 5), hasFiniteHandler: true);
    RegisterMappedProfile(registry, "Lava Slime", new(59, 59, 1), hasFiniteHandler: false);
    RegisterMappedProfile(registry, "Guide", new(22, 22, 7), hasFiniteHandler: false);
    RegisterMappedProfile(registry, "Old Man", new(37, 37, 7), hasFiniteHandler: false);
    RegisterMappedProfile(registry, "Training Dummy", new(488, 488, 92), hasFiniteHandler: false);
    return registry;
  }

  private static void RegisterMappedProfile(
    NpcAiProfileCoverageRegistry registry,
    string name,
    NpcAiProfileIdentity identity,
    bool hasFiniteHandler)
  {
    registry.Register(new NpcAiProfileCoverageRegistration(
      name,
      identity,
      NpcAiCoverageStage.Mapped,
      hasFiniteHandler,
      HasOpenWork: true));
  }
}
