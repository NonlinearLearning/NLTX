using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerPassExecutionSetResult(
  string PassName,
  IReadOnlyList<LegacyTileRunnerPassProvenance> Recipes,
  int InvocationCount,
  int TileCommandCount,
  int LiquidCommandCount);

public static class LegacyTileRunnerPassExecutionSetFactory
{
  public static LegacyTileRunnerPassExecutionSetResult Complete(
    string passName,
    IReadOnlyCollection<LegacyTileRunnerPassExecutionLedger> ledgers)
  {
    ArgumentException.ThrowIfNullOrEmpty(passName);
    ArgumentNullException.ThrowIfNull(ledgers);
    if (ledgers.Count == 0)
    {
      throw new ArgumentException("A cave pass must contain at least one recipe.", nameof(ledgers));
    }

    List<LegacyTileRunnerPassProvenance> recipes = new();
    HashSet<string> recipeNames = new(StringComparer.Ordinal);
    foreach (LegacyTileRunnerPassExecutionLedger ledger in ledgers)
    {
      ArgumentNullException.ThrowIfNull(ledger);
      if (ledger.PassName != passName || !recipeNames.Add(ledger.RecipeName))
      {
        throw new InvalidOperationException(
          "TileRunner recipe ledger did not belong uniquely to the cave pass.");
      }

      IReadOnlyList<LegacyTileRunnerInvocationProvenance> invocations = ledger.Complete();
      recipes.Add(LegacyTileRunnerPassProvenanceFactory.Create(passName, invocations));
    }

    recipes.Sort(static (first, second) =>
      string.CompareOrdinal(first.Invocations[0].RecipeName, second.Invocations[0].RecipeName));
    return new LegacyTileRunnerPassExecutionSetResult(
      passName,
      recipes.AsReadOnly(),
      recipes.Sum(recipe => recipe.Invocations.Count),
      recipes.Sum(recipe => recipe.TileCommandCount),
      recipes.Sum(recipe => recipe.LiquidCommandCount));
  }
}
