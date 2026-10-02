namespace Terraria.LeashedEntity;

public readonly record struct LeashedSectionSlotChange(
  LeashedEntityHandle Handle,
  int PreviousSlot,
  int CurrentSlot);

