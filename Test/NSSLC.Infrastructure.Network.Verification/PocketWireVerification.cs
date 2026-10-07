using System.Buffers.Binary;
using System.Text.Json;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Generated;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class PocketWireVerification {
  private sealed record WireFixture(string Packet, byte Id, string Variant, string Hex);

  public static Task<int> RunAsync() {
    // Match the input values used to capture the upstream baseline in its own process.
    var inputs = new ProtocolInputs(
        frameImportant: new bool[65536],
        allowsSaveCompressionBatching: Enumerable.Repeat(true, 65536).ToArray(),
        tileEntityCodecs: PacketTileEntityCodecsV4.Create(),
        isServer: false,
        catchableTypes: new bool[65536],
        lifeWidthResolver: static (_, _, _) => 1,
        needsUuid: static _ => false,
        slotCount: 0,
        moduleCodecs: Packet82KnownModuleCodecsV4.Create(),
        tagEffectNpcSlotCount: 0,
        tagEffectUsesProcTimes: static _ => false);
    ProtocolProfile profile = TerrariaProtocolProfile.Create(new ProtocolFacts(inputs));
    WireFixture[] fixtures = JsonSerializer.Deserialize<WireFixture[]>(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "WireFixtures.json")))!;
    Verify.That(TerrariaV4Protocol.All.Count == 165
        && fixtures.Select(item => item.Packet).Distinct().Count() == 165,
        "Wire fixtures must cover every one of the 165 generated packet layouts.");
    foreach (PacketGraphManifest manifest in PacketAllDefinitions.CreateInput().Manifests) {
      Verify.That(manifest.PacketType.GetProperty("Payload") is null
          && manifest.PacketType.GetProperty("Body") is null
          && manifest.WireFields.All(field =>
              field.Member.DeclaringType == TypeRef.From(manifest.PacketType)),
          manifest.PacketType.Name + " must declare its wire members directly.");
    }
    foreach (WireFixture fixture in fixtures) {
      ProtocolPacketDescriptor descriptor = TerrariaV4Protocol.All.Single(item =>
          item.PacketType.Name == fixture.Packet);
      PacketDirection direction = descriptor.WireDirection.HasFlag(WireDirection.ClientToServer)
          ? PacketDirection.ClientToServer : PacketDirection.ServerToClient;
      PacketBinding binding = profile.Find(direction, descriptor.PacketType);
      byte[] expected = Convert.FromHexString(fixture.Hex);
      object packet = CreatePacket(descriptor.PacketType, fixture.Id, fixture.Variant);
      byte[] encoded = binding.Encode(packet);
      string caseName = fixture.Packet + "/" + fixture.Variant;
      Verify.That(encoded[2] == fixture.Id
          && BinaryPrimitives.ReadUInt16LittleEndian(encoded) == encoded.Length
          && encoded.AsSpan(3).SequenceEqual(expected),
          caseName + " must preserve the baseline bytes and add exactly one frame header.");
      object decoded = binding.Decode(expected);
      Verify.That(binding.Encode(decoded).AsSpan(3).SequenceEqual(expected),
          caseName + " must preserve the baseline bytes through the gateway binding.");
      if (packet is PlayerControlsPacket controls) {
        var actual = (PlayerControlsPacket)decoded;
        Verify.That(actual.Position == controls.Position && actual.Velocity == controls.Velocity
            && actual.CameraTarget == controls.CameraTarget && actual.MountType == controls.MountType,
            caseName + " must retain the conditional member values.");
      }
      if (packet is TileSectionPacket tiles) {
        var actual = (TileSectionPacket)decoded;
        Verify.That(actual.Width == tiles.Width && actual.Height == tiles.Height
            && actual.Tiles.SequenceEqual(tiles.Tiles),
            caseName + " must retain the compressed tile section values.");
      }
    }
    Console.WriteLine($"PASS {fixtures.Length} baseline wire cases across 165 packet layouts.");
    return Task.FromResult(0);
  }

  private static object CreatePacket(Type type, byte id, string variant) {
    switch (id) {
      case 10:
        return new TileSectionPacket { Width = 1, Height = 1, Tiles = [default] };
      case 20:
        return new AreaTileChangePacket { Width = 1, Height = 1, Tiles = [default] };
      case 23:
        return new SyncNPCPacket { FullLife = true };
      case 82:
        return new NetModulesPacket { ModuleId = 0, Data = new Packet82LiquidData([]) };
      case 121:
        return new TEDisplayDollDataSyncPacket { Item = default(PacketTileEntityItem) };
      case 146:
        return new ShimmerActionsPacket { Position = new(1.25f, -2.5f) };
      case 13 when variant == "conditional":
        return new PlayerControlsPacket {
          Player = 3, MovementFlags = 0x84, PlayerFeatureFlags = 0x40, ActionFlags = 0x20,
          SelectedItem = 5, Position = new(1.25f, -2.5f), Velocity = new(3.5f, -4.75f),
          MountType = 7, PotionOfReturnUsePosition = new(5.25f, 6.5f),
          PotionOfReturnHomePosition = new(7.25f, 8.5f), CameraTarget = new(9.25f, 10.5f)
        };
      case 88 when variant == "conditional":
        return new ItemTweakerPacket {
          Flags = 0x7f, Color = 0x89abcdef, Damage = 321, Knockback = 1.25f,
          UseAnimation = 123, UseTime = 234, Shoot = 42, ShootSpeed = 3.75f
        };
      default:
        return Activator.CreateInstance(type)!;
    }
  }
}
