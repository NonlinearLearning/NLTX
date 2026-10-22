using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Encoded combat packet body plus the session excluded from Version4 server broadcast.</summary>
public sealed class ProjectilePacketBody
{
  private readonly byte[] _payload;

  public ProjectilePacketBody(byte messageId, SessionEpoch? ignoreEpoch, ReadOnlySpan<byte> payload)
  {
    if (messageId is not (27 or 29 or 142))
    {
      throw new ArgumentOutOfRangeException(nameof(messageId));
    }

    if (ignoreEpoch is SessionEpoch epoch && !epoch.IsValid)
    {
      throw new ArgumentException("A broadcast exclusion requires a valid session epoch.", nameof(ignoreEpoch));
    }

    if (ignoreEpoch is null && messageId is not (27 or 29 or 142))
    {
      throw new ArgumentException(
        "A null exclusion is reserved for server-origin packet 27, portal termination, " +
        "and targeted tracker snapshots.",
        nameof(ignoreEpoch));
    }

    MessageId = messageId;
    IgnoreEpoch = ignoreEpoch;
    _payload = payload.ToArray();
  }

  public byte MessageId { get; }

  public SessionEpoch? IgnoreEpoch { get; }

  public ReadOnlyMemory<byte> Payload => _payload;
}
