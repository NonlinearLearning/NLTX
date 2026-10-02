namespace Terraria.EntityLifecycleAttribution;

public sealed class EntitySpawnCommitSystem
{
  private readonly EntityPoolSet _pools;

  public EntitySpawnCommitSystem(EntityPoolSet pools)
  {
    _pools = pools ?? throw new ArgumentNullException(nameof(pools));
  }

  public bool TryCommit(AllocateEntitySlotCommand command, out EntitySlotHandle handle)
  {
    ArgumentNullException.ThrowIfNull(command);
    handle = default;

    return command.PoolKind switch
    {
      EntitySlotPoolKind.WorldItem when command.WorldItem is not null =>
        _pools.WorldItems.TryAllocate(command.RuntimeEntityId, command.WorldItem, out handle),
      EntitySlotPoolKind.Npc =>
        _pools.Npcs.TryAllocate(command.RuntimeEntityId, out handle),
      EntitySlotPoolKind.Projectile when command.Owner is int owner &&
                                        command.Identity is int identity &&
                                        command.ProjectileType is int projectileType =>
        _pools.Projectiles.TryAllocate(
          command.RuntimeEntityId,
          owner,
          identity,
          projectileType,
          out handle),
      EntitySlotPoolKind.PresentationEffect when command.PresentationKind is PresentationEffectKind kind &&
                                                 command.PresentationLifetimeTicks is int lifetimeTicks =>
        _pools.PresentationEffects.TryAllocate(
          kind,
          command.RuntimeEntityId,
          lifetimeTicks,
          out handle),
      _ => false,
    };
  }
}
