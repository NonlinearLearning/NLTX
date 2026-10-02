using System;

namespace Terraria.Npc;

public sealed class NpcParentHealthTarget
{
  public NpcParentHealthTarget(
    NpcEntityIdentityComponent identity,
    NpcTypeId npcType,
    NpcHealthComponent health,
    NpcLifecycleComponent lifecycle,
    bool isBoss)
  {
    ArgumentNullException.ThrowIfNull(identity);
    ArgumentNullException.ThrowIfNull(health);
    ArgumentNullException.ThrowIfNull(lifecycle);
    if (!identity.InstanceId.IsValid)
    {
      throw new ArgumentException(
        "The parent identity must refer to an initialized NPC.",
        nameof(identity));
    }

    if (!npcType.IsValid)
    {
      throw new ArgumentException(
        "The parent NPC type must be valid.",
        nameof(npcType));
    }

    Identity = identity;
    NpcType = npcType;
    Health = health;
    Lifecycle = lifecycle;
    IsBoss = isBoss;
  }

  public NpcEntityIdentityComponent Identity { get; }

  public NpcTypeId NpcType { get; }

  public NpcHealthComponent Health { get; }

  public NpcLifecycleComponent Lifecycle { get; }

  public bool IsBoss { get; }
}
