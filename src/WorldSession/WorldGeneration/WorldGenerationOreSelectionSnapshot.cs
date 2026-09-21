namespace Terraria.WorldGeneration.Components;

public readonly record struct WorldGenerationOreSelectionSnapshot(
  long GenerationId,
  int Copper,
  int Iron,
  int Silver,
  int Gold,
  int CopperBar,
  int IronBar,
  int SilverBar,
  int GoldBar);
