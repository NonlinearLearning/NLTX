using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Share.Combat;

/// <summary>Drops expired per-attacker immunity windows once per simulation tick.</summary>
/// <remarks>
/// 稀疏表没有上限增长的保护，所以 Prune 必须无条件每 tick 跑一次，而不是等某个消费者来触发。
/// </remarks>
public sealed class AttackerImmunityExpirySystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly QueryDescription _query = new QueryDescription()
      .WithAll<AttackerImmunityComponent>();

  public AttackerImmunityExpirySystem(ArchWorld world) : base(world)
  {
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    World.Query(
        in _query,
        (ref AttackerImmunityComponent windows) => windows.Prune(tick));
  }
}
