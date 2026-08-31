using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record SecretSeedRuleSnapshotSet
{
  public SecretSeedRuleSnapshotSet(IReadOnlyList<SecretSeedRuleSnapshot> rules)
  {
    ArgumentNullException.ThrowIfNull(rules);
    Rules = new List<SecretSeedRuleSnapshot>(rules).AsReadOnly();
    int enabledCount = 0;
    foreach (SecretSeedRuleSnapshot rule in Rules)
    {
      if (rule.Enabled)
      {
        enabledCount++;
      }
    }

    ActiveSecretSeedCount = enabledCount;
  }

  public IReadOnlyList<SecretSeedRuleSnapshot> Rules { get; }

  public int ActiveSecretSeedCount { get; }
}

public static class SecretSeedRuleSnapshotQuery
{
  public static SecretSeedRuleSnapshotSet Create(IReadOnlySet<string> enabledVariants)
  {
    ArgumentNullException.ThrowIfNull(enabledVariants);
    IReadOnlyList<SecretSeedDefinition> definitions =
      SecretSeedDefinitionRegistry.RegisterDefaults();
    List<SecretSeedRuleSnapshot> rules = new(definitions.Count);
    foreach (SecretSeedDefinition definition in definitions)
    {
      rules.Add(new SecretSeedRuleSnapshot(
        definition,
        enabledVariants.Contains(definition.Variant)));
    }

    return new SecretSeedRuleSnapshotSet(rules);
  }
}
