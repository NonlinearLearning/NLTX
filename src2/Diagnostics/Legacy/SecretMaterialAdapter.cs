namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class SecretMaterialAdapter
{
  private readonly byte[] _salt;

  public SecretMaterialAdapter(ReadOnlySpan<byte> salt)
  {
    _salt = salt.ToArray();
  }

  public ReadOnlyMemory<byte> Salt => _salt;
}
