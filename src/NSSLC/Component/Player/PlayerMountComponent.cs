using Terraria.Player.Mount;

namespace Terraria.Player;

/// <summary>
/// 保存玩家坐骑的类型、飞行、疲劳、能力和下车状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Mount。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Mount.cs。</para>
/// <para>
/// 主要源成员：_fatigue（第 343 行）； _fatigueMax（第 345 行）； _abilityCooldown（第 351 行）； _abilityDuration（第
/// 353 行）； Active（第 387 行）； Type（第 389 行）； FlyTime（第 393 行）； AbilityCharge（第 588 行）。
/// </para>
/// <para>重组说明：下车请求和下车锁定是坐骑生命周期模型新增的表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P03-mount-vehicle-component-design.md。</para>
/// <para>依据位置：第 23 行。</para>
/// </remarks>
public sealed class PlayerMountComponent
{
  public ContentId<MountDefinition>? MountType { get; set; }

  public bool IsActive { get; set; }

  public int FlightRemainingTicks { get; set; }

  public float Fatigue { get; set; }

  public float MaximumFatigue { get; set; }

  public int AbilityCharge { get; set; }

  public int AbilityCooldownRemainingTicks { get; set; }

  public int AbilityDurationRemainingTicks { get; set; }

  public bool IsDismountLocked { get; set; }

  public bool IsDismountRequested { get; set; }

  public bool IsMinecart { get; set; }
}
