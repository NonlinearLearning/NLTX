namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct StructureFootprintComponent(int Width, int Height)
{
  public bool Contains(int localX, int localY)
  {
    return localX >= 0 && localX < Width && localY >= 0 && localY < Height;
  }
}
