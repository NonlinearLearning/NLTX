namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Persisted Defender's Forge event progression from a WorldFile header continuation.
/// </summary>
public sealed class WorldFileDefenderEventSection
{
  public const string SectionId = "world.defender-event";

  public WorldFileDefenderEventSection(
    bool savedBartender,
    bool downedInvasionTier1,
    bool downedInvasionTier2,
    bool downedInvasionTier3)
  {
    SavedBartender = savedBartender;
    DownedInvasionTier1 = downedInvasionTier1;
    DownedInvasionTier2 = downedInvasionTier2;
    DownedInvasionTier3 = downedInvasionTier3;
  }

  public bool SavedBartender { get; }

  public bool DownedInvasionTier1 { get; }

  public bool DownedInvasionTier2 { get; }

  public bool DownedInvasionTier3 { get; }

  public static WorldFileDefenderEventSection Empty => new(
    savedBartender: false,
    downedInvasionTier1: false,
    downedInvasionTier2: false,
    downedInvasionTier3: false);
}
