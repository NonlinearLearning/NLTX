using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Terraria.Dome.Protocol.V1456.Dispatch;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Protocol.V1456.Session;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.WorldObjects.Sign;

WorldGrid world = new(width: 400, height: 300);
_ = world.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 1));
_ = world.TrySetTile(11, 10, new WorldTile(IsActive: true, Type: 1));
_ = world.TrySetTile(12, 10, new WorldTile(IsActive: true, Type: 2));
WorldTile complexTile = new(
  IsActive: true,
  Type: 320,
  LiquidAmount: 200,
  LiquidType: 3,
  FrameX: 18,
  FrameY: 36,
  WallType: 257,
  HasWire: true,
  HasWire2: true,
  HasWire3: true,
  HasWire4: true,
  IsHalfBrick: false,
  Slope: 4,
  IsActuated: true,
  IsInactive: true,
  TileColor: 7,
  WallColor: 8,
  IsInvisibleBlock: true,
  IsInvisibleWall: true,
  IsFullbrightBlock: true,
  IsFullbrightWall: true);
_ = world.TrySetTile(14, 10, complexTile);
WorldSectionSnapshot snapshot = world.CreateSectionSnapshot(new WorldSectionCoordinates(0, 0));

byte[] frameBytes = TerrariaPacketCodec.Encode(snapshot);
TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
if (frame.MessageId != TerrariaMessageId.TileSection)
{
  throw new InvalidOperationException("World section encoder did not use V1456 TileSection.");
}

using MemoryStream compressedStream = new(frame.Payload.ToArray(), writable: false);
using DeflateStream decompressor = new(
  compressedStream,
  CompressionMode.Decompress,
  leaveOpen: false);
using BinaryReader reader = new(decompressor);
int originX = reader.ReadInt32();
int originY = reader.ReadInt32();
short width = reader.ReadInt16();
short height = reader.ReadInt16();
if (originX != 0 || originY != 0 || width != 200 || height != 150)
{
  throw new InvalidOperationException("World section encoder wrote incorrect section coordinates.");
}

WorldTile[] decodedTiles = ReadTiles(reader, width * height);
WorldTile decodedComplexTile = decodedTiles[14 + 10 * width];
short chestCount = reader.ReadInt16();
short signCount = reader.ReadInt16();
short entityCount = reader.ReadInt16();
if (decodedTiles[10 + 10 * width] != new WorldTile(IsActive: true, Type: 1) ||
    decodedTiles[11 + 10 * width] != new WorldTile(IsActive: true, Type: 1) ||
    decodedTiles[12 + 10 * width] != new WorldTile(IsActive: true, Type: 2) ||
    decodedTiles[13 + 10 * width] != default ||
    decodedComplexTile != complexTile ||
    chestCount != 0 || signCount != 0 || entityCount != 0)
{
  throw new InvalidOperationException(
    "World section encoder did not preserve authoritative tile state. " +
    $"Expected complex tile {complexTile}; actual {decodedComplexTile}.");
}

Console.WriteLine("PASS: V1456 authoritative TileSection encoding");

SignReplicationSnapshot expectedSign = new(
  SignId: 17,
  TileX: 10,
  TileY: 10,
  Text: "TileSection sign",
  PlayerSlot: byte.MaxValue,
  SuppressOpenSign: true,
  Revision: 1);
byte[] signFrameBytes = TerrariaPacketCodec.Encode(
  snapshot,
  Array.Empty<ChestSnapshot>(),
  [expectedSign]);
TerrariaFrame signFrame = TerrariaFrameCodec.Decode(signFrameBytes);
using MemoryStream signCompressedStream = new(signFrame.Payload.ToArray(), writable: false);
using DeflateStream signDecompressor = new(
  signCompressedStream,
  CompressionMode.Decompress,
  leaveOpen: false);
using BinaryReader signReader = new(signDecompressor);
_ = signReader.ReadInt32();
_ = signReader.ReadInt32();
short signWidth = signReader.ReadInt16();
short signHeight = signReader.ReadInt16();
_ = ReadTiles(signReader, signWidth * signHeight);
short signChestCount = signReader.ReadInt16();
short encodedSignCount = signReader.ReadInt16();
short signId = signReader.ReadInt16();
short signTileX = signReader.ReadInt16();
short signTileY = signReader.ReadInt16();
string signText = signReader.ReadString();
short signEntityCount = signReader.ReadInt16();
if (signChestCount != 0 || encodedSignCount != 1 || signId != expectedSign.SignId ||
    signTileX != expectedSign.TileX || signTileY != expectedSign.TileY ||
    signText != expectedSign.Text || signEntityCount != 0)
{
  throw new InvalidOperationException(
    "TileSection sign tail did not preserve the V1456 id, coordinates, text and entity count.");
}

Console.WriteLine("PASS: V1456 TileSection sign tail is source-shaped");

SignTombstoneProjection tombstoneProjection = SignTombstoneProjection.Create(
  new SignTombstoneSnapshot(
    SignId: 17,
    Revision: 3,
    Reason: SignTombstoneReason.Deleted,
    Section: new WorldSectionCoordinates(0, 0)));
if (tombstoneProjection.Status != SignTombstoneProjectionStatus.Deferred ||
    !tombstoneProjection.Detail.Contains("message 47", StringComparison.Ordinal))
{
  throw new InvalidOperationException(
    "V1456 sign tombstone projection did not explicitly report its deferred wire contract.");
}

SignTombstoneProjection invalidTombstoneProjection = SignTombstoneProjection.Create(
  new SignTombstoneSnapshot(
    SignId: short.MaxValue + 1,
    Revision: 3,
    Reason: SignTombstoneReason.Deleted,
    Section: new WorldSectionCoordinates(0, 0)));
if (invalidTombstoneProjection.Status != SignTombstoneProjectionStatus.Rejected)
{
  throw new InvalidOperationException(
    "An out-of-range V1456 sign tombstone was not rejected.");
}

Console.WriteLine("PASS: V1456 sign tombstone projection is explicit and non-silent");

byte[] chestRevisionEnvelope = ContractExtensionCodec.EncodeChestTransferRevision(
  new ChestTransferRevisionEnvelope(12, 7));
ChestTransferRevisionEnvelope decodedChestRevision =
  ContractExtensionCodec.DecodeChestTransferRevision(chestRevisionEnvelope);
if (decodedChestRevision != new ChestTransferRevisionEnvelope(12, 7))
{
  throw new InvalidOperationException(
    "Versioned chest revision envelope did not preserve its typed fields.");
}

byte[] signDeletionFrame = ContractExtensionCodec.EncodeSignDeletion(
  new SignDeletionFrame(17, 3, SignTombstoneReason.Deleted));
SignDeletionFrame decodedSignDeletion =
  ContractExtensionCodec.DecodeSignDeletion(signDeletionFrame);
if (decodedSignDeletion != new SignDeletionFrame(17, 3, SignTombstoneReason.Deleted))
{
  throw new InvalidOperationException("Versioned Sign deletion frame did not round-trip.");
}

byte[] unsupportedVersionFrame = (byte[])signDeletionFrame.Clone();
unsupportedVersionFrame[5] = 2;
try
{
  _ = ContractExtensionCodec.DecodeSignDeletion(unsupportedVersionFrame);
  throw new InvalidOperationException(
    "An unsupported contract extension version was accepted.");
}
catch (InvalidDataException)
{
}

Console.WriteLine(
  "PASS: versioned chest revision envelope and Sign deletion frame reject unknown versions");

byte[] negotiationFrame = ContractExtensionCodec.EncodeVersionNegotiation(
  new ContractExtensionNegotiation(1));
if (ContractExtensionCodec.DecodeVersionNegotiation(negotiationFrame) !=
    new ContractExtensionNegotiation(1))
{
  throw new InvalidOperationException(
    "Contract extension version negotiation did not round-trip.");
}

Console.WriteLine("PASS: contract extension version negotiation is explicit");

TerrariaSession negotiatedSession = CreateActiveSession(1);
byte[] capabilityOffer = ContractExtensionCodec.EncodeCapabilityOffer(
  new ContractCapabilityOffer(1, 1));
TerrariaPacketDispatchResult capabilityResult = new TerrariaPacketDispatcher().Dispatch(
  negotiatedSession,
  capabilityOffer);
if (negotiatedSession.ContractCapabilities.State != ContractNegotiationState.Negotiated ||
    !negotiatedSession.ContractCapabilities.SupportsSignDeletion ||
    !negotiatedSession.ContractCapabilities.SupportsChestTransferRevision ||
    capabilityResult.ResponseFrame is null)
{
  throw new InvalidOperationException("Session capability negotiation did not enable both extensions.");
}

ContractCapabilityAck capabilityAck = ContractExtensionCodec.DecodeCapabilityAck(
  capabilityResult.ResponseFrame);
if (capabilityAck != new ContractCapabilityAck(1, 1))
{
  throw new InvalidOperationException("Capability negotiation did not return the server intersection.");
}

try
{
  _ = new TerrariaPacketDispatcher().Dispatch(negotiatedSession, capabilityOffer);
  throw new InvalidOperationException("Duplicate capability negotiation was accepted.");
}
catch (InvalidDataException)
{
}

Console.WriteLine("PASS: capability negotiation is session-scoped, intersected and single-use");

static TerrariaSession CreateActiveSession(byte assignedPlayerSlot)
{
  TerrariaSession session = new(assignedPlayerSlot);
  _ = session.AcceptHello(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaColor color = new(0, 0, 0);
  _ = session.AcceptPlayerProfile(TerrariaPacketCodec.Encode(new PlayerProfilePacket(
    assignedPlayerSlot,
    0,
    0,
    0.0f,
    0,
    "Protocol",
    0,
    0,
    0,
    color,
    color,
    color,
    color,
    color,
    color,
    color,
    0,
    0,
    0)));
  _ = session.AcceptPlayerUuid(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    "7e8f76a0-c3bc-41f1-bdc1-50b75fe1e8d8")));
  _ = session.AcceptRequestWorldData(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = session.AcceptSpawnTileData(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(2100, 300, 0)));
  session.MarkInitialWorldStreamSent();
  _ = session.AcceptPlayerSpawn(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    assignedPlayerSlot,
    2100,
    300,
    0,
    0,
    0,
    0,
    0)));
  return session;
}

IReadOnlyList<byte[]> initialWorldStream = TerrariaPacketCodec.CreateInitialWorldStream(
  [snapshot]);
if (initialWorldStream.Count != 3 ||
    TerrariaFrameCodec.Decode(initialWorldStream[0]).MessageId != TerrariaMessageId.StatusTextSize ||
    TerrariaFrameCodec.Decode(initialWorldStream[1]).MessageId != TerrariaMessageId.TileSection ||
    TerrariaFrameCodec.Decode(initialWorldStream[2]).MessageId != TerrariaMessageId.InitialSpawn)
{
  throw new InvalidOperationException("V1456 initial world stream did not use supplied section snapshots.");
}

Console.WriteLine("PASS: V1456 snapshot-based initial world stream");

static WorldTile[] ReadTiles(BinaryReader reader, int count)
{
  WorldTile[] tiles = new WorldTile[count];
  int index = 0;
  while (index < tiles.Length)
  {
    byte primaryHeader = reader.ReadByte();
    byte secondaryHeader = (primaryHeader & 1) != 0 ? reader.ReadByte() : (byte)0;
    byte tertiaryHeader = (secondaryHeader & 1) != 0 ? reader.ReadByte() : (byte)0;
    byte quaternaryHeader = (tertiaryHeader & 1) != 0 ? reader.ReadByte() : (byte)0;
    bool isActive = (primaryHeader & 2) != 0;
    bool hasWall = (primaryHeader & 4) != 0;
    int liquidFlags = primaryHeader & 0x18;
    byte liquidType = liquidFlags == 0x10 ? (byte)1 :
      liquidFlags == 0x18 ? (byte)2 :
      (tertiaryHeader & 0x80) != 0 ? (byte)3 : (byte)0;
    ushort type = 0;
    if (isActive)
    {
      type = reader.ReadByte();
      if ((primaryHeader & 0x20) != 0)
      {
        type |= (ushort)(reader.ReadByte() << 8);
      }
    }

    short frameX = 0;
    short frameY = 0;
    if (isActive && type == 320)
    {
      frameX = reader.ReadInt16();
      frameY = reader.ReadInt16();
    }

    byte tileColor = (tertiaryHeader & 8) != 0 ? reader.ReadByte() : (byte)0;
    ushort wallType = hasWall ? reader.ReadByte() : (ushort)0;
    byte wallColor = (tertiaryHeader & 0x10) != 0 ? reader.ReadByte() : (byte)0;
    byte liquidAmount = liquidFlags != 0 ? reader.ReadByte() : (byte)0;
    if (hasWall && (tertiaryHeader & 0x40) != 0)
    {
      wallType |= (ushort)(reader.ReadByte() << 8);
    }

    int repeatCount = 0;
    if ((primaryHeader & 0x80) != 0)
    {
      repeatCount = reader.ReadUInt16();
    }
    else if ((primaryHeader & 0x40) != 0)
    {
      repeatCount = reader.ReadByte();
    }

    byte shape = (byte)(secondaryHeader >> 4);
    bool isHalfBrick = shape == 1;
    byte slope = shape > 1 ? (byte)(shape - 1) : (byte)0;
    WorldTile tile = new(
      IsActive: isActive,
      Type: type,
      LiquidAmount: liquidAmount,
      LiquidType: liquidType,
      FrameX: frameX,
      FrameY: frameY,
      WallType: wallType,
      HasWire: (secondaryHeader & 2) != 0,
      HasWire2: (secondaryHeader & 4) != 0,
      HasWire3: (secondaryHeader & 8) != 0,
      HasWire4: (tertiaryHeader & 0x20) != 0,
      IsHalfBrick: isHalfBrick,
      Slope: slope,
      IsActuated: (tertiaryHeader & 2) != 0,
      IsInactive: (tertiaryHeader & 4) != 0,
      TileColor: tileColor,
      WallColor: wallColor,
      IsInvisibleBlock: (quaternaryHeader & 2) != 0,
      IsInvisibleWall: (quaternaryHeader & 4) != 0,
      IsFullbrightBlock: (quaternaryHeader & 8) != 0,
      IsFullbrightWall: (quaternaryHeader & 0x10) != 0);
    for (int repeatIndex = 0; repeatIndex <= repeatCount; repeatIndex++)
    {
      if (index >= tiles.Length)
      {
        throw new InvalidOperationException("World section encoder overran its declared tile count.");
      }

      tiles[index] = tile;
      index++;
    }
  }

  return tiles;
}
