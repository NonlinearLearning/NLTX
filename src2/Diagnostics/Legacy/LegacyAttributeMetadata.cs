namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class LegacyAttributeMetadata
{
  public LegacyAttributeMetadata(string message)
  {
    Message = message ?? string.Empty;
  }

  public string Message { get; }
}
