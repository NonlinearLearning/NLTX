using NSSLC.Infrastructure.Network;
using Terraria.Network;

namespace NSSLC.NetworkVerification;

internal static class LargeReadVerification {
  public static async Task ClaimedCancellationAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    var claimed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    await using PacketConnection connection = Create(transport, budget, claimed);
    await budget.LargeCodecGate.WaitAsync();
    bool held = true;
    try {
      using var cancellation = new CancellationTokenSource();
      connection.ReceiveBytes(connection.Identity.Epoch, new byte[] { 4, 0, 10, 17 });
      Task<PacketMessage?> reading = connection.ReadPacketAsync(cancellation.Token).AsTask();
      await claimed.Task.WaitAsync(TimeSpan.FromSeconds(5));
      Verify.That(!reading.IsCompleted && budget.Used == 4,
          "The large-frame fixture must claim and retain the frame while its codec slot is held.");
      cancellation.Cancel();
      Verify.That(!reading.IsCompleted,
          "Caller cancellation after claiming must not consume the frame without delivery.");
      budget.LargeCodecGate.Release();
      held = false;
      PacketMessage? message = await reading.WaitAsync(TimeSpan.FromSeconds(5));
      Verify.That(message?.Get<BindingVerification.BytePacket>().Value == 17
          && message.Sequence == 1 && budget.Used == 0 && transport.CloseCalls == 0,
          "A claimed frame must deliver exactly once after caller cancellation and release capacity.");
      connection.ReceiveBytes(connection.Identity.Epoch, new byte[] { 4, 0, 10, 18 });
      PacketMessage? next = await connection.ReadPacketAsync();
      Verify.That(next?.Get<BindingVerification.BytePacket>().Value == 18 && next.Sequence == 2,
          "Claimed-frame cancellation must leave the next uncanceled read usable and ordered.");
    } finally {
      if (held) {
        budget.LargeCodecGate.Release();
      }
    }
  }

  public static async Task CleanEofAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    var claimed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    await using PacketConnection connection = Create(transport, budget, claimed);
    await budget.LargeCodecGate.WaitAsync();
    bool held = true;
    try {
      connection.ReceiveBytes(connection.Identity.Epoch, new byte[] { 4, 0, 10, 17, 4, 0, 10, 18 });
      Task<PacketMessage?> reading = connection.ReadPacketAsync().AsTask();
      await claimed.Task.WaitAsync(TimeSpan.FromSeconds(5));
      connection.NotifyClosed(connection.Identity.Epoch);
      Verify.That(!reading.IsCompleted && budget.Used == 8,
          "Clean EOF must preserve the claimed codec waiter and queued complete frames.");
      budget.LargeCodecGate.Release();
      held = false;
      PacketMessage? first = await reading.WaitAsync(TimeSpan.FromSeconds(5));
      PacketMessage? second = await connection.ReadPacketAsync();
      PacketMessage? eof = await connection.ReadPacketAsync();
      Verify.That(first?.Get<BindingVerification.BytePacket>().Value == 17
          && second?.Get<BindingVerification.BytePacket>().Value == 18
          && first.Sequence == 1 && second.Sequence == 2 && eof is null && budget.Used == 0,
          "Normal EOF must drain claimed and queued large frames in order before returning null.");
    } finally {
      if (held) {
        budget.LargeCodecGate.Release();
      }
    }
  }

  public static async Task DisposeWaiterAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    var claimed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    await using PacketConnection connection = Create(transport, budget, claimed);
    await budget.LargeCodecGate.WaitAsync();
    try {
      connection.ReceiveBytes(connection.Identity.Epoch, new byte[] { 4, 0, 10, 17, 4, 0, 10, 18 });
      Task<PacketMessage?> reading = connection.ReadPacketAsync().AsTask();
      await claimed.Task.WaitAsync(TimeSpan.FromSeconds(5));
      await connection.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
      await Verify.ThrowsAsync<IOException>(async () =>
          await reading.WaitAsync(TimeSpan.FromSeconds(5)));
      Verify.That(budget.Used == 0 && budget.LargeCodecGate.CurrentCount == 0
          && transport.CloseCalls > 0,
          "Dispose must cancel and reclaim a claimed codec waiter without requiring its occupied slot.");
    } finally {
      budget.LargeCodecGate.Release();
    }
  }

  public static async Task FatalCloseWaiterAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    var claimed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    await using PacketConnection connection = Create(transport, budget, claimed);
    await budget.LargeCodecGate.WaitAsync();
    try {
      connection.ReceiveBytes(connection.Identity.Epoch, new byte[] { 4, 0, 10, 17, 4, 0, 10, 18 });
      Task<PacketMessage?> reading = connection.ReadPacketAsync().AsTask();
      await claimed.Task.WaitAsync(TimeSpan.FromSeconds(5));
      var fatal = new PacketProtocolException("FixtureFatal", 10, 2);
      connection.NotifyClosed(connection.Identity.Epoch, fatal);
      PacketProtocolException failure = await Verify.ThrowsAsync<PacketProtocolException>(async () =>
          await reading.WaitAsync(TimeSpan.FromSeconds(5)));
      PacketProtocolException subsequent = await Verify.ThrowsAsync<PacketProtocolException>(
          async () => await connection.ReadPacketAsync());
      Verify.That(ReferenceEquals(failure, fatal) && ReferenceEquals(subsequent, fatal)
          && budget.Used == 0 && budget.LargeCodecGate.CurrentCount == 0,
          "Fatal close must reclaim codec waiters and queued frames while preserving its terminal error.");
    } finally {
      budget.LargeCodecGate.Release();
    }
  }

  private static PacketConnection Create(RecordingTransport transport, PacketByteBudget budget,
      TaskCompletionSource claimed) {
    var profile = new ProtocolProfile("large-read-fixture", "Terraria319", new[] {
      BindingVerification.CreateBinding(10, PacketDirection.ClientToServer)
    });
    var connection = new PacketConnection(new(Guid.NewGuid(), 1), transport, profile,
        PacketDirection.ClientToServer, ConnectionVerification.Options(), budget);
    connection.Admission = (_, _) => {
      claimed.TrySetResult();
      return true;
    };
    return connection;
  }
}
