namespace Terraria.Items;

public readonly record struct WorldVector(float X, float Y)
{
  public static WorldVector Zero => new(0.0f, 0.0f);
}
