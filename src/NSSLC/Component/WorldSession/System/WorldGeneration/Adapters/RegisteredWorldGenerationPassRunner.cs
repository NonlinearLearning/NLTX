using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;
using Terraria.WorldGeneration.Passes;

namespace Terraria.WorldGeneration.Adapters;

/// <summary>
/// Dispatches active passes to callbacks registered by exact pass identifier.
/// </summary>
/// <remarks>
/// The callback collection is copied at construction. Callback implementations must obey the
/// synchronous progress-reporting contract of <see cref="IWorldGenerationPassRunner"/>.
/// </remarks>
public sealed class RegisteredWorldGenerationPassRunner : IWorldGenerationPassRunner
{
  private readonly Dictionary<string, WorldGenerationPassExecutionCallback> _callbacks;

  public RegisteredWorldGenerationPassRunner(
    IReadOnlyCollection<KeyValuePair<string, WorldGenerationPassExecutionCallback>> callbacks)
  {
    ArgumentNullException.ThrowIfNull(callbacks);
    _callbacks = new Dictionary<string, WorldGenerationPassExecutionCallback>(
      callbacks.Count,
      StringComparer.Ordinal);

    foreach (KeyValuePair<string, WorldGenerationPassExecutionCallback> callback in callbacks)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(callback.Key);
      ArgumentNullException.ThrowIfNull(callback.Value);
      if (!_callbacks.TryAdd(callback.Key, callback.Value))
      {
        throw new ArgumentException(
          $"A callback is already registered for pass '{callback.Key}'.",
          nameof(callbacks));
      }
    }
  }

  public WorldGenerationPassRunOutput Execute(
    in WorldGenerationPassStateComponent activeState,
    GenerationPassDescriptor descriptor,
    WorldGenerationConfigurationSnapshot configuration,
    Action<string, double> reportProgress)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(descriptor.Id);
    ArgumentNullException.ThrowIfNull(reportProgress);
    if (!activeState.HasActivePass ||
        !string.Equals(activeState.ActivePassId, descriptor.Id, StringComparison.Ordinal) ||
        activeState.PassVersion != descriptor.Version)
    {
      throw new ArgumentException(
        "The active pass state must match the registered pass descriptor.",
        nameof(activeState));
    }

    if (!_callbacks.TryGetValue(descriptor.Id, out WorldGenerationPassExecutionCallback? callback))
    {
      throw new InvalidOperationException(
        $"No execution callback is registered for pass '{descriptor.Id}'.");
    }

    return callback(
      in activeState,
      descriptor,
      configuration,
      reportProgress) ?? throw new InvalidOperationException(
        $"The execution callback for pass '{descriptor.Id}' returned no output.");
  }
}
