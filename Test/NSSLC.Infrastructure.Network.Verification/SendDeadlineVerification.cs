using NSSLC.Infrastructure.Network;
using Terraria.Network;

namespace NSSLC.NetworkVerification;

internal static class SendDeadlineVerification {
  public static async Task ProgressResetsDeadlineAsync() {
    var time = new ManualTimeProvider();
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using PacketConnection connection = ConnectionVerification.Create(transport, budget,
        ConnectionVerification.Options() with { PartialFrameTimeout = TimeSpan.FromMinutes(1) }, time);
    Task<PacketWriteReceipt> sending = connection.WritePacketAsync(
        new BindingVerification.BytePacket(17)).AsTask();
    await Verify.EventuallyAsync(() => transport.FrameCount == 1
        && time.NextTimerDelay == TimeSpan.FromSeconds(2), "The sending deadline was not installed.");
    time.Advance(TimeSpan.FromSeconds(1));
    connection.NotifySent(connection.Identity.Epoch, 1);
    await Verify.EventuallyAsync(() => time.NextTimerDelay == TimeSpan.FromSeconds(2),
        "The first real sending progress did not reset the deadline.");
    time.Advance(TimeSpan.FromSeconds(1));
    Verify.That(!sending.IsCompleted && transport.CloseCalls == 0,
        "A progressing frame must survive its original whole-frame timeout boundary.");
    connection.NotifySent(connection.Identity.Epoch, 1);
    await Verify.EventuallyAsync(() => time.NextTimerDelay == TimeSpan.FromSeconds(2),
        "The next real sending progress did not reset the deadline.");
    time.Advance(TimeSpan.FromSeconds(1));
    connection.NotifySent(connection.Identity.Epoch, 2);
    Verify.That((await sending).FrameBytes == 4 && budget.Used == 0 && transport.CloseCalls == 0,
        "A continuously progressing frame may take longer than SendTimeout in total and still succeed.");

    Task<PacketWriteReceipt> stalled = connection.WritePacketAsync(
        new BindingVerification.BytePacket(18)).AsTask();
    await Verify.EventuallyAsync(() => transport.FrameCount == 2
        && time.NextTimerDelay == TimeSpan.FromSeconds(2), "The next sending deadline was not installed.");
    time.Advance(TimeSpan.FromSeconds(1));
    connection.NotifySent(connection.Identity.Epoch, 1);
    await Verify.EventuallyAsync(() => time.NextTimerDelay == TimeSpan.FromSeconds(2),
        "Stalled-frame progress did not reset its deadline.");
    time.Advance(TimeSpan.FromSeconds(2));
    PacketSendException error = await Verify.ThrowsAsync<PacketSendException>(async () => await stalled);
    await Verify.EventuallyAsync(() => transport.CloseCalls > 0,
        "A frame lacking further progress after its reset deadline must close.");
    Verify.That(error.Certainty == PacketSendCertainty.OutcomeUnknown && budget.Used == 0,
        "A partial frame stalled after real progress must release bytes and retain unknown outcome.");
  }

  public static async Task QueuedExpiryAsync() {
    var time = new ManualTimeProvider();
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using PacketConnection connection = ConnectionVerification.Create(transport, budget,
        ConnectionVerification.Options() with { PartialFrameTimeout = TimeSpan.FromMinutes(1) }, time);
    Task<PacketWriteReceipt> first = connection.WritePacketAsync(new BindingVerification.BytePacket(17)).AsTask();
    await Verify.EventuallyAsync(() => transport.FrameCount == 1
        && time.NextTimerDelay == TimeSpan.FromSeconds(2), "The first frame was not submitted.");
    Task<PacketWriteReceipt> queued = connection.WritePacketAsync(new BindingVerification.BytePacket(18)).AsTask();
    Verify.That(budget.Used == 8 && transport.FrameCount == 1,
        "The expiry fixture must queue the second frame before advancing time.");
    for (int progress = 0; progress < 2; progress++) {
      time.Advance(TimeSpan.FromSeconds(1));
      connection.NotifySent(connection.Identity.Epoch, 1);
      await Verify.EventuallyAsync(() => time.NextTimerDelay == TimeSpan.FromSeconds(2),
          "The current frame did not remain alive while its queued successor aged.");
    }
    time.Advance(TimeSpan.FromSeconds(1));
    connection.NotifySent(connection.Identity.Epoch, 2);
    await first;
    PacketSendException expired = await Verify.ThrowsAsync<PacketSendException>(async () => await queued);
    Verify.That(expired.Certainty == PacketSendCertainty.NotSubmitted
        && expired.Code == "SendQueueTimeout" && transport.FrameCount == 1
        && transport.CloseCalls == 0 && budget.Used == 0,
        "An aged queued frame must release capacity and report unsubmitted without entering the transport.");

    Task<PacketWriteReceipt> next = connection.WritePacketAsync(new BindingVerification.BytePacket(19)).AsTask();
    await Verify.EventuallyAsync(() => transport.FrameCount == 2,
        "A fresh writer did not resume after expired queued capacity was released.");
    connection.NotifySent(connection.Identity.Epoch, 4);
    Verify.That((await next).Sequence == 3 && budget.Used == 0,
        "Expiry must preserve unique admission sequences and leave the progressing connection usable.");
  }
}
