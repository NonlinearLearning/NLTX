namespace Terraria.ExternalPlatformBoundaries.Cryptography;

public interface ISecretDerivationPort
{
  string ToSecret(string plainInput);
}
