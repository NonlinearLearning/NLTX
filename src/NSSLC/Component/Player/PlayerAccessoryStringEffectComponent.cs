namespace Terraria.Player;

/// <summary>
/// 保存悠悠球线、配重、饰品槽和连续攻击相关能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：extraAccessorySlots（第 537 行）； extraAccessory（第 539 行）； tankPet（第 541 行）； tankPetReset（第
/// 543 行）； stringColor（第 545 行）； counterWeight（第 547 行）； vanityCounterWeight（第 549 行）；
/// magicString（第 551 行）； yoyoString（第 553 行）； yoyoGlove（第 555 行）； rapidAttackBonus（第 557 行）；
/// stressBall（第 559 行）； stressBallPrevious（第 561 行）； staffOfRegrowthBonus（第 563 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 73 行。</para>
/// </remarks>
public sealed class PlayerAccessoryStringEffectComponent
{
  public int ExtraAccessorySlots { get; internal set; } = 2;

  public bool ExtraAccessory { get; internal set; }

  public int TankPet { get; internal set; } = -1;

  public bool TankPetReset { get; internal set; }

  public int StringColor { get; internal set; }

  public int CounterWeight { get; internal set; }

  public int VanityCounterWeight { get; internal set; }

  public bool MagicString { get; internal set; }

  public bool YoyoString { get; internal set; }

  public bool YoyoGlove { get; internal set; }

  public float RapidAttackBonus { get; internal set; }

  public bool StressBall { get; internal set; }

  public bool StressBallPrevious { get; internal set; }

  public bool StaffOfRegrowthBonus { get; internal set; }

  public void CommitTankPet(int projectileSlot)
  {
    ArgumentOutOfRangeException.ThrowIfLessThan(projectileSlot, -1);
    TankPet = projectileSlot;
  }
}
