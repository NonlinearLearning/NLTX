namespace Terraria.Dome.Simulation.Wiring.Components;

public readonly record struct MechanismCommitResult(bool Committed, bool RolledBack, long Sequence)
{
  public static MechanismCommitResult Rejected(long sequence) => new(false, true, sequence);

  public static MechanismCommitResult Accepted(long sequence) => new(true, false, sequence);
}
