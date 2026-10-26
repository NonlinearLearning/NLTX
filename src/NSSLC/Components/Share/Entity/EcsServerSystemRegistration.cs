using System;
using Arch.System;

namespace Terraria.Entity;

/// <summary>
/// Associates one Arch system with an explicit NSSLC server phase.
/// </summary>
public sealed class EcsServerSystemRegistration
{
  public EcsServerSystemRegistration(
      EcsServerPhase phase,
      ISystem<EcsServerTickContext> system)
  {
    if (!Enum.IsDefined(phase))
    {
      throw new ArgumentOutOfRangeException(nameof(phase));
    }

    ArgumentNullException.ThrowIfNull(system);
    Phase = phase;
    System = system;
  }

  public EcsServerPhase Phase { get; }

  public ISystem<EcsServerTickContext> System { get; }
}
