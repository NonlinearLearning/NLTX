namespace Terraria.WorldStorage;

public readonly record struct EntityHandle<TSlot>(TSlot Slot, uint Generation)
  where TSlot : struct;
