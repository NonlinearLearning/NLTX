using System;

namespace Terraria.Npc;

public sealed class NpcBuffStateUpdateResult
{
  public enum EffectKind
  {
    BuffSlotSyncRequested,
    WaterPerishableCleanupRequested,
  }

  public readonly record struct EffectIntent(EffectKind Kind);

  private readonly EffectIntent[] _effects;

  internal NpcBuffStateUpdateResult(
    bool applied,
    bool requiredInputMissing,
    bool buffSlotsChanged,
    EffectIntent[] effects)
  {
    ArgumentNullException.ThrowIfNull(effects);
    Applied = applied;
    RequiredInputMissing = requiredInputMissing;
    BuffSlotsChanged = buffSlotsChanged;
    _effects = effects.ToArray();
  }

  public bool Applied { get; }

  public bool RequiredInputMissing { get; }

  public bool BuffSlotsChanged { get; }

  public ReadOnlyMemory<EffectIntent> Effects => new(_effects.ToArray());
}
