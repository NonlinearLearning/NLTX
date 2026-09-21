namespace Terraria.WorldSession.Runtime;

public readonly record struct RuntimeBuildIdentity
{
  public string VersionNumber { get; }

  public string ProtocolVersion { get; }

  public RuntimeBuildIdentity(string versionNumber, string protocolVersion)
  {
    VersionNumber = RequireText(versionNumber, nameof(versionNumber));
    ProtocolVersion = RequireText(protocolVersion, nameof(protocolVersion));
  }

  private static string RequireText(string value, string parameterName)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException("A build identity value is required.", parameterName);
    }

    return value;
  }
}
