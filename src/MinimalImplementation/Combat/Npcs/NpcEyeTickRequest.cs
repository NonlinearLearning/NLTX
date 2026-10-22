using MinimalImplementation.Combat.Identity;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Trusted world/player facts for one type-2 owner update.</summary>
public sealed class NpcEyeTickRequest {
  public NpcEyeTickRequest(
      CombatEntityIdentity entity, long expectedRevision, bool isDay,
      double worldSurfaceTiles, bool confused, IReadOnlyList<NpcTargetPlayer> players) {
    ArgumentNullException.ThrowIfNull(players);
    Entity = entity;
    ExpectedRevision = expectedRevision;
    IsDay = isDay;
    WorldSurfaceTiles = worldSurfaceTiles;
    Confused = confused;
    Players = Array.AsReadOnly(players.ToArray());
  }

  public CombatEntityIdentity Entity { get; }
  public long ExpectedRevision { get; }
  public bool IsDay { get; }
  public double WorldSurfaceTiles { get; }
  public bool Confused { get; }
  public IReadOnlyList<NpcTargetPlayer> Players { get; }
}
