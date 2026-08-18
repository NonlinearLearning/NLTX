namespace Terraria.Dome.Simulation.StatusEffects.Components;

public readonly record struct BuffEntry(
  ushort Type,
  int RemainingTicks,
  PlayerHandle Source);
