using NSSLC.Infrastructure.Network;
using Terraria.Network;

namespace NSSLC.NetworkVerification;

internal static class ConnectionVerification {
  public static async Task ReceiveAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using PacketConnection connection = Create(transport, budget);
    byte[] callbackBuffer = { 4, 0, 13, 17, 4, 0, 13, 18 };
    connection.ReceiveBytes(connection.Identity.Epoch, callbackBuffer.AsSpan(0, 1));
    connection.ReceiveBytes(connection.Identity.Epoch, callbackBuffer.AsSpan(1));
    Array.Fill(callbackBuffer, (byte)255);
    connection.NotifyClosed(connection.Identity.Epoch);
    PacketMessage first = (await connection.ReadPacketAsync())!;
    PacketMessage second = (await connection.ReadPacketAsync())!;
    Verify.That(first.Get<BindingVerification.BytePacket>().Value == 17
        && second.Get<BindingVerification.BytePacket>().Value == 18
        && first.Sequence == 1 && second.Sequence == 2,
        "Callbacks must copy borrowed bytes and complete frames must drain in FIFO order.");
    Verify.That(await connection.ReadPacketAsync() is null && budget.Used == 0,
        "A clean EOF must drain complete frames then return null without leaking bytes.");
  }

  public static async Task PartialEofAsync() {
    var budget = new PacketByteBudget(4096);
    await using PacketConnection connection = Create(new RecordingTransport(), budget);
    connection.ReceiveBytes(connection.Identity.Epoch, new byte[] { 4, 0, 13, 17, 4, 0, 13 });
    connection.NotifyClosed(connection.Identity.Epoch);
    Verify.That((await connection.ReadPacketAsync())!.Get<BindingVerification.BytePacket>().Value == 17,
        "A partial EOF must preserve already completed frames before reporting the failure.");
    PacketProtocolException error = await Verify.ThrowsAsync<PacketProtocolException>(
        async () => await connection.ReadPacketAsync());
    Verify.That(error.Code == "IncompleteFrameAtEof" && budget.Used == 0,
        "A partial EOF must be reported explicitly and release all receive bytes.");
  }

  public static async Task ReadingCancellationAsync() {
    await using PacketConnection connection = Create(new RecordingTransport(), new PacketByteBudget(4096));
    using var cancellation = new CancellationTokenSource();
    Task<PacketMessage?> reading = connection.ReadPacketAsync(cancellation.Token).AsTask();
    await Verify.ThrowsAsync<InvalidOperationException>(async () => await connection.ReadPacketAsync());
    cancellation.Cancel();
    await Verify.ThrowsAsync<OperationCanceledException>(async () => await reading);
    connection.ReceiveBytes(connection.Identity.Epoch - 1, new byte[] { 4, 0, 13, 99 });
    connection.NotifyClosed(connection.Identity.Epoch - 1);
    connection.ReceiveBytes(connection.Identity.Epoch, new byte[] { 4, 0, 13, 17 });
    Verify.That((await connection.ReadPacketAsync())!.Get<BindingVerification.BytePacket>().Value == 17,
        "A canceled reader and stale callbacks must not consume or close the next valid message.");
  }

  public static async Task AdmissionAndOverflowAsync() {
    int decodes = 0;
    var binding = new PacketBinding<BindingVerification.BytePacket>(13,
        PacketDirection.ClientToServer,
        _ => {
          decodes++;
          return Terraria.NetWork.Prototype.PacketDesignCompiler.Wire
              .PacketReadResult<BindingVerification.BytePacket>.Succeeded(new(17), 1);
        }, _ => new MemoryStream(new byte[] { 17 }));
    var profile = new ProtocolProfile("admission", "Terraria319", new[] { binding });
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using (var connection = new PacketConnection(new(Guid.NewGuid(), 1), transport,
        profile, PacketDirection.ClientToServer, Options(), budget)) {
      connection.Admission = (_, _) => false;
      connection.ReceiveBytes(1, new byte[] { 4, 0, 13, 17 });
      PacketProtocolException rejection = await Verify.ThrowsAsync<PacketProtocolException>(
          async () => await connection.ReadPacketAsync());
      Verify.That(rejection.Code == "AdmissionRejected" && decodes == 0
          && transport.CloseCalls > 0 && budget.Used == 0,
          "Admission must reject before decoding and clean up the rejected connection.");
    }

    var limitedBudget = new PacketByteBudget(4096);
    var limitedTransport = new RecordingTransport();
    await using (PacketConnection connection = Create(limitedTransport, limitedBudget,
        Options() with { ReceiveItems = 1 })) {
      connection.ReceiveBytes(connection.Identity.Epoch, new byte[] { 4, 0, 13, 17, 4, 0, 13, 18 });
      Verify.That(limitedTransport.CloseCalls > 0,
          "A full receive mailbox must close the connection instead of silently dropping input.");
      PacketProtocolException error = await Verify.ThrowsAsync<PacketProtocolException>(
          async () => await connection.ReadPacketAsync());
      Verify.That(error.Code == "ReceiveCapacityExceeded" && limitedBudget.Used == 0,
          "Fatal mailbox overflow must discard and release both the failed and published frames.");
    }
  }

  public static async Task SynchronousSendAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using PacketConnection connection = Create(transport, budget);
    transport.OnSubmit = frame => {
      connection.NotifySent(connection.Identity.Epoch, frame.Length);
      return true;
    };
    PacketWriteReceipt receipt = await connection.WritePacketAsync(new BindingVerification.BytePacket(17));
    Verify.That(receipt.Connection == connection.Identity && receipt.Sequence == 1
        && receipt.FrameBytes == 4 && budget.Used == 0,
        "A synchronous OnSent inside Submit must complete only after acceptance is confirmed.");
    object boxed = new BindingVerification.BytePacket(17);
    Verify.Throws<PacketEncodingException>(() => connection.WritePacketAsync(boxed));
  }

  public static async Task PartialSendAndCancellationAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using PacketConnection connection = Create(transport, budget);
    using var cancellation = new CancellationTokenSource();
    Task<PacketWriteReceipt> sending = connection.WritePacketAsync(
        new BindingVerification.BytePacket(17), cancellation.Token).AsTask();
    await Verify.EventuallyAsync(() => transport.FrameCount == 1, "The first frame was not submitted.");
    cancellation.Cancel();
    connection.NotifySent(connection.Identity.Epoch - 1, 4);
    connection.NotifySent(connection.Identity.Epoch, 2);
    Verify.That(!sending.IsCompleted && budget.Used == 4,
        "Cancellation after submission and a partial or stale callback must not complete the frame.");
    connection.NotifySent(connection.Identity.Epoch, 2);
    Verify.That((await sending).FrameBytes == 4 && budget.Used == 0,
        "A submitted write must survive caller cancellation until all local bytes are sent.");
  }

  public static async Task QueuedCancellationAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using PacketConnection connection = Create(transport, budget);
    Task<PacketWriteReceipt> first = connection.WritePacketAsync(new BindingVerification.BytePacket(17)).AsTask();
    await Verify.EventuallyAsync(() => transport.FrameCount == 1, "The first frame was not submitted.");
    using var cancellation = new CancellationTokenSource();
    Task<PacketWriteReceipt> second = connection.WritePacketAsync(
        new BindingVerification.BytePacket(18), cancellation.Token).AsTask();
    Verify.That(budget.Used == 8, "The queued second frame must reserve its own bytes.");
    cancellation.Cancel();
    await Verify.ThrowsAsync<OperationCanceledException>(async () => await second);
    Verify.That(budget.Used == 4 && transport.FrameCount == 1,
        "Canceling a queued write must release its reservation without submitting any of its bytes.");
    connection.NotifySent(connection.Identity.Epoch, 4);
    await first;
  }

  public static async Task DisconnectCertaintyAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using PacketConnection connection = Create(transport, budget);
    Task<PacketWriteReceipt> first = connection.WritePacketAsync(new BindingVerification.BytePacket(17)).AsTask();
    await Verify.EventuallyAsync(() => transport.FrameCount == 1, "The first frame was not submitted.");
    Task<PacketWriteReceipt> second = connection.WritePacketAsync(new BindingVerification.BytePacket(18)).AsTask();
    connection.NotifySent(connection.Identity.Epoch, 2);
    connection.NotifyClosed(connection.Identity.Epoch, new IOException("Simulated reset."));
    PacketSendException accepted = await Verify.ThrowsAsync<PacketSendException>(async () => await first);
    PacketSendException queued = await Verify.ThrowsAsync<PacketSendException>(async () => await second);
    Verify.That(accepted.Certainty == PacketSendCertainty.OutcomeUnknown
        && queued.Certainty == PacketSendCertainty.NotSubmitted && budget.Used == 0,
        "Disconnect must distinguish submitted and queued writes and release both reservations.");
  }

  public static async Task CapacityAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using PacketConnection connection = Create(transport, budget,
        Options() with { SendItems = 1, MaximumWaitingWriters = 1 });
    Task<PacketWriteReceipt> first = connection.WritePacketAsync(new BindingVerification.BytePacket(17)).AsTask();
    await Verify.EventuallyAsync(() => transport.FrameCount == 1, "The first frame was not submitted.");
    using var cancellation = new CancellationTokenSource();
    Task<PacketWriteReceipt> second = connection.WritePacketAsync(
        new BindingVerification.BytePacket(18), cancellation.Token).AsTask();
    PacketSendException rejection = await Verify.ThrowsAsync<PacketSendException>(
        async () => await connection.WritePacketAsync(new BindingVerification.BytePacket(19)));
    Verify.That(rejection.Certainty == PacketSendCertainty.NotSubmitted
        && rejection.Message == "SendCapacityExceeded" && transport.FrameCount == 1,
        "Waiting producers must be bounded and excess producers must remain unsubmitted.");
    cancellation.Cancel();
    await Verify.ThrowsAsync<OperationCanceledException>(async () => await second);
    connection.NotifySent(connection.Identity.Epoch, 4);
    await first;
    Verify.That(budget.Used == 0, "Canceled capacity waiters must release all acquired reservations.");
  }

  public static async Task SendingTimeoutAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using PacketConnection connection = Create(transport, budget,
        Options() with { SendTimeout = TimeSpan.FromMilliseconds(80) });
    PacketSendException error = await Verify.ThrowsAsync<PacketSendException>(
        async () => await connection.WritePacketAsync(new BindingVerification.BytePacket(17)));
    Verify.That(error.Certainty == PacketSendCertainty.OutcomeUnknown
        && transport.CloseCalls > 0 && budget.Used == 0,
        "A no-progress submitted frame must time out with unknown outcome and close its connection.");
  }

  public static async Task SubmissionFailureAsync() {
    var transport = new RecordingTransport { OnSubmit = _ => false };
    await using PacketConnection connection = Create(transport, new PacketByteBudget(4096));
    PacketSendException error = await Verify.ThrowsAsync<PacketSendException>(
        async () => await connection.WritePacketAsync(new BindingVerification.BytePacket(17)));
    Verify.That(error.Certainty == PacketSendCertainty.NotSubmitted,
        "A false submission with no sent evidence must remain provably unsubmitted.");
  }

  public static async Task AmbiguousSubmissionAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using PacketConnection connection = Create(transport, budget);
    transport.OnSubmit = _ => {
      connection.NotifySent(connection.Identity.Epoch, 1);
      return false;
    };
    PacketSendException error = await Verify.ThrowsAsync<PacketSendException>(
        async () => await connection.WritePacketAsync(new BindingVerification.BytePacket(17)));
    Verify.That(error.Certainty == PacketSendCertainty.OutcomeUnknown
        && transport.CloseCalls > 0 && budget.Used == 0,
        "A failed submission with sent evidence must close rather than reuse an ambiguous stream.");
  }

  public static async Task ConcurrentWritesAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(32768);
    await using PacketConnection connection = Create(transport, budget,
        Options() with { SendBytes = 8192 });
    transport.OnSubmit = frame => {
      connection.NotifySent(connection.Identity.Epoch, frame.Length);
      return true;
    };
    Task<PacketWriteReceipt>[] writers = Enumerable.Range(0, 32).Select(value => Task.Run(async () =>
        await connection.WritePacketAsync(new BindingVerification.BytePacket((byte)value)))).ToArray();
    PacketWriteReceipt[] receipts = await Task.WhenAll(writers);
    Verify.That(transport.FrameCount == 32 && receipts.Select(item => item.Sequence).Distinct().Count() == 32
        && receipts.Min(item => item.Sequence) == 1 && receipts.Max(item => item.Sequence) == 32
        && Enumerable.Range(0, 32).All(index => transport.GetFrame(index).Length == 4)
        && Enumerable.Range(0, 32).Select(index => transport.GetFrame(index)[3]).Distinct().Count() == 32
        && budget.Used == 0,
        "Concurrent writers must submit complete contiguous frames with unique ordered admission IDs.");
  }

  public static async Task InlineDisconnectAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using PacketConnection connection = Create(transport, budget);
    transport.OnSubmit = _ => {
      connection.NotifyClosed(connection.Identity.Epoch);
      return true;
    };
    PacketSendException error = await Verify.ThrowsAsync<PacketSendException>(
        async () => await connection.WritePacketAsync(new BindingVerification.BytePacket(17)));
    Verify.That(error.Certainty == PacketSendCertainty.OutcomeUnknown && budget.Used == 0,
        "A true Submit return must not overwrite an inline disconnect or revive its write.");
  }

  public static async Task InvalidCompletionAsync() {
    foreach (long reported in new long[] { -1, 0, 5 }) {
      var transport = new RecordingTransport();
      var budget = new PacketByteBudget(4096);
      await using PacketConnection connection = Create(transport, budget);
      Task<PacketWriteReceipt> sending = connection.WritePacketAsync(
          new BindingVerification.BytePacket(17)).AsTask();
      await Verify.EventuallyAsync(() => transport.FrameCount == 1, "The frame was not submitted.");
      connection.NotifySent(connection.Identity.Epoch, reported);
      PacketSendException error = await Verify.ThrowsAsync<PacketSendException>(async () => await sending);
      await Verify.EventuallyAsync(() => transport.CloseCalls > 0,
          "Invalid sent accounting must close the untrustworthy byte stream.");
      Verify.That(error.Certainty == PacketSendCertainty.OutcomeUnknown && budget.Used == 0,
          "Negative, zero and excess sent counts must invalidate completion accounting.");
    }
  }

  public static async Task PartialFrameTimeoutAsync() {
    var time = new ManualTimeProvider();
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    var profile = new ProtocolProfile("partialTimer", "Terraria319", new[] {
      BindingVerification.CreateBinding(13, PacketDirection.ClientToServer)
    });
    await using var connection = new PacketConnection(new(Guid.NewGuid(), 7), transport,
        profile, PacketDirection.ClientToServer, Options(), budget, time);
    connection.ReceiveBytes(7, new byte[] { 4, 0 });
    time.Advance(TimeSpan.FromSeconds(1));
    Verify.That(transport.CloseCalls == 0, "A partial frame must retain its configured grace period.");
    time.Advance(TimeSpan.FromSeconds(1));
    PacketProtocolException error = await Verify.ThrowsAsync<PacketProtocolException>(
        async () => await connection.ReadPacketAsync());
    Verify.That(error.Code == "PartialFrameTimeout" && transport.CloseCalls > 0 && budget.Used == 0,
        "Expired partial frames must close and release their full declared reservation.");
  }

  public static async Task FatalDecodeAsync() {
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using PacketConnection connection = Create(transport, budget);
    connection.ReceiveBytes(connection.Identity.Epoch,
        new byte[] { 3, 0, 13, 4, 0, 13, 17 });
    PacketProtocolException original = await Verify.ThrowsAsync<PacketProtocolException>(
        async () => await connection.ReadPacketAsync());
    Verify.That(original.Code == "Truncated" && transport.CloseCalls > 0,
        "An incomplete codec body inside a complete frame must be a fatal protocol failure.");
    PacketProtocolException repeated = await Verify.ThrowsAsync<PacketProtocolException>(
        async () => await connection.ReadPacketAsync());
    Verify.That(repeated.Code == "Truncated" && budget.Used == 0,
        "After fatal decoding, later queued input must not be delivered as a valid message.");
  }

  public static PacketConnection Create(RecordingTransport transport, PacketByteBudget budget,
      PacketConnectionOptions? options = null, TimeProvider? timeProvider = null) {
    var profile = new ProtocolProfile("verification", "Terraria319", new[] {
      BindingVerification.CreateBinding(13, PacketDirection.ClientToServer),
      BindingVerification.CreateBinding(13, PacketDirection.ServerToClient)
    });
    return new PacketConnection(new(Guid.NewGuid(), 7), transport, profile,
        PacketDirection.ClientToServer, options ?? Options(), budget, timeProvider);
  }

  public static PacketConnectionOptions Options() {
    return new PacketConnectionOptions {
      MaximumFrameBytes = 64, ReceiveBytes = 256, SendBytes = 256,
      ReceiveItems = 8, SendItems = 8, MaximumWaitingWriters = 64,
      ControlReserveBytes = 0, ControlReserveItems = 0,
      SendTimeout = TimeSpan.FromSeconds(2), PartialFrameTimeout = TimeSpan.FromSeconds(2)
    };
  }
}
