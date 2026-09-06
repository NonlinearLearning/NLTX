using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldGenerationPlanComponent
{
  public WorldGenerationPlanComponent(
    long generationId,
    int planVersion,
    IReadOnlyList<GenerationPassDescriptor> passDescriptors,
    IReadOnlyCollection<string>? disabledPassIds = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (planVersion <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(planVersion));
    }

    ArgumentNullException.ThrowIfNull(passDescriptors);
    List<GenerationPassDescriptor> descriptors = new(passDescriptors.Count);
    HashSet<string> descriptorIds = new(StringComparer.Ordinal);
    double totalWeight = 0;
    foreach (GenerationPassDescriptor descriptor in passDescriptors)
    {
      if (string.IsNullOrWhiteSpace(descriptor.Id) ||
          !double.IsFinite(descriptor.Weight) || descriptor.Weight < 0 ||
          descriptor.Version <= 0)
      {
        throw new ArgumentException(
          "Generation pass descriptors must contain valid identity, weight, and version values.",
          nameof(passDescriptors));
      }

      if (!descriptorIds.Add(descriptor.Id))
      {
        throw new ArgumentException(
          "Generation pass identifiers must be unique.",
          nameof(passDescriptors));
      }

      descriptors.Add(descriptor);
      totalWeight += descriptor.Weight;
      if (!double.IsFinite(totalWeight))
      {
        throw new ArgumentOutOfRangeException(
          nameof(passDescriptors),
          "The total generation pass weight must be finite.");
      }
    }

    List<string> disabledIds = disabledPassIds is null
      ? []
      : new List<string>(disabledPassIds.Count);
    if (disabledPassIds is not null)
    {
      HashSet<string> disabledSet = new(StringComparer.Ordinal);
      foreach (string passId in disabledPassIds)
      {
        ArgumentException.ThrowIfNullOrWhiteSpace(passId);
        if (!descriptorIds.Contains(passId) || !disabledSet.Add(passId))
        {
          throw new ArgumentException(
            "Disabled passes must be unique members of the plan.",
            nameof(disabledPassIds));
        }

        disabledIds.Add(passId);
      }
    }

    GenerationId = generationId;
    PlanVersion = planVersion;
    PassDescriptors = descriptors.AsReadOnly();
    DisabledPassIds = disabledIds.AsReadOnly();
    TotalWeight = totalWeight;
  }

  public long GenerationId { get; }

  public int PlanVersion { get; }

  public IReadOnlyList<GenerationPassDescriptor> PassDescriptors { get; }

  public double TotalWeight { get; }

  public IReadOnlyList<string> DisabledPassIds { get; }
}
