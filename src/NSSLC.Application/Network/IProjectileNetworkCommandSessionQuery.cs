namespace Terraria.Network;

/// <summary>
/// Resolves current authenticated connection and published-world state for a command.
/// </summary>
public interface IProjectileNetworkCommandSessionQuery
{
  /// <summary>
  /// Resolves the command only when the sender's connection epoch, actor binding, active stage,
  /// and enqueue-time world runtime token still match the current published session. Implementors
  /// must not rebuild an EntityReference from an old root identity against a newer runtime.
  /// This method is called on the EntityRuntime owner thread.
  /// </summary>
  bool TryResolveCurrent(
    NetworkSessionContext sender,
    out ProjectileNetworkCommandSession? session);
}
