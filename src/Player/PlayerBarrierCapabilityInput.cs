namespace Terraria.Player;

public readonly record struct PlayerBarrierCapabilityInput(
  int CurrentLife,
  int EffectiveLifeMaximum,
  bool IceBarrierBuffActive,
  bool PalladiumRegen);
