using System.Collections.Immutable;

namespace Terraria.Content;

public sealed record RecipeDefinition(
  int RecipeId,
  RecipeResultDefinition Result,
  ImmutableArray<RecipeIngredientDefinition> Ingredients,
  ImmutableArray<int> RequiredTileTypeIds,
  bool NotDecraftable = false,
  ImmutableArray<RecipeConditionDefinition> Conditions = default,
  ImmutableArray<RecipeResultDefinition> CustomShimmerResults = default,
  bool NeedHoney = false,
  bool NeedWater = false,
  bool NeedLava = false,
  bool NeedTorchGodsFavor = false,
  bool Alchemy = false,
  bool NeedSnowBiome = false,
  bool NeedGraveyardBiome = false,
  bool NeedMechdusa = false,
  bool Crimson = false,
  bool Corruption = false);
