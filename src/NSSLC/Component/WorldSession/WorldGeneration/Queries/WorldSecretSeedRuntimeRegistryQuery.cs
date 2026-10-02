using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

public static class WorldSecretSeedRuntimeRegistryQuery
{
  public static WorldSecretSeedRuntimeRegistrySnapshot Snapshot(
    WorldSecretSeedRuntimeRegistryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
