using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Authoritative runtime state for one projectile instance.</summary>
public sealed class Projectile
{
  private ProjectileDefinition _definition;

  internal Projectile(
    CombatEntityIdentity entity,
    int runtimeSlot,
    ProjectileSyncRequest request,
    byte owner,
    short? normalizedProtocolUuid,
    ProjectileDefinition definition,
    long revision,
    CommitVersion commitVersion)
  {
    Entity = entity;
    RuntimeSlot = runtimeSlot;
    Owner = owner;
    OwnerEpoch = request.Sender.Epoch;
    ProtocolIdentity = request.Identity;
    _definition = definition;
    RemainingPenetration = definition.Penetrate;
    TimeLeft = definition.DefaultTimeLeft;
    ApplySyncFields(request, normalizedProtocolUuid);
    Revision = revision;
    CommitVersion = commitVersion;
  }

  internal Projectile(
      CombatEntityIdentity entity,
      int runtimeSlot,
      ProjectileLocalArrowSpawnRequest request,
      byte owner,
      ProjectileDefinition definition,
      ProjectileLiquidState liquidState,
      Vector2 position,
      Vector2 velocity,
      CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(liquidState);
    Entity = entity;
    RuntimeSlot = runtimeSlot;
    Owner = owner;
    ProtocolIdentity = checked((short)runtimeSlot);
    _definition = definition;
    RemainingPenetration = definition.Penetrate;
    TimeLeft = definition.DefaultTimeLeft;
    Position = position;
    Velocity = velocity;
    Ai0 = request.Ai0;
    Ai1 = request.Ai1;
    Ai2 = request.Ai2;
    Damage = request.Damage;
    Knockback = request.Knockback;
    LiquidState = liquidState;
    Revision = 1;
    CommitVersion = commitVersion;
  }

  public CombatEntityIdentity Entity { get; }

  public int RuntimeSlot { get; }

  public long SpawnOrdinal { get; private set; }

  public byte Owner { get; private set; }

  public SessionEpoch? OwnerEpoch { get; private set; }

  public short ProtocolIdentity { get; }

  public short Type => _definition.Type;

  public Vector2 Position { get; private set; }

  public Vector2 Velocity { get; private set; }

  public float Ai0 { get; private set; }

  public float Ai1 { get; private set; }

  public float Ai2 { get; private set; }

  public ushort BannerId { get; private set; }

  public int Damage { get; private set; }

  public float Knockback { get; private set; }

  public int OriginalDamage { get; private set; }

  public short? ProtocolUuid { get; private set; }

  public int TimeLeft { get; private set; }

  public int ExtraUpdates { get; private set; }

  public ProjectileLiquidState LiquidState { get; private set; } = ProjectileLiquidState.Dry;

  public bool IsActive { get; private set; } = true;

  public bool IsDirty { get; private set; } = true;

  public long Revision { get; private set; }

  public CommitVersion CommitVersion { get; private set; }

  public bool NetImportant => _definition.NetImportant;

  public bool Friendly => _definition.Friendly;

  public bool Hostile => _definition.Hostile;

  public ProjectileAiProfile AiProfile => _definition.AiProfile;

  public bool Minion => _definition.Minion;

  public bool Whip => _definition.Whip;

  public int Width => _definition.Width;

  public int Height => _definition.Height;

  public bool TileCollide => _definition.TileCollide;

  public short Penetrate => _definition.Penetrate;

  /// <summary>
  /// Server-owned remaining penetration for this projectile generation.
  /// A value of -1 means that the projectile does not consume penetration.
  /// </summary>
  public short RemainingPenetration { get; private set; }

  internal void ApplySync(
    ProjectileSyncRequest request,
    byte owner,
    short? normalizedProtocolUuid,
    ProjectileDefinition definition,
    CommitVersion commitVersion)
  {
    long nextRevision = NextRevision(Revision);
    if (Type != definition.Type)
    {
      _definition = definition;
      RemainingPenetration = definition.Penetrate;
      TimeLeft = definition.DefaultTimeLeft;
      ExtraUpdates = 0;
      // SetDefaults resets wet/lava/count; honey and shimmer survive a type change.
      LiquidState = new ProjectileLiquidState(
          false, false, LiquidState.HoneyWet, LiquidState.ShimmerWet, 0);
    }

    Owner = owner;
    OwnerEpoch = request.Sender.Epoch;
    ApplySyncFields(request, normalizedProtocolUuid);
    Revision = nextRevision;
    CommitVersion = commitVersion;
    IsActive = true;
    IsDirty = true;
  }

  internal void SetSpawnOrdinal(long spawnOrdinal)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(spawnOrdinal);
    if (SpawnOrdinal != 0)
    {
      throw new InvalidOperationException("A projectile spawn ordinal can only be assigned once.");
    }

    SpawnOrdinal = spawnOrdinal;
  }

  internal void Deactivate()
  {
    IsActive = false;
    IsDirty = false;
  }

  internal void ApplyTick(
    Vector2 position,
    Vector2 velocity,
    float ai0,
    float ai1,
    float ai2,
    int timeLeft,
    ProjectileLiquidState liquidState,
    int extraUpdates,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(liquidState);
    if (Revision == long.MaxValue)
    {
      throw new InvalidOperationException("Projectile revision range is exhausted.");
    }

    Position = position;
    Velocity = velocity;
    Ai0 = ai0;
    Ai1 = ai1;
    Ai2 = ai2;
    TimeLeft = timeLeft;
    LiquidState = liquidState;
    ExtraUpdates = extraUpdates;
    Revision++;
    CommitVersion = commitVersion;
    IsDirty = true;
  }

  internal void RestoreTickCommit(ProjectileView prior) {
    Position = prior.Position;
    Velocity = prior.Velocity;
    Ai0 = prior.Ai0;
    Ai1 = prior.Ai1;
    Ai2 = prior.Ai2;
    TimeLeft = prior.TimeLeft;
    LiquidState = prior.LiquidState;
    ExtraUpdates = prior.ExtraUpdates;
    Revision = prior.Revision;
    CommitVersion = prior.CommitVersion;
    IsDirty = prior.IsDirty;
  }

  internal void ApplyPenetration(int hitCount, CommitVersion commitVersion)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hitCount);
    if (RemainingPenetration < 0)
    {
      throw new InvalidOperationException(
        "An infinite-penetration projectile cannot consume penetration.");
    }

    if (hitCount > RemainingPenetration)
    {
      throw new ArgumentOutOfRangeException(nameof(hitCount));
    }

    if (Revision == long.MaxValue)
    {
      throw new InvalidOperationException("Projectile revision range is exhausted.");
    }

    RemainingPenetration = checked((short)(RemainingPenetration - hitCount));
    Revision++;
    CommitVersion = commitVersion;
    IsDirty = true;
  }

  internal void RestorePenetrationCommit(
    short remainingPenetration,
    long revision,
    CommitVersion commitVersion,
    bool isDirty)
  {
    if (remainingPenetration < -1 || revision < 0 || !commitVersion.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(remainingPenetration));
    }

    RemainingPenetration = remainingPenetration;
    Revision = revision;
    CommitVersion = commitVersion;
    IsDirty = isDirty;
  }

  internal void Acknowledge(long revision)
  {
    if (Revision == revision)
    {
      IsDirty = false;
    }
  }

  internal ProjectileView Capture()
  {
    return new ProjectileView(
      Entity,
      RuntimeSlot,
      ProtocolIdentity,
      Owner,
      OwnerEpoch,
      Type,
      Position,
      Velocity,
      Ai0,
      Ai1,
      Ai2,
      BannerId,
      Damage,
      Knockback,
      OriginalDamage,
      ProtocolUuid,
      TimeLeft,
      Friendly,
      Hostile,
      NetImportant,
      _definition.NeedsProtocolUuid,
      Revision,
      CommitVersion,
      _definition.AiStyle,
      Width,
      Height,
      TileCollide,
      Penetrate,
      RemainingPenetration,
      IsDirty) {
      LiquidState = this.LiquidState,
      ExtraUpdates = this.ExtraUpdates
    };
  }

  private static long NextRevision(long revision)
  {
    if (revision == long.MaxValue)
    {
      throw new InvalidOperationException("Projectile revision range is exhausted.");
    }

    return revision + 1;
  }

  private void ApplySyncFields(ProjectileSyncRequest request, short? normalizedProtocolUuid)
  {
    Position = request.Position;
    Velocity = request.Velocity;
    Ai0 = request.Ai0;
    Ai1 = request.Ai1;
    Ai2 = request.Ai2;
    BannerId = request.BannerId.GetValueOrDefault();
    Damage = request.Damage.GetValueOrDefault();
    Knockback = request.Knockback.GetValueOrDefault();
    OriginalDamage = request.OriginalDamage.GetValueOrDefault();
    ProtocolUuid = normalizedProtocolUuid;
  }
}
