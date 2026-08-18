using System;
using System.Collections.Generic;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Player.Systems;

internal sealed class PlayerGravitySystem
{
  private const float GravityPerTick = -1.0f;

  public void Apply(Arch.Core.World world, IReadOnlyCollection<Entity> players)
  {
    foreach (Entity entity in players)
    {
      ref VelocityComponent velocity = ref world.Get<VelocityComponent>(entity);
      velocity.Y = Math.Max(velocity.Y + GravityPerTick, GravityPerTick);
    }
  }
}
