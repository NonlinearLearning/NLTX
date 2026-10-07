namespace Terraria.DeathPenaltyAndRevenge;

/// <summary>
/// 保存金币复仇标记的运行身份与旧编号。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/CoinLossRevengeSystem.cs。</para>
/// <para>主要源成员：_uniqueID（第 56 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 204 行。</para>
/// </remarks>
public sealed class RevengeMarkerIdentityComponent
{
  public RevengeMarkerIdentityComponent()
    : this(RevengeMarkerId.Unassigned)
  {
  }

  public RevengeMarkerIdentityComponent(RevengeMarkerId markerId)
  {
    MarkerId = markerId;
  }

  public RevengeMarkerId MarkerId { get; }

  public int LegacyId => MarkerId.Value;

  public bool IsAssigned => MarkerId.IsAssigned;
}
