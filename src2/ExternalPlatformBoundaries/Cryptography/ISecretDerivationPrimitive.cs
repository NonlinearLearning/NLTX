namespace Terraria.ExternalPlatformBoundaries.Cryptography;

public interface ISecretDerivationPrimitive
{
  byte[] CryptRaw(byte[] input, byte[] salt, int workFactor);
}
