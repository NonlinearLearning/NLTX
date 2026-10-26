using System;
using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>Stores an active server-side crafting station and recipe selection.</summary>
public sealed class PlayerCraftingContextComponent
{
  public EntityUuid StationId { get; private set; }

  public string RecipeId { get; private set; } = string.Empty;

  public bool IsActive => StationId.IsValid && RecipeId.Length > 0;

  public void Begin(EntityUuid stationId, string recipeId)
  {
    if (!stationId.IsValid)
    {
      throw new ArgumentException(
          "A crafting context requires a valid station identity.", nameof(stationId));
    }

    if (string.IsNullOrWhiteSpace(recipeId))
    {
      throw new ArgumentException("A crafting context requires a recipe id.", nameof(recipeId));
    }

    StationId = stationId;
    RecipeId = recipeId;
  }

  public void Clear()
  {
    StationId = default;
    RecipeId = string.Empty;
  }
}
