namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Serializes writes that must participate in one inventory and container transaction.</summary>
public sealed class ServerStateCommitGate
{
  internal object SyncRoot { get; } = new();
}
