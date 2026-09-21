namespace Terraria.WorldSession.Components;

public sealed class WorldSavedOreTierStateComponent
{
  public WorldSavedOreTierStateComponent()
    : this(OreTierState.Uninitialized)
  {
  }

  public WorldSavedOreTierStateComponent(OreTierState value)
  {
    Value = value;
  }

  public OreTierState Value { get; private set; }

  public OreTierState CreateSnapshot()
  {
    return Value;
  }

  internal void Replace(OreTierState value)
  {
    Value = value;
  }
}
