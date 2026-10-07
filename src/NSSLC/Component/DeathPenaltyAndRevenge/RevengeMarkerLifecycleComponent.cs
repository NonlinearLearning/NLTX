using System;

namespace Terraria.DeathPenaltyAndRevenge;

/// <summary>
/// 保存金币复仇标记的到期、强制失效和重生尝试状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/CoinLossRevengeSystem.cs。</para>
/// <para>
/// 主要源成员：_expirationTime（第 52 行）； _forceExpire（第 58 行）； RespawnAttemptLocked（第 62 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 202 行。</para>
/// </remarks>
public sealed class RevengeMarkerLifecycleComponent
{
  private readonly RevengeRespawnAttemptComponent _respawnAttemptState = new();

  public RevengeMarkerLifecycleComponent(int expiresAtGameTime)
  {
    if (expiresAtGameTime < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(expiresAtGameTime));
    }

    ExpiresAtGameTime = expiresAtGameTime;
  }

  public int ExpiresAtGameTime { get; }

  public bool ForceExpire => _respawnAttemptState.ForceExpire;

  public bool RespawnAttemptLocked => _respawnAttemptState.IsLocked;

  public bool IsExpiredAt(int currentGameTime)
  {
    return ForceExpire || currentGameTime >= ExpiresAtGameTime;
  }

  internal void MarkForceExpired()
  {
    _respawnAttemptState.MarkForceExpired();
  }

  internal void SetRespawnAttemptLocked(bool locked)
  {
    _respawnAttemptState.SetAttemptedRespawn(locked);
  }

  internal RevengeRespawnAttemptComponent RespawnAttemptState => _respawnAttemptState;
}
