using System;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Session;

/// <summary>Captures the closing identity before dependent state and the binding are retired.</summary>
public sealed record SessionCloseContext {
  public ConnectionClosed Connection { get; }

  public SessionBinding? Binding { get; }

  public SessionCloseContext(ConnectionClosed connection, SessionBinding? binding) {
    ArgumentNullException.ThrowIfNull(connection);
    if (binding is not null && binding.Epoch != connection.Epoch) {
      throw new ArgumentException("The binding belongs to another closing epoch.", nameof(binding));
    }

    Connection = connection;
    Binding = binding;
  }
}
