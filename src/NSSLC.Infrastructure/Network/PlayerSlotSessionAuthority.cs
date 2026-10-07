using System.Security.Cryptography;
using System.Text;
using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

/// <summary>Allocates server-owned player slots for one running game session.</summary>
public sealed class PlayerSlotSessionAuthority : INetworkSessionAuthority {
  private readonly object _gate = new();
  private readonly SortedSet<byte> _availableSlots = new();
  private readonly Dictionary<ConnectionIdentity, SenderBinding> _bindings = new();
  private readonly byte[]? _passwordHash;
  private readonly byte[]? _hostTokenHash;
  private int _admissionCount;

  public bool RequiresPassword => _passwordHash is not null;
  public SessionClientUuidRegistry ClientUuids { get; } = new();
  public Guid GameSessionKey { get; } = Guid.NewGuid();
  public int AdmissionCount => Volatile.Read(ref _admissionCount);
  public int ActiveBindings {
    get { lock (_gate) { return _bindings.Count; } }
  }

  public PlayerSlotSessionAuthority(int playerSlotCount = 255, string? password = null,
      string? hostToken = null) {
    if (playerSlotCount is < 1 or > byte.MaxValue) {
      throw new ArgumentOutOfRangeException(nameof(playerSlotCount));
    }
    if (password is not null) {
      _passwordHash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
    }
    if (!string.IsNullOrWhiteSpace(hostToken)) {
      _hostTokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(hostToken));
    }
    for (int slot = 0; slot < playerSlotCount; slot++) {
      _availableSlots.Add((byte)slot);
    }
  }

  public ValueTask<SessionAdmission> AdmitAsync(ConnectionIdentity connection,
      string? password, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (!PasswordMatches(password)) {
      return ValueTask.FromResult(new SessionAdmission(null, "AuthenticationRejected"));
    }
    lock (_gate) {
      if (_bindings.ContainsKey(connection)) {
        return ValueTask.FromResult(new SessionAdmission(null, "DuplicateConnection"));
      }
      if (_availableSlots.Count == 0) {
        return ValueTask.FromResult(new SessionAdmission(null, "ServerFull"));
      }
      byte slot = _availableSlots.Min;
      _availableSlots.Remove(slot);
      var binding = new SenderBinding(slot, GameSessionKey);
      _bindings.Add(connection, binding);
      Interlocked.Increment(ref _admissionCount);
      return ValueTask.FromResult(new SessionAdmission(binding));
    }
  }

  public ValueTask<bool> AuthorizeHostAsync(NetworkSessionContext context, string token,
      CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (_hostTokenHash is null || string.IsNullOrWhiteSpace(token)
        || !IsCurrentBinding(context)) {
      return ValueTask.FromResult(false);
    }
    byte[] candidate = SHA256.HashData(Encoding.UTF8.GetBytes(token));
    return ValueTask.FromResult(CryptographicOperations.FixedTimeEquals(_hostTokenHash, candidate));
  }

  public ValueTask ReleaseAsync(ConnectionIdentity connection, SenderBinding binding,
      CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    lock (_gate) {
      if (!_bindings.TryGetValue(connection, out SenderBinding? current) || current != binding) {
        throw new InvalidOperationException("The released player binding is not current.");
      }
      _bindings.Remove(connection);
      _availableSlots.Add(binding.PlayerSlot);
      ClientUuids.Remove(connection);
    }
    return ValueTask.CompletedTask;
  }

  private bool PasswordMatches(string? password) {
    if (_passwordHash is null) {
      return true;
    }
    byte[] candidate = SHA256.HashData(Encoding.UTF8.GetBytes(password ?? string.Empty));
    return CryptographicOperations.FixedTimeEquals(_passwordHash, candidate);
  }

  private bool IsCurrentBinding(NetworkSessionContext context) {
    lock (_gate) {
      return _bindings.TryGetValue(context.Connection, out SenderBinding? current)
          && current == context.Actor
          && current.GameSessionKey == GameSessionKey;
    }
  }
}
