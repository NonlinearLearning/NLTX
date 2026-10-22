namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// Receives the effects produced after one authoritative leashed-entity tick commit.
/// </summary>
/// <remarks>
/// The section service owns state mutation. This port owns the host-side handling of changed
/// snapshots and periodic projection candidates; a production adapter must provide that handling
/// explicitly instead of silently discarding the returned effects.
/// </remarks>
public interface ILeashedEntityTickEffectPort
{
  void Publish(LeashedEntitySectionService.SectionTickResult result);
}
