using System.Numerics;
using Terraria.Player.Environment;

namespace Terraria.Player.Luck;

public static class PlayerWallRescanSystem
{
  private const int UnbreakableWallRescanPeriod = 20;
  private const int UnbreakableWallRescanDistance = 256;

  public static PlayerWallRescanResult Execute(
    in PlayerWallRescanInput input,
    PlayerLuckAndRescanStateComponent luckAndRescan,
    PlayerEnvironmentDetectionAndSpawnStateComponent environment,
    IPlayerWallRescanPort port)
  {
    ArgumentNullException.ThrowIfNull(luckAndRescan);
    ArgumentNullException.ThrowIfNull(environment);
    ArgumentNullException.ThrowIfNull(port);

    bool wasInside = environment.InsideUnbreakableWalls;
    if (!input.DualDungeonsSeed)
    {
      return new PlayerWallRescanResult(false, wasInside, wasInside);
    }

    bool shouldScan = input.Force;
    if (!shouldScan)
    {
      luckAndRescan.UnbreakableWallScanCooldown--;
      shouldScan =
        luckAndRescan.UnbreakableWallScanCooldown <= 0 ||
        !(Vector2.Distance(
          input.PlayerCenter,
          luckAndRescan.UnbreakableWallScanLastPosition) <
          UnbreakableWallRescanDistance);
    }

    if (!shouldScan)
    {
      return new PlayerWallRescanResult(false, wasInside, wasInside);
    }

    bool isInside = port.ScanInsideUnbreakableWalls(input.PlayerCenter);
    environment.InsideUnbreakableWalls = isInside;
    luckAndRescan.UnbreakableWallScanCooldown = UnbreakableWallRescanPeriod;
    luckAndRescan.UnbreakableWallScanLastPosition = input.PlayerCenter;

    if (isInside != wasInside)
    {
      port.BroadcastChange();
    }

    return new PlayerWallRescanResult(true, wasInside, isInside);
  }
}
