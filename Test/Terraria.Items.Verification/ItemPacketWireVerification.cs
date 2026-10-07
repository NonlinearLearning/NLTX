using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.Items.Verification;

internal static class ItemPacketWireVerification
{
  public static void Run()
  {
    VerifyItem21LiteralLayouts();
    VerifyItem21RejectsMalformedBodies();
    VerifyItem22LiteralLayout();
    VerifyItem22RejectsMalformedBodies();
    VerifyItem39LiteralLayout();
    VerifyItem90LiteralLayouts();
    VerifyItem88LiteralLayout();
    VerifyItem151LiteralLayout();
    VerifyItem160LiteralLayout();
    VerifySteamItemProfileDirections();
  }

  private static void VerifyItem21LiteralLayouts()
  {
    byte[][] fixtures =
    [
      Convert.FromHexString("01000000803F00000040000080BF0000003F0A0003002A00"),
      Convert.FromHexString("01000000803F00000040000080BF0000003F0A0003042A00010000003F"),
      Convert.FromHexString("01000000803F00000040000080BF0000003F0A0003082A000C"),
      Convert.FromHexString("01000000803F00000040000080BF0000003F0A00030C2A00010000003F0C")
    ];

    byte[] flags = [0x00, 0x04, 0x08, 0x0C];
    for (int index = 0; index < fixtures.Length; index++)
    {
      byte stateFlags = flags[index];
      var packet = new SyncItemPacket
      {
        ItemIndex = 1,
        PositionX = 1.0f,
        PositionY = 2.0f,
        VelocityX = -1.0f,
        VelocityY = 0.5f,
        Stack = 10,
        Prefix = 3,
        StateFlags = stateFlags,
        ItemType = 42,
        Shimmered = (stateFlags & 0x04) != 0 ? true : null,
        ShimmerTime = (stateFlags & 0x04) != 0 ? 0.5f : null,
        EnemyGrabDelayTime = (stateFlags & 0x08) != 0 ? (byte)12 : null
      };

      AssertBytes(fixtures[index], SteamItemPacketCodec.EncodeSyncItemBody(packet),
        $"packet 21 flags 0x{stateFlags:X2} encoding");
      var decoded = SteamItemPacketCodec.DecodeSyncItemBody(fixtures[index]);
      Assert(decoded.Success, $"packet 21 flags 0x{stateFlags:X2} fixture decodes");
      AssertItemPacket(decoded.Packet!, stateFlags);
    }
  }

  private static void VerifyItem21RejectsMalformedBodies()
  {
    byte[] shimmerTruncated = Convert.FromHexString(
      "01000000803F00000040000080BF0000003F0A0003042A0001000000");
    byte[] trailingByte = Convert.FromHexString(
      "01000000803F00000040000080BF0000003F0A0003002A0000");
    byte[] unknownFlag = Convert.FromHexString(
      "01000000803F00000040000080BF0000003F0A0003102A00");

    Assert(!SteamItemPacketCodec.DecodeSyncItemBody(shimmerTruncated).Success,
      "packet 21 rejects a truncated shimmer extension");
    Assert(!SteamItemPacketCodec.DecodeSyncItemBody(trailingByte).Success,
      "packet 21 rejects trailing bytes");
    Assert(!SteamItemPacketCodec.DecodeSyncItemBody(unknownFlag).Success,
      "packet 21 rejects unknown extension flags");

    var inconsistent = new SyncItemPacket
    {
      ItemIndex = 1,
      PositionX = 1.0f,
      PositionY = 2.0f,
      VelocityX = -1.0f,
      VelocityY = 0.5f,
      Stack = 10,
      Prefix = 3,
      StateFlags = 0x04,
      ItemType = 42,
      Shimmered = true
    };
    AssertThrows<PacketEncodingException>(
      () => SteamItemPacketCodec.EncodeSyncItemBody(inconsistent),
      "packet 21 rejects a missing flagged shimmer timer");
  }

  private static void VerifyItem22LiteralLayout()
  {
    byte[] fixture = Convert.FromHexString(
      "070003AC02FF80010000C03F000010C0");
    var packet = new ItemOwnerPacket
    {
      ItemIndex = 7,
      ReservedForPlayer = 3,
      TimeToKeepReservation = 300,
      GrabDelayPlayer = 255,
      GrabDelayTime = 128,
      PositionX = 1.5f,
      PositionY = -2.25f
    };

    AssertBytes(fixture, SteamItemPacketCodec.EncodeItemOwnerBody(packet),
      "packet 22 encoding");
    var decoded = SteamItemPacketCodec.DecodeItemOwnerBody(fixture);
    Assert(decoded.Success, "packet 22 literal fixture decodes");
    Assert(decoded.Packet!.ItemIndex == 7, "packet 22 item slot");
    Assert(decoded.Packet.ReservedForPlayer == 3, "packet 22 reserved player");
    Assert(decoded.Packet.TimeToKeepReservation == 300,
      "packet 22 7-bit reservation time");
    Assert(decoded.Packet.GrabDelayPlayer == 255, "packet 22 grab-delay player");
    Assert(decoded.Packet.GrabDelayTime == 128, "packet 22 7-bit grab delay");
    Assert(decoded.Packet.PositionX == 1.5f && decoded.Packet.PositionY == -2.25f,
      "packet 22 position");
  }

  private static void VerifyItem22RejectsMalformedBodies()
  {
    byte[] fixture = Convert.FromHexString(
      "070003AC02FF80010000C03F000010C0");
    byte[] truncated = fixture[..^1];
    byte[] trailing = [.. fixture, 0x00];
    byte[] overlongTimer = Convert.FromHexString(
      "0700038000FF00000000000000000000");
    byte[] negativeReservationTime = Convert.FromHexString(
      "070003FFFFFFFF0FFF80010000C03F000010C0");
    byte[] negativeGrabDelayTime = Convert.FromHexString(
      "070003AC02FFFFFFFFFF0F0000C03F000010C0");

    Assert(!SteamItemPacketCodec.DecodeItemOwnerBody(truncated).Success,
      "packet 22 rejects truncation");
    Assert(!SteamItemPacketCodec.DecodeItemOwnerBody(trailing).Success,
      "packet 22 rejects trailing bytes");
    Assert(!SteamItemPacketCodec.DecodeItemOwnerBody(overlongTimer).Success,
      "packet 22 rejects noncanonical 7-bit integers");
    Assert(!SteamItemPacketCodec.DecodeItemOwnerBody(negativeReservationTime).Success,
      "packet 22 rejects a negative reservation timer");
    Assert(!SteamItemPacketCodec.DecodeItemOwnerBody(negativeGrabDelayTime).Success,
      "packet 22 rejects a negative grab-delay timer");
  }

  private static void VerifyItem39LiteralLayout()
  {
    byte[] noForce = Convert.FromHexString("070000");
    byte[] force = Convert.FromHexString("070001");
    var packet = new ReleaseItemOwnershipPacket
    {
      ItemIndex = 7,
      ForceAssignToServer = true
    };

    AssertBytes(force,
      SteamItemPacketCodec.EncodeReleaseItemOwnershipBody(packet),
      "packet 39 encoding with force flag");
    PacketReadResult<ReleaseItemOwnershipPacket> decodedForce =
      SteamItemPacketCodec.DecodeReleaseItemOwnershipBody(force);
    Assert(decodedForce.Success && decodedForce.Packet!.ItemIndex == 7 &&
      decodedForce.Packet.ForceAssignToServer,
      "packet 39 reads the force-assign flag");

    var withoutForce = new ReleaseItemOwnershipPacket
    {
      ItemIndex = 7,
      ForceAssignToServer = false
    };
    AssertBytes(noForce,
      SteamItemPacketCodec.EncodeReleaseItemOwnershipBody(withoutForce),
      "packet 39 encoding without force flag");
    Assert(SteamItemPacketCodec.DecodeReleaseItemOwnershipBody(noForce).Success,
      "packet 39 reads the false force-assign flag");
    Assert(!SteamItemPacketCodec.DecodeReleaseItemOwnershipBody(force[..^1]).Success,
      "packet 39 rejects a missing force flag");
    byte[] trailingForce = [.. force, 0x00];
    Assert(!SteamItemPacketCodec.DecodeReleaseItemOwnershipBody(
      trailingForce).Success,
      "packet 39 rejects trailing bytes");
    PacketReadResult<ReleaseItemOwnershipPacket> nonZeroBoolean =
      SteamItemPacketCodec.DecodeReleaseItemOwnershipBody(
        Convert.FromHexString("070002"));
    Assert(nonZeroBoolean.Success && nonZeroBoolean.Packet!.ForceAssignToServer,
      "packet 39 accepts the nonzero boolean representation used by ReadBoolean");
  }

  private static void VerifyItem90LiteralLayouts()
  {
    byte[][] fixtures =
    [
      Convert.FromHexString("01000000803F00000040000080BF0000003F0A0003002A00"),
      Convert.FromHexString("01000000803F00000040000080BF0000003F0A0003042A00010000003F"),
      Convert.FromHexString("01000000803F00000040000080BF0000003F0A0003082A000C"),
      Convert.FromHexString("01000000803F00000040000080BF0000003F0A00030C2A00010000003F0C")
    ];
    byte[] flags = [0x00, 0x04, 0x08, 0x0C];

    for (int index = 0; index < fixtures.Length; index++)
    {
      byte stateFlags = flags[index];
      var packet = new InstancedItemPacket
      {
        ItemIndex = 1,
        PositionX = 1.0f,
        PositionY = 2.0f,
        VelocityX = -1.0f,
        VelocityY = 0.5f,
        Stack = 10,
        Prefix = 3,
        StateFlags = stateFlags,
        ItemType = 42,
        Shimmered = (stateFlags & 0x04) != 0 ? true : null,
        ShimmerTime = (stateFlags & 0x04) != 0 ? 0.5f : null,
        EnemyGrabDelayTime = (stateFlags & 0x08) != 0 ? (byte)12 : null
      };

      AssertBytes(fixtures[index],
        SteamItemPacketCodec.EncodeInstancedItemBody(packet),
        $"packet 90 flags 0x{stateFlags:X2} encoding");
      PacketReadResult<InstancedItemPacket> decoded =
        SteamItemPacketCodec.DecodeInstancedItemBody(fixtures[index]);
      Assert(decoded.Success, $"packet 90 flags 0x{stateFlags:X2} fixture decodes");
      AssertInstancedItemPacket(decoded.Packet!, stateFlags);
    }

    Assert(!SteamItemPacketCodec.DecodeInstancedItemBody(fixtures[1][..^1]).Success,
      "packet 90 rejects a truncated shimmer extension");
    byte[] trailingItem = [.. fixtures[0], 0x00];
    Assert(!SteamItemPacketCodec.DecodeInstancedItemBody(
      trailingItem).Success,
      "packet 90 rejects trailing bytes");
  }

  private static void VerifyItem88LiteralLayout()
  {
    byte[] fixture = Convert.FromHexString(
      "0300FF443322114101000020400C0014000700000090403F200010000000A03F0700080001");
    var extra = new Packet88ExtraValues(
      Flags: 0x3F,
      Width: 32,
      Height: 16,
      Scale: 1.25f,
      Ammo: 7,
      UseAmmo: 8,
      NotAmmo: true);

    using var stream = new MemoryStream();
    var writer = new PacketWireWriter(stream, fixture.Length);
    ItemTweakerPacket.Write(
      writer,
      itemIndex: 3,
      flags: 0xFF,
      color: 0x11223344,
      damage: 321,
      knockback: 2.5f,
      useAnimation: 12,
      useTime: 20,
      shoot: 7,
      shootSpeed: 4.5f,
      extra: extra);
    AssertBytes(fixture, stream.ToArray(), "packet 88 conditional-field encoding");

    var reader = new PacketWireReader(fixture);
    var decoded = ItemTweakerPacket.Read(reader);
    reader.RequireFrameEnd();
    Assert(decoded.ItemIndex == 3 && decoded.Flags == 0xFF,
      "packet 88 slot and primary flags");
    Assert(decoded.Color == 0x11223344 && decoded.Damage == 321 &&
      decoded.Knockback == 2.5f && decoded.UseAnimation == 12 &&
      decoded.UseTime == 20 && decoded.Shoot == 7 && decoded.ShootSpeed == 4.5f,
      "packet 88 primary conditional values");
    Assert(decoded.Extra == extra,
      "packet 88 extended conditional values");
    AssertThrows<PacketWireTruncationException>(
      () => ItemTweakerPacket.Read(new PacketWireReader(fixture[..^1])),
      "packet 88 rejects a truncated extended value");
    byte[] trailingFixture = [.. fixture, 0x00];
    AssertThrows<PacketWireFormatException>(() =>
    {
      var trailingReader = new PacketWireReader(trailingFixture);
      _ = ItemTweakerPacket.Read(trailingReader);
      trailingReader.RequireFrameEnd();
    }, "packet 88 rejects trailing bytes");

    using var invalidStream = new MemoryStream();
    var invalidWriter = new PacketWireWriter(invalidStream, fixture.Length);
    AssertThrows<PacketWireFormatException>(
      () => ItemTweakerPacket.Write(
        invalidWriter,
        itemIndex: 3,
        flags: 0x01,
        color: null,
        damage: null,
        knockback: null,
        useAnimation: null,
        useTime: null,
        shoot: null,
        shootSpeed: null,
        extra: null),
      "packet 88 rejects a field-presence flag without its value");
    AssertThrows<PacketWireFormatException>(
      () => ItemTweakerPacket.Write(
        invalidWriter,
        itemIndex: 3,
        flags: 0x80,
        color: null,
        damage: null,
        knockback: null,
        useAnimation: null,
        useTime: null,
        shoot: null,
        shootSpeed: null,
        extra: new Packet88ExtraValues(0x40, null, null, null, null, null, null)),
      "packet 88 rejects unknown extended flags");
    AssertThrows<PacketWireFormatException>(
      () => ItemTweakerPacket.Read(new PacketWireReader(
        Convert.FromHexString("03008040"))),
      "packet 88 rejects unknown extended flags when reading");
    AssertThrows<PacketWireFormatException>(
      () => ItemTweakerPacket.Write(
        invalidWriter,
        itemIndex: 3,
        flags: 0x04,
        color: null,
        damage: null,
        knockback: float.NaN,
        useAnimation: null,
        useTime: null,
        shoot: null,
        shootSpeed: null,
        extra: null),
      "packet 88 rejects non-finite item values");
    AssertThrows<PacketWireFormatException>(
      () => ItemTweakerPacket.Read(new PacketWireReader(
        Convert.FromHexString("0300040000C07F"))),
      "packet 88 rejects non-finite item values when reading");
  }

  private static void VerifyItem151LiteralLayout()
  {
    byte[] fixture = Convert.FromHexString("0700");
    using var stream = new MemoryStream();
    var writer = new PacketWireWriter(stream, fixture.Length);
    SyncItemDespawnPacket.Write(writer, itemIndex: 7);
    AssertBytes(fixture, stream.ToArray(), "packet 151 encoding");

    var reader = new PacketWireReader(fixture);
    short itemIndex = SyncItemDespawnPacket.Read(reader);
    reader.RequireFrameEnd();
    Assert(itemIndex == 7, "packet 151 item slot");
    AssertThrows<PacketWireTruncationException>(
      () => SyncItemDespawnPacket.Read(new PacketWireReader(fixture[..^1])),
      "packet 151 rejects a truncated slot");
    byte[] trailingFixture = [.. fixture, 0x00];
    AssertThrows<PacketWireFormatException>(() =>
    {
      var trailingReader = new PacketWireReader(trailingFixture);
      _ = SyncItemDespawnPacket.Read(trailingReader);
      trailingReader.RequireFrameEnd();
    }, "packet 151 rejects trailing bytes");
    AssertThrows<PacketWireFormatException>(
      () => SyncItemDespawnPacket.Write(writer, itemIndex: 400),
      "packet 151 rejects the 400 creation sentinel as a slot");
    AssertThrows<PacketWireFormatException>(
      () => SyncItemDespawnPacket.Read(new PacketWireReader(
        Convert.FromHexString("9001"))),
      "packet 151 rejects an out-of-range item slot when reading");
  }

  private static void VerifyItem160LiteralLayout()
  {
    byte[] fixture = Convert.FromHexString("07000000C03F000010C0");
    var position = new PacketVector2(1.5f, -2.25f);
    using var stream = new MemoryStream();
    var writer = new PacketWireWriter(stream, fixture.Length);
    ItemPositionPacket.Write(writer, itemIndex: 7, position: position);
    AssertBytes(fixture, stream.ToArray(), "packet 160 encoding");

    var reader = new PacketWireReader(fixture);
    (short itemIndex, PacketVector2 decodedPosition) = ItemPositionPacket.Read(reader);
    reader.RequireFrameEnd();
    Assert(itemIndex == 7, "packet 160 item slot");
    Assert(decodedPosition == position, "packet 160 position");
    AssertThrows<PacketWireTruncationException>(
      () => ItemPositionPacket.Read(new PacketWireReader(fixture[..^1])),
      "packet 160 rejects a truncated position");
    byte[] trailingFixture = [.. fixture, 0x00];
    AssertThrows<PacketWireFormatException>(() =>
    {
      var trailingReader = new PacketWireReader(trailingFixture);
      _ = ItemPositionPacket.Read(trailingReader);
      trailingReader.RequireFrameEnd();
    }, "packet 160 rejects trailing bytes");
    AssertThrows<PacketWireFormatException>(
      () => ItemPositionPacket.Write(
        writer,
        itemIndex: 400,
        position: position),
      "packet 160 rejects the 400 creation sentinel as a slot");
    AssertThrows<PacketWireFormatException>(
      () => ItemPositionPacket.Read(new PacketWireReader(
        Convert.FromHexString("07000000C07F000010C0"))),
      "packet 160 rejects non-finite positions when reading");
  }

  private static void VerifySteamItemProfileDirections()
  {
    if (!ProtocolInputs.IsInitialized)
    {
      _ = new ProtocolInputs(
        frameImportant: new bool[65536],
        allowsSaveCompressionBatching: Enumerable.Repeat(true, 65536).ToArray(),
        tileEntityCodecs: PacketTileEntityCodecsV4.Create(),
        isServer: true,
        catchableTypes: new bool[65536],
        lifeWidthResolver: static (_, _, _) => 4,
        needsUuid: static _ => false,
        slotCount: 40,
        moduleCodecs: Packet82KnownModuleCodecsV4.Create(),
        tagEffectNpcSlotCount: 200,
        tagEffectUsesProcTimes: static _ => false);
    }

    ProtocolProfile profile = SteamProtocolProfile.Create(
      new ProtocolFacts(ProtocolInputs.Instance, "items-wire-profile"));
    AssertHasBinding(profile, 21, PacketDirection.ClientToServer);
    AssertHasBinding(profile, 21, PacketDirection.ServerToClient);
    AssertMissingBinding(profile, 22, PacketDirection.ClientToServer);
    AssertHasBinding(profile, 22, PacketDirection.ServerToClient);
    AssertHasBinding(profile, 39, PacketDirection.ClientToServer);
    AssertHasBinding(profile, 39, PacketDirection.ServerToClient);
    AssertMissingBinding(profile, 88, PacketDirection.ClientToServer);
    AssertHasBinding(profile, 88, PacketDirection.ServerToClient);
    AssertHasBinding(profile, 90, PacketDirection.ClientToServer);
    AssertHasBinding(profile, 90, PacketDirection.ServerToClient);
    AssertMissingBinding(profile, 145, PacketDirection.ClientToServer);
    AssertMissingBinding(profile, 145, PacketDirection.ServerToClient);
    AssertMissingBinding(profile, 148, PacketDirection.ClientToServer);
    AssertMissingBinding(profile, 148, PacketDirection.ServerToClient);
    AssertHasBinding(profile, 151, PacketDirection.ClientToServer);
    AssertHasBinding(profile, 151, PacketDirection.ServerToClient);
    AssertMissingBinding(profile, 160, PacketDirection.ClientToServer);
    AssertHasBinding(profile, 160, PacketDirection.ServerToClient);
  }

  private static void AssertHasBinding(
    ProtocolProfile profile,
    byte messageId,
    PacketDirection direction)
  {
    _ = profile.Find(direction, messageId);
  }

  private static void AssertMissingBinding(
    ProtocolProfile profile,
    byte messageId,
    PacketDirection direction)
  {
    try
    {
      _ = profile.Find(direction, messageId);
    }
    catch (PacketProtocolException)
    {
      return;
    }

    throw new InvalidOperationException(
      $"Expected Steam profile to omit item message {messageId} in {direction} direction.");
  }

  private static void AssertItemPacket(SyncItemPacket packet, byte flags)
  {
    Assert(packet.ItemIndex == 1, "packet 21 item slot");
    Assert(packet.PositionX == 1.0f && packet.PositionY == 2.0f,
      "packet 21 position");
    Assert(packet.VelocityX == -1.0f && packet.VelocityY == 0.5f,
      "packet 21 velocity");
    Assert(packet.Stack == 10 && packet.Prefix == 3 && packet.ItemType == 42,
      "packet 21 stack, prefix, and type");
    Assert(packet.StateFlags == flags, "packet 21 flags");
    Assert(packet.Shimmered == ((flags & 0x04) != 0 ? true : null),
      "packet 21 shimmered extension");
    Assert(packet.ShimmerTime == ((flags & 0x04) != 0 ? 0.5f : null),
      "packet 21 shimmer-time extension");
    Assert(packet.EnemyGrabDelayTime ==
      ((flags & 0x08) != 0 ? (byte)12 : null),
      "packet 21 enemy-grab-delay extension");
  }

  private static void AssertInstancedItemPacket(
    InstancedItemPacket packet,
    byte flags)
  {
    Assert(packet.ItemIndex == 1, "packet 90 item slot");
    Assert(packet.PositionX == 1.0f && packet.PositionY == 2.0f,
      "packet 90 position");
    Assert(packet.VelocityX == -1.0f && packet.VelocityY == 0.5f,
      "packet 90 velocity");
    Assert(packet.Stack == 10 && packet.Prefix == 3 && packet.ItemType == 42,
      "packet 90 stack, prefix, and type");
    Assert(packet.StateFlags == flags, "packet 90 flags");
    Assert(packet.Shimmered == ((flags & 0x04) != 0 ? true : null),
      "packet 90 shimmered extension");
    Assert(packet.ShimmerTime == ((flags & 0x04) != 0 ? 0.5f : null),
      "packet 90 shimmer-time extension");
    Assert(packet.EnemyGrabDelayTime ==
      ((flags & 0x08) != 0 ? (byte)12 : null),
      "packet 90 enemy-grab-delay extension");
  }

  private static void AssertBytes(byte[] expected, byte[] actual, string name)
  {
    if (!expected.AsSpan().SequenceEqual(actual))
    {
      throw new InvalidOperationException(
        $"{name}: expected {Convert.ToHexString(expected)}, got {Convert.ToHexString(actual)}.");
    }
  }

  private static void Assert(bool condition, string name)
  {
    if (!condition)
    {
      throw new InvalidOperationException($"Assertion failed: {name}.");
    }
  }

  private static void AssertThrows<TException>(Action action, string name)
    where TException : Exception
  {
    try
    {
      action();
    }
    catch (TException)
    {
      return;
    }

    throw new InvalidOperationException($"Expected {typeof(TException).Name}: {name}.");
  }
}
