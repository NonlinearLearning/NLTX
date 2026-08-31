using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public readonly record struct ProjectileNetworkUpdateResult(
  ProjectileNetworkUpdateComponent State,
  bool ShouldSend);

public sealed class ProjectileNetworkUpdatePolicy
{
  private const int NetSpamCap = 60;
  private const int NetSpamIncrement = 5;

  public ProjectileNetworkUpdateComponent RequestSecondaryUpdate(
    ProjectileNetworkUpdateComponent state)
  {
    return new ProjectileNetworkUpdateComponent(
      true,
      state.NetSpam,
      state.PrimaryUpdatePending,
      state.SendRequested);
  }

  public ProjectileNetworkUpdateComponent RequestPrimaryUpdate(
    ProjectileNetworkUpdateComponent state)
  {
    return new ProjectileNetworkUpdateComponent(
      state.SecondaryUpdatePending,
      state.NetSpam,
      true,
      state.SendRequested);
  }

  public ProjectileNetworkUpdateComponent AcknowledgePrimaryUpdate(
    ProjectileNetworkUpdateComponent state)
  {
    return new ProjectileNetworkUpdateComponent(
      state.SecondaryUpdatePending,
      state.NetSpam,
      false,
      state.SendRequested);
  }

  public ProjectileNetworkUpdateResult Tick(ProjectileNetworkUpdateComponent state)
  {
    int netSpam = state.NetSpam;
    bool shouldSend = false;
    bool pending = state.PrimaryUpdatePending || state.SecondaryUpdatePending;
    if (pending && netSpam < NetSpamCap)
    {
      netSpam += NetSpamIncrement;
      pending = false;
      shouldSend = true;
    }

    if (netSpam > 0)
    {
      netSpam--;
    }

    return new ProjectileNetworkUpdateResult(
      new ProjectileNetworkUpdateComponent(
        pending,
        netSpam,
        primaryUpdatePending: false,
        sendRequested: shouldSend),
      shouldSend);
  }
}
