using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Passes;

/// <summary>
/// Executes one registered world-generation pass operation.
/// </summary>
public delegate WorldGenerationPassRunOutput WorldGenerationPassExecutionCallback(
  in WorldGenerationPassStateComponent activeState,
  GenerationPassDescriptor descriptor,
  WorldGenerationConfigurationSnapshot configuration,
  Action<string, double> reportProgress);
