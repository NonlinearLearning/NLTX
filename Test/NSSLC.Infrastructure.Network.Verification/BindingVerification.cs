using System.Buffers.Binary;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace NSSLC.NetworkVerification;

internal static class BindingVerification {
  internal readonly record struct BytePacket(byte Value);
  private readonly record struct OtherPacket(byte Value);

  internal sealed class TrackingStream : MemoryStream {
    public bool Disposed { get; private set; }

    protected override void Dispose(bool disposing) {
      Disposed = true;
      base.Dispose(disposing);
    }
  }

  public static PacketBinding<BytePacket> CreateBinding(byte id, PacketDirection direction) {
    return new PacketBinding<BytePacket>(id, direction, DecodeByte, EncodeByte);
  }

  public static Task RunAsync() {
    PacketBinding<BytePacket> binding = CreateBinding(13, PacketDirection.ClientToServer);
    byte[] frame = binding.Encode(new BytePacket(41));
    Verify.That(frame.SequenceEqual(new byte[] { 4, 0, 13, 41 }),
        "A binding must write exactly one little-endian frame header.");
    Verify.That((BytePacket)binding.Decode(new byte[] { 41 }) == new BytePacket(41),
        "Successful decoding must preserve the typed packet value.");

    PacketProtocolException truncated = Verify.Throws<PacketProtocolException>(
        () => binding.Decode(ReadOnlyMemory<byte>.Empty));
    Verify.That(truncated.Code == "Truncated" && truncated.MessageId == 13,
        "A failed value-type read must report a protocol failure instead of a default packet.");
    PacketProtocolException trailing = Verify.Throws<PacketProtocolException>(
        () => binding.Decode(new byte[] { 41, 42 }));
    Verify.That(trailing.Code == "TrailingBytes" && trailing.BodyOffset == 1,
        "A complete frame must reject bytes remaining after its body codec.");
    Verify.Throws<PacketEncodingException>(() => binding.Encode(new OtherPacket(41)));

    var empty = new PacketBinding<BytePacket>(6, PacketDirection.ClientToServer,
        DecodeByte, _ => new MemoryStream());
    Verify.That(empty.Encode(new BytePacket()).SequenceEqual(new byte[] { 3, 0, 6 }),
        "An empty body must produce the minimum valid three-byte frame.");
    var nullWriter = new PacketBinding<BytePacket>(6, PacketDirection.ClientToServer,
        DecodeByte, _ => null);
    Verify.Throws<PacketEncodingException>(() => nullWriter.Encode(new BytePacket()));

    var maximumStream = new TrackingStream();
    maximumStream.Write(new byte[ushort.MaxValue - 3]);
    var maximum = new PacketBinding<BytePacket>(10, PacketDirection.ServerToClient,
        DecodeByte, _ => maximumStream);
    byte[] maximumFrame = maximum.Encode(new BytePacket());
    Verify.That(maximumFrame.Length == ushort.MaxValue
        && BinaryPrimitives.ReadUInt16LittleEndian(maximumFrame) == ushort.MaxValue
        && maximumStream.Disposed,
        "The maximum legal frame must be encoded and its intermediate stream released.");

    var overlargeStream = new TrackingStream();
    overlargeStream.Write(new byte[ushort.MaxValue - 2]);
    var overlarge = new PacketBinding<BytePacket>(10, PacketDirection.ServerToClient,
        DecodeByte, _ => overlargeStream);
    Verify.Throws<PacketEncodingException>(() => overlarge.Encode(new BytePacket()));
    Verify.That(overlargeStream.Disposed,
        "Rejected oversized encoding must release its intermediate stream.");

    PacketBinding<BytePacket> response = CreateBinding(13, PacketDirection.ServerToClient);
    var profile = new ProtocolProfile("verification", "Terraria319", new[] { binding, response });
    Verify.That(ReferenceEquals(profile.Find(PacketDirection.ClientToServer, (byte)13), binding)
        && ReferenceEquals(profile.Find(PacketDirection.ServerToClient, typeof(BytePacket)), response),
        "Formats sharing an ID across different directions must remain independently indexed.");
    Verify.Throws<ArgumentException>(() => new ProtocolProfile("duplicate", "Terraria319",
        new[] { binding, CreateBinding(13, PacketDirection.ClientToServer) }));
    Verify.Throws<ArgumentException>(() => new ProtocolProfile("duplicateType", "Terraria319",
        new[] { binding, CreateBinding(14, PacketDirection.ClientToServer) }));
    Verify.Throws<ArgumentException>(() => new ProtocolProfile("invalidDirection", "Terraria319",
        new[] { CreateBinding(13, (PacketDirection)7) }));
    Verify.Throws<PacketProtocolException>(() => profile.Find(PacketDirection.ClientToServer, (byte)0));
    Verify.Throws<PacketEncodingException>(
        () => profile.Find(PacketDirection.ClientToServer, typeof(OtherPacket)));

    var facts = new ProtocolFacts();
    facts.Bind(13, PacketDirection.ClientToServer, "slot", 4);
    Verify.That(facts.Get<int>(13, PacketDirection.ClientToServer, "slot") == 4,
        "Facts must be resolved by message, direction, name and expected type.");
    Verify.Throws<InvalidOperationException>(
        () => facts.Bind(14, PacketDirection.ClientToServer, "slot", 5));
    Verify.Throws<InvalidOperationException>(
        () => facts.Get<int>(13, PacketDirection.ServerToClient, "slot"));

    var identity = new ConnectionIdentity(Guid.NewGuid(), 7);
    var message = new PacketMessage("verification", PacketDirection.ClientToServer,
        13, identity, 1, new BytePacket(41));
    Verify.That(message.Get<BytePacket>() == new BytePacket(41)
        && message.Connection == identity,
        "The decoded envelope must preserve both the value and its connection generation.");
    Verify.Throws<InvalidOperationException>(() => message.Get<OtherPacket>());
    return Task.CompletedTask;
  }

  private static PacketReadResult<BytePacket> DecodeByte(ReadOnlyMemory<byte> body) {
    if (body.IsEmpty) {
      return PacketReadResult<BytePacket>.Failed(new PacketReadError(
          PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, 0, "Value", "Missing byte."));
    }
    return PacketReadResult<BytePacket>.Succeeded(new BytePacket(body.Span[0]), 1);
  }

  private static MemoryStream EncodeByte(BytePacket packet) {
    var stream = new MemoryStream();
    stream.WriteByte(packet.Value);
    return stream;
  }
}
