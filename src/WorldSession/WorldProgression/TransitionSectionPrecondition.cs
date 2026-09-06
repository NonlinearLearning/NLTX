namespace Terraria.WorldProgression.Components;

public readonly record struct TransitionSectionPrecondition(
  WorldSectionId SectionId,
  WorldSectionVersion ExpectedVersion);
