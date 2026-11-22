using Terraria.Combat;
using Terraria.Relationships;

namespace Terraria.Network;

/// <summary>Bounds and content mapping used by the game communication adapter.</summary>
public sealed class GameCommunicationOptions
{
  public const int DefaultMaximumCoordinate = 1_000_000;
  public const float DefaultMaximumVelocity = 50f;

  /// <summary>Maps a wire NPC slot and client damage value to an attack definition.</summary>
  public Func<short, short, CombatAttackDefinitionId?>? AttackDefinitionResolver { get; init; }

  /// <summary>Maps the protocol NPC slot to the authoritative runtime handle.</summary>
  public Func<short, EntityRuntimeHandle?>? NpcResolver { get; init; }

  public float MaximumVelocity { get; init; } = DefaultMaximumVelocity;

  public int MaximumCoordinate { get; init; } = DefaultMaximumCoordinate;

  public int MovementPriority { get; init; } = 100;

  public void Validate()
  {
    if (!float.IsFinite(MaximumVelocity) || MaximumVelocity <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(MaximumVelocity));
    }

    if (MaximumCoordinate <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(MaximumCoordinate));
    }
  }
}
