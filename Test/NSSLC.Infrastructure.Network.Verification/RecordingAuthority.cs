using Terraria.Network;

namespace NSSLC.NetworkVerification;

internal sealed class RecordingAuthority : INetworkSessionAuthority {
  private readonly object _gate = new();
  private readonly Dictionary<ConnectionIdentity, SenderBinding> _admitted = new();
  private int _released;

  public bool RequiresPassword { get; init; }
  public bool RejectAdmissions { get; init; }
  public int Admissions {
    get {
      lock (_gate) {
        return _admitted.Count;
      }
    }
  }
  public int Releases => Volatile.Read(ref _released);
  public Guid GameSessionKey { get; } = Guid.NewGuid();

  public ValueTask<SessionAdmission> AdmitAsync(ConnectionIdentity connection, string? password,
      CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (RejectAdmissions || (RequiresPassword && password != "correct")) {
      return ValueTask.FromResult(new SessionAdmission(null, "AdmissionDenied"));
    }
    lock (_gate) {
      var binding = new SenderBinding((byte)_admitted.Count, GameSessionKey);
      _admitted.Add(connection, binding);
      return ValueTask.FromResult(new SessionAdmission(binding));
    }
  }

  public ValueTask<bool> AuthorizeHostAsync(NetworkSessionContext context, string token,
      CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    return ValueTask.FromResult(token == "trusted-host");
  }

  public ValueTask ReleaseAsync(ConnectionIdentity connection, SenderBinding binding,
      CancellationToken cancellationToken) {
    lock (_gate) {
      Verify.That(_admitted.TryGetValue(connection, out SenderBinding? expected)
          && expected == binding,
          "Session cleanup must release the exact binding established by application authority.");
    }
    Interlocked.Increment(ref _released);
    return ValueTask.CompletedTask;
  }
}
