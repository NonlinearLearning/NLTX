namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class DebugOptionsReplicationAdapter
{
  private readonly Action<byte, DebugRuntimeOptionsSnapshot> _send;

  public DebugOptionsReplicationAdapter(
    Action<byte, DebugRuntimeOptionsSnapshot> send)
  {
    _send = send ?? throw new ArgumentNullException(nameof(send));
  }

  public void Sync(byte playerSlot, DebugRuntimeOptionsSnapshot snapshot)
  {
    _send.Invoke(playerSlot, snapshot);
  }
}
