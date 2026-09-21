namespace Terraria.ExternalPlatformBoundaries.Social.WeGame;

public interface IWeGameIpcPipe : IAsyncDisposable
{
  ValueTask WriteAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
}
