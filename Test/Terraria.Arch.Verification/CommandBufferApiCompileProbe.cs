using Arch.Buffer;
using Arch.Core;

namespace Terraria.Arch.Verification;

internal static class CommandBufferApiCompileProbe {
  private struct PositionComponent {
    public float X;
    public float Y;
  }

  private struct MarkerComponent {
  }

  private static void CompileCommandBufferSurface() {
    World world = World.Create();
    Entity target = world.Create(new[] { (ComponentType)typeof(PositionComponent) });
    CommandBuffer buffer = new(8);
    Entity buffered = buffer.Create(new[] { (ComponentType)typeof(PositionComponent) });
    buffer.Set(buffered, new PositionComponent { X = 1, Y = 2 });
    buffer.Add(buffered, new MarkerComponent());
    buffer.Remove<MarkerComponent>(buffered);
    buffer.Destroy(target);
    buffer.Playback(world, dispose: false);
    buffer.Dispose();
    World.Destroy(world);
  }
}
