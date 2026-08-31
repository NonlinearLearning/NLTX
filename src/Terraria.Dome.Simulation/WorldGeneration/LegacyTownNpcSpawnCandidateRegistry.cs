using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTownNpcSpawnCandidateRegistry
{
  private const int NpcTypeCount = 697;

  private static readonly FrozenSet<int> _candidateTypes = new HashSet<int>
  {
    17, 18, 19, 20, 22, 38, 54, 107, 108, 124, 142, 160, 178, 207, 208, 209,
    227, 228, 229, 353, 369, 441, 550, 588, 633, 637, 638, 656, 663, 670, 678,
    679, 680, 681, 682, 683, 684
  }.ToFrozenSet();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return _candidateTypes;
  }

  public static bool IsCandidateType(int npcType)
  {
    return npcType >= 0 && npcType < NpcTypeCount && _candidateTypes.Contains(npcType);
  }
}
