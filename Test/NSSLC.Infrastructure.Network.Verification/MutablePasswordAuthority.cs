using Terraria.Network;

namespace NSSLC.NetworkVerification;

internal sealed class MutablePasswordAuthority : INetworkSessionAuthority {
  private readonly RecordingAuthority _authority = new() { RequiresPassword = true };
  private int _reads;

  public bool PasswordRequired { get; set; } = true;
  public int Reads => Volatile.Read(ref _reads);
  public int Releases => _authority.Releases;
  public bool RequiresPassword {
    get {
      Interlocked.Increment(ref _reads);
      return PasswordRequired;
    }
  }

  public ValueTask<SessionAdmission> AdmitAsync(ConnectionIdentity connection, string? password,
      CancellationToken cancellationToken) {
    return _authority.AdmitAsync(connection, password, cancellationToken);
  }

  public ValueTask<bool> AuthorizeHostAsync(NetworkSessionContext context, string token,
      CancellationToken cancellationToken) {
    return _authority.AuthorizeHostAsync(context, token, cancellationToken);
  }

  public ValueTask ReleaseAsync(ConnectionIdentity connection, SenderBinding binding,
      CancellationToken cancellationToken) {
    return _authority.ReleaseAsync(connection, binding, cancellationToken);
  }
}
