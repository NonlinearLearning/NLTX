using System;
using System.Numerics;

using EntityEcs.Components;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.MOTION_HISTORY
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存实体上一轮位置、速度和采样时刻。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>主要源成员：oldPosition（第 14 行）； oldVelocity（第 16 行）； oldDirection（第 18 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-component-design.md。
/// </para>
/// <para>依据位置：第 238 行。</para>
/// </remarks>
public struct MotionHistoryComponent
{
  public MotionHistoryComponent(
    Vector2 previousPosition = default,
    Vector2 previousVelocity = default,
    int previousDirection = 0,
    long? recordedAtTick = null,
    MotionHistoryKind kind = MotionHistoryKind.Tick)
  {
    EnsureFinite(previousPosition, nameof(previousPosition));
    EnsureFinite(previousVelocity, nameof(previousVelocity));
    EnsureValidTick(recordedAtTick, nameof(recordedAtTick));

    PreviousPosition = previousPosition;
    PreviousVelocity = previousVelocity;
    PreviousDirection = previousDirection;
    RecordedAtTick = recordedAtTick;
    Kind = kind;
  }

  public Vector2 PreviousPosition;
  public Vector2 PreviousVelocity;
  public int PreviousDirection;
  public long? RecordedAtTick;
  public MotionHistoryKind Kind;

  public MotionHistoryKind HistoryKind
  {
    get => Kind;
    set => Kind = value;
  }

  public void Record(
    Vector2 previousPosition,
    Vector2 previousVelocity,
    int previousDirection,
    long? recordedAtTick,
    MotionHistoryKind kind)
  {
    EnsureFinite(previousPosition, nameof(previousPosition));
    EnsureFinite(previousVelocity, nameof(previousVelocity));
    EnsureValidTick(recordedAtTick, nameof(recordedAtTick));

    PreviousPosition = previousPosition;
    PreviousVelocity = previousVelocity;
    PreviousDirection = previousDirection;
    RecordedAtTick = recordedAtTick;
    Kind = kind;
  }

  public void Clear()
  {
    PreviousPosition = Vector2.Zero;
    PreviousVelocity = Vector2.Zero;
    PreviousDirection = 0;
    RecordedAtTick = null;
    Kind = MotionHistoryKind.Tick;
  }

  private static void EnsureFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Vector components must be finite.");
    }
  }

  private static void EnsureValidTick(long? value, string parameterName)
  {
    if (value is < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Tick must be non-negative.");
    }
  }
}
