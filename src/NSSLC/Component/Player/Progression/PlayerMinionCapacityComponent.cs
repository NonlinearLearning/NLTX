using System;
using System.Collections.Generic;
using Terraria.Relationships;

namespace Terraria.Player.Progression;

/// <summary>
/// 保存玩家召唤物数量、槽位占用和容量。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：maxMinions（第 832 行）； numMinions（第 834 行）； slotsMinions（第 836 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 188 行。</para>
/// </remarks>
public sealed class PlayerMinionCapacityComponent
{
  private readonly EntityReference _owner;
  private readonly object _sync = new();
  private readonly HashSet<Guid> _appliedTokens = new();

  public PlayerMinionCapacityComponent(EntityReference owner)
  {
    if (owner.IsEmpty || owner.Scope != EntityReferenceScope.Player)
    {
      throw new ArgumentException("A player owner identity is required.", nameof(owner));
    }

    _owner = owner;
  }

  internal EntityReference Owner => _owner;

  internal object SyncRoot => _sync;

  public int MaxMinions { get; internal set; } = 1;

  public int NumMinions { get; internal set; }

  public float SlotsMinions { get; internal set; }

  internal bool HasApplied(Guid token)
  {
    return _appliedTokens.Contains(token);
  }

  internal bool TryMarkApplied(Guid token)
  {
    return _appliedTokens.Add(token);
  }

  internal void ClearAppliedTokens()
  {
    _appliedTokens.Clear();
  }
}
