namespace Terraria.ExternalPlatformBoundaries.Social.WeGame;

public sealed class WeGameIpcTransportOptions
{
  public WeGameIpcTransportOptions(int bufferSize = 256, int maxFrameBytes = 1024 * 1024)
  {
    if (bufferSize <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(bufferSize));
    }

    if (maxFrameBytes < bufferSize)
    {
      throw new ArgumentOutOfRangeException(nameof(maxFrameBytes));
    }

    BufferSize = bufferSize;
    MaxFrameBytes = maxFrameBytes;
  }

  public int BufferSize { get; }

  public int MaxFrameBytes { get; }
}
