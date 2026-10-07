namespace Terraria.WorldStorage;

public sealed class EntitySlotEntry<TState>
  where TState : class
{
  public TState? State { get; internal set; }
  public uint Generation { get; internal set; }
  public bool IsOccupied { get; internal set; }
  public bool IsGenerationExhausted { get; internal set; }
}
