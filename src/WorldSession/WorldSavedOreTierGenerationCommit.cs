namespace Terraria.WorldSession.Components;

/// <summary>
/// Carries the successful first-four ore selection from world generation.
/// </summary>
public readonly record struct WorldSavedOreTierGenerationCommit(
  int Copper,
  int Iron,
  int Silver,
  int Gold);
