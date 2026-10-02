namespace Terraria.EntityLifecycleAttribution;

public sealed class EntityIdentityComponent
{
  public EntityIdentityComponent(EntityIdentityState state)
  {
    Validate(state);
    State = state;
  }

  public EntityIdentityState State { get; private set; }

  public void Replace(EntityIdentityState state)
  {
    Validate(state);
    State = state;
  }

  private static void Validate(EntityIdentityState state)
  {
    if (state.RuntimeEntityId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(state), "Runtime entity IDs cannot be negative.");
    }

    if (state.CompatibilitySlot < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(state), "Compatibility slots below -1 are invalid.");
    }

    if (state.Generation < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(state), "Generations cannot be negative.");
    }
  }
}
