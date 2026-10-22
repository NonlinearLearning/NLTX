using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Inventory.State;

public readonly record struct WorldItemHandle(
  WorldScopeId WorldScope,
  int SlotId,
  WorldItemGeneration Generation)
{
  public bool IsValid => WorldScope.IsValid && SlotId >= 0 && Generation.IsValid;
}
