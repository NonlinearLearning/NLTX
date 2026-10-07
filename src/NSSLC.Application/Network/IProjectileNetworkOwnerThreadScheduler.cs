namespace Terraria.Network;

/// <summary>Schedules ECS-mutating network owner work on its EntityRuntime owner thread.</summary>
public interface IProjectileNetworkOwnerThreadScheduler
{
  /// <summary>
  /// Queues the synchronous operation for the owning EntityRuntime thread. Implementations must
  /// not silently run it inline from a non-owner thread, must observe cancellation before executing
  /// queued work, and must serialize it with world publication and other owner-thread commands.
  /// </summary>
  ValueTask<PacketHandlingResult> ExecuteAsync(
    Func<PacketHandlingResult> operation,
    CancellationToken cancellationToken);
}
