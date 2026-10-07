using System;
using System.Linq;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Adapters;

public static class WorldSecretSeedRuntimeRegistrySerializationAdapter
{
  public static WorldSecretSeedRuntimeRegistrySerializedState Serialize(
    WorldSecretSeedRuntimeRegistrySnapshot snapshot)
  {
    string[] keys = snapshot.EnabledVariants
      .OrderBy(static variant => variant, StringComparer.Ordinal)
      .ToArray();
    return new WorldSecretSeedRuntimeRegistrySerializedState(
      snapshot.GenerationId,
      snapshot.RuntimeVersion,
      keys);
  }
}
