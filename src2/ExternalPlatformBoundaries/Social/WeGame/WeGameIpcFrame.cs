namespace Terraria.ExternalPlatformBoundaries.Social.WeGame;

public sealed class WeGameIpcFrame
{
  public WeGameIpcFrame(byte[] payload)
  {
    ArgumentNullException.ThrowIfNull(payload);
    Payload = payload.ToArray();
  }

  public ReadOnlyMemory<byte> Payload { get; }
}
