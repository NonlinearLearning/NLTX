using System.Buffers.Binary;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class GeneratedProfileVerification {
  public static Task RunAsync() {
    Verify.Throws<InvalidOperationException>(() => TerrariaProtocolProfile.Create(new ProtocolFacts()));
    Verify.Throws<InvalidOperationException>(() =>
        Terraria.NetWork.Generated.TerrariaV4Protocol.Packets.HelloPacket.CreateReader(
            ReadOnlyMemory<byte>.Empty));
    ProtocolFacts facts = CreateFacts();
    ProtocolProfile profile = TerrariaProtocolProfile.Create(facts);
    Verify.That(profile.Key == "TerrariaV4:" + TerrariaProtocolProfile.Fingerprint + ":" + facts.Version
        && profile.HelloVersion == "Terraria319"
        && profile.Bindings.Select(binding => binding.MessageId).Distinct().Count() == 162,
        "The generated profile must retain all 162 wire IDs and its fingerprint.");
    VerifySessionDirections(profile);
    Verify.That(TerrariaProtocolProfile.Create(CreateFacts()).Key != profile.Key,
        "Separate fact snapshots must not share a profile cache identity by default.");
    Verify.Throws<InvalidOperationException>(() => facts.Bind(1, PacketDirection.ClientToServer,
        "late", true));

    byte[] hello = profile.Find(PacketDirection.ClientToServer, typeof(HelloPacket))
        .Encode(new HelloPacket { Version = "Terraria319" });
    Verify.That(hello[2] == 1 && BinaryPrimitives.ReadUInt16LittleEndian(hello) == hello.Length,
        "Generated Hello must have exactly one complete-frame prefix.");
    Verify.That(((HelloPacket)profile.Find(PacketDirection.ClientToServer, (byte)1)
        .Decode(hello.AsMemory(3))).Version == "Terraria319",
        "Generated Hello must round-trip its wire version marker.");
    Verify.Throws<PacketProtocolException>(() => profile.Find(PacketDirection.ClientToServer, (byte)1)
        .Decode(hello.AsMemory(3, hello.Length - 4)));
    Verify.Throws<PacketProtocolException>(() => profile.Find(PacketDirection.ClientToServer, (byte)1)
        .Decode(hello.AsMemory(3).ToArray().Concat(new byte[] { 0 }).ToArray()));

    var request56 = new UniqueTownNPCInfoSyncRequestPacket { NpcIndex = 123 };
    byte[] request56Frame = profile.Find(PacketDirection.ClientToServer, typeof(UniqueTownNPCInfoSyncRequestPacket))
        .Encode(request56);
    Verify.That(request56Frame.Length == 5
        && ((UniqueTownNPCInfoSyncRequestPacket)profile.Find(PacketDirection.ClientToServer, (byte)56)
        .Decode(request56Frame.AsMemory(3))).NpcIndex == request56.NpcIndex,
        "ID 56 C2S must use its short request layout.");
    var response56 = new UniqueTownNPCInfoSyncResponsePacket {
      NpcIndex = 123, GivenName = "Guide", Variation = 4
    };
    byte[] response56Frame = profile.Find(PacketDirection.ServerToClient, typeof(UniqueTownNPCInfoSyncResponsePacket))
        .Encode(response56);
    var decoded56 = (UniqueTownNPCInfoSyncResponsePacket)profile.Find(
        PacketDirection.ServerToClient, (byte)56).Decode(response56Frame.AsMemory(3));
    Verify.That(decoded56.NpcIndex == 123 && decoded56.GivenName == "Guide"
        && decoded56.Variation == 4,
        "ID 56 S2C must use its response layout including name and variation.");
    Verify.Throws<PacketEncodingException>(
        () => profile.Find(PacketDirection.ServerToClient, typeof(UniqueTownNPCInfoSyncRequestPacket)));
    Verify.Throws<PacketProtocolException>(
        () => profile.Find(PacketDirection.ServerToClient, (byte)56).Decode(request56Frame.AsMemory(3)));
    Verify.Throws<PacketProtocolException>(
        () => profile.Find(PacketDirection.ClientToServer, (byte)56).Decode(response56Frame.AsMemory(3)));

    var request69 = new ChestNameRequestPacket { ChestIndex = 8, X = 12, Y = 20 };
    byte[] request69Frame = profile.Find(PacketDirection.ClientToServer, typeof(ChestNameRequestPacket))
        .Encode(request69);
    var response69 = new ChestNameResponsePacket {
      ChestIndex = 8, X = 12, Y = 20, ChestName = "Supplies"
    };
    byte[] response69Frame = profile.Find(PacketDirection.ServerToClient, typeof(ChestNameResponsePacket))
        .Encode(response69);
    var decodedRequest69 = (ChestNameRequestPacket)profile.Find(PacketDirection.ClientToServer,
        (byte)69).Decode(request69Frame.AsMemory(3));
    var decodedResponse69 = (ChestNameResponsePacket)profile.Find(PacketDirection.ServerToClient,
        (byte)69).Decode(response69Frame.AsMemory(3));
    Verify.That(decodedRequest69.ChestIndex == 8 && decodedRequest69.X == 12
        && decodedRequest69.Y == 20 && decodedResponse69.ChestIndex == 8
        && decodedResponse69.X == 12 && decodedResponse69.Y == 20
        && decodedResponse69.ChestName == "Supplies",
        "ID 69 request and response bodies must remain direction-specific.");
    Verify.Throws<PacketEncodingException>(
        () => profile.Find(PacketDirection.ClientToServer, typeof(ChestNameResponsePacket)));

    var ping = new NetModulesPacket {
      ModuleId = (ushort)Packet82ModuleId.Ping,
      Data = new Packet82PingData(new PacketVector2(125.5f, -42.25f))
    };
    byte[] moduleFrame = profile.Find(PacketDirection.ClientToServer, typeof(NetModulesPacket)).Encode(ping);
    PacketBinding moduleBinding = profile.Find(PacketDirection.ClientToServer, (byte)82);
    var decodedPing = (NetModulesPacket)moduleBinding.Decode(moduleFrame.AsMemory(3));
    Verify.That(moduleFrame.SequenceEqual(Convert.FromHexString("0D005202000000FB42000029C2"))
        && decodedPing.ModuleId == (ushort)Packet82ModuleId.Ping
        && decodedPing.Data is Packet82PingData data && data.Position == new PacketVector2(125.5f, -42.25f),
        "Module Ping must carry two little-endian floats matching the real client's frame.");
    Verify.Throws<PacketProtocolException>(() => moduleBinding.Decode(new byte[] { 2, 0 }));
    Verify.Throws<PacketProtocolException>(() => moduleBinding.Decode(moduleFrame.AsMemory(3, 9)));
    Verify.Throws<PacketProtocolException>(() => moduleBinding.Decode(
        moduleFrame.AsMemory(3).ToArray().Concat(new byte[] { 0 }).ToArray()));

    Verify.That(ReferenceEquals(facts.Inputs, ProtocolInputs.Instance),
        "All generated codecs must use the process's initialized protocol model.");
    Verify.Throws<InvalidOperationException>(() => CreateInputs());
    return Task.CompletedTask;
  }

  public static ProtocolFacts CreateFacts(bool steamModules = false) {
    if (!ProtocolInputs.IsInitialized) {
      _ = CreateInputs(steamModules);
    }
    return new ProtocolFacts(ProtocolInputs.Instance);
  }

  private static void VerifySessionDirections(ProtocolProfile profile) {
    foreach (byte id in new byte[] { 1, 6, 38, 68, 161 }) {
      _ = profile.Find(PacketDirection.ClientToServer, id);
      Verify.Throws<PacketProtocolException>(() =>
          profile.Find(PacketDirection.ServerToClient, id));
    }
    foreach (byte id in new byte[] { 2, 3, 9, 37, 49, 129, 139 }) {
      _ = profile.Find(PacketDirection.ServerToClient, id);
      Verify.Throws<PacketProtocolException>(() =>
          profile.Find(PacketDirection.ClientToServer, id));
    }
    _ = profile.Find(PacketDirection.ClientToServer, (byte)154);
    _ = profile.Find(PacketDirection.ServerToClient, (byte)154);
    ProtocolProfile production = ServerProtocolProfile.Create(profile);
    Verify.Throws<PacketProtocolException>(() =>
        production.Find(PacketDirection.ClientToServer, (byte)93));
    Verify.Throws<PacketProtocolException>(() =>
        production.Find(PacketDirection.ServerToClient, (byte)93));
    foreach (byte id in new byte[] { 7, 10, 11, 18, 57, 146 }) {
      _ = production.Find(PacketDirection.ServerToClient, id);
      Verify.Throws<PacketProtocolException>(() =>
          production.Find(PacketDirection.ClientToServer, id));
    }
    _ = production.Find(PacketDirection.ClientToServer, (byte)8);
    Verify.Throws<PacketProtocolException>(() =>
        production.Find(PacketDirection.ServerToClient, (byte)8));
    _ = production.Find(PacketDirection.ClientToServer, (byte)65);
    _ = production.Find(PacketDirection.ServerToClient, (byte)65);
  }

  private static ProtocolInputs CreateInputs(bool steamModules = false) {
    return new ProtocolInputs(
        frameImportant: new bool[65536],
        allowsSaveCompressionBatching: Enumerable.Repeat(true, 65536).ToArray(),
        tileEntityCodecs: PacketTileEntityCodecsV4.Create(),
        isServer: true,
        catchableTypes: new bool[65536],
        lifeWidthResolver: static (_, _, _) => 4,
        needsUuid: static _ => false,
        slotCount: 40,
        moduleCodecs: steamModules
            ? Packet82KnownModuleCodecsV4.CreateSteam() : Packet82KnownModuleCodecsV4.Create(),
        tagEffectNpcSlotCount: 200,
        tagEffectUsesProcTimes: static _ => false);
  }
}
