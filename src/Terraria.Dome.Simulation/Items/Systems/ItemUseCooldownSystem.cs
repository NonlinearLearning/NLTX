using Terraria.Dome.Simulation.Items.Components;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class ItemUseCooldownSystem
{
  public void Tick(ref ItemUseStateComponent state)
  {
    if (state.CooldownTicks > 0)
    {
      state.CooldownTicks--;
    }

    if (state.AnimationTicks > 0)
    {
      state.AnimationTicks--;
    }

    if (state.AnimationTicks == 0)
    {
      state.IsChanneling = false;
    }
  }
}
