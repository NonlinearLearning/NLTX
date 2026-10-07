using Arch.Core;
using Arch.Core.Events;

namespace Terraria.Arch.Verification;

internal static class EventsApiCompileProbe {
  private static void CompileEventsSurface() {
    World world = World.Create();
    world.SubscribeEntityCreated((in Entity entity) => { _ = entity.Id; });
    world.SubscribeEntityDestroyed((in Entity entity) => { _ = entity.Version; });
    Entity created = world.Create();
    world.Destroy(created);
    World.Destroy(world);
  }

  private static void Main() { }
}
