using MinimalImplementation.Contracts;

namespace MinimalImplementation.Modules.Creative;

public sealed record CreativePowerApplyResult(
  OwnerCommitResult Commit,
  CreativePowerEffect? Effect);
