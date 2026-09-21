namespace Terraria.WorldSession.Runtime;

public readonly record struct WorldGenerationRequest(string? SeedName)
{
  public bool HasSeed => !string.IsNullOrWhiteSpace(SeedName);
}
