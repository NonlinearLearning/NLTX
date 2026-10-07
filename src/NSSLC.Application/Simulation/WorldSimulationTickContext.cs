using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldSession.Components;

namespace Terraria.NonAuthoritative.Simulation;

public sealed class WorldSimulationTickContext
{
  internal WorldSimulationTickContext(
    LoadedWorldSession session,
    long tickNumber,
    WorldTickSnapshot worldSnapshot)
  {
    Session = session;
    TickNumber = tickNumber;
    WorldSnapshot = worldSnapshot;
  }

  public LoadedWorldSession Session { get; }

  public long TickNumber { get; }

  public WorldTickSnapshot WorldSnapshot { get; }
}
