using System.Numerics;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Complete server-authored mutable state for one existing NPC.</summary>
public sealed class NpcStateUpdate
{
  public NpcStateUpdate(
    Vector2 position,
    Vector2 velocity,
    ushort target,
    bool directionPositive,
    bool directionYPositive,
    bool spriteDirectionPositive,
    IReadOnlyList<float> ai,
    int life,
    int maximumLife,
    bool spawnedFromStatue,
    bool spawnNeedsSyncing,
    bool shimmering,
    byte? playerCount,
    float? difficulty,
    byte? catchableReleaseOwner)
  {
    ArgumentNullException.ThrowIfNull(ai);
    if (ai.Count != 4)
    {
      throw new ArgumentException(
        "Version4 NPC state requires exactly four AI values.",
        nameof(ai));
    }

    Position = position;
    Velocity = velocity;
    Target = target;
    DirectionPositive = directionPositive;
    DirectionYPositive = directionYPositive;
    SpriteDirectionPositive = spriteDirectionPositive;
    Ai = Array.AsReadOnly(ai.ToArray());
    Life = life;
    MaximumLife = maximumLife;
    SpawnedFromStatue = spawnedFromStatue;
    SpawnNeedsSyncing = spawnNeedsSyncing;
    Shimmering = shimmering;
    PlayerCount = playerCount;
    Difficulty = difficulty;
    CatchableReleaseOwner = catchableReleaseOwner;
  }

  public Vector2 Position { get; }

  public Vector2 Velocity { get; }

  public ushort Target { get; }

  public bool DirectionPositive { get; }

  public bool DirectionYPositive { get; }

  public bool SpriteDirectionPositive { get; }

  public IReadOnlyList<float> Ai { get; }

  public int Life { get; }

  /// <summary>
  /// Effective life maximum after server-owned player-count and difficulty scaling.
  /// </summary>
  public int MaximumLife { get; }

  public bool SpawnedFromStatue { get; }

  public bool SpawnNeedsSyncing { get; }

  public bool Shimmering { get; }

  public byte? PlayerCount { get; }

  public float? Difficulty { get; }

  public byte? CatchableReleaseOwner { get; }
}
