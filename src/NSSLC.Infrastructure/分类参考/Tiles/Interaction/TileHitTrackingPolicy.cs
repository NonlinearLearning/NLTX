namespace Terraria.Tiles.Interaction;

public static class TileHitTrackingPolicy
{
  public const int Capacity = 500;
  public const int ArraySize = Capacity + 1;
  public const int SentinelSlotIndex = Capacity;
  public const int DefaultLifetimeTicks = 60;
  public const int CrackStyleCount = 4;
}
