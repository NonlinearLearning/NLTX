using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

/// <summary>
/// Stores critter-only behavior state for one leashed entity.
/// status: implemented
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
/// <remarks>
/// <para>职责：保存拴系小动物的类型、目标位置和行为状态。</para>
/// <para>拆分来源：Terraria.GameContent.LeashedEntities.LeashedCritter。</para>
/// <para>
/// 原始文件：D:/TRbackup/Version4/Terraria.GameContent.LeashedEntities/LeashedCritter.cs。
/// </para>
/// <para>
/// 主要源成员：anchorStyle（第 15 行）； npcType（第 17 行）； WaitTime（第 27 行）； State（第 29 行）； TargetPosition（第
/// 31 行）； isAquatic（第 39 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P02-leashed-entity-component-design.md。</para>
/// <para>依据位置：第 1150 行。</para>
/// </remarks>
public struct LeashedCritterBehaviorComponent
{
  /// <summary>
  /// NPC content id, not an NPC runtime instance id. A null value means content binding is pending.
  /// </summary>
  public int? NpcType;

  /// <summary>
  /// Anchor compatibility/style snapshot candidate. Definition catalog ownership remains pending.
  /// </summary>
  public int AnchorStyle;

  /// <summary>
  /// Critter capability snapshot candidate. The immutable definition owner remains pending.
  /// </summary>
  public bool IsAquatic;

  /// <summary>
  /// Current behavior target; null means no accepted target yet.
  /// </summary>
  public TileCoordinate? TargetPosition;

  /// <summary>
  /// Wire-compatible random cursor candidate; the source LCG representation remains unresolved.
  /// </summary>
  public uint RandomState;

  /// <summary>
  /// Behavior wait timer candidate.
  /// </summary>
  public short WaitTime;

  /// <summary>
  /// Critter behavior state interpreted by the selected definition.
  /// </summary>
  public byte State;

  /// <summary>
  /// Derived target-presence view only.
  /// </summary>
  public bool HasTargetPosition => TargetPosition.HasValue;

  public LeashedCritterBehaviorComponent(
    int? npcType,
    int anchorStyle,
    bool isAquatic,
    TileCoordinate? targetPosition,
    uint randomState,
    short waitTime,
    byte state)
  {
    NpcType = npcType;
    AnchorStyle = anchorStyle;
    IsAquatic = isAquatic;
    TargetPosition = targetPosition;
    RandomState = randomState;
    WaitTime = waitTime;
    State = state;
  }
}
