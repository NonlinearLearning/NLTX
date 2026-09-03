using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct DoorAuthorizationPolicy(PlayerHandle? Actor, bool IsAllowed)
{
  public static DoorAuthorizationPolicy Denied(PlayerHandle? actor = null) => new(actor, false);

  public static DoorAuthorizationPolicy Allowed(PlayerHandle actor) => new(actor, true);
}
