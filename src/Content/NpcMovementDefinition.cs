namespace Terraria.Content;

public sealed record NpcMovementDefinition(int Width, int Height, float Scale = 1f)
{
  public float WaterMovementSpeed { get; init; } = 1f;

  public float LavaMovementSpeed { get; init; } = 1f;

  public float HoneyMovementSpeed { get; init; } = 1f;

  public float? GravityMultiplier { get; init; }
}
