using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Movement.Systems;

internal sealed class MovementSystem
{
  public void Apply(Arch.Core.World world, in QueryDescription query)
  {
    world.Query(
      in query,
      (Entity entity, ref TransformComponent transform, ref VelocityComponent velocity) =>
      {
        transform.X += velocity.X;
        transform.Y += velocity.Y;
      });
  }
}
