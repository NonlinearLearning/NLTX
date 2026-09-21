namespace Terraria.EntityLifecycleAttribution;

public sealed class EntityDespawnRecycleSystem
{
  private readonly EntityPoolSet _pools;

  public EntityDespawnRecycleSystem(EntityPoolSet pools)
  {
    _pools = pools ?? throw new ArgumentNullException(nameof(pools));
  }

  public bool TryCommit(ReleaseEntitySlotCommand command)
  {
    return command.PoolKind switch
    {
      EntitySlotPoolKind.WorldItem => _pools.WorldItems.TryRelease(command.Handle, command.ReuseDelayTicks),
      EntitySlotPoolKind.Npc => _pools.Npcs.TryRelease(command.Handle),
      EntitySlotPoolKind.Projectile => _pools.Projectiles.TryRelease(command.Handle),
      EntitySlotPoolKind.PresentationEffect when command.PresentationKind is PresentationEffectKind kind =>
        _pools.PresentationEffects.TryRelease(kind, command.Handle),
      _ => false,
    };
  }
}
