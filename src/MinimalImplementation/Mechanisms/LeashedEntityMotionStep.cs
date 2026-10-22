using MinimalImplementation.Server.Protocol;

namespace MinimalImplementation.Server.Mechanisms;

public sealed record LeashedEntityMotionStep(
  Version4ModulePayloads.LeashedEntityState PartialState,
  LeashedEntityMotionState Motion);
