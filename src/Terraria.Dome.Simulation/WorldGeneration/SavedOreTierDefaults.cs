namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct SavedOreTierDefaults(
  ushort CopperTileType,
  ushort IronTileType,
  ushort SilverTileType,
  ushort GoldTileType,
  ushort CobaltTileType,
  ushort MythrilTileType,
  ushort AdamantiteTileType)
{
  public static SavedOreTierDefaults Version4 { get; } = new(
    CopperTileType: 7,
    IronTileType: 6,
    SilverTileType: 9,
    GoldTileType: 8,
    CobaltTileType: 107,
    MythrilTileType: 108,
    AdamantiteTileType: 111);
}
