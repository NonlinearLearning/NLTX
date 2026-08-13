using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Player.Systems;

internal sealed class PlayerGravitySystem
{
  private const float GravityPerTick = -1.0f;

  public void Apply(World world, IReadOnlyCollection<Entity> players)
  {
    foreach (Entity entity in players)
    {
      ref VelocityComponent velocity = ref world.Get<VelocityComponent>(entity);
      velocity.Y += GravityPerTick;
    }
  }
}
