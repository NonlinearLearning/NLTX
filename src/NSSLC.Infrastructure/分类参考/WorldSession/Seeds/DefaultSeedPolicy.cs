namespace Terraria.WorldSession.Seeds;

public readonly record struct DefaultSeedPolicy(string? SeedName)
{
  public bool HasSeed => !string.IsNullOrWhiteSpace(SeedName);
}
