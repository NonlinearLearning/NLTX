namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Persisted combat-book and Lantern Night state following WorldFile backgrounds.
/// </summary>
public sealed class WorldFileEventSection
{
  public const string SectionId = "world.event-flags";

  public WorldFileEventSection(
    bool combatBookWasUsed,
    int lanternNightCooldown,
    bool lanternNightGenuine,
    bool lanternNightManual,
    bool lanternNightNextNightIsGenuine)
  {
    CombatBookWasUsed = combatBookWasUsed;
    LanternNightCooldown = lanternNightCooldown;
    LanternNightGenuine = lanternNightGenuine;
    LanternNightManual = lanternNightManual;
    LanternNightNextNightIsGenuine = lanternNightNextNightIsGenuine;
  }

  public bool CombatBookWasUsed { get; }

  public int LanternNightCooldown { get; }

  public bool LanternNightGenuine { get; }

  public bool LanternNightManual { get; }

  public bool LanternNightNextNightIsGenuine { get; }

  public static WorldFileEventSection Empty => new(
    combatBookWasUsed: false,
    lanternNightCooldown: 0,
    lanternNightGenuine: false,
    lanternNightManual: false,
    lanternNightNextNightIsGenuine: false);
}
