using MinimalImplementation.Server.Protocol;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Full network projection and unquantized motion created by the same spawn.</summary>
public sealed record LeashedEntitySpawnState(
  Version4ModulePayloads.LeashedEntityState FullState,
  LeashedEntityMotionState Motion);
