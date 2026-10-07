using Terraria.Relationships;

namespace Terraria.Items;

/// <summary>
/// 保存物品使用的阶段、参与实体、目标和冷却。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Player.ItemCheck 的物品使用流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>重组说明：使用阶段、主体和目标关系、序号及绝对冷却时刻是使用流程显式化后的表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-player-gameplay-component-design.md。</para>
/// <para>依据位置：第 498 行。</para>
/// </remarks>
public sealed class ItemUseComponent
{
  public ItemUseComponent(
    ItemUsePhase phase = ItemUsePhase.Idle,
    long startedAtTick = 0,
    long cooldownUntilTick = 0,
    EntityReference owner = default,
    EntityReference targetEntity = default,
    TileCoordinates? targetTile = null,
    long useSequence = 0)
  {
    Phase = phase;
    StartedAtTick = startedAtTick;
    CooldownUntilTick = cooldownUntilTick;
    Owner = owner;
    TargetEntity = targetEntity;
    TargetTile = targetTile;
    UseSequence = useSequence;
  }

  public ItemUsePhase Phase;
  public long StartedAtTick;
  public long CooldownUntilTick;
  public EntityReference Owner;
  public EntityReference TargetEntity;
  public TileCoordinates? TargetTile;
  public long UseSequence;

  public bool IsActive => Phase is ItemUsePhase.Starting or ItemUsePhase.Using or ItemUsePhase.Channeling;
  public bool IsCoolingDown => Phase == ItemUsePhase.Cooldown && CooldownUntilTick > StartedAtTick;
}
