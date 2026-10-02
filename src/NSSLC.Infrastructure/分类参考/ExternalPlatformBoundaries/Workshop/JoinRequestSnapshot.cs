namespace Terraria.ExternalPlatformBoundaries.Workshop;

public sealed class JoinRequestSnapshot
{
  public JoinRequestSnapshot(
    string userDisplayName,
    string externalUserIdentifier,
    DateTimeOffset expiresAt,
    int generation)
  {
    ArgumentNullException.ThrowIfNull(userDisplayName);
    ArgumentNullException.ThrowIfNull(externalUserIdentifier);
    UserDisplayName = userDisplayName;
    ExternalUserIdentifier = externalUserIdentifier;
    ExpiresAt = expiresAt;
    Generation = generation;
  }

  public string UserDisplayName { get; }

  public string ExternalUserIdentifier { get; }

  public DateTimeOffset ExpiresAt { get; }

  public int Generation { get; }
}
