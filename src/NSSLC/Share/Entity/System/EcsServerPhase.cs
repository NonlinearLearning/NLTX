namespace Terraria.Share.Entities;

/// <summary>
/// Ordered boundaries in one NSSLC ECS server update.
/// </summary>
public enum EcsServerPhase : byte
{
  /// <summary>Capture or derive inputs needed by later phases.</summary>
  Preparation = 0,

  /// <summary>Run continuous domain simulation and update owned component values.</summary>
  Simulation = 1,

  /// <summary>Resolve validated results before the next server observation point.</summary>
  Commit = 2,

  /// <summary>Build read-only projections or outbound effects from committed state.</summary>
  Projection = 3,
}
