using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Definitions;

public sealed class WorldSeedOptionDefinition
{
  public WorldSeedOptionDefinition(
    WorldSeedOptionId id,
    string keyName,
    string? serverConfigName,
    IReadOnlyList<string>? specialSeedNames = null,
    IReadOnlyList<int>? specialSeedValues = null,
    IReadOnlyList<WorldSeedOptionId>? dependencies = null)
  {
    if (!Enum.IsDefined(id))
    {
      throw new ArgumentOutOfRangeException(nameof(id));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(keyName);
    if (serverConfigName is not null &&
        string.IsNullOrWhiteSpace(serverConfigName))
    {
      throw new ArgumentException(
        "A configured server option name must not be blank.",
        nameof(serverConfigName));
    }

    Id = id;
    KeyName = keyName;
    ServerConfigName = serverConfigName;
    SpecialSeedNames = CopyNames(specialSeedNames);
    SpecialSeedValues = CopyValues(specialSeedValues);
    Dependencies = CopyDependencies(dependencies);
  }

  public WorldSeedOptionId Id { get; }

  public string KeyName { get; }

  public string? ServerConfigName { get; }

  public IReadOnlyList<string> SpecialSeedNames { get; }

  public IReadOnlyList<int> SpecialSeedValues { get; }

  public IReadOnlyList<WorldSeedOptionId> Dependencies { get; }

  private static IReadOnlyList<string> CopyNames(IReadOnlyList<string>? source)
  {
    if (source is null || source.Count == 0)
    {
      return Array.Empty<string>();
    }

    List<string> copy = new(source.Count);
    foreach (string? name in source)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(name);
      copy.Add(name);
    }

    return copy.AsReadOnly();
  }

  private static IReadOnlyList<int> CopyValues(IReadOnlyList<int>? source)
  {
    if (source is null || source.Count == 0)
    {
      return Array.Empty<int>();
    }

    return new List<int>(source).AsReadOnly();
  }

  private static IReadOnlyList<WorldSeedOptionId> CopyDependencies(
    IReadOnlyList<WorldSeedOptionId>? source)
  {
    if (source is null || source.Count == 0)
    {
      return Array.Empty<WorldSeedOptionId>();
    }

    List<WorldSeedOptionId> copy = new(source.Count);
    foreach (WorldSeedOptionId id in source)
    {
      if (!Enum.IsDefined(id))
      {
        throw new ArgumentOutOfRangeException(nameof(source));
      }

      copy.Add(id);
    }

    return copy.AsReadOnly();
  }
}
