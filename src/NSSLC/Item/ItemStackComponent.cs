using System;

namespace Terraria.Item;

/// <summary>
/// Authoritative item stack state attached to an ECS entity.
/// </summary>
public sealed class ItemStackComponent {
  public ItemStackComponent(ItemStack stack) {
    if (stack.IsEmpty) {
      throw new ArgumentException("An item entity cannot contain an empty stack.", nameof(stack));
    }

    Stack = stack;
  }

  public ItemStack Stack { get; private set; }

  public bool IsEmpty => Stack.IsEmpty;

  public void Replace(ItemStack stack) {
    if (stack.IsEmpty) {
      throw new ArgumentException("An item entity cannot contain an empty stack.", nameof(stack));
    }

    Stack = stack;
  }

  /// <summary>
  /// Takes part or all of the current stack. Taking all leaves an empty value;
  /// the owning item system must then remove the entity or replace the component.
  /// </summary>
  public bool TryTake(int quantity, out ItemStack taken) {
    if (quantity <= 0 || quantity > Stack.Quantity) {
      taken = default;
      return false;
    }

    taken = new ItemStack(Stack.ItemId, quantity, Stack.Prefix);
    Stack = Stack.WithQuantity(Stack.Quantity - quantity);
    return true;
  }
}
