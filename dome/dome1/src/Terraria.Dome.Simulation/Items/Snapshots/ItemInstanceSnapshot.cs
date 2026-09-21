using Terraria.Dome.Simulation.Items.Components;

namespace Terraria.Dome.Simulation.Items.Snapshots;

public sealed class ItemInstanceSnapshot
{
  public ItemInstanceSnapshot(ItemStack stack, ItemInstanceStateComponent state)
  {
    if (stack.IsEmpty && stack != ItemStack.Empty)
    {
      throw new System.ArgumentException(
        "An empty item snapshot must use the canonical empty stack.");
    }

    if (stack.IsEmpty && state != default)
    {
      throw new System.ArgumentException("An empty item snapshot cannot carry instance state.");
    }

    state.Validate();

    Stack = stack;
    State = state;
  }

  public ItemStack Stack { get; }

  public ItemInstanceStateComponent State { get; }
}
