namespace Terraria.LeashedEntity;

/// <summary>
/// Stores lifecycle state for one leashed entity.
/// status: implemented
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: none for local state
/// </summary>
/// <remarks>
/// <para>职责：保存拴系实体的活动、生成和移除状态。</para>
/// <para>拆分来源：Terraria.GameContent.LeashedEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/LeashedEntity.cs。</para>
/// <para>主要源成员：active（第 185 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P02-leashed-entity-component-design.md。</para>
/// <para>依据位置：第 1143 行。</para>
/// </remarks>
public struct LeashedEntityLifecycleComponent
{
  /// <summary>
  /// The only stored lifecycle authority for active/inactive state.
  /// </summary>
  public LeashedEntityLifecycle State;

  /// <summary>
  /// Whether the instance has entered its spawned runtime state.
  /// </summary>
  public bool Spawned;

  /// <summary>
  /// Candidate idempotency/revision field. Version4 does not provide this field;
  /// registration ownership remains unresolved.
  /// </summary>
  public ulong TransitionSequence;

  /// <summary>
  /// Derived view only; do not persist as a second active authority.
  /// </summary>
  public bool IsActive => State == LeashedEntityLifecycle.Active;

  /// <summary>
  /// Derived terminal-state view only.
  /// </summary>
  public bool IsRemoved => State == LeashedEntityLifecycle.Removed;

  public LeashedEntityLifecycleComponent()
    : this(LeashedEntityLifecycle.Inactive, false, 0)
  {
  }

  public LeashedEntityLifecycleComponent(
    LeashedEntityLifecycle state,
    bool spawned,
    ulong transitionSequence)
  {
    State = state;
    Spawned = spawned;
    TransitionSequence = transitionSequence;
  }
}
