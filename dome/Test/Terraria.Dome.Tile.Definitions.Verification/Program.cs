using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel.Definitions;

TileDefinitionRegistry first = TileDefinitionRegistry.RegisterDefaults();
TileDefinitionRegistry second = TileDefinitionRegistry.RegisterDefaults();
if (first.Definitions.Count != TileDefinitionRegistry.Version4TileCount ||
    second.Definitions.Count != TileDefinitionRegistry.Version4TileCount ||
    first.Definitions[0] != second.Definitions[0] ||
    first.Definitions[^1] != second.Definitions[^1])
{
  throw new InvalidOperationException("Tile defaults were not stable across registration.");
}

if (first.TryGet(-1, out _) || first.TryGet(TileDefinitionRegistry.Version4TileCount, out _))
{
  throw new InvalidOperationException("Unknown tile IDs were accepted by the registry.");
}

try
{
  ((IList<TileDefinition>)first.Definitions)[0] = first.Definitions[0];
  throw new InvalidOperationException("Tile definition projection was mutable.");
}
catch (NotSupportedException)
{
}

List<TileDefinition> duplicate = new(first.Definitions);
duplicate[1] = duplicate[0];
ExpectInvalid(duplicate, "duplicate tile IDs");

List<TileDefinition> outOfOrder = new(first.Definitions);
(outOfOrder[0], outOfOrder[1]) = (outOfOrder[1], outOfOrder[0]);
ExpectInvalid(outOfOrder, "out-of-order tile IDs");

List<TileDefinition> incomplete = new(first.Definitions);
incomplete.RemoveAt(incomplete.Count - 1);
ExpectInvalid(incomplete, "incomplete tile table");

Console.WriteLine("PASS: tile definition registry defaults, bounds, order, and immutability");

static void ExpectInvalid(IReadOnlyList<TileDefinition> definitions, string description)
{
  try
  {
    _ = TileDefinitionRegistry.Create(definitions);
  }
  catch (ArgumentException)
  {
    return;
  }

  throw new InvalidOperationException($"Registry accepted {description}.");
}
