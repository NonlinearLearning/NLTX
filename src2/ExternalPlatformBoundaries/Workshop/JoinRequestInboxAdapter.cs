namespace Terraria.ExternalPlatformBoundaries.Workshop;

public sealed class JoinRequestInboxAdapter
{
  private readonly Dictionary<string, JoinRequestSnapshot> _requests =
    new(StringComparer.Ordinal);

  public void AddOrReplace(JoinRequestSnapshot request)
  {
    ArgumentNullException.ThrowIfNull(request);
    _requests[request.ExternalUserIdentifier] = request;
  }

  public bool Remove(string externalUserIdentifier, int generation)
  {
    ArgumentNullException.ThrowIfNull(externalUserIdentifier);
    if (!_requests.TryGetValue(externalUserIdentifier, out JoinRequestSnapshot? request) ||
      request.Generation != generation)
    {
      return false;
    }

    return _requests.Remove(externalUserIdentifier);
  }

  public int PruneExpired(DateTimeOffset now)
  {
    string[] expired = _requests.Values
      .Where(request => request.ExpiresAt <= now)
      .Select(request => request.ExternalUserIdentifier)
      .ToArray();
    foreach (string identifier in expired)
    {
      _requests.Remove(identifier);
    }

    return expired.Length;
  }

  public IReadOnlyList<JoinRequestSnapshot> Snapshot()
  {
    return Array.AsReadOnly(_requests.Values.ToArray());
  }
}
