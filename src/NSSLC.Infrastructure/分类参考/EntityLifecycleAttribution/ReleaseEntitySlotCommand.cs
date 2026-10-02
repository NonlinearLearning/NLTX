namespace Terraria.EntityLifecycleAttribution;

public readonly record struct ReleaseEntitySlotCommand(
  EntitySlotPoolKind PoolKind,
  EntitySlotHandle Handle,
  int ReuseDelayTicks = 0,
  PresentationEffectKind? PresentationKind = null);
