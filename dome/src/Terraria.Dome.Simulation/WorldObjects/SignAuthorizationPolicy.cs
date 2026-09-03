namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct SignAuthorizationPolicy(bool IsAllowed, PlayerHandle? Actor = null)
{
  public static SignAuthorizationPolicy Denied => new(false);

  public static SignAuthorizationPolicy AllowedFor(PlayerHandle actor) => new(true, actor);
}
