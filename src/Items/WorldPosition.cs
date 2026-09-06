namespace Terraria.Items;

public readonly record struct WorldPosition(float X, float Y)
{
  public static WorldPosition Origin => new(0.0f, 0.0f);
}
