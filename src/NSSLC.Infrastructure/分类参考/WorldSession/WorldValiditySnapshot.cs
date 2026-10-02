namespace Terraria.NonAuthoritative.WorldSession;

public sealed record WorldValiditySnapshot
{
  public WorldValiditySnapshot(WorldLoadStatus status, string? detail)
  {
    Status = status;
    Diagnostic = Sanitize(detail);
  }

  public WorldLoadStatus Status { get; }

  public string? Diagnostic { get; }

  public bool IsValid => Status == WorldLoadStatus.Ok;

  private static string? Sanitize(string? detail)
  {
    if (string.IsNullOrWhiteSpace(detail))
    {
      return null;
    }

    char[] sanitized = detail
      .Where(character => character is not '\r' and not '\n' and not '\0')
      .Take(256)
      .ToArray();
    return new string(sanitized).Trim();
  }
}
