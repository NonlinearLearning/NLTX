using NSSLC.Infrastructure.Network;

namespace NSSLC.NetworkVerification;

internal sealed class RecordingTransport : IPacketTransport {
  private readonly object _gate = new();
  private readonly List<byte[]> _frames = new();
  private int _closeCalls;

  public Func<byte[], bool>? OnSubmit { get; set; }
  public int CloseCalls => Volatile.Read(ref _closeCalls);
  public int FrameCount {
    get {
      lock (_gate) {
        return _frames.Count;
      }
    }
  }

  public bool Submit(ReadOnlySpan<byte> frame) {
    byte[] owned = frame.ToArray();
    lock (_gate) {
      _frames.Add(owned);
    }
    return OnSubmit?.Invoke(owned) ?? true;
  }

  public void Close() {
    Interlocked.Increment(ref _closeCalls);
  }

  public byte[] GetFrame(int index) {
    lock (_gate) {
      return _frames[index].ToArray();
    }
  }
}
