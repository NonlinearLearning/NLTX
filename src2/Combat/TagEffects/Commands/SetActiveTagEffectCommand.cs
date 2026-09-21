using Terraria.Content.StatusEffects;

namespace Terraria.Combat.TagEffects.Commands;

public readonly record struct SetActiveTagEffectCommand(
  int EffectTypeId,
  TagEffectDefinition? Definition);
