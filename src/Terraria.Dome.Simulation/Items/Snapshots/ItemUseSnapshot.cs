using Terraria.Dome.Simulation.Items.Components;

namespace Terraria.Dome.Simulation.Items.Snapshots;

public sealed class ItemUseSnapshot
{
  public ItemUseSnapshot(PlayerHandle player, ItemUseStateComponent state, long revision)
  {
    if (revision < 0)
    {
      throw new System.ArgumentOutOfRangeException(nameof(revision));
    }

    Player = player;
    State = state;
    Revision = revision;
  }

  public PlayerHandle Player { get; }

  public ItemUseStateComponent State { get; }

  public long Revision { get; }
}
