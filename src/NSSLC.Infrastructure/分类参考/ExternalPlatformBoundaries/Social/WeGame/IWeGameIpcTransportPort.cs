namespace Terraria.ExternalPlatformBoundaries.Social.WeGame;

public interface IWeGameIpcTransportPort
{
  IpcTransportResult Open();

  IpcTransportResult Close();

  IpcTransportResult AcceptReadChunk(ReadOnlySpan<byte> chunk, bool isMessageComplete);

  IReadOnlyList<WeGameIpcFrame> DrainCompleteFrames();

  ValueTask<IpcTransportResult> SendAsync(
    string payload,
    CancellationToken cancellationToken = default);

  WeGameIpcTransportStatus Status { get; }
}
