using System.Collections.Immutable;
using NLTX.EconomyCraftingFishingLoot.Loot;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class DropRuleOptionSelector
{
  public DropRuleOptionSelector(
    IEnumerable<DropRuleReference> ruleOptions,
    int chanceDenominator,
    int chanceNumerator)
  {
    ArgumentNullException.ThrowIfNull(ruleOptions);
    if (chanceDenominator <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(chanceDenominator));
    }

    if (chanceNumerator < 0 || chanceNumerator > chanceDenominator)
    {
      throw new ArgumentOutOfRangeException(nameof(chanceNumerator));
    }

    RuleOptions = ruleOptions.ToImmutableArray();
    if (RuleOptions.Any(rule => string.IsNullOrWhiteSpace(rule.RuleId)))
    {
      throw new ArgumentException(
        "Drop rule options must have non-empty IDs.",
        nameof(ruleOptions));
    }

    ChanceDenominator = chanceDenominator;
    ChanceNumerator = chanceNumerator;
  }

  public ImmutableArray<DropRuleReference> RuleOptions { get; }

  public int ChanceDenominator { get; }

  public int ChanceNumerator { get; }

  public float PersonalDropRate => (float)ChanceNumerator / ChanceDenominator;

  public bool TrySelect(
    DropResolutionContext context,
    out DropRuleReference rule)
  {
    ArgumentNullException.ThrowIfNull(context);
    rule = default;
    if (RuleOptions.Length == 0)
    {
      return false;
    }

    if (context.Random.Next(ChanceDenominator) >= ChanceNumerator)
    {
      return false;
    }

    int selectedIndex = context.Random.Next(RuleOptions.Length);
    if ((uint)selectedIndex >= (uint)RuleOptions.Length)
    {
      throw new InvalidOperationException(
        "The drop random source returned an index outside the option range.");
    }

    rule = RuleOptions[selectedIndex];
    return true;
  }
}
