using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Passes;

namespace Terraria.WorldGeneration.Adapters;

/// <summary>
/// Builds an ordered plan and callback runner from explicit pass registrations.
/// </summary>
/// <remarks>
/// This adapter does not discover legacy passes or infer their effects. The caller owns the
/// registration order, callback implementations, and configuration interpretation.
/// </remarks>
public sealed class WorldGenerationPassCatalog
{
  public WorldGenerationPassCatalog(
    long generationId,
    int planVersion,
    IReadOnlyList<WorldGenerationPassRegistration> registrations)
  {
    ArgumentNullException.ThrowIfNull(registrations);

    List<GenerationPassDescriptor> descriptors = new(registrations.Count);
    List<string> disabledPassIds = new();
    List<KeyValuePair<string, WorldGenerationPassExecutionCallback>> callbacks = new();
    HashSet<string> passIds = new(StringComparer.Ordinal);
    foreach (WorldGenerationPassRegistration registration in registrations)
    {
      GenerationPassDescriptor descriptor = registration.Descriptor;
      if (!passIds.Add(descriptor.Id))
      {
        throw new ArgumentException(
          $"A pass is registered more than once: '{descriptor.Id}'.",
          nameof(registrations));
      }

      descriptors.Add(descriptor);
      if (!registration.Enabled)
      {
        disabledPassIds.Add(descriptor.Id);
        continue;
      }

      WorldGenerationPassExecutionCallback callback = registration.Callback ??
        throw new ArgumentException(
          "An enabled pass must provide an execution callback.",
          nameof(registrations));
      callbacks.Add(new KeyValuePair<string, WorldGenerationPassExecutionCallback>(
        descriptor.Id,
        callback));
    }

    Plan = new WorldGenerationPlanComponent(
      generationId,
      planVersion,
      descriptors,
      disabledPassIds);
    Runner = new RegisteredWorldGenerationPassRunner(callbacks);
  }

  public WorldGenerationPlanComponent Plan { get; }

  public IWorldGenerationPassRunner Runner { get; }
}
