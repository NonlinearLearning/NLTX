using Terraria.Network;

namespace NSSLC.NetworkVerification;

internal sealed class DelayedAuthority : INetworkSessionAuthority {
  private int _releases;

  public bool RequiresPassword => false;
  public TaskCompletionSource Entered { get; } = new(
      TaskCreationOptions.RunContinuationsAsynchronously);
  public TaskCompletionSource<SessionAdmission> Result { get; } = new(
      TaskCreationOptions.RunContinuationsAsynchronously);
  public SenderBinding Binding { get; } = new(7, Guid.NewGuid());
  public ConnectionIdentity? AdmittedConnection { get; private set; }
  public int Releases => Volatile.Read(ref _releases);

  public ValueTask<SessionAdmission> AdmitAsync(ConnectionIdentity connection, string? password,
      CancellationToken cancellationToken) {
    AdmittedConnection = connection;
    Entered.TrySetResult();
    return new(Result.Task);
  }

  public ValueTask<bool> AuthorizeHostAsync(NetworkSessionContext context, string token,
      CancellationToken cancellationToken) {
    throw new InvalidOperationException("This fixture never authorizes a host.");
  }

  public ValueTask ReleaseAsync(ConnectionIdentity connection, SenderBinding binding,
      CancellationToken cancellationToken) {
    Verify.That(connection == AdmittedConnection && binding == Binding,
        "Late cleanup must release exactly the lease returned by its original authority request.");
    Interlocked.Increment(ref _releases);
    return ValueTask.CompletedTask;
  }
}
