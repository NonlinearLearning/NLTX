namespace Terraria.ExternalPlatformBoundaries.Workshop;

public static class RichPresenceQuery
{
  public static RichPresenceGameMode Compute(RichPresenceGameSnapshot snapshot)
  {
    if (snapshot.IsMenu)
    {
      return RichPresenceGameMode.Menu;
    }

    if (snapshot.IsServer)
    {
      return RichPresenceGameMode.DedicatedServer;
    }

    return snapshot.IsMultiplayer
      ? RichPresenceGameMode.Multiplayer
      : RichPresenceGameMode.SinglePlayer;
  }
}
