using System;

namespace Terraria.Player.Environment;

public static class PlayerEnvironmentBuffImmunitySystem
{
  private const int TeleportImmunityDuration = 4;

  public static int Tick(PlayerZoneAndEnvironmentStateComponent environment)
  {
    ArgumentNullException.ThrowIfNull(environment);

    environment.EnvironmentBuffImmunityTimer = Math.Max(
      0,
      environment.EnvironmentBuffImmunityTimer - 1);
    return environment.EnvironmentBuffImmunityTimer;
  }

  public static int BeginTeleportImmunity(
    PlayerZoneAndEnvironmentStateComponent environment)
  {
    ArgumentNullException.ThrowIfNull(environment);

    environment.EnvironmentBuffImmunityTimer = TeleportImmunityDuration;
    return TeleportImmunityDuration;
  }
}
