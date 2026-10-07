using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹同步重要性、待发送状态和同步限流信息。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>
/// 主要源成员：netImportant（第 106 行）； netUpdate（第 172 行）； netUpdate2（第 174 行）； netSpam（第 176 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
public struct ProjectileNetworkStateComponent
{
  public ProjectileNetworkStateComponent()
  {
    NetworkImportant = false;
    PrimaryUpdatePending = false;
    SecondaryUpdatePending = false;
    NetSpam = 0;
    SectionSyncSkippedForPlayer = new bool[255];
    SendRequested = false;
  }

  public ProjectileNetworkStateComponent(
    int playerCapacity = 255,
    bool networkImportant = false,
    bool primaryUpdatePending = false,
    bool secondaryUpdatePending = false,
    int netSpam = 0,
    bool sendRequested = false)
  {
    if (playerCapacity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(playerCapacity));
    }

    if (netSpam < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(netSpam));
    }

    NetworkImportant = networkImportant;
    PrimaryUpdatePending = primaryUpdatePending;
    SecondaryUpdatePending = secondaryUpdatePending;
    NetSpam = netSpam;
    SectionSyncSkippedForPlayer = new bool[playerCapacity];
    SendRequested = sendRequested;
  }

  public bool NetworkImportant;
  public bool PrimaryUpdatePending;
  public bool SecondaryUpdatePending;
  public int NetSpam;
  public bool[] SectionSyncSkippedForPlayer;
  public bool SendRequested;
}
