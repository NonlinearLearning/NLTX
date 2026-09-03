using System;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerIdentityComponent
{
  public PlayerIdentityComponent(PlayerHandle player, byte assignedSlot, string canonicalAccountUuid)
  {
    ArgumentNullException.ThrowIfNull(canonicalAccountUuid);
    Player = player;
    AssignedSlot = assignedSlot;
    CanonicalAccountUuid = canonicalAccountUuid;
  }

  public byte AssignedSlot;
  public string CanonicalAccountUuid;
  public PlayerHandle Player;
}
