using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Detached NPC state captured for one producer or owner-thread read.</summary>
public sealed record NpcView
{
  public NpcView(
    CombatEntityIdentity entity,
    short slot,
    short type,
    int aiStyle,
    int width,
    int height,
    int maximumLife,
    Vector2 position,
    Vector2 velocity,
    ushort target,
    bool directionPositive,
    bool directionYPositive,
    bool spriteDirectionPositive,
    IReadOnlyList<float> ai,
    int life,
    bool isBoss,
    bool isTownNpc,
    bool netAlways,
    bool catchable,
    byte? catchableReleaseOwner,
    bool spawnedFromStatue,
    bool spawnNeedsSyncing,
    bool shimmering,
    byte? playerCount,
    float? difficulty,
    long revision,
    CommitVersion commitVersion,
    bool isDirty,
    int activityTimeLeft = 750,
    int damage = 0,
    int defense = 0,
    float knockbackResist = 1f,
    int extraValue = 0,
Vector2 extraValuePosition = default,
    bool hasCustomMaximumLife = false)
  {
    ArgumentNullException.ThrowIfNull(ai);
    Entity = entity;
    Slot = slot;
    Type = type;
    AiStyle = aiStyle;
    Width = width;
    Height = height;
    MaximumLife = maximumLife;
    HasCustomMaximumLife = hasCustomMaximumLife;
    Position = position;
    Velocity = velocity;
    Target = target;
    DirectionPositive = directionPositive;
    DirectionYPositive = directionYPositive;
    SpriteDirectionPositive = spriteDirectionPositive;
    Ai = Array.AsReadOnly(ai.ToArray());
    Life = life;
    IsBoss = isBoss;
    IsTownNpc = isTownNpc;
    NetAlways = netAlways;
    Catchable = catchable;
    CatchableReleaseOwner = catchableReleaseOwner;
    SpawnedFromStatue = spawnedFromStatue;
    SpawnNeedsSyncing = spawnNeedsSyncing;
    Shimmering = shimmering;
    PlayerCount = playerCount;
    Difficulty = difficulty;
    Revision = revision;
    CommitVersion = commitVersion;
    IsDirty = isDirty;
    ActivityTimeLeft = activityTimeLeft;
    Damage = damage;
    Defense = defense;
    KnockbackResist = knockbackResist;
    ExtraValue = extraValue;
    ExtraValuePosition = extraValuePosition;
  }

  public CombatEntityIdentity Entity { get; }

  public short Slot { get; }

  public short Type { get; }

  public int AiStyle { get; }

  public int Width { get; }

  public int Height { get; }

  /// <summary>Committed effective maximum, captured with PlayerCount and Difficulty.</summary>
  public int MaximumLife { get; }

  public bool HasCustomMaximumLife { get; }

  public Vector2 Position { get; }

  public Vector2 Velocity { get; }

  public ushort Target { get; }

  public bool DirectionPositive { get; }

  public bool DirectionYPositive { get; }

  public bool SpriteDirectionPositive { get; }

  public IReadOnlyList<float> Ai { get; }

  public int Life { get; }

  public bool IsBoss { get; }

  public bool IsTownNpc { get; }

  public bool NetAlways { get; }

  public bool Catchable { get; }

  public byte? CatchableReleaseOwner { get; }

  public bool SpawnedFromStatue { get; }

  public bool SpawnNeedsSyncing { get; }

  public bool Shimmering { get; }

  public byte? PlayerCount { get; }

  public float? Difficulty { get; }

  public long Revision { get; }

  public CommitVersion CommitVersion { get; }

  public bool IsDirty { get; }

  public int ActivityTimeLeft { get; }

  public int Damage { get; }

  public int Defense { get; }

  public float KnockbackResist { get; }

  public int ExtraValue { get; }

  public Vector2 ExtraValuePosition { get; }
}
