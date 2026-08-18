using System;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public enum NpcBehaviorId
{
  OrdinaryChase = 1,
  TownHome = 2,
  Segment = 3
}

public enum NpcFaction
{
  Hostile = 1,
  Town = 2,
  Neutral = 3
}

public enum NpcCategory
{
  Enemy = 1,
  Town = 2,
  Segment = 3
}

public readonly record struct NpcDefinition
{
  public NpcDefinition(
    int DefinitionId,
    int NetId,
    int MaximumHealth,
    int Defense,
    float ColliderWidth,
    float ColliderHeight,
    NpcBehaviorId BehaviorId,
    int LootTableId,
    NpcFaction Faction = NpcFaction.Hostile,
    NpcCategory Category = NpcCategory.Enemy)
  {
    if (DefinitionId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(DefinitionId));
    }

    if (NetId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(NetId));
    }

    if (MaximumHealth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(MaximumHealth));
    }

    if (ColliderWidth <= 0.0f || ColliderHeight <= 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(ColliderWidth));
    }

    this.DefinitionId = DefinitionId;
    this.NetId = NetId;
    this.MaximumHealth = MaximumHealth;
    this.Defense = Defense;
    this.ColliderWidth = ColliderWidth;
    this.ColliderHeight = ColliderHeight;
    this.BehaviorId = BehaviorId;
    this.LootTableId = LootTableId;
    this.Faction = Faction;
    this.Category = Category;
  }

  public int DefinitionId { get; }
  public int NetId { get; }
  public int MaximumHealth { get; }
  public int Defense { get; }
  public float ColliderWidth { get; }
  public float ColliderHeight { get; }
  public NpcBehaviorId BehaviorId { get; }
  public int LootTableId { get; }
  public NpcFaction Faction { get; }
  public NpcCategory Category { get; }
}
