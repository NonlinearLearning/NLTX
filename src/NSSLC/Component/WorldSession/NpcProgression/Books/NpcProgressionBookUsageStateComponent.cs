using System;

namespace Terraria.WorldSession.NpcProgression.Books;

/// <summary>
/// 保存世界战斗书和商贩背包的使用记录。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：combatBookWasUsed（第 6205 行）； combatBookVolumeTwoWasUsed（第 6207 行）；
/// peddlersSatchelWasUsed（第 6209 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 471 行。</para>
/// </remarks>
public sealed class NpcProgressionBookUsageStateComponent
{
  public bool CombatBookWasUsed { get; private set; }

  public bool CombatBookVolumeTwoWasUsed { get; private set; }

  public bool PeddlersSatchelWasUsed { get; private set; }

  public bool MarkUsed(NpcProgressionBookKind bookKind)
  {
    if (IsUsed(bookKind))
    {
      return false;
    }

    switch (bookKind)
    {
      case NpcProgressionBookKind.CombatBook:
        CombatBookWasUsed = true;
        break;
      case NpcProgressionBookKind.CombatBookVolumeTwo:
        CombatBookVolumeTwoWasUsed = true;
        break;
      case NpcProgressionBookKind.PeddlersSatchel:
        PeddlersSatchelWasUsed = true;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(bookKind), bookKind, "Unknown NPC progression book kind.");
    }

    return true;
  }

  public bool IsUsed(NpcProgressionBookKind bookKind)
  {
    return bookKind switch
    {
      NpcProgressionBookKind.CombatBook => CombatBookWasUsed,
      NpcProgressionBookKind.CombatBookVolumeTwo => CombatBookVolumeTwoWasUsed,
      NpcProgressionBookKind.PeddlersSatchel => PeddlersSatchelWasUsed,
      _ => throw new ArgumentOutOfRangeException(nameof(bookKind), bookKind, "Unknown NPC progression book kind."),
    };
  }

  public void Reset()
  {
    CombatBookWasUsed = false;
    CombatBookVolumeTwoWasUsed = false;
    PeddlersSatchelWasUsed = false;
  }
}
