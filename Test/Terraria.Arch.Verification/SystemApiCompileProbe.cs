using Arch.Core;
using Arch.System;

namespace Terraria.Arch.Verification;

internal readonly struct TickContext {
  public TickContext(World world, long tick) {
    World = world;
    Tick = tick;
  }

  public World World { get; }
  public long Tick { get; }
}

internal sealed class InterfaceSystemProbe : ISystem<TickContext> {
  public void Initialize() { }
  public void BeforeUpdate(in TickContext context) { _ = context.Tick; }
  public void Update(in TickContext context) { _ = context.World; }
  public void AfterUpdate(in TickContext context) { _ = context.Tick; }
  public void Dispose() { }
}

internal sealed class BaseSystemProbe : BaseSystem<World, TickContext> {
  public BaseSystemProbe(World world) : base(world) { }
}

internal static class SystemApiCompileProbe {
  private static void CompileSystemSurface() {
    World world = World.Create();
    InterfaceSystemProbe interfaceSystem = new();
    BaseSystemProbe baseSystem = new(world);
    Group<TickContext> nested = new("nested", interfaceSystem);
    Group<TickContext> group = new("root", interfaceSystem, baseSystem, nested);
    TickContext context = new(world, 1);
    group.Initialize();
    group.BeforeUpdate(in context);
    group.Update(in context);
    group.AfterUpdate(in context);
    group.Dispose();
    World.Destroy(world);
  }
}

