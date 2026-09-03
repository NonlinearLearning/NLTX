using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonFeaturePlacementPolicy policy = new();
if (!policy.CanGenerateFeatureAt(int.MinValue, int.MinValue) ||
    !policy.CanGenerateFeatureAt(0, 0) ||
    !policy.CanGenerateFeatureAt(int.MaxValue, int.MaxValue))
{
  throw new InvalidOperationException("Default dungeon feature policy diverged.");
}

Console.WriteLine("PASS: Dungeon feature placement policy contract");
