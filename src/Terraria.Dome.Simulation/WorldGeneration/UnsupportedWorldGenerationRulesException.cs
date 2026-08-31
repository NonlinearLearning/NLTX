using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class UnsupportedWorldGenerationRulesException : NotSupportedException
{
  public UnsupportedWorldGenerationRulesException(WorldGenerationRequest request)
    : base(CreateMessage(request))
  {
    ArgumentNullException.ThrowIfNull(request);
    SeedVariant = request.SeedVariant;
    Difficulty = request.Rules.Difficulty;
    IsHardmode = request.Rules.IsHardmode;
    Reasons = CreateReasons(request);
  }

  public string SeedVariant { get; }

  public int Difficulty { get; }

  public bool IsHardmode { get; }

  public IReadOnlyList<string> Reasons { get; }

  private static string CreateMessage(WorldGenerationRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    return "The current world-generation pipeline supports only default world rules: " +
      string.Join(", ", CreateReasons(request));
  }

  private static IReadOnlyList<string> CreateReasons(WorldGenerationRequest request)
  {
    List<string> reasons = new();
    if (request.SeedVariant != "default" || request.Rules.SecretSeedVariant != "default")
    {
      string variant = request.SeedVariant != "default"
        ? request.SeedVariant
        : request.Rules.SecretSeedVariant;
      if (SecretSeedDefinitionRegistry.TryGet(variant, out SecretSeedDefinition definition))
      {
        reasons.Add(
          $"secret-seed '{definition.Variant}' is {definition.Status} ({definition.SourceAnchor})");
      }
      else
      {
        reasons.Add($"secret-seed variant '{variant}' is unknown and deferred");
      }
    }

    if (request.Rules.Difficulty != 0)
    {
      reasons.Add("difficulty behavior is deferred");
    }

    if (request.Rules.IsHardmode)
    {
      reasons.Add("hardmode behavior is deferred");
    }

    return reasons.AsReadOnly();
  }
}
