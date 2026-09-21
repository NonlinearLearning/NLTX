namespace Terraria.WorldGeneration.Definitions;

public interface IWorldSecretSeedCodeTransformer
{
  string Transform(string normalizedInput);
}
