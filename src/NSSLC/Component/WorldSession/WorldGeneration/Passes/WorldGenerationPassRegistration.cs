using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Passes;

/// <summary>
/// Describes one explicitly registered pass operation and its enabled state.
/// </summary>
public readonly record struct WorldGenerationPassRegistration
{
  public WorldGenerationPassRegistration(
    GenerationPassDescriptor descriptor,
    WorldGenerationPassExecutionCallback? callback,
    bool enabled = true)
  {
    if (enabled && callback is null)
    {
      throw new ArgumentException(
        "An enabled pass must provide an execution callback.",
        nameof(callback));
    }

    Descriptor = descriptor;
    Callback = callback;
    Enabled = enabled;
  }

  public GenerationPassDescriptor Descriptor { get; }

  public WorldGenerationPassExecutionCallback? Callback { get; }

  public bool Enabled { get; }
}
