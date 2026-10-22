namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Version4 PlayerItemSlotID allocation order, including the 200-slot banks.</summary>
public static class InventorySlotLayout
{
  public const int MainCount = 50;
  public const int CoinStart = 50;
  public const int AmmoStart = 54;
  public const int Mouse = 58;
  public const int ArmorStart = 59;
  public const int ArmorCount = 20;
  public const int DyeStart = 79;
  public const int DyeCount = 10;
  public const int VoidStart = 700;
  public const int BankCount = 200;
  public const int LoadoutStart = 900;
  public const int LoadoutSize = ArmorCount + DyeCount;
  public const int LoadoutCount = 3;
  public const int Count = LoadoutStart + LoadoutSize * LoadoutCount;
}
