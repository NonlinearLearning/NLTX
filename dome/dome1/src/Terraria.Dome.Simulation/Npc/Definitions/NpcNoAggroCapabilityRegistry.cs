using System.Collections.Frozen;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public sealed class NpcNoAggroCapabilityRegistry
{
  private readonly FrozenSet<int> _item3090DefinitionIds;

  private NpcNoAggroCapabilityRegistry(FrozenSet<int> item3090DefinitionIds)
  {
    _item3090DefinitionIds = item3090DefinitionIds;
  }

  public int Item3090Count => _item3090DefinitionIds.Count;

  public FrozenSet<int> Item3090DefinitionIds => _item3090DefinitionIds;

  public bool IsItem3090NoAggroNpc(int definitionId)
  {
    return _item3090DefinitionIds.Contains(definitionId);
  }

  public static NpcNoAggroCapabilityRegistry CreateVersion1456Item3090()
  {
    return new NpcNoAggroCapabilityRegistry([
      1,
      16,
      59,
      71,
      81,
      121,
      122,
      138,
      141,
      147,
      183,
      184,
      204,
      225,
      244,
      302,
      333,
      334,
      335,
      336,
      537,
      667,
      676]);
  }
}
