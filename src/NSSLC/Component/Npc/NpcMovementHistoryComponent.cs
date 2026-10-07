using System;
using System.Numerics;

namespace Terraria.Npc;

// status: partial
// sourceMembers: oldPos, oldRot
// crossSubsystemOwner: movement commit and history reset integration-review
/// <summary>
/// 保存 NPC 历史位置和旋转序列。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：oldPos（第 6001 行）； oldRot（第 6003 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P12-npc-combat-network-damage-component-design.md。</para>
/// <para>依据位置：第 13 行。</para>
/// </remarks>
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
