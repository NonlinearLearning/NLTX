namespace NSSLC.Infrastructure.Lvcs;

/// <summary>
/// In-memory <see cref="ILvcsUnit" />, used by sources that already hold their bytes.
/// </summary>
public sealed class LvcsUnit : ILvcsUnit
{
  public LvcsUnit(string location, ReadOnlyMemory<byte> payload)
  {
    if (string.IsNullOrWhiteSpace(location))
    {
      throw new ArgumentException("A unit location is required.", nameof(location));
    }

    Location = location;
    Payload = payload;
  }

  public string Location { get; }

  public ReadOnlyMemory<byte> Payload { get; }
}
