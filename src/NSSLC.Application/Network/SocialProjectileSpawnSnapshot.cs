using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.Network;

/// <summary>Detached state read from a projectile committed by the lifecycle owner.</summary>
public sealed class SocialProjectileSpawnSnapshot
{
  private readonly IReadOnlyList<bool> _sectionSyncSkippedForPlayer;

  public SocialProjectileSpawnSnapshot(
    RuntimeEntityHandle runtimeHandle,
    EntityReference entityReference,
    ProjectileSlot slot,
    uint slotGeneration,
    int ownerSlot,
    int identity,
    int? projectileUuid,
    int projectileType,
    Vector2 position,
    Vector2 velocity,
    int damage,
    int originalDamage,
    float knockback,
    float ai0,
    float ai1,
    float ai2,
    int bannerIdToRespondTo,
    int timeLeft,
    bool networkImportant,
    bool primaryUpdatePending,
    bool secondaryUpdatePending,
    int netSpam,
    bool sendRequested,
    IEnumerable<bool> sectionSyncSkippedForPlayer)
  {
    ArgumentNullException.ThrowIfNull(sectionSyncSkippedForPlayer);
    if (!runtimeHandle.IsAssigned || entityReference.IsEmpty ||
        entityReference.Scope != EntityReferenceScope.Projectile ||
        entityReference.RuntimeId != runtimeHandle.RuntimeId ||
        slot.Value < 0 || slotGeneration == 0 ||
        (uint)ownerSlot > byte.MaxValue || identity < 0 || identity > 1000 ||
        projectileType <= 0 || projectileType > short.MaxValue ||
        projectileUuid is < 0 || timeLeft < 0 || netSpam < 0 ||
        !float.IsFinite(position.X) || !float.IsFinite(position.Y) ||
        !float.IsFinite(velocity.X) || !float.IsFinite(velocity.Y) ||
        !float.IsFinite(knockback) || !float.IsFinite(ai0) ||
        !float.IsFinite(ai1) || !float.IsFinite(ai2) ||
        (uint)bannerIdToRespondTo > ushort.MaxValue)
    {
      throw new ArgumentException("The projectile snapshot contains invalid committed state.");
    }

    bool[] sectionSync = sectionSyncSkippedForPlayer.ToArray();
    RuntimeHandle = runtimeHandle;
    EntityReference = entityReference;
    Slot = slot;
    SlotGeneration = slotGeneration;
    OwnerSlot = ownerSlot;
    Identity = identity;
    ProjectileUuid = projectileUuid;
    ProjectileType = projectileType;
    Position = position;
    Velocity = velocity;
    Damage = damage;
    OriginalDamage = originalDamage;
    Knockback = knockback;
    Ai0 = ai0;
    Ai1 = ai1;
    Ai2 = ai2;
    BannerIdToRespondTo = bannerIdToRespondTo;
    TimeLeft = timeLeft;
    NetworkImportant = networkImportant;
    PrimaryUpdatePending = primaryUpdatePending;
    SecondaryUpdatePending = secondaryUpdatePending;
    NetSpam = netSpam;
    SendRequested = sendRequested;
    _sectionSyncSkippedForPlayer = Array.AsReadOnly(sectionSync);
  }

  public RuntimeEntityHandle RuntimeHandle { get; }

  public EntityReference EntityReference { get; }

  public ProjectileSlot Slot { get; }

  public uint SlotGeneration { get; }

  public int OwnerSlot { get; }

  public int Identity { get; }

  /// <summary>Steam projectile 326 key: identity in bits 8..17 and owner in bits 0..7.</summary>
  public uint SteamKey => ((uint)Identity << 8) | (byte)OwnerSlot;

  public int? ProjectileUuid { get; }

  public int ProjectileType { get; }

  /// <summary>The committed projectile's top-left location from its LocationComponent.</summary>
  public Vector2 Position { get; }

  public Vector2 Velocity { get; }

  public int Damage { get; }

  public int OriginalDamage { get; }

  public float Knockback { get; }

  public float Ai0 { get; }

  public float Ai1 { get; }

  public float Ai2 { get; }

  public int BannerIdToRespondTo { get; }

  public int TimeLeft { get; }

  public bool NetworkImportant { get; }

  public bool PrimaryUpdatePending { get; }

  public bool SecondaryUpdatePending { get; }

  public int NetSpam { get; }

  public bool SendRequested { get; }

  public IReadOnlyList<bool> SectionSyncSkippedForPlayer => _sectionSyncSkippedForPlayer;

  public int? CodecDamage => Damage == 0 ? null : Damage;

  public int? CodecOriginalDamage => OriginalDamage == 0 ? null : OriginalDamage;

  public float? CodecKnockback => Knockback == 0.0f ? null : Knockback;

  public ushort? CodecBannerId => BannerIdToRespondTo == 0
    ? null
    : (ushort)BannerIdToRespondTo;

  public int? CodecProjectileUuid => ProjectileUuid;
}
