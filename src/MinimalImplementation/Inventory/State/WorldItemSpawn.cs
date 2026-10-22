using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record WorldItemSpawn(
  WorldScopeId WorldScope,
  int SlotId,
  WorldItemMutation InitialState);
