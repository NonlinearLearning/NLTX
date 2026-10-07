using Terraria.NonAuthoritative.Persistence;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>Hydrates the network host's world through the generated owner load APIs.</summary>
/// <remarks>
/// Call on the thread that will own the returned session. The network host retains this complete
/// candidate; this does not publish legacy Main state or run simulation load recovery effects.
/// The caller owns disposal, including after subsequent entity hydration fails.
/// </remarks>
public static class NetworkWorldSessionLoader {
  public static LoadedWorldSession Load(string worldPath) {
    ArgumentException.ThrowIfNullOrWhiteSpace(worldPath);
    var session = new LoadedWorldSession();
    try {
      var catalog = new GeneratedWorldLoadApiCatalog();
      WorldLoadCoordinator coordinator =
          WorldStorageCoordinatorFactory.CreateLoadCoordinator(catalog);
      WorldRecoveryOutcome outcome = coordinator.Load(worldPath,
          WorldStorageCoordinatorFactory.CreateGeneratedWorldLoadBindings(session));
      if (!outcome.CanPublishWorldLoaded || !session.IsComplete || session.SourceDocument is null) {
        throw new InvalidDataException($"The network world load failed: {outcome.Failure}; " +
            $"{outcome.ApiExecution?.Failure}.");
      }
      return session;
    } catch {
      session.Dispose();
      throw;
    }
  }
}
