using System.Collections.Generic;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Server.Protocol;

namespace Terraria.Dome.Server.Replication;

public static class PlayerBootstrapProjection
{
  public static IReadOnlyList<byte[]> CreateFrames(
    byte playerSlot,
    PlayerPersistentState account)
  {
    return PlayerPersistentStateMapper.ToAuthorityFrames(playerSlot, account);
  }
}
