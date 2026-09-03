using System;

namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileNetworkUpdateComponent
{
  public ProjectileNetworkUpdateComponent(
    bool secondaryUpdatePending = false,
    int netSpam = 0,
    bool primaryUpdatePending = false,
    bool sendRequested = false)
  {
    if (netSpam < 0 || netSpam > 63)
    {
      throw new ArgumentOutOfRangeException(nameof(netSpam));
    }

    SecondaryUpdatePending = secondaryUpdatePending;
    NetSpam = netSpam;
    PrimaryUpdatePending = primaryUpdatePending;
    SendRequested = sendRequested;
  }

  public bool SecondaryUpdatePending { get; }

  public int NetSpam { get; }

  public bool PrimaryUpdatePending { get; }

  public bool SendRequested { get; }
}
