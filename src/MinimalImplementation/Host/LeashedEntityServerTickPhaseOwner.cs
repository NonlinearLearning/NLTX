using System;
using MinimalImplementation.Persistence;
using MinimalImplementation.Server.Mechanisms;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Adapts the P06 active-section tick boundary to the fixed server tick contract.
/// </summary>
/// <remarks>
/// A real registration must supply both the prototype-specific motion port and the effect port.
/// This owner never invents motion, ignores section output, or treats a missing adapter as a
/// successful tick.
/// </remarks>
public sealed class LeashedEntityServerTickPhaseOwner : IServerTickPhaseOwner
{
  private readonly LeashedEntitySectionService _sections;
  private readonly ILeashedEntityMotionPort _motion;
  private readonly ILeashedEntityTickEffectPort _effects;

  public LeashedEntityServerTickPhaseOwner(
    LeashedEntitySectionService sections,
    ILeashedEntityMotionPort motion,
    ILeashedEntityTickEffectPort effects)
  {
    _sections = sections ?? throw new ArgumentNullException(nameof(sections));
    _motion = motion ?? throw new ArgumentNullException(nameof(motion));
    _effects = effects ?? throw new ArgumentNullException(nameof(effects));
  }

  public ServerTickPhase Phase => ServerTickPhase.LeashedEntity;

  public object TransactionIdentity => this;

  public void Advance(long gameTickIndex)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(gameTickIndex);
    LeashedEntitySectionService.SectionTickResult result =
      _sections.AdvanceActiveSections(gameTickIndex, _motion);
    _effects.Publish(result);
  }
}
