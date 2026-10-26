using System;
using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.Relationships;

namespace Terraria.Projectile;

/// <summary>Advances active projectile lifetime and records expiry for the lifecycle commit.</summary>
public sealed class ProjectileLifetimeTickSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      ProjectileLifetimeStateComponent,
      ProjectileDispositionStateComponent>();
  private readonly ProjectileTerminationBuffer _terminations;
  private readonly IEntityRuntimeHandleResolver _handles;

  public ProjectileLifetimeTickSystem(
      ArchWorld world,
      ProjectileTerminationBuffer terminations,
      IEntityRuntimeHandleResolver handles)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _terminations = terminations ?? throw new ArgumentNullException(nameof(terminations));
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    World.Query(
        in _query,
        (Arch.Core.Entity entity,
            ref ProjectileLifetimeStateComponent lifetime,
            ref ProjectileDispositionStateComponent disposition) =>
        {
          if (!disposition.IsActive)
          {
            return;
          }

          if (lifetime.IsExpired)
          {
            RequestExpiry(entity, tick, ref disposition);
            return;
          }

          lifetime = lifetime.Advance();
          if (lifetime.IsExpired)
          {
            RequestExpiry(entity, tick, ref disposition);
          }
        });
  }

  private void RequestExpiry(
      Arch.Core.Entity entity,
      long tick,
      ref ProjectileDispositionStateComponent disposition)
  {
    if (_handles.TryGetHandle(entity, out EntityRuntimeHandle handle))
    {
      _terminations.Enqueue(handle, ProjectileTerminationReason.Expired, tick);
    }
  }

}
