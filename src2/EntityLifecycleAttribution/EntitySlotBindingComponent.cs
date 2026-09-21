namespace Terraria.EntityLifecycleAttribution;

public sealed class EntitySlotBindingComponent
{
  public int RuntimeEntityId { get; private set; } = -1;

  public int CompatibilitySlot { get; private set; } = -1;

  public int Generation { get; private set; }

  public bool IsBound { get; private set; }

  public EntitySlotHandle Handle
  {
    get
    {
      if (!IsBound)
      {
        throw new InvalidOperationException("The entity does not have an active slot binding.");
      }

      return new EntitySlotHandle(RuntimeEntityId, CompatibilitySlot, Generation);
    }
  }

  public void Bind(int runtimeEntityId, int compatibilitySlot, int generation)
  {
    if (runtimeEntityId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(runtimeEntityId));
    }

    if (compatibilitySlot < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(compatibilitySlot));
    }

    if (generation <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generation));
    }

    if (IsBound)
    {
      throw new InvalidOperationException("An active slot binding must be released before rebinding.");
    }

    RuntimeEntityId = runtimeEntityId;
    CompatibilitySlot = compatibilitySlot;
    Generation = generation;
    IsBound = true;
  }

  public bool Matches(EntitySlotHandle handle)
  {
    return IsBound && Handle == handle;
  }

  public void Release()
  {
    IsBound = false;
    RuntimeEntityId = -1;
    CompatibilitySlot = -1;
    Generation = 0;
  }
}
