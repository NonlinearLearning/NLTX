using System.Buffers.Binary;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class GeneratedProfileVerification {
  public static Task RunAsync() {
    Verify.Throws<InvalidOperationException>(() => TerrariaProtocolProfile.Create(new ProtocolFacts()));
    ProtocolFacts facts = CreateFacts();
    ProtocolProfile profile = TerrariaProtocolProfile.Create(facts);
    Verify.That(profile.Key == "TerrariaV4:" + TerrariaProtocolProfile.Fingerprint + ":" + facts.Version
        && profile.HelloVersion == "Terraria319" && profile.Bindings.Count == 324,
        "The generated profile must cover 162 IDs in both actual directions with its fingerprint.");
    Verify.That(TerrariaProtocolProfile.Create(CreateFacts()).Key != profile.Key,
        "Separate fact snapshots must not share a profile cache identity by default.");
    Verify.Throws<InvalidOperationException>(() => facts.Bind(1, PacketDirection.ClientToServer,
        "late", true));

    byte[] hello = profile.Find(PacketDirection.ClientToServer, typeof(Packet1Packet))
        .Encode(new Packet1Packet { Version = "Terraria319" });
    Verify.That(hello[2] == 1 && BinaryPrimitives.ReadUInt16LittleEndian(hello) == hello.Length,
        "Generated Hello must have exactly one complete-frame prefix.");
    Verify.That(((Packet1Packet)profile.Find(PacketDirection.ClientToServer, (byte)1)
        .Decode(hello.AsMemory(3))).Version == "Terraria319",
        "Generated Hello must round-trip its wire version marker.");
    Verify.Throws<PacketProtocolException>(() => profile.Find(PacketDirection.ClientToServer, (byte)1)
        .Decode(hello.AsMemory(3, hello.Length - 4)));
    Verify.Throws<PacketProtocolException>(() => profile.Find(PacketDirection.ClientToServer, (byte)1)
        .Decode(hello.AsMemory(3).ToArray().Concat(new byte[] { 0 }).ToArray()));

    var request56 = new Packet56RequestPacket { Payload = new(123) };
    byte[] request56Frame = profile.Find(PacketDirection.ClientToServer, typeof(Packet56RequestPacket))
        .Encode(request56);
    Verify.That(request56Frame.Length == 5
        && ((Packet56RequestPacket)profile.Find(PacketDirection.ClientToServer, (byte)56)
        .Decode(request56Frame.AsMemory(3))).Payload == request56.Payload,
        "ID 56 C2S must use its short request layout.");
    var response56 = new Packet56ResponsePacket { Payload = new(123, "Guide", 4) };
    byte[] response56Frame = profile.Find(PacketDirection.ServerToClient, typeof(Packet56ResponsePacket))
        .Encode(response56);
    Verify.That(((Packet56ResponsePacket)profile.Find(PacketDirection.ServerToClient, (byte)56)
        .Decode(response56Frame.AsMemory(3))).Payload == response56.Payload,
        "ID 56 S2C must use its response layout including name and variation.");
    Verify.Throws<PacketEncodingException>(
        () => profile.Find(PacketDirection.ServerToClient, typeof(Packet56RequestPacket)));
    Verify.Throws<PacketProtocolException>(
        () => profile.Find(PacketDirection.ServerToClient, (byte)56).Decode(request56Frame.AsMemory(3)));
    Verify.Throws<PacketProtocolException>(
        () => profile.Find(PacketDirection.ClientToServer, (byte)56).Decode(response56Frame.AsMemory(3)));

    var request69 = new Packet69RequestPacket { Payload = new(8, 12, 20) };
    byte[] request69Frame = profile.Find(PacketDirection.ClientToServer, typeof(Packet69RequestPacket))
        .Encode(request69);
    var response69 = new Packet69ResponsePacket { Payload = new(8, 12, 20, "Supplies") };
    byte[] response69Frame = profile.Find(PacketDirection.ServerToClient, typeof(Packet69ResponsePacket))
        .Encode(response69);
    Verify.That(((Packet69RequestPacket)profile.Find(PacketDirection.ClientToServer, (byte)69)
        .Decode(request69Frame.AsMemory(3))).Payload == request69.Payload
        && ((Packet69ResponsePacket)profile.Find(PacketDirection.ServerToClient, (byte)69)
        .Decode(response69Frame.AsMemory(3))).Payload == response69.Payload,
        "ID 69 request and response bodies must remain direction-specific.");
    Verify.Throws<PacketEncodingException>(
        () => profile.Find(PacketDirection.ClientToServer, typeof(Packet69ResponsePacket)));

    var ping = new Packet82Packet { Payload = new((ushort)Packet82ModuleId.Ping,
        new Packet82EmptyModulePayload()) };
    byte[] moduleFrame = profile.Find(PacketDirection.ClientToServer, typeof(Packet82Packet)).Encode(ping);
    Verify.That(((Packet82Packet)profile.Find(PacketDirection.ClientToServer, (byte)82)
        .Decode(moduleFrame.AsMemory(3))).Payload.ModuleId == (ushort)Packet82ModuleId.Ping,
        "Generated module binding must preserve the module selector.");

    bool[] table = new bool[16];
    var frozenTiles = new Packet20ProtocolFacts(table, true);
    table[2] = true;
    Verify.That(!frozenTiles.IsFrameImportant(2),
        "Packet facts must defensively own table data across profile lifetime.");
    return Task.CompletedTask;
  }

  public static ProtocolFacts CreateFacts() {
    var facts = new ProtocolFacts();
    PacketTileEntityCodecs tiles = PacketTileEntityCodecsV4.Create();
    foreach (PacketDirection direction in Enum.GetValues<PacketDirection>()) {
      facts.Bind(10, direction, "facts", new Packet10ProtocolFacts(new bool[4096],
          new bool[4096], tiles));
      facts.Bind(20, direction, "facts", new Packet20ProtocolFacts(new bool[4096],
          direction == PacketDirection.ClientToServer));
      facts.Bind(23, direction, "facts", new Packet23ProtocolFacts(new bool[4096], (_, _, _) => 4));
      facts.Bind(27, direction, "facts", new Packet27ProtocolFacts(_ => false));
      facts.Bind(72, direction, "facts", new Packet72ProtocolFacts(40));
      facts.Bind(82, direction, "facts", Packet82ProtocolFacts.CreateVersion4(200, _ => false));
      facts.Bind(86, direction, "facts", new Packet86ProtocolFacts(tiles));
    }
    return facts;
  }
}
