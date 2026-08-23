using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Dispatch;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Protocol.V1456.Session;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Projectile.Systems;
using Terraria.Dome.Simulation.WorldModel;

byte[] encodedHelloFrame = TerrariaPacketCodec.Encode(new HelloPacket());
byte[] expectedHelloFrame =
[
  0x0F, 0x00, 0x01, 0x0B,
  0x54, 0x65, 0x72, 0x72, 0x61, 0x72, 0x69, 0x61, 0x33, 0x31, 0x39
];

if (!encodedHelloFrame.SequenceEqual(expectedHelloFrame))
{
  throw new InvalidOperationException(
    $"V1456 Hello frame differs from the reference bytes: " +
    $"{Convert.ToHexString(encodedHelloFrame)}.");
}

HelloPacket decodedHelloPacket = TerrariaPacketCodec.DecodeHello(encodedHelloFrame);
if (decodedHelloPacket.ProtocolIdentifier != TerrariaProtocolVersion.HelloIdentifier)
{
  throw new InvalidOperationException("V1456 Hello packet did not round-trip.");
}

try
{
  TerrariaFrameCodec.Decode(expectedHelloFrame.AsSpan(0, 14));
  throw new InvalidOperationException("V1456 decoder accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

Console.WriteLine("PASS: V1456 frame layout and boundary validation");

TerrariaSession session = new(assignedPlayerSlot: 1);
byte[] setUserSlotFrame = session.AcceptHello(encodedHelloFrame);
byte[] expectedSetUserSlotFrame = [0x05, 0x00, 0x03, 0x01, 0x00];
if (!setUserSlotFrame.SequenceEqual(expectedSetUserSlotFrame))
{
  throw new InvalidOperationException(
    $"V1456 SetUserSlot differs from the reference bytes: " +
    $"{Convert.ToHexString(setUserSlotFrame)}.");
}

if (session.State != TerrariaSessionState.UserSlotAssigned)
{
  throw new InvalidOperationException("V1456 session did not enter UserSlotAssigned state.");
}

try
{
  session.AcceptHello(encodedHelloFrame);
  throw new InvalidOperationException("V1456 session accepted a duplicate Hello packet.");
}
catch (InvalidDataException)
{
}

Console.WriteLine("PASS: V1456 handshake session state");

PlayerProfilePacket expectedPlayerProfile = new(
  PlayerSlot: 1,
  SkinVariant: 2,
  VoiceVariant: 3,
  VoicePitchOffset: 0.25f,
  Hair: 4,
  Name: "Dome",
  HairDye: 5,
  AccessoryVisibility: 0x0003,
  HideMisc: 6,
  HairColor: new TerrariaColor(1, 2, 3),
  SkinColor: new TerrariaColor(4, 5, 6),
  EyeColor: new TerrariaColor(7, 8, 9),
  ShirtColor: new TerrariaColor(10, 11, 12),
  UnderShirtColor: new TerrariaColor(13, 14, 15),
  PantsColor: new TerrariaColor(16, 17, 18),
  ShoeColor: new TerrariaColor(19, 20, 21),
  DifficultyFlags: 0x05,
  BiomeTorchFlags: 0x06,
  ConsumableFlags: 0x07);
PlayerUuidPacket testPlayerUuid = new("2eecdeea-c45e-456f-8244-75ec32da6172");
byte[] encodedPlayerProfile = TerrariaPacketCodec.Encode(expectedPlayerProfile);
TerrariaFrame playerProfileFrame = TerrariaFrameCodec.Decode(encodedPlayerProfile);
if (playerProfileFrame.MessageId != TerrariaMessageId.SyncPlayer)
{
  throw new InvalidOperationException("V1456 player profile must use original message ID 4.");
}

PlayerProfilePacket decodedPlayerProfile = TerrariaPacketCodec.DecodePlayerProfile(
  encodedPlayerProfile);
if (decodedPlayerProfile != expectedPlayerProfile)
{
  throw new InvalidOperationException("V1456 player profile did not round-trip.");
}

TerrariaSession profileSession = new(assignedPlayerSlot: 1);
_ = profileSession.AcceptHello(encodedHelloFrame);
PlayerProfilePacket acceptedPlayerProfile = profileSession.AcceptPlayerProfile(encodedPlayerProfile);
if (acceptedPlayerProfile != expectedPlayerProfile ||
    profileSession.State != TerrariaSessionState.PlayerProfileReceived)
{
  throw new InvalidOperationException("V1456 session did not bind a profile to its assigned slot.");
}

PlayerProfilePacket foreignPlayerProfile = expectedPlayerProfile with { PlayerSlot = 2 };
try
{
  profileSession.AcceptPlayerProfile(TerrariaPacketCodec.Encode(foreignPlayerProfile));
  throw new InvalidOperationException("V1456 session accepted a profile for another player slot.");
}
catch (InvalidDataException)
{
}

Console.WriteLine("PASS: V1456 player profile compatibility boundary");

byte[] requestWorldDataFrame = [0x03, 0x00, 0x06];
byte[] syncEquipmentFrame = [0x04, 0x00, 0x05, 0x00];
byte[] playerLifeManaFrame = [0x04, 0x00, 0x10, 0x00];
byte[] itemRotationAndAnimationFrame = [0x04, 0x00, 0x2A, 0x00];
byte[] playerBuffsFrame = [0x04, 0x00, 0x32, 0x00];
byte[] playerUuidFrame = [0x04, 0x00, 0x44, 0x00];
byte[] syncLoadoutFrame = [0x04, 0x00, 0x93, 0x00];
byte[] spawnTileDataFrame =
[
  0x0C, 0x00, 0x08,
  0x34, 0x08, 0x00, 0x00,
  0x2C, 0x01, 0x00, 0x00,
  0x00
];
byte[] playerSpawnFrame =
[
  0x12, 0x00, 0x0C,
  0x01,
  0x34, 0x08,
  0x2C, 0x01,
  0x00, 0x00, 0x00, 0x00,
  0x00, 0x00,
  0x00, 0x00,
  0x00,
  0x00
];

byte[] encodedPlayerControls =
[
  0x19, 0x00, 0x0D,
  0x01, 0x7C, 0x04, 0x00, 0x00, 0x09,
  0x00, 0x00, 0xC0, 0x3F,
  0x00, 0x00, 0x00, 0xC0,
  0x00, 0x00, 0x00, 0x3F,
  0x00, 0x00, 0x80, 0x3E
];
PlayerControlIntent playerControlIntent = TerrariaPacketCodec.DecodePlayerControls(
  encodedPlayerControls);
if (playerControlIntent.PlayerSlot != 1 ||
    !playerControlIntent.MoveLeft ||
    !playerControlIntent.MoveRight ||
    !playerControlIntent.Jump ||
    !playerControlIntent.UseItem ||
    !playerControlIntent.FacingRight ||
    playerControlIntent.SelectedItem != 9)
{
  throw new InvalidOperationException("V1456 player controls did not preserve input intent.");
}

byte[] encodedPlayerControlsWithoutVelocity =
[
  0x11, 0x00, 0x0D,
  0x01, 0x7C, 0x00, 0x00, 0x00, 0x09,
  0x00, 0x00, 0xC0, 0x3F,
  0x00, 0x00, 0x00, 0xC0
];
PlayerControlIntent controlIntentWithoutVelocity = TerrariaPacketCodec.DecodePlayerControls(
  encodedPlayerControlsWithoutVelocity);
if (controlIntentWithoutVelocity != playerControlIntent)
{
  throw new InvalidOperationException(
    "V1456 player controls must accept the original no-velocity packet variant.");
}

TerrariaSession controlSession = new(assignedPlayerSlot: 1);
AdvanceSessionToActive(
  controlSession,
  encodedHelloFrame,
  encodedPlayerProfile,
  requestWorldDataFrame,
  spawnTileDataFrame,
  playerSpawnFrame);
PlayerControlIntent acceptedControlIntent = controlSession.AcceptPlayerControls(
  encodedPlayerControls);
if (acceptedControlIntent != playerControlIntent)
{
  throw new InvalidOperationException("V1456 session did not bind controls to its assigned player.");
}

byte[] foreignPlayerControls = [.. encodedPlayerControls];
foreignPlayerControls[3] = 2;
try
{
  controlSession.AcceptPlayerControls(foreignPlayerControls);
  throw new InvalidOperationException("V1456 session accepted controls for another player slot.");
}
catch (InvalidDataException)
{
}

Console.WriteLine("PASS: V1456 player controls compatibility boundary");

TerrariaSession worldRequestSession = new(assignedPlayerSlot: 1);
_ = worldRequestSession.AcceptHello(encodedHelloFrame);
_ = worldRequestSession.AcceptPlayerProfile(encodedPlayerProfile);
_ = worldRequestSession.AcceptPlayerUuid(TerrariaPacketCodec.Encode(testPlayerUuid));
worldRequestSession.AcceptRequestWorldData(requestWorldDataFrame);
if (worldRequestSession.State != TerrariaSessionState.WorldDataRequested)
{
  throw new InvalidOperationException("V1456 session did not accept RequestWorldData after profile.");
}

try
{
  worldRequestSession.AcceptRequestWorldData([0x04, 0x00, 0x06, 0x00]);
  throw new InvalidOperationException("V1456 session accepted a RequestWorldData payload.");
}
catch (InvalidDataException)
{
}

Console.WriteLine("PASS: V1456 world data request boundary");

byte[][] unsupportedBootstrapFrames =
[
  syncEquipmentFrame,
  playerLifeManaFrame,
  itemRotationAndAnimationFrame,
  playerBuffsFrame,
  playerUuidFrame,
  syncLoadoutFrame
];
for (int index = 0; index < unsupportedBootstrapFrames.Length; index++)
{
  TerrariaSession unsupportedSession = new(assignedPlayerSlot: 1);
  _ = unsupportedSession.AcceptHello(encodedHelloFrame);
  _ = unsupportedSession.AcceptPlayerProfile(encodedPlayerProfile);
  try
  {
    _ = new TerrariaPacketDispatcher().Dispatch(unsupportedSession, unsupportedBootstrapFrames[index]);
    throw new InvalidOperationException("An unbacked V1456 bootstrap packet was accepted.");
  }
  catch (InvalidDataException)
  {
  }
}

Console.WriteLine("PASS: malformed V1456 bootstrap packets are rejected");

WorldDataPacket defaultWorldData = WorldDataPacket.CreateDefault();
byte[] encodedWorldData = TerrariaPacketCodec.Encode(defaultWorldData);
TerrariaFrame worldDataFrame = TerrariaFrameCodec.Decode(encodedWorldData);
if (worldDataFrame.MessageId != TerrariaMessageId.WorldData)
{
  throw new InvalidOperationException("V1456 WorldData must use original message ID 7.");
}

WorldDataPacket decodedWorldData = TerrariaPacketCodec.DecodeWorldData(encodedWorldData);
if (!decodedWorldData.Payload.Span.SequenceEqual(defaultWorldData.Payload.Span))
{
  throw new InvalidOperationException("V1456 WorldData did not round-trip.");
}

Console.WriteLine("PASS: V1456 world data compatibility boundary");

TerrariaSession tileRequestSession = new(assignedPlayerSlot: 1);
_ = tileRequestSession.AcceptHello(encodedHelloFrame);
_ = tileRequestSession.AcceptPlayerProfile(encodedPlayerProfile);
_ = tileRequestSession.AcceptPlayerUuid(TerrariaPacketCodec.Encode(testPlayerUuid));
tileRequestSession.AcceptRequestWorldData(requestWorldDataFrame);
SpawnTileDataRequestPacket tileRequest = tileRequestSession.AcceptSpawnTileData(
  spawnTileDataFrame);
if (tileRequest.SpawnX != 2100 || tileRequest.SpawnY != 300 ||
    tileRequestSession.State != TerrariaSessionState.TileDataRequested)
{
  throw new InvalidOperationException("V1456 session did not accept a valid SpawnTileData request.");
}

IReadOnlyList<byte[]> worldStream = TerrariaPacketCodec.CreateInitialWorldStream();
if (worldStream.Count != 17 ||
    TerrariaFrameCodec.Decode(worldStream[0]).MessageId != TerrariaMessageId.StatusTextSize ||
    TerrariaFrameCodec.Decode(worldStream[1]).MessageId != TerrariaMessageId.TileSection ||
    TerrariaFrameCodec.Decode(worldStream[15]).MessageId != TerrariaMessageId.TileSection ||
    TerrariaFrameCodec.Decode(worldStream[16]).MessageId != TerrariaMessageId.InitialSpawn)
{
  throw new InvalidOperationException("V1456 initial world stream order is incompatible.");
}

TerrariaFrame emptyTileSection = TerrariaFrameCodec.Decode(worldStream[1]);
using (MemoryStream compressedTileSection = new(emptyTileSection.Payload.ToArray(), writable: false))
using (DeflateStream tileDecompressor = new(
  compressedTileSection,
  CompressionMode.Decompress,
  leaveOpen: false))
using (BinaryReader tileReader = new(tileDecompressor))
{
  int tileSectionX = tileReader.ReadInt32();
  int tileSectionY = tileReader.ReadInt32();
  short tileSectionWidth = tileReader.ReadInt16();
  short tileSectionHeight = tileReader.ReadInt16();
  byte tileRunFlags = tileReader.ReadByte();
  short tileRunLength = tileReader.ReadInt16();
  short chestCount = tileReader.ReadInt16();
  short signCount = tileReader.ReadInt16();
  short entityCount = tileReader.ReadInt16();
  if (tileSectionX != 1600 || tileSectionY != 150 ||
      tileSectionWidth != 200 || tileSectionHeight != 150 ||
      tileRunFlags != 0x80 || tileRunLength != 29999 ||
      chestCount != 0 || signCount != 0 || entityCount != 0)
  {
    throw new InvalidOperationException("V1456 empty TileSection differs from original RLE layout.");
  }
}

Console.WriteLine("PASS: V1456 tile load compatibility boundary");

WorldGrid world = new(width: 400, height: 300);
WorldSectionCoordinates firstSection = world.GetSectionCoordinates(10, 10);
WorldSectionCoordinates adjacentSection = world.GetSectionCoordinates(210, 10);
if (firstSection != new WorldSectionCoordinates(0, 0) ||
    adjacentSection != new WorldSectionCoordinates(1, 0))
{
  throw new InvalidOperationException("WorldGrid did not map tiles to Terraria section units.");
}

if (!world.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 1)))
{
  throw new InvalidOperationException("WorldGrid rejected a tile inside the world boundary.");
}

WorldSectionSnapshot firstSnapshot = world.CreateSectionSnapshot(firstSection);
if (firstSnapshot.Width != 200 || firstSnapshot.Height != 150 ||
    firstSnapshot.Version != 1 ||
    firstSnapshot.GetTile(10, 10) != new WorldTile(IsActive: true, Type: 1))
{
  throw new InvalidOperationException("WorldGrid did not expose the authoritative tile section state.");
}

if (!world.TrySetTile(210, 10, new WorldTile(IsActive: true, Type: 2)))
{
  throw new InvalidOperationException("WorldGrid rejected a tile in an adjacent section.");
}

if (world.GetSectionVersion(firstSection) != 1 ||
    world.GetSectionVersion(adjacentSection) != 1)
{
  throw new InvalidOperationException("WorldGrid changed an unrelated section version.");
}

if (!world.TrySetTile(11, 10, new WorldTile(IsActive: true, Type: 3)) ||
    world.GetSectionVersion(firstSection) != 2 ||
    firstSnapshot.GetTile(10, 10) != new WorldTile(IsActive: true, Type: 1) ||
    firstSnapshot.GetTile(11, 10) != default)
{
  throw new InvalidOperationException("WorldGrid section snapshots are not immutable.");
}

if (world.TrySetTile(-1, 0, new WorldTile(IsActive: true, Type: 4)) ||
    world.TrySetTile(400, 0, new WorldTile(IsActive: true, Type: 4)))
{
  throw new InvalidOperationException("WorldGrid accepted an out-of-bounds tile mutation.");
}

Console.WriteLine("PASS: authoritative WorldGrid section and snapshot boundaries");

TerrariaSession spawnSession = new(assignedPlayerSlot: 1);
_ = spawnSession.AcceptHello(encodedHelloFrame);
_ = spawnSession.AcceptPlayerProfile(encodedPlayerProfile);
_ = spawnSession.AcceptPlayerUuid(TerrariaPacketCodec.Encode(testPlayerUuid));
spawnSession.AcceptRequestWorldData(requestWorldDataFrame);
_ = spawnSession.AcceptSpawnTileData(spawnTileDataFrame);
spawnSession.MarkInitialWorldStreamSent();
PlayerSpawnPacket playerSpawn = spawnSession.AcceptPlayerSpawn(playerSpawnFrame);
if (playerSpawn.PlayerSlot != 1 || playerSpawn.SpawnX != 2100 || playerSpawn.SpawnY != 300)
{
  throw new InvalidOperationException("V1456 session did not accept the original PlayerSpawn layout.");
}

byte[] finishedConnectingFrame = TerrariaPacketCodec.EncodeFinishedConnectingToServer();
if (TerrariaFrameCodec.Decode(finishedConnectingFrame).MessageId !=
    TerrariaMessageId.FinishedConnectingToServer)
{
  throw new InvalidOperationException("V1456 finished-connection packet has an incorrect message ID.");
}

Console.WriteLine("PASS: V1456 player spawn compatibility boundary");

TerrariaSession dispatcherSession = new(assignedPlayerSlot: 1);
_ = dispatcherSession.AcceptHello(encodedHelloFrame);
TerrariaPacketDispatcher dispatcher = new();
TerrariaPacketDispatchResult profileDispatchResult = dispatcher.Dispatch(
  dispatcherSession,
  encodedPlayerProfile);
if (profileDispatchResult.Outcome != TerrariaPacketDispatchOutcome.PlayerProfileAccepted ||
    profileDispatchResult.PlayerProfile != expectedPlayerProfile)
{
  throw new InvalidOperationException("V1456 dispatcher did not route SyncPlayer to the profile boundary.");
}

_ = dispatcher.Dispatch(dispatcherSession, TerrariaPacketCodec.Encode(testPlayerUuid));
TerrariaPacketDispatchResult dispatcherWorldRequest = dispatcher.Dispatch(
  dispatcherSession,
  requestWorldDataFrame);
if (dispatcherWorldRequest.Outcome != TerrariaPacketDispatchOutcome.WorldDataRequested)
{
  throw new InvalidOperationException("V1456 dispatcher did not route its session world request.");
}

_ = dispatcherSession.AcceptSpawnTileData(spawnTileDataFrame);
dispatcherSession.MarkInitialWorldStreamSent();
TerrariaPacketDispatchResult initialSpawnDispatchResult = dispatcher.Dispatch(
  dispatcherSession,
  playerSpawnFrame);
if (initialSpawnDispatchResult.Outcome != TerrariaPacketDispatchOutcome.PlayerSpawnAccepted)
{
  throw new InvalidOperationException("V1456 dispatcher did not route the initial PlayerSpawn.");
}

TerrariaPacketDispatchResult repeatedSpawnDispatchResult = dispatcher.Dispatch(
  dispatcherSession,
  playerSpawnFrame);
if (repeatedSpawnDispatchResult.Outcome != TerrariaPacketDispatchOutcome.PlayerSpawnUpdated ||
    dispatcherSession.State != TerrariaSessionState.Active)
{
  throw new InvalidOperationException(
    "V1456 dispatcher did not preserve the active session for a repeated PlayerSpawn.");
}

TerrariaSession crossSlotSpawnSession = new(assignedPlayerSlot: 1);
_ = crossSlotSpawnSession.AcceptHello(encodedHelloFrame);
_ = crossSlotSpawnSession.AcceptPlayerProfile(encodedPlayerProfile);
_ = crossSlotSpawnSession.AcceptPlayerUuid(TerrariaPacketCodec.Encode(testPlayerUuid));
crossSlotSpawnSession.AcceptRequestWorldData(requestWorldDataFrame);
_ = crossSlotSpawnSession.AcceptSpawnTileData(spawnTileDataFrame);
crossSlotSpawnSession.MarkInitialWorldStreamSent();
_ = crossSlotSpawnSession.AcceptPlayerSpawn(playerSpawnFrame);
byte[] crossSlotPlayerSpawnFrame = (byte[])playerSpawnFrame.Clone();
crossSlotPlayerSpawnFrame[3] = 2;
ExpectInvalidData(
  () => crossSlotSpawnSession.AcceptPlayerSpawn(crossSlotPlayerSpawnFrame),
  "An active PlayerSpawn for another player slot must be rejected.");

TerrariaPacketDispatchResult controlsDispatchResult = dispatcher.Dispatch(
  dispatcherSession,
  encodedPlayerControls);
if (controlsDispatchResult.Outcome != TerrariaPacketDispatchOutcome.PlayerControlsAccepted ||
    controlsDispatchResult.PlayerControls != playerControlIntent)
{
  throw new InvalidOperationException("V1456 dispatcher did not route PlayerControls to the intent boundary.");
}

byte[] capturedJourneySpawnRateFrame =
[
  0x0C, 0x00, 0x52,
  0x05, 0x00,
  0x0E, 0x00,
  0x01,
  0x00, 0x00, 0x00, 0x3F
];
TerrariaPacketDispatchResult journeySpawnRateDispatchResult = dispatcher.Dispatch(
  dispatcherSession,
  capturedJourneySpawnRateFrame);
if (journeySpawnRateDispatchResult.Outcome != TerrariaPacketDispatchOutcome.NetModuleAccepted ||
    dispatcherSession.State != TerrariaSessionState.Active)
{
  throw new InvalidOperationException(
    "V1456 dispatcher did not accept the captured active Journey spawn-rate NetModule.");
}

byte[] capturedActivePlayerZoneFrame =
[
  0x0A, 0x00, 0x24,
  0x01, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00
];
byte[] capturedActiveBuffFrame =
[
  0x0A, 0x00, 0x32,
  0x01, 0xBE, 0x00, 0x4C, 0x01, 0x00, 0x00
];
byte[] capturedActiveProjectileFrame =
[
  0x19, 0x00, 0x1B,
  0x00, 0x00, 0x00, 0x3E, 0x03, 0x47, 0x00, 0x08, 0x95, 0x45,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x8A, 0x02, 0x00
];
_ = dispatcher.Dispatch(dispatcherSession, capturedActivePlayerZoneFrame);
_ = dispatcher.Dispatch(dispatcherSession, capturedActiveBuffFrame);
_ = dispatcher.Dispatch(dispatcherSession, capturedActiveProjectileFrame);
if (dispatcherSession.State != TerrariaSessionState.Active)
{
  throw new InvalidOperationException(
    "V1456 dispatcher did not preserve the session after active client synchronization.");
}

byte[] crossSlotZoneFrame = (byte[])capturedActivePlayerZoneFrame.Clone();
crossSlotZoneFrame[3] = 2;
ExpectInvalidData(
  () => dispatcher.Dispatch(dispatcherSession, crossSlotZoneFrame),
  "An active SyncPlayerZone for another player slot must be rejected.");

TerrariaSession crossSlotProjectileSession = new(assignedPlayerSlot: 1);
AdvanceSessionToActive(
  crossSlotProjectileSession,
  encodedHelloFrame,
  encodedPlayerProfile,
  requestWorldDataFrame,
  spawnTileDataFrame,
  playerSpawnFrame);
byte[] crossSlotProjectileFrame = (byte[])capturedActiveProjectileFrame.Clone();
crossSlotProjectileFrame[21] = 2;
ExpectInvalidData(
  () => dispatcher.Dispatch(crossSlotProjectileSession, crossSlotProjectileFrame),
  "An active SyncProjectile for another player slot must be rejected.");

ExpectInvalidData(
  () => dispatcher.Dispatch(
    dispatcherSession,
    [0x0C, 0x00, 0x52, 0x06, 0x00, 0x0E, 0x00, 0x01, 0x00, 0x00, 0x00, 0x3F]),
  "An unapproved NetModules module must be rejected.");
ExpectInvalidData(
  () => dispatcher.Dispatch(
    dispatcherSession,
    [0x0C, 0x00, 0x52, 0x05, 0x00, 0x0D, 0x00, 0x01, 0x00, 0x00, 0x00, 0x3F]),
  "An unapproved Journey power must be rejected.");
ExpectInvalidData(
  () => dispatcher.Dispatch(
    dispatcherSession,
    [0x0C, 0x00, 0x52, 0x05, 0x00, 0x0E, 0x00, 0x02, 0x00, 0x00, 0x00, 0x3F]),
  "A Journey NetModule for another player slot must be rejected.");
ExpectInvalidData(
  () => dispatcher.Dispatch(
    dispatcherSession,
    [0x0C, 0x00, 0x52, 0x05, 0x00, 0x0E, 0x00, 0x01, 0x00, 0x00, 0xC0, 0x7F]),
  "A Journey NetModule with a non-finite slider value must be rejected.");
ExpectInvalidData(
  () => dispatcher.Dispatch(
    dispatcherSession,
    [0x0B, 0x00, 0x52, 0x05, 0x00, 0x0E, 0x00, 0x01, 0x00, 0x00, 0x3F]),
  "A truncated Journey NetModule must be rejected.");

TerrariaSession dispatcherWorldSession = new(assignedPlayerSlot: 1);
_ = dispatcherWorldSession.AcceptHello(encodedHelloFrame);
_ = dispatcher.Dispatch(dispatcherWorldSession, encodedPlayerProfile);
_ = dispatcher.Dispatch(dispatcherWorldSession, TerrariaPacketCodec.Encode(testPlayerUuid));
TerrariaPacketDispatchResult worldRequestDispatchResult = dispatcher.Dispatch(
  dispatcherWorldSession,
  requestWorldDataFrame);
if (worldRequestDispatchResult.Outcome != TerrariaPacketDispatchOutcome.WorldDataRequested)
{
  throw new InvalidOperationException("V1456 dispatcher did not route RequestWorldData.");
}

if (dispatcherWorldSession.State != TerrariaSessionState.WorldDataRequested)
{
  throw new InvalidOperationException("V1456 dispatcher did not advance the world request session.");
}

try
{
  dispatcher.Dispatch(dispatcherSession, [0x03, 0x00, 0x0F]);
  throw new InvalidOperationException("V1456 dispatcher accepted an unsupported inbound packet.");
}
catch (InvalidDataException)
{
}

Console.WriteLine("PASS: V1456 packet dispatcher boundaries");

ReadOnlySpan<TerrariaMessageDescriptor> messageCatalog = TerrariaMessageCatalog.All;
if (messageCatalog.Length != 162)
{
  throw new InvalidOperationException(
    $"V1456 message catalog must cover IDs 0 through 161, not {messageCatalog.Length} entries.");
}

for (int messageIndex = 1; messageIndex <= 161; messageIndex++)
{
  TerrariaMessageDescriptor descriptor = TerrariaMessageCatalog.Get(
    (TerrariaMessageId)messageIndex);
  if ((byte)descriptor.MessageId != messageIndex || string.IsNullOrWhiteSpace(descriptor.Name))
  {
    throw new InvalidOperationException($"V1456 catalog entry {messageIndex} is incomplete.");
  }
}

TerrariaMessageDescriptor setUserSlotDescriptor = TerrariaMessageCatalog.Get(
  TerrariaMessageId.SetUserSlot);
if (setUserSlotDescriptor.Name != "PlayerInfo" ||
    setUserSlotDescriptor.Direction != TerrariaPacketDirection.ServerToClient ||
    setUserSlotDescriptor.Support != TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException("V1456 SetUserSlot catalog contract drifted from the reference protocol.");
}

TerrariaMessageDescriptor syncPlayerDescriptor = TerrariaMessageCatalog.Get(
  TerrariaMessageId.SyncPlayer);
if (syncPlayerDescriptor.Name != "SyncPlayer" ||
    syncPlayerDescriptor.Direction != TerrariaPacketDirection.Bidirectional ||
    syncPlayerDescriptor.Support != TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException("V1456 SyncPlayer catalog contract drifted from the reference protocol.");
}

Console.WriteLine("PASS: V1456 complete message catalog");

DomeSimulation simulation = new();
PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
NpcHandle npc = simulation.CreateNpc(new SimulationVector(30.0f, 0.0f));
SimulationSnapshot before = simulation.CreateSnapshot();

simulation.Tick(new SimulationInputBatch(
  new PlayerInput(player, MoveLeft: false, MoveRight: true, Jump: false, Fire: false)));

SimulationSnapshot after = simulation.CreateSnapshot();
PlayerSnapshot beforePlayer = before.FindPlayer(player);
PlayerSnapshot afterPlayer = after.FindPlayer(player);

if (afterPlayer.Position.X <= beforePlayer.Position.X)
{
  throw new InvalidOperationException("Right input must move the server-owned player right.");
}

Console.WriteLine("PASS: player movement");

DomeSimulation jumpSimulation = new();
PlayerHandle jumpingPlayer = jumpSimulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
jumpSimulation.Tick(new SimulationInputBatch(
  new PlayerInput(jumpingPlayer, MoveLeft: false, MoveRight: false, Jump: true, Fire: false)));
PlayerSnapshot airbornePlayer = jumpSimulation.CreateSnapshot().FindPlayer(jumpingPlayer);

if (airbornePlayer.Position.Y <= 0.0f || airbornePlayer.IsGrounded)
{
  throw new InvalidOperationException("Jump input must move a grounded player into the air.");
}

for (int index = 0; index < 8; index++)
{
  jumpSimulation.Tick(new SimulationInputBatch());
}

PlayerSnapshot landedPlayer = jumpSimulation.CreateSnapshot().FindPlayer(jumpingPlayer);
if (landedPlayer.Position.Y != 0.0f || !landedPlayer.IsGrounded)
{
  throw new InvalidOperationException("Gravity must return a jumping player to the ground.");
}

Console.WriteLine("PASS: player jump and ground collision");

SimulationSnapshot beforeNpcTick = simulation.CreateSnapshot();
simulation.Tick(new SimulationInputBatch());
SimulationSnapshot afterNpcTick = simulation.CreateSnapshot();
NpcSnapshot beforeNpc = beforeNpcTick.FindNpc(npc);
NpcSnapshot afterNpc = afterNpcTick.FindNpc(npc);

if (afterNpc.Position.X >= beforeNpc.Position.X || !afterNpc.HasTarget)
{
  throw new InvalidOperationException("Npc must select and chase the active player.");
}

Console.WriteLine("PASS: npc targeting and chase");

DomeSimulation combatSimulation = new();
PlayerHandle combatPlayer = combatSimulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
NpcHandle combatNpc = combatSimulation.CreateNpc(new SimulationVector(16.0f, 0.0f));

combatSimulation.Tick(new SimulationInputBatch(
  new PlayerInput(combatPlayer, MoveLeft: false, MoveRight: false, Jump: false, Fire: true)));
SimulationSnapshot afterFire = combatSimulation.CreateSnapshot();

if (afterFire.Projectiles.Count != 1)
{
  throw new InvalidOperationException("Fire input must create one projectile.");
}

combatSimulation.Tick(new SimulationInputBatch());

SimulationSnapshot afterHit = combatSimulation.CreateSnapshot();
NpcSnapshot hitNpc = afterHit.FindNpc(combatNpc);
if (hitNpc.Health != 90 || afterHit.Projectiles.Count != 0)
{
  throw new InvalidOperationException(
    "Projectile hit must resolve damage and despawn through the lifecycle commit. " +
    $"NpcX={hitNpc.Position.X}, Health={hitNpc.Health}, " +
    $"Projectiles={afterHit.Projectiles.Count}.");
}

Console.WriteLine("PASS: projectile damage and lifecycle");

WorldGrid projectileCollisionWorld = new(200, 150);
_ = projectileCollisionWorld.TrySetTile(2, 1, new WorldTile(true, 1));
ProjectileCollisionSystem projectileCollision = new();
if (!projectileCollision.HitsSolidTile(
      projectileCollisionWorld,
      new TransformComponent(1.0f, 1.0f),
      new ColliderComponent(1.0f, 1.0f)) ||
    projectileCollision.HitsSolidTile(
      projectileCollisionWorld,
      new TransformComponent(float.NaN, 1.0f),
      new ColliderComponent(1.0f, 1.0f)) ||
    projectileCollision.PathHitsSolidTile(
      projectileCollisionWorld,
      new TransformComponent(1.0f, 1.0f),
      new TransformComponent(2.0f, float.PositiveInfinity),
      new ColliderComponent(1.0f, 1.0f)) ||
    projectileCollision.HitsSolidTile(
      projectileCollisionWorld,
      new TransformComponent(1.0f, 1.0f),
      new ColliderComponent(0.0f, 1.0f)))
{
  throw new InvalidOperationException(
    "Projectile collision did not fail closed for invalid geometry.");
}

Console.WriteLine("PASS: projectile collision geometry boundary");

using DomeServer server = new();
server.Start();

using (TcpClient terrariaProtocolClient = new())
{
  await terrariaProtocolClient.ConnectAsync("127.0.0.1", server.Port);
  using NetworkStream terrariaProtocolStream = terrariaProtocolClient.GetStream();
  await terrariaProtocolStream.WriteAsync(encodedHelloFrame);
  using CancellationTokenSource protocolReadCancellation = new(TimeSpan.FromSeconds(3));
  byte[] setUserSlotPrefix = new byte[2];
  await terrariaProtocolStream.ReadExactlyAsync(
    setUserSlotPrefix,
    protocolReadCancellation.Token);
  int setUserSlotLength = setUserSlotPrefix[0] | setUserSlotPrefix[1] << 8;
  byte[] setUserSlotBody = new byte[setUserSlotLength - 2];
  await terrariaProtocolStream.ReadExactlyAsync(
    setUserSlotBody,
    protocolReadCancellation.Token);
  byte[] rawSetUserSlotFrame = [.. setUserSlotPrefix, .. setUserSlotBody];
  if (!rawSetUserSlotFrame.SequenceEqual(expectedSetUserSlotFrame))
  {
    throw new InvalidOperationException(
      $"DomeServer V1456 Hello response differs from reference: " +
      $"{Convert.ToHexString(rawSetUserSlotFrame)}.");
  }

  byte[] rawInitialNetModulesFrame = await ReadFrameAsync(terrariaProtocolStream);
  byte[] expectedInitialNetModulesFrame = [0x07, 0x00, 0x52, 0x00, 0x00, 0x00, 0x00];
  if (!rawInitialNetModulesFrame.SequenceEqual(expectedInitialNetModulesFrame))
  {
    throw new InvalidOperationException(
      "DomeServer did not emit the initial NetModules state after SetUserSlot.");
  }

  await terrariaProtocolStream.WriteAsync(encodedPlayerProfile);
  await terrariaProtocolStream.WriteAsync(TerrariaPacketCodec.Encode(testPlayerUuid));
  await terrariaProtocolStream.WriteAsync(requestWorldDataFrame);
  byte[] rawWorldDataFrame = await ReadFrameAsync(terrariaProtocolStream);
  if (TerrariaFrameCodec.Decode(rawWorldDataFrame).MessageId != TerrariaMessageId.WorldData)
  {
    throw new InvalidOperationException("DomeServer did not respond to RequestWorldData with WorldData.");
  }

  await terrariaProtocolStream.WriteAsync(spawnTileDataFrame);
  TerrariaFrame initialWorldFrame = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(terrariaProtocolStream));
  if (initialWorldFrame.MessageId == TerrariaMessageId.WorldData)
  {
    initialWorldFrame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(terrariaProtocolStream));
  }

  if (initialWorldFrame.MessageId != TerrariaMessageId.StatusTextSize)
  {
    throw new InvalidOperationException("DomeServer did not begin with original StatusTextSize.");
  }

  int tileSectionCount = 0;
  bool receivedInitialSpawn = false;
  for (int index = 0; index < 1_100; index++)
  {
    TerrariaMessageId messageId = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(terrariaProtocolStream)).MessageId;
    if (messageId == TerrariaMessageId.TileSection)
    {
      tileSectionCount++;
      continue;
    }

    if (messageId == TerrariaMessageId.SyncChestSize ||
        messageId == TerrariaMessageId.SyncChestItem ||
        messageId == TerrariaMessageId.SyncNPC ||
        messageId == TerrariaMessageId.NpcBuffs ||
        messageId == TerrariaMessageId.NetModules ||
        messageId == TerrariaMessageId.WorldBiomeTypes ||
        messageId == TerrariaMessageId.TowerShieldStrengths ||
        messageId == TerrariaMessageId.CavernMonsterTypes ||
        messageId == TerrariaMessageId.AnglerQuest)
    {
      continue;
    }

    if (messageId == TerrariaMessageId.InitialSpawn && tileSectionCount == 15)
    {
      receivedInitialSpawn = true;
      break;
    }

    throw new InvalidOperationException("DomeServer returned an unexpected initial world frame.");
  }

  if (!receivedInitialSpawn)
  {
    throw new InvalidOperationException("DomeServer did not end initial world data with InitialSpawn.");
  }

  await terrariaProtocolStream.WriteAsync(playerSpawnFrame);
  bool receivedPlayerActive = false;
  bool receivedPlayerProfile = false;
  bool receivedPlayerControls = false;
  bool receivedFinishedConnecting = false;
  for (int index = 0; index < 1_100; index++)
  {
    TerrariaMessageId messageId = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(terrariaProtocolStream)).MessageId;
    receivedPlayerActive |= messageId == TerrariaMessageId.PlayerActive;
    receivedPlayerProfile |= messageId == TerrariaMessageId.SyncPlayer;
    receivedPlayerControls |= messageId == TerrariaMessageId.PlayerControls;
    if (messageId == TerrariaMessageId.FinishedConnectingToServer)
    {
      receivedFinishedConnecting = true;
      break;
    }
  }

  if (!receivedFinishedConnecting)
  {
    throw new InvalidOperationException(
      "DomeServer did not complete the authoritative PlayerSpawn projection flow.");
  }

  await terrariaProtocolStream.WriteAsync(playerSpawnFrame);
  await terrariaProtocolStream.WriteAsync(capturedActivePlayerZoneFrame);
  await terrariaProtocolStream.WriteAsync(capturedActiveBuffFrame);
  await terrariaProtocolStream.WriteAsync(capturedActiveProjectileFrame);

  byte[] serverPlayerControls =
  new byte[]
  {
    0x11, 0x00, 0x0D,
    0x01, 0x68, 0x00, 0x00, 0x00, 0x09,
    0x00, 0x00, 0xC0, 0x3F,
    0x00, 0x00, 0x00, 0xC0
  };
  await terrariaProtocolStream.WriteAsync(serverPlayerControls);
  SimulationSnapshot controlledServerSnapshot = await WaitForPlayerMovementAsync(server);
  if (controlledServerSnapshot.Players.Count != 1 ||
      controlledServerSnapshot.Players[0].Position.X <= 10.0f)
  {
    throw new InvalidOperationException(
      "V1456 PlayerControls did not reach the server-owned simulation tick.");
  }
}

Console.WriteLine("PASS: V1456 loopback server handshake");

using (TcpClient incompatibleProtocolClient = new())
{
  await incompatibleProtocolClient.ConnectAsync("127.0.0.1", server.Port);
  using NetworkStream incompatibleProtocolStream = incompatibleProtocolClient.GetStream();
  byte[] incompatibleHelloFrame = TerrariaPacketCodec.Encode(new HelloPacket("Terraria000"));
  await incompatibleProtocolStream.WriteAsync(incompatibleHelloFrame);
  using CancellationTokenSource incompatibleReadCancellation = new(TimeSpan.FromSeconds(3));
  byte[] firstResponseByte = new byte[1];
  int receivedByteCount = await incompatibleProtocolStream.ReadAsync(
    firstResponseByte,
    incompatibleReadCancellation.Token);
  if (receivedByteCount != 0)
  {
    throw new InvalidOperationException("V1456 server responded to an incompatible Hello.");
  }
}

using (TcpClient laterProtocolClient = new())
{
  await laterProtocolClient.ConnectAsync("127.0.0.1", server.Port);
  using NetworkStream laterProtocolStream = laterProtocolClient.GetStream();
  await laterProtocolStream.WriteAsync(encodedHelloFrame);
  using CancellationTokenSource laterReadCancellation = new(TimeSpan.FromSeconds(3));
  byte[] laterSetUserSlotPrefix = new byte[2];
  await laterProtocolStream.ReadExactlyAsync(
    laterSetUserSlotPrefix,
    laterReadCancellation.Token);
  int laterSetUserSlotLength = laterSetUserSlotPrefix[0] | laterSetUserSlotPrefix[1] << 8;
  byte[] laterSetUserSlotBody = new byte[laterSetUserSlotLength - 2];
  await laterProtocolStream.ReadExactlyAsync(
    laterSetUserSlotBody,
    laterReadCancellation.Token);
  byte[] laterSetUserSlotFrame = [.. laterSetUserSlotPrefix, .. laterSetUserSlotBody];
  TerrariaFrame laterSetUserSlot = TerrariaFrameCodec.Decode(laterSetUserSlotFrame);
  if (laterSetUserSlot.MessageId != TerrariaMessageId.SetUserSlot ||
      laterSetUserSlot.Payload.Length != 2 ||
      laterSetUserSlot.Payload.Span[0] == 0)
  {
    throw new InvalidOperationException(
      "V1456 server did not accept a valid client after rejecting an incompatible Hello.");
  }
}

Console.WriteLine("PASS: V1456 invalid session isolation");

using DomeServer concurrentServer = new();
concurrentServer.Start();
using TcpClient persistentProtocolClient = new();
await persistentProtocolClient.ConnectAsync("127.0.0.1", concurrentServer.Port);
using NetworkStream persistentProtocolStream = persistentProtocolClient.GetStream();
await persistentProtocolStream.WriteAsync(encodedHelloFrame);
byte[] persistentSetUserSlotFrame = await ReadFrameAsync(persistentProtocolStream);
if (!persistentSetUserSlotFrame.SequenceEqual(expectedSetUserSlotFrame))
{
  throw new InvalidOperationException("V1456 server did not complete the persistent session handshake.");
}

using TcpClient concurrentProtocolClient = new();
await concurrentProtocolClient.ConnectAsync("127.0.0.1", concurrentServer.Port);
using NetworkStream concurrentProtocolStream = concurrentProtocolClient.GetStream();
await concurrentProtocolStream.WriteAsync(encodedHelloFrame);
byte[] concurrentSetUserSlotFrame = await ReadFrameAsync(concurrentProtocolStream);
byte[] expectedConcurrentSetUserSlotFrame = [0x05, 0x00, 0x03, 0x02, 0x00];
if (!concurrentSetUserSlotFrame.SequenceEqual(expectedConcurrentSetUserSlotFrame))
{
  throw new InvalidOperationException(
    "V1456 server did not accept a second client while the first session remained open.");
}

Console.WriteLine("PASS: V1456 concurrent session acceptance");

byte[] capturedUuidFrame = TerrariaFrameCodec.Encode(new TerrariaFrame(
  TerrariaMessageId.PlayerUuid,
  new byte[]
  {
    0x24,
    0x32, 0x65, 0x65, 0x63, 0x64, 0x65, 0x65, 0x61, 0x2D,
    0x63, 0x34, 0x35, 0x65, 0x2D, 0x34, 0x35, 0x36, 0x66, 0x2D,
    0x38, 0x32, 0x34, 0x34, 0x2D, 0x37, 0x35, 0x65, 0x63,
    0x33, 0x32, 0x64, 0x61, 0x36, 0x31, 0x37, 0x32
  }));
PlayerUuidPacket capturedUuid = TerrariaPacketCodec.DecodePlayerUuid(capturedUuidFrame);
if (capturedUuid.Value != "2eecdeea-c45e-456f-8244-75ec32da6172")
{
  throw new InvalidOperationException("The captured PlayerUuid packet was not decoded.");
}

if (!TerrariaPacketCodec.Encode(capturedUuid).SequenceEqual(capturedUuidFrame))
{
  throw new InvalidOperationException("The captured PlayerUuid packet did not round-trip.");
}

byte[] capturedLifeFrame = TerrariaFrameCodec.Encode(new TerrariaFrame(
  TerrariaMessageId.PlayerLifeMana,
  new byte[] { 0x01, 0xFE, 0x00, 0xF4, 0x01 }));
PlayerVitalsPacket capturedLife = TerrariaPacketCodec.DecodePlayerLifeMana(capturedLifeFrame);
if (capturedLife.PlayerSlot != 1 || capturedLife.Current != 254 || capturedLife.Maximum != 500)
{
  throw new InvalidOperationException("The captured PlayerLifeMana packet was not decoded.");
}

byte[] capturedManaFrame = TerrariaFrameCodec.Encode(new TerrariaFrame(
  TerrariaMessageId.ItemRotationAndAnimation,
  new byte[] { 0x01, 0x04, 0x01, 0xC8, 0x00 }));
PlayerVitalsPacket capturedMana = TerrariaPacketCodec.DecodePlayerMana(capturedManaFrame);
if (capturedMana.PlayerSlot != 1 || capturedMana.Current != 260 || capturedMana.Maximum != 200)
{
  throw new InvalidOperationException("The captured player mana packet was not decoded.");
}

byte[] capturedBuffFrame = TerrariaFrameCodec.Encode(new TerrariaFrame(
  TerrariaMessageId.PlayerBuffs,
  new byte[] { 0x01, 0xBE, 0x00, 0x00, 0x00 }));
PlayerBuffsPacket capturedBuffs = TerrariaPacketCodec.DecodePlayerBuffs(capturedBuffFrame);
if (capturedBuffs.PlayerSlot != 1 || capturedBuffs.BuffTypes.Count != 1 ||
    capturedBuffs.BuffTypes[0] != 190)
{
  throw new InvalidOperationException("The captured PlayerBuffs packet was not decoded.");
}

byte[] capturedLoadoutFrame = TerrariaFrameCodec.Encode(new TerrariaFrame(
  TerrariaMessageId.SyncLoadout,
  new byte[] { 0x01, 0x00, 0xF8, 0x03 }));
PlayerLoadoutPacket capturedLoadout = TerrariaPacketCodec.DecodePlayerLoadout(capturedLoadoutFrame);
if (capturedLoadout.PlayerSlot != 1 || capturedLoadout.SelectedLoadout != 0 ||
    capturedLoadout.AccessoryVisibility != 0x03F8)
{
  throw new InvalidOperationException("The captured SyncLoadout packet was not decoded.");
}

byte[] capturedEquipmentFrame = TerrariaFrameCodec.Encode(new TerrariaFrame(
  TerrariaMessageId.SyncEquipment,
  new byte[] { 0x01, 0x00, 0x00, 0x01, 0x00, 0xD7, 0x14, 0x01, 0x01 }));
PlayerEquipmentPacket capturedEquipment = TerrariaPacketCodec.DecodePlayerEquipment(
  capturedEquipmentFrame);
if (capturedEquipment.PlayerSlot != 1 || capturedEquipment.SlotId != 0 ||
    capturedEquipment.Stack != 1 || capturedEquipment.Prefix != 215 ||
    capturedEquipment.ItemType != 276 || !capturedEquipment.IsFavorited)
{
  throw new InvalidOperationException("The captured SyncEquipment packet was not decoded.");
}

if (!TerrariaPacketCodec.EncodePlayerLifeMana(
      capturedLife.PlayerSlot,
      capturedLife.Current,
      capturedLife.Maximum).SequenceEqual(capturedLifeFrame) ||
    !TerrariaPacketCodec.EncodePlayerMana(
      capturedMana.PlayerSlot,
      capturedMana.Current,
      capturedMana.Maximum).SequenceEqual(capturedManaFrame) ||
    !TerrariaPacketCodec.Encode(capturedBuffs).SequenceEqual(capturedBuffFrame) ||
    !TerrariaPacketCodec.Encode(capturedLoadout).SequenceEqual(capturedLoadoutFrame) ||
    !TerrariaPacketCodec.Encode(capturedEquipment).SequenceEqual(capturedEquipmentFrame))
{
  throw new InvalidOperationException("The captured player bootstrap packets did not round-trip.");
}

Console.WriteLine("PASS: captured original player bootstrap packet decoding");

byte[] outOfRangeEquipmentFrame = TerrariaFrameCodec.Encode(new TerrariaFrame(
  TerrariaMessageId.SyncEquipment,
  new byte[] { 0x01, 0xDE, 0x03, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00 }));
ExpectInvalidData(
  () => TerrariaPacketCodec.DecodePlayerEquipment(outOfRangeEquipmentFrame),
  "SyncEquipment slot 990 must be rejected.");

byte[] negativeStackEquipmentFrame = TerrariaFrameCodec.Encode(new TerrariaFrame(
  TerrariaMessageId.SyncEquipment,
  new byte[] { 0x01, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x01, 0x00, 0x00 }));
ExpectInvalidData(
  () => TerrariaPacketCodec.DecodePlayerEquipment(negativeStackEquipmentFrame),
  "SyncEquipment with a negative stack must be rejected.");

byte[] malformedUuidFrame = TerrariaFrameCodec.Encode(new TerrariaFrame(
  TerrariaMessageId.PlayerUuid,
  new byte[] { 0x01, 0x20 }));
ExpectInvalidData(
  () => TerrariaPacketCodec.DecodePlayerUuid(malformedUuidFrame),
  "A whitespace PlayerUuid must be rejected.");

byte[] unterminatedBuffFrame = TerrariaFrameCodec.Encode(new TerrariaFrame(
  TerrariaMessageId.PlayerBuffs,
  new byte[] { 0x01, 0xBE, 0x00 }));
ExpectInvalidData(
  () => TerrariaPacketCodec.DecodePlayerBuffs(unterminatedBuffFrame),
  "An unterminated PlayerBuffs packet must be rejected.");

byte[] tooManyBuffsPayload = new byte[93];
tooManyBuffsPayload[0] = 1;
for (int index = 0; index < 45; index++)
{
  int offset = 1 + index * 2;
  tooManyBuffsPayload[offset] = 1;
}

byte[] tooManyBuffsFrame = TerrariaFrameCodec.Encode(new TerrariaFrame(
  TerrariaMessageId.PlayerBuffs,
  tooManyBuffsPayload));
ExpectInvalidData(
  () => TerrariaPacketCodec.DecodePlayerBuffs(tooManyBuffsFrame),
  "A PlayerBuffs packet with more than 44 buffs must be rejected.");

Console.WriteLine("PASS: player bootstrap packet validation boundaries");

TerrariaSession bootstrapSession = new(assignedPlayerSlot: 1);
TerrariaPacketDispatcher bootstrapDispatcher = new();
_ = bootstrapSession.AcceptHello(encodedHelloFrame);
_ = bootstrapDispatcher.Dispatch(bootstrapSession, encodedPlayerProfile);
_ = bootstrapDispatcher.Dispatch(bootstrapSession, capturedUuidFrame);
_ = bootstrapDispatcher.Dispatch(bootstrapSession, capturedLifeFrame);
_ = bootstrapDispatcher.Dispatch(bootstrapSession, capturedManaFrame);
_ = bootstrapDispatcher.Dispatch(bootstrapSession, capturedBuffFrame);
_ = bootstrapDispatcher.Dispatch(bootstrapSession, capturedLoadoutFrame);
_ = bootstrapDispatcher.Dispatch(bootstrapSession, capturedEquipmentFrame);
TerrariaPacketDispatchResult bootstrapWorldRequest = bootstrapDispatcher.Dispatch(
  bootstrapSession,
  requestWorldDataFrame);
if (bootstrapWorldRequest.Outcome != TerrariaPacketDispatchOutcome.WorldDataRequested ||
    bootstrapWorldRequest.PlayerBootstrap is not PlayerBootstrapState bootstrap ||
    bootstrap.Uuid != capturedUuid.Value || bootstrap.Profile != expectedPlayerProfile ||
    bootstrap.Life != capturedLife || bootstrap.Mana != capturedMana ||
    bootstrap.BuffTypes.Count != 1 || bootstrap.BuffTypes[0] != 190 ||
    bootstrap.Loadout != capturedLoadout || bootstrap.Equipment.Count != 990 ||
    bootstrap.Equipment[0] != capturedEquipment ||
    bootstrapSession.State != TerrariaSessionState.WorldDataRequested)
{
  throw new InvalidOperationException(
    "The bootstrap session did not freeze the captured player state at RequestWorldData.");
}

_ = bootstrapSession.AcceptSpawnTileData(spawnTileDataFrame);
bootstrapSession.MarkInitialWorldStreamSent();
_ = bootstrapSession.AcceptPlayerSpawn(playerSpawnFrame);
byte[] activeEquipmentFrame = TerrariaPacketCodec.Encode(new PlayerEquipmentPacket(
  PlayerSlot: capturedEquipment.PlayerSlot,
  SlotId: capturedEquipment.SlotId,
  Stack: 0,
  Prefix: 0,
  ItemType: 0,
  IsFavorited: false,
  IsNewAndShiny: false));
TerrariaPacketDispatchResult activeEquipment = bootstrapDispatcher.Dispatch(
  bootstrapSession,
  activeEquipmentFrame);
if (activeEquipment.Outcome != TerrariaPacketDispatchOutcome.ActiveSynchronizationAccepted ||
    bootstrapSession.State != TerrariaSessionState.Active ||
    bootstrap.Equipment[0] != capturedEquipment)
{
  throw new InvalidOperationException(
    "Active SyncEquipment must preserve frozen bootstrap equipment and the active session.");
}

TerrariaPacketDispatchResult activeLife = bootstrapDispatcher.Dispatch(
  bootstrapSession,
  TerrariaPacketCodec.EncodePlayerLifeMana(capturedLife.PlayerSlot, 1, 20));
TerrariaPacketDispatchResult activeMana = bootstrapDispatcher.Dispatch(
  bootstrapSession,
  TerrariaPacketCodec.EncodePlayerMana(capturedMana.PlayerSlot, 1, 20));
if (activeLife.Outcome != TerrariaPacketDispatchOutcome.ActiveSynchronizationAccepted ||
    activeMana.Outcome != TerrariaPacketDispatchOutcome.ActiveSynchronizationAccepted ||
    bootstrapSession.State != TerrariaSessionState.Active || bootstrap.Life != capturedLife ||
    bootstrap.Mana != capturedMana)
{
  throw new InvalidOperationException(
    "Active player vitals must preserve frozen bootstrap state and the active session.");
}

byte[] forgedActiveEquipmentFrame = TerrariaPacketCodec.Encode(new PlayerEquipmentPacket(
  PlayerSlot: 2,
  SlotId: capturedEquipment.SlotId,
  Stack: capturedEquipment.Stack,
  Prefix: capturedEquipment.Prefix,
  ItemType: capturedEquipment.ItemType,
  IsFavorited: capturedEquipment.IsFavorited,
  IsNewAndShiny: capturedEquipment.IsNewAndShiny));
ExpectInvalidData(
  () => bootstrapDispatcher.Dispatch(bootstrapSession, forgedActiveEquipmentFrame),
  "Active SyncEquipment for a different player must be rejected.");

Console.WriteLine("PASS: V1456 bootstrap state freeze and active equipment compatibility");

static async Task<byte[]> ReadFrameAsync(NetworkStream stream)
{
  using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(3));
  byte[] prefix = new byte[2];
  await stream.ReadExactlyAsync(prefix, cancellation.Token);
  int length = prefix[0] | prefix[1] << 8;
  byte[] body = new byte[length - 2];
  await stream.ReadExactlyAsync(body, cancellation.Token);
  return [.. prefix, .. body];
}

static void ExpectInvalidData(Action action, string failureMessage)
{
  try
  {
    action();
  }
  catch (InvalidDataException)
  {
    return;
  }

  throw new InvalidOperationException(failureMessage);
}

static void AdvanceSessionToActive(
  TerrariaSession session,
  byte[] helloFrame,
  byte[] playerProfileFrame,
  byte[] worldRequestFrame,
  byte[] tileRequestFrame,
  byte[] playerSpawnFrame)
{
  _ = session.AcceptHello(helloFrame);
  _ = session.AcceptPlayerProfile(playerProfileFrame);
  _ = session.AcceptPlayerUuid(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    "2eecdeea-c45e-456f-8244-75ec32da6172")));
  _ = session.AcceptRequestWorldData(worldRequestFrame);
  _ = session.AcceptSpawnTileData(tileRequestFrame);
  session.MarkInitialWorldStreamSent();
  _ = session.AcceptPlayerSpawn(playerSpawnFrame);
}

static async Task<SimulationSnapshot> WaitForPlayerMovementAsync(DomeServer server)
{
  using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(3));
  while (!cancellation.IsCancellationRequested)
  {
    SimulationSnapshot snapshot = server.LatestSnapshot;
    if (snapshot.Players.Count == 1 && snapshot.Players[0].Position.X > 10.0f)
    {
      return snapshot;
    }

    await Task.Delay(TimeSpan.FromMilliseconds(10), cancellation.Token);
  }

  throw new InvalidOperationException("V1456 server did not publish a movement snapshot.");
}
