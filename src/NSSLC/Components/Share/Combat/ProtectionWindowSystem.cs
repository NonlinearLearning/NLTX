using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Combat;

/// <summary>Advances shared target protection windows once per simulation tick.</summary>
public sealed class ProtectionWindowSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly QueryDescription _query = new QueryDescription()
      .WithAll<ProtectionWindowComponent>();

  public ProtectionWindowSystem(ArchWorld world) : base(world)
  {
  }

  public override void Update(in EcsServerTickContext context)
  {
    World.Query(
        in _query,
        static (ref ProtectionWindowComponent protection) => protection.Advance());
  }
}
