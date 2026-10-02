using System;
using System.Numerics;

namespace Terraria.Npc;

// status: partial
// sourceMembers: oldPos, oldRot
// crossSubsystemOwner: movement commit and history reset integration-review
public sealed class NpcMovementHistoryComponent
{
  public const int DefaultHistoryLength = 10;

  private readonly Vector2[] _oldPositions;
  private readonly float[] _oldRotations;

  public NpcMovementHistoryComponent(int historyLength = DefaultHistoryLength)
  {
    if (historyLength < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(historyLength));
    }

    _oldPositions = new Vector2[historyLength];
    _oldRotations = new float[historyLength];
  }

  public NpcMovementHistoryComponent(
    ReadOnlySpan<Vector2> oldPositions,
    ReadOnlySpan<float> oldRotations)
  {
    if (oldPositions.Length != oldRotations.Length)
    {
      throw new ArgumentException(
        "NPC movement position and rotation histories must have equal lengths.",
        nameof(oldRotations));
    }

    _oldPositions = oldPositions.ToArray();
    _oldRotations = oldRotations.ToArray();
  }

  public ReadOnlyMemory<Vector2> OldPositions => _oldPositions;

  public ReadOnlyMemory<float> OldRotations => _oldRotations;

  public int Capacity => _oldPositions.Length;
}
