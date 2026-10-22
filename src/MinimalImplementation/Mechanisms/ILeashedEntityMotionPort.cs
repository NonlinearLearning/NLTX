namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// Prepares deterministic motion and its Partial projection without mutating the registry.
/// </summary>
/// <remarks>
/// The port owns prototype-specific movement and definition rules. The P06 section service
/// owns section eligibility, state validation, revisioning and projection cadence; it does not
/// implement NPC or projectile movement itself.
/// </remarks>
public interface ILeashedEntityMotionPort
{
  bool TryAdvance(
    LeashedEntityRuntimeSnapshot entity,
    long tick,
    out LeashedEntityMotionStep? step);
}
