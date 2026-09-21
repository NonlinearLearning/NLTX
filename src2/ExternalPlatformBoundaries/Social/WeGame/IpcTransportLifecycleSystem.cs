namespace Terraria.ExternalPlatformBoundaries.Social.WeGame;

public sealed class IpcTransportLifecycleSystem
{
  private readonly IWeGameIpcTransportPort _transport;

  public IpcTransportLifecycleSystem(IWeGameIpcTransportPort transport)
  {
    _transport = transport ?? throw new ArgumentNullException(nameof(transport));
  }

  public int Drain(Action<WeGameIpcFrame> publish)
  {
    ArgumentNullException.ThrowIfNull(publish);
    IReadOnlyList<WeGameIpcFrame> frames = _transport.DrainCompleteFrames();
    foreach (WeGameIpcFrame frame in frames)
    {
      publish(frame);
    }

    return frames.Count;
  }
}
