using System;
using System.Collections.Generic;

namespace Terraria.Combat;

/// <summary>
/// 保存受击主体按通道区分的免伤窗口。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：immune（第 968 行）； immuneNoBlink（第 970 行）； immuneTime（第 972 行）； hurtCooldowns（第 2475 行）。
/// </para>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：immune（第 6303 行）。</para>
/// <para>重组说明：按通道组织的窗口键和 Revision 是统一免伤模型时新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 50 行。</para>
/// </remarks>
public struct ImmunityComponent
{
  public ImmunityComponent(int remainingTicks)
  {
    _windows = remainingTicks > 0
      ? new[]
      {
        new HitImmunityWindow(
          new HitImmunityKey(HitImmunityScope.General, null, null),
          remainingTicks)
      }
      : null;
    Revision = 0;
  }

  public ImmunityComponent(
    IReadOnlyList<HitImmunityWindow>? windows = null,
    int revision = 0)
  {
    _windows = windows is null
      ? null
      : new List<HitImmunityWindow>(windows).ToArray();
    Revision = revision;
  }

  private HitImmunityWindow[]? _windows;

  public int Revision;

  public ReadOnlyMemory<HitImmunityWindow> Windows =>
    new(_windows ?? Array.Empty<HitImmunityWindow>());

  public bool HasActiveWindow => _windows is { Length: > 0 };

  // Compatibility projection for the former single general cooldown field.
  // The general window remains stored in _windows; this property is not a second state source.
  public int RemainingTicks
  {
    get
    {
      if (_windows is null)
      {
        return 0;
      }

      foreach (HitImmunityWindow window in _windows)
      {
        if (window.Key.Scope == HitImmunityScope.General)
        {
          return window.RemainingTicks;
        }
      }

      return 0;
    }
    set
    {
      List<HitImmunityWindow> windows = _windows is null
        ? []
        : new List<HitImmunityWindow>(_windows);

      for (int index = windows.Count - 1; index >= 0; index--)
      {
        if (windows[index].Key.Scope == HitImmunityScope.General)
        {
          windows.RemoveAt(index);
        }
      }

      if (value > 0)
      {
        windows.Add(new HitImmunityWindow(
          new HitImmunityKey(HitImmunityScope.General, null, null),
          value));
      }

      _windows = windows.Count == 0 ? null : windows.ToArray();
      Revision++;
    }
  }
}
