using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public readonly record struct NetworkItemStackSlice(ushort ItemType, int Quantity)
{
  public static NetworkItemStackSlice From(ItemStack stack)
  {
    return new NetworkItemStackSlice(stack.ItemType, stack.Quantity);
  }
}
