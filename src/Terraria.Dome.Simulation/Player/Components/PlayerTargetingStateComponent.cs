using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerTargetingStateComponent
{
  public int Aggro;
  public NpcHandle? MinionAttackTarget;
  public FrozenSet<int>? NoAggroNpcTypes;

  public bool IsNoAggroNpc(int definitionId)
  {
    return NoAggroNpcTypes?.Contains(definitionId) == true;
  }

  public void SetNoAggroNpcTypes(IEnumerable<int> definitionIds)
  {
    NoAggroNpcTypes = definitionIds.ToFrozenSet();
  }

  public void SetMinionAttackTarget(NpcHandle? target)
  {
    MinionAttackTarget = target;
  }
}
