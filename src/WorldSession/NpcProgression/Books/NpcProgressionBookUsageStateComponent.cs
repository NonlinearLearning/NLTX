using System;

namespace Terraria.WorldSession.NpcProgression.Books;

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
