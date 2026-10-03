using System.Buffers.Binary;
using NSSLC.Infrastructure.Network;
using Terraria.Network;

namespace NSSLC.NetworkVerification;

internal static class FramingVerification {
  public static Task RunAsync() {
    byte[] frame = { 8, 0, 13, 1, 2, 3, 4, 5 };
    for (int split = 0; split <= frame.Length; split++) {
      var local = new PacketByteBudget(1024);
      var global = new PacketByteBudget(1024);
      using var assembler = new PacketFrameAssembler(1024, local, global);
      var received = new List<byte[]>();
      assembler.Append(frame.AsSpan(0, split), received.Add);
      assembler.Append(frame.AsSpan(split), received.Add);
      Verify.That(received.Count == 1 && received[0].SequenceEqual(frame),
          $"Frame split at byte {split} must preserve its complete contents.");
      Verify.That(local.Used == frame.Length && global.Used == frame.Length,
          "A completed frame remains charged until its consumer releases it.");
      assembler.Release(received[0]);
      Verify.That(local.Used == 0 && global.Used == 0,
          "Consuming a frame must release both byte budgets.");
    }

    var bytewiseLocal = new PacketByteBudget(1024);
    var bytewiseGlobal = new PacketByteBudget(1024);
    using (var assembler = new PacketFrameAssembler(1024, bytewiseLocal, bytewiseGlobal)) {
      var received = new List<byte[]>();
      foreach (byte value in frame) {
        assembler.Append(new[] { value }, received.Add);
      }
      Verify.That(received.Count == 1 && !assembler.HasPartial,
          "One-byte callbacks must eventually yield one complete frame.");
      assembler.Release(received[0]);
      assembler.Append(new byte[] { 3, 0, 6, 3, 0, 6 }, received.Add);
      Verify.That(received.Count == 3, "One callback may contain multiple minimum-size frames.");
      assembler.Release(received[1]);
      assembler.Release(received[2]);
    }

    byte[] maximum = new byte[ushort.MaxValue];
    BinaryPrimitives.WriteUInt16LittleEndian(maximum, ushort.MaxValue);
    maximum[2] = 10;
    var maximumBudget = new PacketByteBudget(ushort.MaxValue);
    using (var assembler = new PacketFrameAssembler(ushort.MaxValue, maximumBudget,
        new PacketByteBudget(ushort.MaxValue))) {
      byte[]? received = null;
      assembler.Append(maximum.AsSpan(0, 3), value => received = value);
      assembler.Append(maximum.AsSpan(3), value => received = value);
      Verify.That(received is not null && received.SequenceEqual(maximum),
          "A legal 65535-byte frame must survive a fragmented body.");
      assembler.Release(received!);
    }

    foreach (byte invalid in new byte[] { 0, 1, 2, 65 }) {
      var local = new PacketByteBudget(64);
      var global = new PacketByteBudget(64);
      using var assembler = new PacketFrameAssembler(64, local, global);
      PacketProtocolException error = Verify.Throws<PacketProtocolException>(
          () => assembler.Append(new byte[] { invalid, 0 }, _ => { }));
      Verify.That(error.Code == "InvalidFrameLength" && global.Used == 0,
          "Invalid lengths must be rejected before frame allocation.");
    }

    var partialBudget = new PacketByteBudget(64);
    using (var assembler = new PacketFrameAssembler(64, partialBudget, new PacketByteBudget(64))) {
      assembler.Append(new byte[] { 20, 0, 13, 1 }, _ => { });
      Verify.That(assembler.HasPartial && partialBudget.Used == 20,
          "A partial frame must reserve its full declared size.");
    }
    Verify.That(partialBudget.Used == 0, "Disposing a half-frame must release its reservation.");

    var constrainedGlobal = new PacketByteBudget(4);
    var rollbackLocal = new PacketByteBudget(64);
    using (var assembler = new PacketFrameAssembler(64, rollbackLocal, constrainedGlobal)) {
      PacketProtocolException error = Verify.Throws<PacketProtocolException>(
          () => assembler.Append(frame, _ => { }));
      Verify.That(error.Code == "GlobalCapacityExceeded" && rollbackLocal.Used == 0,
          "Failure to reserve global capacity must roll back the local reservation.");
    }
    return Task.CompletedTask;
  }
}
