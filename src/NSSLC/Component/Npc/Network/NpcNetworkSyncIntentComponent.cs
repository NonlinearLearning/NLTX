using System;

namespace Terraria.Npc.Network;

/// <summary>
/// 保存 NPC 待同步意图及其请求和确认版本。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：netUpdate（第 6015 行）； netUpdatePendingSpamCooldown（第 6017 行）；
/// netUpdatePendingFullSpamCooldown（第 6019 行）。
/// </para>
/// <para>重组说明：请求版本与确认版本是同步意图管理时新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 129 行。</para>
/// </remarks>
public sealed class NpcNetworkSyncIntentComponent
{
  private bool _isPending;
  private uint _revision;

  public bool IsPending => _isPending;

  public uint Revision => _revision;

  public uint LastAcknowledgedRevision { get; private set; }

  public bool Mark()
  {
    if (_isPending)
    {
      return false;
    }

    if (_revision == uint.MaxValue)
    {
      throw new InvalidOperationException(
        "NPC network synchronization revision exhausted its range.");
    }

    _revision++;
    _isPending = true;
    return true;
  }

  public bool Acknowledge(uint revision)
  {
    if (!_isPending || revision != _revision)
    {
      return false;
    }

    _isPending = false;
    LastAcknowledgedRevision = revision;
    return true;
  }

  public bool Retry()
  {
    return _isPending;
  }

  public void ResetForEntityReuse()
  {
    _isPending = false;
    LastAcknowledgedRevision = 0;
  }
}
