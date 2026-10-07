using System;

namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存世界事件实例的身份、生命周期和外部关联。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Main 的世界事件开始、结束和广播流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Main.cs。</para>
/// <para>重组说明：事件实例 ID、生命周期、时间戳和外部身份关系是拆分时新增的模型。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md。
/// </para>
/// <para>依据位置：第 110 行。</para>
/// </remarks>
public sealed class WorldEventInstanceStateComponent
{
  public WorldEventInstanceStateComponent(
    EventInstanceId eventInstanceId,
    WorldEventKind eventKind,
    WorldEventLifecycleState lifecycleState)
  {
    EventInstanceId = eventInstanceId;
    EventKind = eventKind;
    LifecycleState = lifecycleState;
    StartedAtTick = null;
    EndedAtTick = null;
    OwnerEntityId = null;
    PersistentEntityId = null;
    NetworkId = null;
    Validate();
  }

  public EventInstanceId EventInstanceId;
  public WorldEventKind EventKind;
  public WorldEventLifecycleState LifecycleState;
  public long? StartedAtTick;
  public long? EndedAtTick;
  public EntityId? OwnerEntityId;
  public PersistentEntityId? PersistentEntityId;
  public NetworkId? NetworkId;

  public void Validate()
  {
    if (StartedAtTick.HasValue && StartedAtTick.Value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(StartedAtTick));
    }

    if (EndedAtTick.HasValue && EndedAtTick.Value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(EndedAtTick));
    }

    if (StartedAtTick.HasValue &&
        EndedAtTick.HasValue &&
        EndedAtTick.Value < StartedAtTick.Value)
    {
      throw new ArgumentException(
        "An event cannot end before it starts.",
        nameof(EndedAtTick));
    }
  }
}
