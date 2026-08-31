using System;
using Terraria.Dome.Simulation.Npc.Definitions;

namespace Terraria.Dome.Simulation.Npc.Components;

public readonly record struct NpcDefinitionComponent
{
  private const int MaxNetId = 696;

  public NpcDefinitionComponent(
    int DefinitionId,
    int NetId,
    NpcFaction Faction,
    NpcCategory Category)
  {
    if (DefinitionId <= 0 || NetId <= 0 || NetId > MaxNetId)
    {
      throw new ArgumentOutOfRangeException(nameof(NetId));
    }

    if (!Enum.IsDefined(Faction) || !Enum.IsDefined(Category))
    {
      throw new ArgumentOutOfRangeException(nameof(Faction));
    }

    this.DefinitionId = DefinitionId;
    this.NetId = NetId;
    this.Faction = Faction;
    this.Category = Category;
  }

  public int DefinitionId { get; }
  public int NetId { get; }
  public NpcFaction Faction { get; }
  public NpcCategory Category { get; }
}
