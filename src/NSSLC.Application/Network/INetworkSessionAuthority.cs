namespace Terraria.Network;

public interface INetworkSessionAuthority {
  bool RequiresPassword { get; }

  ValueTask<SessionAdmission> AdmitAsync(ConnectionIdentity connection, string? password,
      CancellationToken cancellationToken);

  ValueTask<bool> AuthorizeHostAsync(NetworkSessionContext context, string token,
      CancellationToken cancellationToken);

  ValueTask ReleaseAsync(ConnectionIdentity connection, SenderBinding binding,
      CancellationToken cancellationToken);
}
