using System.Numerics;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Spawn facts emitted by a trusted world, AI, or gameplay producer.</summary>
public sealed class NpcSpawnRequest
{
  public NpcSpawnRequest(
short type, NpcStateUpdate initialState, NpcHomeState? home = null,
      int? maximumLifeOverride = null)
  {
    ArgumentNullException.ThrowIfNull(initialState);
    Type = type;
    InitialState = initialState;
    Home = home;
    MaximumLifeOverride = maximumLifeOverride;
  }

  /// <summary>Trusted runtime-only health override; never supplied by a client snapshot.</summary>
  public int? MaximumLifeOverride { get; }

  public short Type { get; }

  public NpcStateUpdate InitialState { get; }

  /// <summary>Required for town-like NPCs; otherwise null uses NPC.SetDefaults reset state.</summary>
  public NpcHomeState? Home { get; }
}
