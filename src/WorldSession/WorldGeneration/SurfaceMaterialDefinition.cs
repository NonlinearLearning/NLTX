namespace Terraria.WorldGeneration.Components;

public readonly record struct SurfaceMaterialDefinition(
  ushort CrimsonStoneWall,
  ushort CrimsonStone,
  ushort EbonStoneWall,
  ushort EbonStone,
  ushort MossTile,
  ushort MossWall)
{
  public static SurfaceMaterialDefinition Version4 { get; } = new(
    CrimsonStoneWall: 83,
    CrimsonStone: 203,
    EbonStoneWall: 3,
    EbonStone: 25,
    MossTile: 179,
    MossWall: 54);
}
