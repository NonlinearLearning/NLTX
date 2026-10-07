using System;

using Terraria.Relationships;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹拥有者、数组槽位、网络身份和 UUID 索引。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：owner（第 126 行）； identity（第 168 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 17 行。</para>
/// </remarks>
public struct ProjectileIdentityComponent
{
  public ProjectileIdentityComponent()
  {
    OwnerReference = EntityReference.None;
    OwnerSlot = byte.MaxValue;
    SlotIndex = -1;
    Identity = 0;
    ProjectileUuid = -1;
  }

  public ProjectileIdentityComponent(
    EntityReference ownerReference,
    int slotIndex = -1,
    int identity = 0,
    int projectileUuid = -1,
    int ownerSlot = byte.MaxValue)
  {
    if (slotIndex < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(slotIndex));
    }

    if (identity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(identity));
    }

    if (projectileUuid < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileUuid));
    }

    if ((uint)ownerSlot > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(ownerSlot));
    }

    OwnerReference = ownerReference;
    OwnerSlot = ownerSlot;
    SlotIndex = slotIndex;
    Identity = identity;
    ProjectileUuid = projectileUuid;
  }

  public EntityReference OwnerReference;
  public int OwnerSlot;
  public int SlotIndex;
  public int Identity;
  public int ProjectileUuid;

  public ProjectileOwnerReference Owner =>
    new(OwnerReference, OwnerSlot);

  public bool HasProjectileSlot => SlotIndex >= 0;

  public bool HasProjectileUuid => ProjectileUuid >= 0;

  public bool MatchesOwnerAndIdentity(int ownerSlot, int identity)
  {
    return (uint)ownerSlot <= byte.MaxValue &&
      identity >= 0 &&
      OwnerSlot == ownerSlot &&
      Identity == identity;
  }

  public bool MatchesNetworkIdentity(
    int ownerSlot,
    int identity,
    int projectileUuid)
  {
    if (!MatchesOwnerAndIdentity(ownerSlot, identity))
    {
      return false;
    }

    return projectileUuid < 0 ||
      ProjectileUuid < 0 ||
      ProjectileUuid == projectileUuid;
  }
}
