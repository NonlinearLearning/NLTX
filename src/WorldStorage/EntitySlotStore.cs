namespace Terraria.WorldStorage;

public sealed class EntitySlotStore<TState, TSlot>
  where TState : class
  where TSlot : struct
{
  private EntitySlotEntry<TState>[] _entries = Array.Empty<EntitySlotEntry<TState>>();
  private int _activeCount;

  public int Capacity => _entries.Length;
  public int ActiveCount => _activeCount;
}
