using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Entity;

namespace Terraria.Player;

/// <summary>
/// Commits player death and same-instance respawn state without owning spatial placement.
/// </summary>
public sealed class PlayerDeathRespawnSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly QueryDescription _query = new QueryDescription()
      .WithAll<LifeComponent, PlayerDeathStateComponent, PlayerRespawnStateComponent>();

  public PlayerDeathRespawnSystem(ArchWorld world) : base(world)
  {
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    World.Query(
        in _query,
        (ref LifeComponent life,
            ref PlayerDeathStateComponent death,
            ref PlayerRespawnStateComponent respawn) =>
        {
          if (!life.IsAlive && !death.IsDead)
          {
            death.MarkDead(tick);
            respawn.Schedule(tick);
            return;
          }

          if (life.IsAlive && death.IsDead)
          {
            return;
          }

          if (!death.IsDead || !respawn.IsScheduled || !respawn.Advance())
          {
            return;
          }

          life = life.WithCurrent(life.Maximum);
          death.Clear();
          respawn.Cancel();
        });
  }
}
