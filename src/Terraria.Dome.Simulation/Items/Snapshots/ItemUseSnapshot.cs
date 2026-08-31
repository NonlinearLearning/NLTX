using Terraria.Dome.Simulation.Items.Components;

namespace Terraria.Dome.Simulation.Items.Snapshots;

public sealed class ItemUseSnapshot
{
  public ItemUseSnapshot(
    PlayerHandle player,
    ItemUseStateComponent state,
    long revision,
    int potionDelayTicks = 0)
  {
    if (!player.IsValid || revision < 0 || state.CooldownTicks < 0 || state.AnimationTicks < 0 ||
        state.UseRevision < 0 || potionDelayTicks < 0)
    {
      throw new System.ArgumentOutOfRangeException(nameof(revision));
    }

    Player = player;
    State = state;
    Revision = revision;
    PotionDelayTicks = potionDelayTicks;
  }

  public PlayerHandle Player { get; }

  public ItemUseStateComponent State { get; }

  public int PotionDelayTicks { get; }

  public long Revision { get; }
}
