namespace Terraria.DeathPenaltyAndRevenge;

/// <summary>
/// 保存复仇标记是否已尝试重生及是否强制失效。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/CoinLossRevengeSystem.cs。</para>
/// <para>主要源成员：_forceExpire（第 58 行）； _attemptedRespawn（第 60 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 206 行。</para>
/// </remarks>
public sealed class RevengeRespawnAttemptComponent
{
  public bool ForceExpire { get; private set; }

  public bool AttemptedRespawn { get; private set; }

  public bool IsLocked => AttemptedRespawn;

  internal void MarkForceExpired()
  {
    ForceExpire = true;
  }

  internal void SetAttemptedRespawn(bool attempted)
  {
    AttemptedRespawn = attempted;
  }
}
