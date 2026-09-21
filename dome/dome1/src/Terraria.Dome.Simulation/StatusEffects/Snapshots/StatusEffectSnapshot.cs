namespace Terraria.Dome.Simulation.StatusEffects.Snapshots;

public readonly record struct StatusEffectSnapshot(
  StatusEffectTargetKind TargetKind,
  int TargetId,
  long Revision,
  ushort Type,
  int RemainingTicks);
