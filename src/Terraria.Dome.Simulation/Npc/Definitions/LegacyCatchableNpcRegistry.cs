using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public static class LegacyCatchableNpcRegistry
{
  private static readonly IReadOnlySet<int> CatchableNpcTypes = new HashSet<int>
  {
    46, 55, 74, 148, 149, 297, 298, 299, 300, 355, 356, 357,
    358, 359, 360, 361, 362, 363, 364, 365, 366, 367, 374, 377,
    442, 443, 444, 445, 446, 447, 448, 484, 485, 486, 487, 538,
    539, 583, 584, 585, 592, 593, 595, 596, 597, 598, 599, 600,
    601, 602, 603, 604, 605, 606, 607, 608, 609, 610, 611, 612,
    613, 614, 616, 617, 626, 627, 639, 640, 641, 642, 643, 644,
    645, 646, 647, 648, 649, 650, 651, 652, 653, 654, 655, 661,
    669, 671, 672, 673, 674, 675, 677, 688
  }.ToFrozenSet();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return CatchableNpcTypes;
  }

  public static bool IsCatchable(int npcType)
  {
    return CatchableNpcTypes.Contains(npcType);
  }
}
