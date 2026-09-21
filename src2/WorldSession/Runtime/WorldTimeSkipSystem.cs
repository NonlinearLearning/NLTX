using Terraria.WorldSession.Components;

namespace Terraria.WorldSession.Runtime;

public sealed class WorldTimeSkipSystem
{
  public bool RequestDawn(WorldTimeSkipStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return state.TryRequestDawn();
  }

  public bool Apply(
    WorldTimeSkipStateComponent state,
    WorldTimeSkipCommand command)
  {
    ArgumentNullException.ThrowIfNull(state);
    return command.Direction switch
    {
      WorldTimeSkipDirection.Dawn => state.TryRequestDawn(),
      WorldTimeSkipDirection.Dusk => state.TryRequestDusk(),
      _ => throw new ArgumentOutOfRangeException(nameof(command))
    };
  }

  public bool RequestDusk(WorldTimeSkipStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return state.TryRequestDusk();
  }

  public void TickCooldowns(WorldTimeSkipStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.TickCooldowns();
  }

  public WorldTimeSkipSnapshot Consume(WorldTimeSkipStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return state.ConsumeSnapshot();
  }
}
