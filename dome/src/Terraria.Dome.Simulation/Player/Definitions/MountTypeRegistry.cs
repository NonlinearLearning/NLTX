namespace Terraria.Dome.Simulation.Player.Definitions;

public static class MountTypeRegistry
{
  public const int None = -1;
  public const int Count = 64;

  public static bool IsDefined(int mountType)
  {
    return mountType >= 0 && mountType < Count;
  }

  public static bool IsValid(int mountType)
  {
    return mountType == None || IsDefined(mountType);
  }
}
