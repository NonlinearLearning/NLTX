using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Dome.Server;
using Terraria.Dome.Server.Persistence;
using Terraria.Dome.Server.Replication;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Npc.Snapshots;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.WorldGeneration;
using Terraria.Dome.Simulation.WorldGeneration.Systems;
using Terraria.WorldCompatibility.Model;
using Terraria.WorldCompatibility.Projection;
using Terraria.WorldFile.V319.Format;
using Terraria.WorldFile.V319.Model;

string temporaryDirectory = Path.Combine(
  Path.GetTempPath(),
  $"Terraria.Dome.Persistence.Verification.{Guid.NewGuid():N}");
Directory.CreateDirectory(temporaryDirectory);

try
{
  VerifyProjectileIdentityCursorPersistence();
  VerifyNpcGivenNameSidecarPersistence();
  VerifyNpcGivenNameSaveCoordinator(temporaryDirectory);
  VerifyRoundTrip(temporaryDirectory);
  VerifyLegacyWorldPersistenceFormatCompatibility();
  VerifyVersionTwoWorldPersistenceFormatCompatibility();
  VerifyRemixWorldPersistenceRoundTrip();
  VerifyNoTrapsWorldPersistenceRoundTrip();
  VerifySkyblockWorldPersistenceRoundTrip();
  VerifyGoodWorldPersistenceRoundTrip();
  VerifyVersionThreeWorldSurfaceCompatibility();
  VerifyWorldSurfacePersistenceRoundTrip();
  VerifyWorldGeneratorVersionPersistenceRoundTrip();
  VerifyWorldMetadataBoundaryProjection();
  VerifyWorldSeedTextPersistenceRoundTrip();
  VerifyNpcTypedStatePersistenceRoundTrip();
  VerifyMoonPhasePersistenceRoundTrip();
  VerifyRawWeatherPersistenceRoundTrip();
  VerifyWorldEventRandomStatePersistenceRoundTrip();
  VerifyWorldTimeRatePersistenceRoundTrip();
  VerifyFractionalWorldClockPersistenceRoundTrip();
  VerifyTerrainBaseClearsConfiguredSpawn();
  VerifyDeterministicBaseGeneration();
  VerifyInvalidPayloadDoesNotProduceCandidate(temporaryDirectory);
  VerifyInvalidDimensionsDoNotProduceCandidate(temporaryDirectory);
  VerifyUnsupportedVersionDoesNotProduceCandidate(temporaryDirectory);
VerifyExistingFileIsReplacedAtomically(temporaryDirectory);
VerifyWorldRollingBackups(temporaryDirectory);
VerifyNpcProjectileCursorServerLifecycle(temporaryDirectory);
VerifyNpcProjectileCursorCrashRecovery(temporaryDirectory);
VerifyNpcProjectileCursorCompaction();
  VerifyNpcProjectileCursorRestartReplay(temporaryDirectory);
  VerifyWorldItemSnapshotWriteValidation();
  VerifyFailedReplacementPreservesExistingTarget(temporaryDirectory);
  VerifyDomeStateRoundTrip(temporaryDirectory);
  VerifyImportedCompatibilityRoundTrip(temporaryDirectory);
  VerifyDuplicateChestCoordinatesAreRejected();
  VerifyVersionOneDomeStateCompatibility();
  VerifyVersionEightWorldRulesCompatibility();
  VerifyVersionTwelveMeteorScheduleCompatibility();
  VerifyVersionElevenLanternNightCompatibility();
  VerifyVersionTenWindCompatibility();
VerifyVersionFourteenSlimeRainCooldownCompatibility();
VerifyLanternNightScheduleRoundTripAndVersionFifteenCompatibility();
VerifyLanternNightCooldownRoundTripAndVersionSixteenCompatibility();
VerifyVersionThirteenWorldItemInstanceCompatibility();
  VerifySlimeRainWarningIsTransient();
  Console.WriteLine("PASS: world persistence round-trip, strict recovery and atomic replacement");
}

finally
{
  Directory.Delete(temporaryDirectory, recursive: true);
}

static void VerifyRoundTrip(string temporaryDirectory)
{
  WorldGrid world = new(width: 400, height: 300);
  world.TrySetTile(1, 2, new WorldTile(IsActive: true, Type: 7));
  world.TrySetTile(201, 151, new WorldTile(IsActive: true, Type: 9));
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
  world.TrySetTile(2, 2, complexTile);
  WorldMetadata metadata = new("Persistence verification", new WorldSeed(712367), 400, 300);
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  string path = Path.Combine(temporaryDirectory, "round-trip.dome");

  WorldSaveCoordinator coordinator = new();
  coordinator.Save(path, snapshot);
  WorldRecoveryResult recovery = coordinator.TryLoad(path);
  if (!recovery.IsSuccess || recovery.Snapshot is not WorldGridSnapshot restoredSnapshot)
  {
    throw new InvalidOperationException("The saved world did not load as a candidate snapshot.");
  }

  WorldGrid restored = WorldGrid.FromSnapshot(restoredSnapshot);
  if (restored.GetTile(1, 2) != new WorldTile(IsActive: true, Type: 7) ||
      restored.GetTile(201, 151) != new WorldTile(IsActive: true, Type: 9) ||
      restored.GetTile(2, 2) != complexTile ||
      restored.GetSectionVersion(new WorldSectionCoordinates(0, 0)) != 2 ||
      restored.GetSectionVersion(new WorldSectionCoordinates(1, 1)) != 1 ||
      restoredSnapshot.Metadata != metadata)
  {
    throw new InvalidOperationException(
      "World persistence did not preserve metadata, tiles and versions.");
  }

  using DomeServer restartedServer = new(restored);
  if (restartedServer.World.GetTile(201, 151) != new WorldTile(IsActive: true, Type: 9))
  {
    throw new InvalidOperationException("The restart server did not adopt the recovered world.");
  }
}

static void VerifyWorldMetadataBoundaryProjection()
{
  WorldMetadata metadata = new("Boundary projection", new WorldSeed(781), 400, 300);
  if (metadata.LeftWorld != 0.0f || metadata.RightWorld != 6400.0f ||
      metadata.TopWorld != 0.0f || metadata.BottomWorld != 4800.0f ||
      metadata.MaxSectionsX != 2 || metadata.MaxSectionsY != 2)
  {
    throw new InvalidOperationException(
      "World metadata boundary and section projections diverged from grid dimensions.");
  }
}

static void VerifyNoTrapsWorldPersistenceRoundTrip()
{
  WorldMetadata metadata = new(
    "No traps persistence",
    new WorldSeed(782),
    400,
    300,
    isNoTrapsWorld: true);
  WorldGrid world = new(metadata.Width, metadata.Height);
  using MemoryStream stream = new();
  WorldPersistenceFormat.Write(stream, world.CreateSnapshot(metadata));
  stream.Position = 0;
  WorldGridSnapshot restored = WorldPersistenceFormat.Read(stream);
  if (restored.Metadata.IsNoTrapsWorld != true)
  {
    throw new InvalidOperationException(
      "World persistence did not preserve the no-traps world metadata flag.");
  }

  Console.WriteLine("PASS: no-traps world metadata persistence round-trip");
}

static void VerifySkyblockWorldPersistenceRoundTrip()
{
  WorldMetadata metadata = new(
    "Skyblock persistence",
    new WorldSeed(783),
    400,
    300,
    isSkyblockWorld: true);
  WorldGrid world = new(metadata.Width, metadata.Height);
  using MemoryStream stream = new();
  WorldPersistenceFormat.Write(stream, world.CreateSnapshot(metadata));
  stream.Position = 0;
  WorldGridSnapshot restored = WorldPersistenceFormat.Read(stream);
  if (restored.Metadata.IsSkyblockWorld != true)
  {
    throw new InvalidOperationException(
      "World persistence did not preserve the skyblock world metadata flag.");
  }

  Console.WriteLine("PASS: skyblock world metadata persistence round-trip");
}

static void VerifyGoodWorldPersistenceRoundTrip()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new(
    "Good World persistence",
    new WorldSeed(784),
    400,
    300,
    isRemixWorld: true,
    isNoTrapsWorld: true,
    isSkyblockWorld: true,
    isGoodWorld: true);
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);

  using MemoryStream worldStream = new();
  WorldPersistenceFormat.Write(worldStream, snapshot);
  worldStream.Position = 0;
  WorldGridSnapshot restoredWorld = WorldPersistenceFormat.Read(worldStream);
  if (restoredWorld.Metadata.IsGoodWorld != true ||
      restoredWorld.Metadata.IsRemixWorld != true ||
      restoredWorld.Metadata.IsNoTrapsWorld != true ||
      restoredWorld.Metadata.IsSkyblockWorld != true)
  {
    throw new InvalidOperationException(
      "World persistence did not retain Good World alongside existing world flags.");
  }

  DomeSimulationSnapshot domeSnapshot = new(snapshot, [], [], tickNumber: 0);
  using MemoryStream domeStream = new();
  DomeStatePersistenceFormat.Write(domeStream, domeSnapshot);
  domeStream.Position = 0;
  DomeSimulationSnapshot restoredDome = DomeStatePersistenceFormat.Read(domeStream);
  if (restoredDome.World.Metadata.IsGoodWorld != true ||
      restoredDome.World.Metadata.IsRemixWorld != true ||
      restoredDome.World.Metadata.IsNoTrapsWorld != true ||
      restoredDome.World.Metadata.IsSkyblockWorld != true)
  {
    throw new InvalidOperationException(
      "Dome state persistence did not retain Good World alongside existing world flags.");
  }

  WorldMetadata falseMetadata = new(
    metadata.Name,
    metadata.Seed,
    metadata.Width,
    metadata.Height,
    metadata.WorldId,
    metadata.SpawnX,
    metadata.SpawnY,
    metadata.SeedVariant,
    metadata.RandomStreamVersion,
    metadata.WorldSurface,
    metadata.RockLayer,
    metadata.IsRemixWorld,
    metadata.WorldGeneratorVersion,
    metadata.UniqueId,
    metadata.SeedText,
    metadata.IsNoTrapsWorld,
    metadata.IsSkyblockWorld,
    isGoodWorld: false);
  using MemoryStream falseDomeStream = new();
  DomeStatePersistenceFormat.Write(
    falseDomeStream,
    new DomeSimulationSnapshot(world.CreateSnapshot(falseMetadata), [], [], tickNumber: 0));
  falseDomeStream.Position = 0;
  DomeSimulationSnapshot restoredFalseDome = DomeStatePersistenceFormat.Read(falseDomeStream);
  if (restoredFalseDome.World.Metadata.IsGoodWorld != false)
  {
    throw new InvalidOperationException(
      "Dome state persistence did not retain an explicit false Good World value.");
  }

  byte[] versionThirtySixBytes = domeStream.ToArray()[..^(sizeof(bool) * 2)];
  BitConverter.GetBytes(36).CopyTo(versionThirtySixBytes, sizeof(int));
  using MemoryStream versionThirtySixStream = new(versionThirtySixBytes, writable: false);
  DomeSimulationSnapshot restoredVersionThirtySix =
    DomeStatePersistenceFormat.Read(versionThirtySixStream);
  if (restoredVersionThirtySix.World.Metadata.IsGoodWorld is not null ||
      restoredVersionThirtySix.World.Metadata.IsRemixWorld != true ||
      restoredVersionThirtySix.World.Metadata.IsNoTrapsWorld != true ||
      restoredVersionThirtySix.World.Metadata.IsSkyblockWorld != true)
  {
    throw new InvalidOperationException(
      "A V36 Dome state incorrectly recovered Good World from its embedded V8 world payload.");
  }

  Console.WriteLine("PASS: V37 Good World true/false and V36 unknown boundary");
  Console.WriteLine("PASS: Good World metadata persistence round-trip");
}

static void VerifyProjectileIdentityCursorPersistence()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("Projectile cursor", new WorldSeed(144), 400, 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    nextProjectileIdentity: 17,
    nextChestMutationSequence: 23,
    nextLiquidSequence: 29,
    nextWiringSequence: 31);
  using MemoryStream stream = new();
  DomeStatePersistenceFormat.Write(stream, snapshot);
  stream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(stream);
  if (restored.NextProjectileIdentity != 17 || restored.NextChestMutationSequence != 23 ||
      restored.NextLiquidSequence != 29 || restored.NextWiringSequence != 31)
  {
    throw new InvalidOperationException(
      "The projectile identity cursor did not survive the current persistence round-trip.");
  }

  byte[] legacyBytes = stream.ToArray()[..^(sizeof(int) + 3 * sizeof(long))];
  Buffer.BlockCopy(BitConverter.GetBytes(35), 0, legacyBytes, sizeof(int), sizeof(int));
  using MemoryStream legacyStream = new(legacyBytes, writable: false);
  DomeSimulationSnapshot legacy = DomeStatePersistenceFormat.Read(legacyStream);
  if (legacy.NextProjectileIdentity != 1 || legacy.NextChestMutationSequence != 0 ||
      legacy.NextLiquidSequence != 0 || legacy.NextWiringSequence != 0)
  {
    throw new InvalidOperationException(
      "A legacy state without a projectile identity cursor did not default safely.");
  }

  byte[] truncatedCurrentBytes = stream.ToArray()[..^(sizeof(long) * 3)];
  using MemoryStream truncatedCurrentStream = new(truncatedCurrentBytes, writable: false);
  try
  {
    _ = DomeStatePersistenceFormat.Read(truncatedCurrentStream);
    throw new InvalidOperationException(
      "A truncated V36 mutation cursor tail was accepted.");
  }
  catch (EndOfStreamException)
  {
  }

  try
  {
    _ = new DomeSimulationSnapshot(
      world.CreateSnapshot(metadata),
      [],
      [],
      tickNumber: 0,
      nextWiringSequence: long.MaxValue);
    throw new InvalidOperationException(
      "A non-advancable wiring sequence cursor was accepted.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  Console.WriteLine("PASS: projectile identity cursor persistence and legacy default");
}

static void VerifyNpcGivenNameSidecarPersistence()
{
  using MemoryStream stream = new();
  NpcGivenNamePersistenceFormat.Write(
    stream,
    [new NpcGivenNameEntry(7, "Guide"), new NpcGivenNameEntry(9, string.Empty)]);
  stream.Position = 0;
  IReadOnlyList<NpcGivenNameEntry> restored = NpcGivenNamePersistenceFormat.Read(stream);
  if (restored.Count != 2 || restored[0].ReplicationId != 7 ||
      restored[0].GivenName != "Guide" || restored[1].GivenName != string.Empty)
  {
    throw new InvalidOperationException("NPC given-name sidecar round-trip did not preserve entries.");
  }

  try
  {
    NpcGivenNamePersistenceFormat.Write(
      new MemoryStream(),
      [new NpcGivenNameEntry(7, "Guide"), new NpcGivenNameEntry(7, "Duplicate")]);
    throw new InvalidOperationException("NPC given-name sidecar accepted duplicate replication IDs.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }
}

static void VerifyNpcGivenNameSaveCoordinator(string temporaryDirectory)
{
  string path = Path.Combine(temporaryDirectory, "npc-given-names.bin");
  NpcGivenNameSaveCoordinator coordinator = new();
  coordinator.Save(path, [new NpcGivenNameEntry(7, "Guide")]);
  File.Copy(path, path + ".tmp", overwrite: true);
  File.WriteAllBytes(path, [1, 2, 3]);
  IReadOnlyList<NpcGivenNameEntry> recovered = coordinator.Load(path);
  if (recovered.Count != 1 || recovered[0].ReplicationId != 7 ||
      recovered[0].GivenName != "Guide" || File.Exists(path + ".tmp"))
  {
    throw new InvalidOperationException("NPC given-name coordinator did not recover its valid temporary file.");
  }

  NpcStateSnapshot state = CreatePersistenceNpcState(7);
  IReadOnlyList<NpcStateSnapshot> merged = coordinator.Apply(
    [state],
    [new NpcGivenNameEntry(7, "Guide"), new NpcGivenNameEntry(99, "Unknown")]);
  if (merged.Count != 1 || merged[0].GivenName != "Guide")
  {
    throw new InvalidOperationException("NPC given-name coordinator did not join by authoritative ID.");
  }

  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("NPC name server", new WorldSeed(41), 400, 300);
  NpcStateSnapshot namedState = CreatePersistenceNpcState(7) with { GivenName = "Guide" };
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [namedState.ToReplicationSnapshot()],
    [],
    tickNumber: 0,
    npcStates: [namedState]);
  coordinator.Save(path, [new NpcGivenNameEntry(7, "Guide")]);
  using (DomeServer server = new(snapshot))
  {
    server.ConfigureNpcGivenNamePersistence(path);
    if (server.CreatePersistenceSnapshot(metadata).NpcStates.Single().GivenName != "Guide")
    {
      throw new InvalidOperationException("DomeServer did not apply the given-name sidecar on startup.");
    }
  }

  if (coordinator.Load(path).Single().GivenName != "Guide")
  {
    throw new InvalidOperationException("DomeServer did not preserve the given-name sidecar on disposal.");
  }
}

static NpcStateSnapshot CreatePersistenceNpcState(int replicationId)
{
  NpcReplicationSnapshot replication = new(
    replicationId,
    1,
    default,
    default,
    100,
    true,
    1,
    default,
    DefinitionId: 1,
    MaximumHealth: 100);
  return NpcStateSnapshot.FromReplication(replication);
}

static void VerifyLegacyWorldPersistenceFormatCompatibility()
{
  const int formatMagic = 0x574D4F44;
  const int height = 300;
  const int width = 400;
  const int sectionCount = 4;
  const string name = "Legacy format";
  using MemoryStream stream = new();
  using (BinaryWriter writer = new(stream, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(formatMagic);
    writer.Write(1);
    writer.Write(width);
    writer.Write(height);
    writer.Write(23);
    byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(name);
    writer.Write(nameBytes.Length);
    writer.Write(nameBytes);
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        bool isFirstTile = x == 0 && y == 0;
        writer.Write(isFirstTile ? (byte)1 : (byte)0);
        writer.Write(isFirstTile ? (ushort)7 : (ushort)0);
      }
    }

    writer.Write(sectionCount);
    for (int index = 0; index < sectionCount; index++)
    {
      writer.Write((long)index);
    }
  }

  stream.Position = 0;
  WorldGridSnapshot snapshot = WorldPersistenceFormat.Read(stream);
  if (snapshot.Metadata != new WorldMetadata(name, new WorldSeed(23), width, height) ||
      snapshot.GetTile(0, 0) != new WorldTile(IsActive: true, Type: 7) ||
      snapshot.GetTile(1, 0) != default ||
      snapshot.GetSectionVersion(new WorldSectionCoordinates(0, 0)) != 0 ||
      snapshot.GetSectionVersion(new WorldSectionCoordinates(1, 0)) != 1 ||
      snapshot.GetSectionVersion(new WorldSectionCoordinates(0, 1)) != 2 ||
      snapshot.GetSectionVersion(new WorldSectionCoordinates(1, 1)) != 3)
  {
    throw new InvalidOperationException(
      "The version-1 WorldPersistenceFormat layout was not read compatibly.");
  }
}

static void VerifyVersionTwoWorldPersistenceFormatCompatibility()
{
  WorldGrid world = new(width: 400, height: 300);
  world.TrySetTile(1, 2, new WorldTile(IsActive: true, Type: 7));
  WorldGridSnapshot snapshot = world.CreateSnapshot(
    new WorldMetadata("V2 compatibility", new WorldSeed(24), 400, 300));
  using MemoryStream currentStream = new();
  WorldPersistenceFormat.Write(currentStream, snapshot);
  byte[] currentBytes = currentStream.ToArray();
  int nameLength = BitConverter.ToInt32(currentBytes, sizeof(int) * 5);
  int worldSurfaceOffset = sizeof(int) * 6 + nameLength;
  if (currentBytes[worldSurfaceOffset] != 0)
  {
    throw new InvalidOperationException(
      "The V2 compatibility fixture expected an unknown world surface.");
  }

  byte[] versionTwoBytes = new byte[currentBytes.Length - sizeof(bool) * 6];
  Buffer.BlockCopy(currentBytes, 0, versionTwoBytes, 0, worldSurfaceOffset);
  Buffer.BlockCopy(
    currentBytes,
    worldSurfaceOffset + sizeof(bool) * 6,
    versionTwoBytes,
    worldSurfaceOffset,
    currentBytes.Length - worldSurfaceOffset - sizeof(bool) * 6);
  BitConverter.GetBytes(2).CopyTo(versionTwoBytes, sizeof(int));
  using MemoryStream versionTwoStream = new(versionTwoBytes, writable: false);
  WorldGridSnapshot versionTwo = WorldPersistenceFormat.Read(versionTwoStream);
  if (versionTwo.Metadata.WorldSurface is not null ||
      versionTwo.GetTile(1, 2) != new WorldTile(IsActive: true, Type: 7))
  {
    throw new InvalidOperationException(
      "A V2 world format did not restore an unknown world surface.");
  }
}

static void VerifyRemixWorldPersistenceRoundTrip()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new(
    "Remix metadata",
    new WorldSeed(2401),
    400,
    300,
    worldSurface: 100,
    isRemixWorld: true);
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  using MemoryStream stream = new();
  WorldPersistenceFormat.Write(stream, snapshot);
  stream.Position = 0;
  WorldGridSnapshot restored = WorldPersistenceFormat.Read(stream);
  if (restored.Metadata.WorldSurface != 100 || restored.Metadata.IsRemixWorld != true)
  {
    throw new InvalidOperationException(
      "World persistence did not retain the authoritative Remix metadata.");
  }
}

static void VerifyVersionThreeWorldSurfaceCompatibility()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldGridSnapshot snapshot = world.CreateSnapshot(
    new WorldMetadata(
      "V3 surface",
      new WorldSeed(2402),
      400,
      300,
      worldSurface: 123.5,
      isRemixWorld: true));
  using MemoryStream currentStream = new();
  WorldPersistenceFormat.Write(currentStream, snapshot);
  byte[] currentBytes = currentStream.ToArray();
  int nameLength = BitConverter.ToInt32(currentBytes, sizeof(int) * 5);
  int worldSurfaceOffset = sizeof(int) * 6 + nameLength;
  int rockLayerMarkerOffset = worldSurfaceOffset + sizeof(bool) + sizeof(double);
  byte[] versionThreeBytes = new byte[currentBytes.Length - sizeof(bool) * 6];
  Buffer.BlockCopy(currentBytes, 0, versionThreeBytes, 0, rockLayerMarkerOffset);
  Buffer.BlockCopy(
    currentBytes,
    rockLayerMarkerOffset + sizeof(bool) * 6,
    versionThreeBytes,
    rockLayerMarkerOffset,
    currentBytes.Length - rockLayerMarkerOffset - sizeof(bool) * 6);
  BitConverter.GetBytes(3).CopyTo(versionThreeBytes, sizeof(int));
  using MemoryStream versionThreeStream = new(versionThreeBytes, writable: false);
  WorldGridSnapshot restored = WorldPersistenceFormat.Read(versionThreeStream);
  if (restored.Metadata.WorldSurface != 123.5 ||
      restored.Metadata.IsRemixWorld is not null)
  {
    throw new InvalidOperationException(
      "A V3 world did not preserve its surface while treating Remix metadata as unknown.");
  }
}

static void VerifyWorldSurfacePersistenceRoundTrip()
{
  const double expectedWorldSurface = 123.5;
  const double expectedRockLayer = 245.75;
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new(
    "World surface",
    new WorldSeed(1456),
    400,
    300,
    worldSurface: expectedWorldSurface,
    rockLayer: expectedRockLayer);
  WorldGridSnapshot worldSnapshot = world.CreateSnapshot(metadata);

  using MemoryStream worldStream = new();
  WorldPersistenceFormat.Write(worldStream, worldSnapshot);
  worldStream.Position = 0;
  WorldGridSnapshot restoredWorld = WorldPersistenceFormat.Read(worldStream);
  if (restoredWorld.Metadata.WorldSurface != expectedWorldSurface ||
      restoredWorld.Metadata.RockLayer != expectedRockLayer)
  {
    throw new InvalidOperationException(
      "Embedded world persistence did not retain the world surface.");
  }

  DomeSimulationSnapshot snapshot = new(worldSnapshot, [], [], tickNumber: 0);
  using MemoryStream stateStream = new();
  DomeStatePersistenceFormat.Write(stateStream, snapshot);
  stateStream.Position = 0;
  DomeSimulationSnapshot restoredState = DomeStatePersistenceFormat.Read(stateStream);
  if (restoredState.World.Metadata.WorldSurface != expectedWorldSurface ||
      restoredState.World.Metadata.RockLayer != expectedRockLayer)
  {
    throw new InvalidOperationException(
      "Dome state persistence did not retain the world surface.");
  }

  byte[] currentBytes = stateStream.ToArray();
  byte[] versionSeventeenBytes = new byte[
    currentBytes.Length - sizeof(uint) - sizeof(bool) - sizeof(float) - sizeof(int) -
    sizeof(byte) - sizeof(bool) - sizeof(double) - sizeof(int) - sizeof(int) - sizeof(double) -
    sizeof(bool) * 6 - sizeof(int) - sizeof(double)];
  Buffer.BlockCopy(currentBytes, 0, versionSeventeenBytes, 0, versionSeventeenBytes.Length);
  BitConverter.GetBytes(17).CopyTo(versionSeventeenBytes, sizeof(int));
  using MemoryStream versionSeventeenStream = new(versionSeventeenBytes, writable: false);
  DomeSimulationSnapshot versionSeventeen = DomeStatePersistenceFormat.Read(versionSeventeenStream);
  if (versionSeventeen.World.Metadata.WorldSurface is not null)
  {
    throw new InvalidOperationException(
      "A V17 Dome state did not restore an unknown world surface.");
  }
}

static void VerifyWorldGeneratorVersionPersistenceRoundTrip()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new(
    "Generator version",
    new WorldSeed(1456),
    400,
    300,
    worldGeneratorVersion: 1370094567425UL);
  DomeSimulationSnapshot snapshot = new(world.CreateSnapshot(metadata), [], [], tickNumber: 0);
  using MemoryStream stream = new();
  DomeStatePersistenceFormat.Write(stream, snapshot);
  stream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(stream);
  if (restored.World.Metadata.WorldGeneratorVersion != 1370094567425UL)
  {
    throw new InvalidOperationException(
      "Dome persistence did not retain the WLD generator version.");
  }
}

static void VerifyWorldSeedTextPersistenceRoundTrip()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new(
    "Seed text",
    new WorldSeed(1456),
    400,
    300,
    seedText: "abc|rainsForAYear");
  DomeSimulationSnapshot snapshot = new(world.CreateSnapshot(metadata), [], [], tickNumber: 0);
  using MemoryStream stream = new();
  DomeStatePersistenceFormat.Write(stream, snapshot);
  stream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(stream);
  if (restored.World.Metadata.SeedText != "abc|rainsForAYear")
  {
    throw new InvalidOperationException("Dome persistence did not retain raw WLD seed text.");
  }
}

static void VerifyNpcTypedStatePersistenceRoundTrip()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("NPC typed state", new WorldSeed(37), 400, 300);
  NpcReplicationSnapshot npc = new(
    ReplicationId: 7,
    NpcType: 1,
    Position: new SimulationVector(20.0f, 20.0f),
    Velocity: default,
    Health: 100,
    IsActive: true,
    Revision: 3,
    Section: world.GetSectionCoordinates(20, 20),
    DefinitionId: 1,
    MaximumHealth: 100,
    BehaviorId: NpcBehaviorId.FloatingEye,
    FlyingHorizontalAcceleration: 0.75f,
    FlyingVerticalAcceleration: 0.5f,
    FlyingMaximumHorizontalSpeed: 6.0f,
    FlyingMaximumVerticalSpeed: 4.0f,
    Faction: NpcFaction.Town,
    Category: NpcCategory.Town);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [npc],
    [],
    tickNumber: 0);
  using MemoryStream stream = new();
  DomeStatePersistenceFormat.Write(stream, snapshot);
  stream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(stream);
  NpcReplicationSnapshot restoredNpc = restored.Npcs.Single();
  if (restoredNpc.BehaviorId != NpcBehaviorId.FloatingEye ||
      restoredNpc.FlyingHorizontalAcceleration != 0.75f ||
      restoredNpc.FlyingVerticalAcceleration != 0.5f ||
      restoredNpc.FlyingMaximumHorizontalSpeed != 6.0f ||
      restoredNpc.FlyingMaximumVerticalSpeed != 4.0f ||
      restoredNpc.Faction != NpcFaction.Town || restoredNpc.Category != NpcCategory.Town)
  {
    throw new InvalidOperationException(
      "NPC typed movement and faction state did not survive server persistence round-trip.");
  }

  Console.WriteLine("PASS: NPC typed movement and faction persistence round-trip");
}

static void VerifyMoonPhasePersistenceRoundTrip()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("Moon phase", new WorldSeed(19), 400, 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    worldClock: new WorldClockSnapshot(0, 0, true, false, 1, MoonPhase: 4),
    worldRules: new WorldRuleState(gameMode: WorldGameMode.Journey));
  using MemoryStream stream = new();
  DomeStatePersistenceFormat.Write(stream, snapshot);
  stream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(stream);
  if (restored.Clock.MoonPhase != 4 ||
      restored.WorldRules.GameMode != WorldGameMode.Journey)
  {
    throw new InvalidOperationException(
      "Dome persistence did not retain authoritative moon phase and game mode.");
  }

  Console.WriteLine("PASS: moon phase and game mode persistence round-trip");
}

static void VerifyWorldEventRandomStatePersistenceRoundTrip()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("Random state", new WorldSeed(31), 400, 300);
  WorldEventRandomState expected = new(123U);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 4,
    worldEventRandomState: expected);

  using MemoryStream stream = new();
  DomeStatePersistenceFormat.Write(stream, snapshot);
  byte[] currentBytes = stream.ToArray();
  stream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(stream);
  if (restored.WorldEventRandomState != expected)
  {
    throw new InvalidOperationException(
      "V22 persistence did not retain the world-event random state.");
  }

  byte[] versionTwentyOne = new byte[
    currentBytes.Length - sizeof(uint) - sizeof(int) - sizeof(int) - sizeof(double) -
    sizeof(bool) * 6 - sizeof(int) - sizeof(double)];
  Buffer.BlockCopy(currentBytes, 0, versionTwentyOne, 0, versionTwentyOne.Length);
  BitConverter.GetBytes(21).CopyTo(versionTwentyOne, sizeof(int));
  using MemoryStream legacyStream = new(versionTwentyOne, writable: false);
  DomeSimulationSnapshot legacy = DomeStatePersistenceFormat.Read(legacyStream);
  WorldEventRandomState expectedFallback = new(unchecked((uint)metadata.Seed.Value));
  if (legacy.WorldEventRandomState != expectedFallback)
  {
    throw new InvalidOperationException(
      "V21 persistence did not derive the documented random-state fallback.");
  }

  byte[] truncated = currentBytes[..^sizeof(byte)];
  try
  {
    using MemoryStream truncatedStream = new(truncated, writable: false);
    _ = DomeStatePersistenceFormat.Read(truncatedStream);
    throw new InvalidOperationException("A truncated V22 random state was accepted.");
  }
  catch (EndOfStreamException)
  {
  }
}

static void VerifyRawWeatherPersistenceRoundTrip()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("Raw weather", new WorldSeed(21), 400, 300);
  WorldRuleState rawWeather = new(
    rainTimeTicks: 120,
    rainStrength: 0.25f,
    isRaining: false,
    maximumRainStrength: 0.75f);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    worldClock: new WorldClockSnapshot(0, 0, true, false, 1),
    worldRules: rawWeather);
  using MemoryStream currentStream = new();
  DomeStatePersistenceFormat.Write(currentStream, snapshot);
  currentStream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(currentStream);
  if (restored.WorldRules != rawWeather)
  {
    throw new InvalidOperationException("A V21 Dome state did not retain raw weather state.");
  }

  byte[] versionTwenty = currentStream.ToArray()[
    ..^(sizeof(uint) + sizeof(bool) + sizeof(float) + sizeof(int) + sizeof(int) +
      sizeof(double) + sizeof(bool) * 6 + sizeof(int) + sizeof(double))];
  BitConverter.GetBytes(20).CopyTo(versionTwenty, sizeof(int));
  using MemoryStream versionTwentyStream = new(versionTwenty, writable: false);
  DomeSimulationSnapshot restoredVersionTwenty = DomeStatePersistenceFormat.Read(versionTwentyStream);
  if (!restoredVersionTwenty.WorldRules.IsRaining ||
      restoredVersionTwenty.WorldRules.MaximumRainStrength != 0.25f)
  {
    throw new InvalidOperationException(
      "A V20 Dome state did not restore the documented derived rain activity.");
  }
}

static void VerifyDomeStateRoundTrip(string temporaryDirectory)
{
  WorldGrid world = new(width: 400, height: 300);
  world.TrySetTile(25, 26, new WorldTile(IsActive: true, Type: 17));
  WorldMetadata metadata = new(
    "Dome state",
    new WorldSeed(912),
    400,
    300,
    worldId: 1200,
    spawnX: 190,
    spawnY: 74,
    seedVariant: "not-the-bees",
    randomStreamVersion: 3,
    worldSurface: 100,
    isRemixWorld: true);
  WorldRuleState rules = new(
    difficulty: 2,
    isExpertMode: true,
    isCrimsonWorld: true,
    rainTimeTicks: 123,
    rainStrength: 0.5f,
    windSpeedTarget: 0.4f,
    windSpeedCurrent: 0.2f);
  WorldProgressionState progression = new(
    isHardMode: true,
    defeatedWallOfFlesh: true,
    defeatedMechanicalBoss: true,
    defeatedPlantera: true,
    isBloodMoon: true,
    isLanternNight: true,
    invasionType: 3,
    invasionSize: 100,
    invasionSizeStart: 100,
    invasionDelayTicks: 3,
    invasionX: 77.25,
    defeatedFrost: true,
    slimeRainTimeTicks: 123,
    isMeteorScheduled: true);
  using DomeSimulation simulation = new(world);
  if (!simulation.TryQueueWorldMeteorImpact(new WorldMeteorImpactCommand(200, 150, 1)))
  {
    throw new InvalidOperationException("Persistence fixture could not queue its meteor impact.");
  }

  simulation.Tick(new SimulationInputBatch());
  PlayerHandle projectileOwner = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    projectileOwner,
    10.0f,
    0.0f,
    Facing: 1,
    Damage: 10,
    LifetimeTicks: 10));
  simulation.Tick(new SimulationInputBatch());
  PlayerPersistentState account = CreatePersistentState();
  _ = simulation.ImportPlayerIfMissing(account);
  _ = simulation.CreateNpc(new SimulationVector(30.0f, 0.0f));
  int itemId = simulation.SpawnWorldItem(
    new Terraria.Dome.Simulation.Items.ItemStack(1, 4),
    new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
      PrefixId: 7,
      VariantId: 2,
      Dye: 3,
      Paint: 4,
      IsFavorited: true,
      IsNewAndShiny: true,
      NameOverride: "World item"),
    new SimulationVector(31.0f, 0.0f),
    pickupDelayTicks: 3);
  simulation.Tick(new SimulationInputBatch());
  DomeSimulationSnapshot snapshot = simulation.CreatePersistenceSnapshot(
    metadata,
    rules,
    progression);
  if (snapshot.NextProjectileIdentity <= 1)
  {
    throw new InvalidOperationException("Persistence fixture did not advance the projectile identity cursor.");
  }
  string path = Path.Combine(temporaryDirectory, "dome-state.dome");

  DomeStateSaveCoordinator coordinator = new();
  coordinator.Save(path, snapshot);
  DomeStateRecoveryResult recovery = coordinator.TryLoad(path);
  if (!recovery.IsSuccess || recovery.Snapshot is not DomeSimulationSnapshot restored)
  {
    throw new InvalidOperationException(
      $"The Dome state did not load as a candidate snapshot: {recovery.FailureReason}");
  }

  if (restored.Progression.InvasionDelayTicks != 3 || restored.Progression.InvasionX != 77.25 ||
      !restored.Progression.DefeatedFrost ||
      restored.World.Metadata.IsRemixWorld != true)
  {
    throw new InvalidOperationException(
      "The v26 Dome state did not retain the invasion travel and clear-flag state.");
  }

  using DomeSimulation restarted = new(restored);
  if (restarted.CreatePersistenceSnapshot(metadata).NextProjectileIdentity !=
      snapshot.NextProjectileIdentity)
  {
    throw new InvalidOperationException(
      "The persisted projectile identity cursor was not restored after restart.");
  }
  NpcReplicationSnapshot npc = restarted.CreateNpcReplicationSnapshots().Single();
  ItemReplicationSnapshot item = restarted.CreateItemReplicationSnapshots()
    .Single(value => value.ReplicationId == itemId);
  Arch.Core.QueryDescription worldItemEntityQuery = new Arch.Core.QueryDescription()
    .WithAll<WorldItemComponent>();
  int restoredWorldItemEntityCount = 0;
  bool restoredWorldItemComponentsMatch = false;
  restarted.World.Query(
    in worldItemEntityQuery,
    (Arch.Core.Entity entity, ref WorldItemComponent runtimeItem) =>
    {
      if (runtimeItem.ReplicationId != itemId)
      {
        return;
      }

      restoredWorldItemEntityCount++;
      Terraria.Dome.Simulation.Items.Components.ItemStackComponent stackComponent =
        restarted.World.Get<Terraria.Dome.Simulation.Items.Components.ItemStackComponent>(entity);
      Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent instanceStateComponent =
        restarted.World.Get<Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent>(entity);
      Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent worldStateComponent =
        restarted.World.Get<Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent>(entity);
      restoredWorldItemComponentsMatch = stackComponent.Stack == runtimeItem.Stack &&
        instanceStateComponent == runtimeItem.InstanceState &&
        worldStateComponent.IsActive == runtimeItem.IsActive &&
        worldStateComponent.Revision == runtimeItem.Revision;
    });
  if (restoredWorldItemEntityCount != 1 || !restoredWorldItemComponentsMatch)
  {
    throw new InvalidOperationException(
      "A restored world item did not reconstruct synchronized ECS runtime components.");
  }

  if (restarted.TickNumber != snapshot.TickNumber ||
      restarted.CreateWorldProgressionSnapshot() != progression ||
      restored.World.Metadata != metadata ||
      restored.WorldRules != rules ||
      restored.Progression != progression ||
      restored.Clock != snapshot.Clock ||
      restarted.WorldGrid.GetTile(25, 26) != new WorldTile(IsActive: true, Type: 17) ||
      restarted.WorldGrid.GetTile(215, 155) != new WorldTile(IsActive: true, Type: 37) ||
      npc.Position != snapshot.Npcs[0].Position ||
      npc.Revision != snapshot.Npcs[0].Revision ||
      item.Stack != snapshot.WorldItems[0].Stack ||
      item.InstanceState != snapshot.WorldItems[0].InstanceState ||
      item.WorldState.PickupDelayTicks != snapshot.WorldItems[0].WorldState.PickupDelayTicks ||
      item.WorldState.Revision != snapshot.WorldItems[0].WorldState.Revision ||
      item.Revision != snapshot.WorldItems[0].Revision ||
      restored.PlayerAccounts.Count != 1 ||
      restored.PlayerAccounts[0].Uuid != account.Uuid ||
      restored.PlayerAccounts[0].Profile != account.Profile ||
      restored.PlayerAccounts[0].Life != account.Life ||
      restored.PlayerAccounts[0].MaximumLife != account.MaximumLife ||
      restored.PlayerAccounts[0].Mana != account.Mana ||
      restored.PlayerAccounts[0].MaximumMana != account.MaximumMana ||
      !restored.PlayerAccounts[0].Buffs.SequenceEqual(account.Buffs) ||
      restored.PlayerAccounts[0].SelectedLoadout != account.SelectedLoadout ||
      restored.PlayerAccounts[0].AccessoryVisibility != account.AccessoryVisibility ||
      !restored.PlayerAccounts[0].Items.SequenceEqual(account.Items))
  {
    throw new InvalidOperationException(
      "Dome state persistence did not preserve world, NPC and item snapshots.");
  }

  PlayerHandle restoredPlayer = restarted.CreatePlayer(
    restored.PlayerAccounts[0],
    new SimulationVector(40.0f, 0.0f));
  InventoryComponent restoredInventory = restarted.GetInventory(restoredPlayer);
  if (restoredInventory.GetSlot(11) != new ItemStack(4, 1) ||
      restoredInventory.GetInstanceState(11).PrefixId != 0)
  {
    throw new InvalidOperationException(
      "Restored account inventory retained an invalid stack or definition-ineligible prefix.");
  }

  restarted.QueueEquipItem(restoredPlayer, 11, isVanity: false);
  restarted.Tick(new SimulationInputBatch());
  int restoredEquipmentRegen = -1;
  Arch.Core.QueryDescription restoredPlayerQuery = new Arch.Core.QueryDescription()
    .WithAll<Terraria.Dome.Simulation.Player.Components.PlayerIdentityComponent>();
  restarted.World.Query(
    in restoredPlayerQuery,
    (Arch.Core.Entity entity,
      ref Terraria.Dome.Simulation.Player.Components.PlayerIdentityComponent identity) =>
    {
      if (identity.Player != restoredPlayer)
      {
        return;
      }

      restoredEquipmentRegen = restarted.World.Get<
        Terraria.Dome.Simulation.Combat.Components.HealthRegenerationComponent>(entity)
        .EquipmentRegenUnitsPerTick;
    });
  if (restoredEquipmentRegen != 60)
  {
    throw new InvalidOperationException(
      "Restored account equipment did not project life regeneration onto the player entity.");
  }

  byte[] bytes = File.ReadAllBytes(path);
  File.WriteAllBytes(path, [.. bytes, 0x7F]);
  if (coordinator.TryLoad(path).IsSuccess)
  {
    throw new InvalidOperationException("Trailing Dome state bytes were accepted.");
  }
}

static void VerifyVersionOneDomeStateCompatibility()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldGridSnapshot snapshot = world.CreateSnapshot(
    new WorldMetadata("V1 compatibility", new WorldSeed(918), 400, 300));
  using MemoryStream worldStream = new();
  WorldPersistenceFormat.Write(worldStream, snapshot);

  using MemoryStream stateStream = new();
  using (BinaryWriter writer = new(stateStream, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(0x44535444);
    writer.Write(1);
    writer.Write((int)worldStream.Length);
    writer.Write(worldStream.ToArray());
    writer.Write(0L);
    writer.Write(0);
    writer.Write(0);
  }

  stateStream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(stateStream);
  if (restored.PlayerAccounts.Count != 0 || restored.TickNumber != 0 ||
      restored.WorldRules != new WorldRuleState() ||
      restored.Progression != new WorldProgressionState() ||
      restored.Clock != new WorldClockSnapshot(0, 0, true, false, 1))
  {
    throw new InvalidOperationException("A V1 Dome state did not load with no player accounts.");
  }
}

static void VerifyVersionEightWorldRulesCompatibility()
{
  WorldMetadata metadata = new(
    "V8 compatibility",
    new WorldSeed(919),
    400,
    300,
    worldId: 4,
    spawnX: 190,
    spawnY: 74,
    seedVariant: "legacy-v8",
    randomStreamVersion: 2);
  WorldGrid world = new(width: 400, height: 300);
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  using MemoryStream worldStream = new();
  WorldPersistenceFormat.Write(worldStream, snapshot);

  using MemoryStream stateStream = new();
  using (BinaryWriter writer = new(stateStream, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(0x44535444);
    writer.Write(8);
    writer.Write((int)worldStream.Length);
    writer.Write(worldStream.ToArray());
    writer.Write(0L);
    writer.Write(0);
    writer.Write(0);
    writer.Write(0);
    writer.Write(0);
    writer.Write(0);
    writer.Write(0);
    writer.Write(0);
    writer.Write(0L);
    writer.Write(0);
    writer.Write(true);
    writer.Write(false);
    writer.Write(1);
    writer.Write(WorldClock.DefaultDayLengthTicks);
    writer.Write(WorldClock.DefaultNightLengthTicks);
    writer.Write(metadata.WorldId);
    writer.Write(metadata.SpawnX);
    writer.Write(metadata.SpawnY);
    writer.Write(metadata.SeedVariant);
    writer.Write(metadata.RandomStreamVersion);
    writer.Write(2);
    writer.Write(true);
    writer.Write(false);
    writer.Write(true);
    writer.Write(false);
    writer.Write(false);
    writer.Write(false);
    writer.Write(false);
    writer.Write(false);
    writer.Write(false);
    writer.Write(false);
    writer.Write(false);
    writer.Write(false);
    writer.Write(false);
    writer.Write(0);
    writer.Write(0);
  }

  stateStream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(stateStream);
  WorldRuleState expectedRules = new(
    difficulty: 2,
    isExpertMode: true,
    isCrimsonWorld: true);
  if (restored.WorldRules != expectedRules || restored.WorldRules.IsRaining ||
      restored.WorldRules.RainTimeTicks != 0 || restored.WorldRules.RainStrength != 0.0f)
  {
    throw new InvalidOperationException("A V8 Dome state did not restore canonical clear rain.");
  }
}

static void VerifyVersionTenWindCompatibility()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("V10 compatibility", new WorldSeed(920), 400, 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    worldClock: new WorldClockSnapshot(0, 0, true, false, 1),
    worldRules: new WorldRuleState(
      rainTimeTicks: 123,
      rainStrength: 0.5f,
      windSpeedTarget: 0.4f,
      windSpeedCurrent: 0.2f),
    progression: new WorldProgressionState(slimeRainTimeTicks: 7));

  using MemoryStream currentStream = new();
  DomeStatePersistenceFormat.Write(currentStream, snapshot);
  byte[] currentBytes = RemoveCurrentWorldSurface(currentStream.ToArray());
  (int windOffset, int afterWindOffset, int lanternOffset, int afterLanternOffset,
    int meteorOffset, int afterMeteorOffset, int cooldownOffset, int afterCooldownOffset) =
    LocateWindAndLanternFields(currentBytes);
  byte[] legacyBytes = new byte[
    currentBytes.Length - sizeof(float) * 2 - sizeof(bool) * 3 - sizeof(int) * 2];
  Buffer.BlockCopy(currentBytes, 0, legacyBytes, 0, windOffset);
  int progressionBooleanCount = lanternOffset - afterWindOffset;
  Buffer.BlockCopy(
    currentBytes,
    afterWindOffset,
    legacyBytes,
    windOffset,
    progressionBooleanCount);
  int progressionValueCount = meteorOffset - afterLanternOffset;
  Buffer.BlockCopy(
    currentBytes,
    afterLanternOffset,
    legacyBytes,
    windOffset + progressionBooleanCount,
    progressionValueCount);
  Buffer.BlockCopy(
    currentBytes,
    afterMeteorOffset,
    legacyBytes,
    windOffset + progressionBooleanCount + progressionValueCount,
    cooldownOffset - afterMeteorOffset);
  Buffer.BlockCopy(
    currentBytes,
    afterCooldownOffset,
    legacyBytes,
    windOffset + progressionBooleanCount + progressionValueCount +
      (cooldownOffset - afterMeteorOffset),
    currentBytes.Length - afterCooldownOffset - sizeof(bool) - sizeof(int));
  BitConverter.GetBytes(10).CopyTo(legacyBytes, sizeof(int));

  using MemoryStream legacyStream = new(legacyBytes, writable: false);
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(legacyStream);
  if (restored.WorldRules.WindSpeedTarget != 0.0f ||
      restored.WorldRules.WindSpeedCurrent != 0.0f ||
      restored.WorldRules.RainTimeTicks != 123 ||
      restored.Progression.SlimeRainTimeTicks != 7 ||
      restored.Progression.IsLanternNight)
  {
    throw new InvalidOperationException(
      "A V10 Dome state did not preserve legacy fields or default new fields.");
  }
}

static void VerifyVersionElevenLanternNightCompatibility()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("V11 compatibility", new WorldSeed(921), 400, 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    worldClock: new WorldClockSnapshot(0, 0, true, false, 1),
    worldRules: new WorldRuleState(
      rainTimeTicks: 123,
      rainStrength: 0.5f,
      windSpeedTarget: 0.4f,
      windSpeedCurrent: 0.2f),
    progression: new WorldProgressionState(
      isLanternNight: true,
      slimeRainTimeTicks: 7,
      isMeteorScheduled: true));

  using MemoryStream currentStream = new();
  DomeStatePersistenceFormat.Write(currentStream, snapshot);
  byte[] currentBytes = RemoveCurrentWorldSurface(currentStream.ToArray());
  (_, _, int lanternOffset, int afterLanternOffset, int meteorOffset, int afterMeteorOffset,
    int cooldownOffset, int afterCooldownOffset) = LocateWindAndLanternFields(currentBytes);
  byte[] legacyBytes = new byte[currentBytes.Length - sizeof(bool) * 3 - sizeof(int) * 2];
  Buffer.BlockCopy(currentBytes, 0, legacyBytes, 0, lanternOffset);
  int progressionValueCount = meteorOffset - afterLanternOffset;
  Buffer.BlockCopy(
    currentBytes,
    afterLanternOffset,
    legacyBytes,
    lanternOffset,
    progressionValueCount);
  Buffer.BlockCopy(
    currentBytes,
    afterCooldownOffset,
    legacyBytes,
    lanternOffset + progressionValueCount,
    currentBytes.Length - afterCooldownOffset - sizeof(bool) - sizeof(int));
  BitConverter.GetBytes(11).CopyTo(legacyBytes, sizeof(int));

  using MemoryStream legacyStream = new(legacyBytes, writable: false);
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(legacyStream);
  if (restored.WorldRules.WindSpeedTarget != 0.4f ||
      restored.WorldRules.WindSpeedCurrent != 0.2f ||
      restored.Progression.SlimeRainTimeTicks != 7 ||
      restored.Progression.IsLanternNight)
  {
    throw new InvalidOperationException(
      "A V11 Dome state did not preserve Wind while defaulting Lantern Night.");
  }
}

static void VerifyVersionTwelveMeteorScheduleCompatibility()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("V12 compatibility", new WorldSeed(922), 400, 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    worldClock: new WorldClockSnapshot(0, 0, true, false, 1),
    worldRules: new WorldRuleState(
      rainTimeTicks: 123,
      rainStrength: 0.5f,
      windSpeedTarget: 0.4f,
      windSpeedCurrent: 0.2f),
    progression: new WorldProgressionState(
      isLanternNight: true,
      isMeteorScheduled: true,
      slimeRainTimeTicks: 7));

  using MemoryStream currentStream = new();
  DomeStatePersistenceFormat.Write(currentStream, snapshot);
  byte[] currentBytes = RemoveCurrentWorldSurface(currentStream.ToArray());
  (_, _, _, _, int meteorOffset, _, _, int afterCooldownOffset) =
    LocateWindAndLanternFields(currentBytes);
  byte[] legacyBytes = new byte[currentBytes.Length - sizeof(bool) * 2 - sizeof(int) * 2];
  Buffer.BlockCopy(currentBytes, 0, legacyBytes, 0, meteorOffset);
  Buffer.BlockCopy(
    currentBytes,
    afterCooldownOffset,
    legacyBytes,
    meteorOffset,
    currentBytes.Length - afterCooldownOffset - sizeof(bool) - sizeof(int));
  BitConverter.GetBytes(12).CopyTo(legacyBytes, sizeof(int));

  using MemoryStream legacyStream = new(legacyBytes, writable: false);
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(legacyStream);
  if (restored.WorldRules.WindSpeedTarget != 0.4f ||
      restored.WorldRules.WindSpeedCurrent != 0.2f ||
      restored.Progression.SlimeRainTimeTicks != 7 ||
      !restored.Progression.IsLanternNight ||
      restored.Progression.IsMeteorScheduled)
  {
    throw new InvalidOperationException(
      "A V12 Dome state did not preserve prior fields while defaulting Meteor schedule.");
  }
}

static void VerifyVersionThirteenWorldItemInstanceCompatibility()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("V13 compatibility", new WorldSeed(923), 400, 300);
  ItemReplicationSnapshot item = new(
    1,
    new ItemStack(1, 2),
    new SimulationVector(10.0f, 0.0f),
    IsActive: true,
    Revision: 1,
    new WorldSectionCoordinates(0, 0),
    new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
      PrefixId: 7,
      VariantId: 8,
      Dye: 9,
      Paint: 10,
      IsFavorited: true,
      IsNewAndShiny: true),
    Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(1));
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [item],
    tickNumber: 0);

  using MemoryStream currentStream = new();
  DomeStatePersistenceFormat.Write(currentStream, snapshot);
  byte[] currentBytes = RemoveCurrentWorldSurface(currentStream.ToArray());
  int newAndShinyOffset = LocateWorldItemNewAndShinyField(currentBytes);
  int cooldownOffset = currentBytes.Length - sizeof(bool) - sizeof(int) * 2;
  byte[] legacyBytes = new byte[currentBytes.Length - sizeof(bool) * 2 - sizeof(int) * 2];
  Buffer.BlockCopy(currentBytes, 0, legacyBytes, 0, newAndShinyOffset);
  Buffer.BlockCopy(
    currentBytes,
    newAndShinyOffset + sizeof(bool),
    legacyBytes,
    newAndShinyOffset,
    cooldownOffset - newAndShinyOffset - sizeof(bool));
  BitConverter.GetBytes(13).CopyTo(legacyBytes, sizeof(int));

  using MemoryStream legacyStream = new(legacyBytes, writable: false);
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(legacyStream);
  Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent restoredState =
    restored.WorldItems.Single().InstanceState;
  if (restoredState.PrefixId != 7 || restoredState.VariantId != 8 ||
      restoredState.Dye != 9 || restoredState.Paint != 10 ||
      !restoredState.IsFavorited || restoredState.IsNewAndShiny)
  {
    throw new InvalidOperationException(
      "A V13 Dome state did not preserve prior world-item metadata while defaulting new-and-shiny.");
  }
}

static void VerifyWorldItemSnapshotWriteValidation()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("Invalid world item", new WorldSeed(925), 400, 300);
  ItemReplicationSnapshot invalidItem = new(
    1,
    ItemStack.Empty,
    new SimulationVector(0.0f, 0.0f),
    IsActive: true,
    Revision: 1,
    new WorldSectionCoordinates(0, 0));
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [invalidItem],
    tickNumber: 0);

  try
  {
    DomeStatePersistenceFormat.Write(new MemoryStream(), snapshot);
    throw new InvalidOperationException(
      "Persistence accepted a world item with a non-canonical active/empty state.");
  }
  catch (InvalidDataException)
  {
  }

  Console.WriteLine("PASS: world-item persistence write validates replication snapshots");
}

static void VerifyVersionFourteenSlimeRainCooldownCompatibility()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("V14 compatibility", new WorldSeed(924), 400, 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    progression: new WorldProgressionState(
      isMeteorScheduled: true,
      slimeRainCooldownTicks: 7));

  using MemoryStream currentStream = new();
  DomeStatePersistenceFormat.Write(currentStream, snapshot);
  byte[] currentBytes = RemoveCurrentWorldSurface(currentStream.ToArray());
  byte[] legacyBytes = new byte[currentBytes.Length - sizeof(int) * 2 - sizeof(bool)];
  Buffer.BlockCopy(currentBytes, 0, legacyBytes, 0, legacyBytes.Length);
  BitConverter.GetBytes(14).CopyTo(legacyBytes, sizeof(int));

  using MemoryStream legacyStream = new(legacyBytes, writable: false);
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(legacyStream);
  if (!restored.Progression.IsMeteorScheduled ||
      restored.Progression.IsSlimeRainCoolingDown ||
      restored.Progression.SlimeRainCooldownTicks != 0)
  {
    throw new InvalidOperationException(
      "A V14 Dome state did not default the newly added Slime Rain cooldown.");
  }
}

static void VerifyLanternNightScheduleRoundTripAndVersionFifteenCompatibility()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("V16 Lantern schedule", new WorldSeed(926), 400, 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    progression: new WorldProgressionState(
      isLanternNight: true,
      isMeteorScheduled: true,
      slimeRainCooldownTicks: 9,
      isNextNightLanternNight: true,
      lanternNightScheduleSequence: 41));

  using MemoryStream currentStream = new();
  DomeStatePersistenceFormat.Write(currentStream, snapshot);
  currentStream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(currentStream);
  if (!restored.Progression.IsLanternNight ||
      !restored.Progression.IsMeteorScheduled ||
       restored.Progression.SlimeRainTimeTicks != 0 ||
      restored.Progression.SlimeRainCooldownTicks != 9 ||
      !restored.Progression.IsNextNightLanternNight ||
      restored.Progression.LanternNightScheduleSequence != 41)
  {
    throw new InvalidOperationException(
      "The current Dome state did not retain the Lantern Night schedule.");
  }

  byte[] currentBytes = RemoveCurrentWorldSurface(currentStream.ToArray());
  byte[] versionFifteenBytes = new byte[currentBytes.Length - sizeof(bool) - sizeof(int)];
  Buffer.BlockCopy(currentBytes, 0, versionFifteenBytes, 0, versionFifteenBytes.Length);
  BitConverter.GetBytes(15).CopyTo(versionFifteenBytes, sizeof(int));
  using MemoryStream versionFifteenStream = new(versionFifteenBytes, writable: false);
  DomeSimulationSnapshot versionFifteen = DomeStatePersistenceFormat.Read(versionFifteenStream);
  if (!versionFifteen.Progression.IsLanternNight ||
      !versionFifteen.Progression.IsMeteorScheduled ||
       versionFifteen.Progression.SlimeRainTimeTicks != 0 ||
      versionFifteen.Progression.SlimeRainCooldownTicks != 9 ||
      versionFifteen.Progression.IsNextNightLanternNight ||
      versionFifteen.Progression.LanternNightScheduleSequence != -1)
  {
    throw new InvalidOperationException(
      "A V15 Dome state did not default the Lantern Night schedule.");
  }
}

static void VerifyLanternNightCooldownRoundTripAndVersionSixteenCompatibility()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("V17 Lantern cooldown", new WorldSeed(927), 400, 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    progression: new WorldProgressionState(
      isNextNightLanternNight: true,
      lanternNightCooldownTicks: 6));

  using MemoryStream currentStream = new();
  DomeStatePersistenceFormat.Write(currentStream, snapshot);
  currentStream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(currentStream);
  if (!restored.Progression.IsNextNightLanternNight ||
      restored.Progression.LanternNightCooldownTicks != 6)
  {
    throw new InvalidOperationException(
      "The current Dome state did not retain the Lantern Night cooldown.");
  }

  byte[] currentBytes = RemoveCurrentWorldSurface(currentStream.ToArray());
  byte[] versionSixteenBytes = new byte[currentBytes.Length - sizeof(int)];
  Buffer.BlockCopy(currentBytes, 0, versionSixteenBytes, 0, versionSixteenBytes.Length);
  BitConverter.GetBytes(16).CopyTo(versionSixteenBytes, sizeof(int));
  using MemoryStream versionSixteenStream = new(versionSixteenBytes, writable: false);
  DomeSimulationSnapshot versionSixteen = DomeStatePersistenceFormat.Read(versionSixteenStream);
  if (!versionSixteen.Progression.IsNextNightLanternNight ||
      versionSixteen.Progression.LanternNightCooldownTicks != 0)
  {
    throw new InvalidOperationException(
      "A V16 Dome state did not default the Lantern Night cooldown.");
  }
}

static void VerifySlimeRainWarningIsTransient()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("Transient warning", new WorldSeed(925), 400, 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    progression: new WorldProgressionState(
      slimeRainTimeTicks: 100,
      slimeRainWarningTicks: 8));

  using MemoryStream stream = new();
  DomeStatePersistenceFormat.Write(stream, snapshot);
  stream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(stream);
  if (restored.Progression.SlimeRainTimeTicks != 100 ||
      restored.Progression.SlimeRainWarningTicks != 0)
  {
    throw new InvalidOperationException(
      "Persistence incorrectly stored the transient Slime Rain warning countdown.");
  }
}

static byte[] RemoveCurrentWorldSurface(byte[] bytes)
{
  int trailerLength = sizeof(uint) + sizeof(bool) + sizeof(float) + sizeof(int) +
    sizeof(byte) + sizeof(bool) + sizeof(int) + sizeof(int) + sizeof(double) + sizeof(bool) * 5 +
    sizeof(int) + sizeof(double);
  if (bytes.Length < trailerLength)
  {
    throw new InvalidOperationException(
      "The current compatibility fixture is shorter than the v19-v21 extension trailer.");
  }

  byte[] result = new byte[bytes.Length - trailerLength];
  Buffer.BlockCopy(bytes, 0, result, 0, result.Length);
  return result;
}

static int LocateWorldItemNewAndShinyField(byte[] bytes)
{
  using MemoryStream stream = new(bytes, writable: false);
  using BinaryReader reader = new(stream);
  _ = reader.ReadInt32();
  _ = reader.ReadInt32();
  int worldPayloadLength = reader.ReadInt32();
  stream.Position += worldPayloadLength;
  _ = reader.ReadInt64();
  int npcCount = reader.ReadInt32();
  if (npcCount != 0 || reader.ReadInt32() != 1)
  {
    throw new InvalidOperationException("The V13 world-item fixture layout changed unexpectedly.");
  }

  _ = reader.ReadInt32();
  _ = reader.ReadUInt16();
  _ = reader.ReadInt32();
  _ = reader.ReadSingle();
  _ = reader.ReadSingle();
  _ = reader.ReadBoolean();
  _ = reader.ReadInt64();
  _ = reader.ReadInt32();
  _ = reader.ReadInt32();
  if (reader.ReadBoolean())
  {
    _ = reader.ReadString();
  }

  _ = reader.ReadUInt16();
  _ = reader.ReadUInt16();
  _ = reader.ReadByte();
  _ = reader.ReadByte();
  _ = reader.ReadBoolean();
  return checked((int)stream.Position);
}

static (int WindOffset, int AfterWindOffset, int LanternOffset, int AfterLanternOffset,
  int MeteorOffset, int AfterMeteorOffset, int CooldownOffset, int AfterCooldownOffset)
  LocateWindAndLanternFields(byte[] bytes)
{
  using MemoryStream stream = new(bytes, writable: false);
  using BinaryReader reader = new(stream);
  _ = reader.ReadInt32();
  _ = reader.ReadInt32();
  int worldPayloadLength = reader.ReadInt32();
  stream.Position += worldPayloadLength;
  _ = reader.ReadInt64();
  for (int index = 0; index < 7; index++)
  {
    _ = reader.ReadInt32();
  }

  _ = reader.ReadInt64();
  _ = reader.ReadInt32();
  _ = reader.ReadBoolean();
  _ = reader.ReadBoolean();
  _ = reader.ReadInt32();
  _ = reader.ReadInt32();
  _ = reader.ReadInt32();
  _ = reader.ReadInt32();
  _ = reader.ReadInt32();
  _ = reader.ReadInt32();
  _ = reader.ReadString();
  _ = reader.ReadInt32();
  _ = reader.ReadInt32();
  _ = reader.ReadBoolean();
  _ = reader.ReadBoolean();
  _ = reader.ReadBoolean();
  _ = reader.ReadInt32();
  _ = reader.ReadSingle();
  int windOffset = checked((int)stream.Position);
  _ = reader.ReadSingle();
  _ = reader.ReadSingle();
  int afterWindOffset = checked((int)stream.Position);
  for (int index = 0; index < 10; index++)
  {
    _ = reader.ReadBoolean();
  }

  int lanternOffset = checked((int)stream.Position);
  _ = reader.ReadBoolean();
  int afterLanternOffset = checked((int)stream.Position);
  _ = reader.ReadInt32();
  _ = reader.ReadInt32();
  _ = reader.ReadInt32();
  int meteorOffset = checked((int)stream.Position);
  _ = reader.ReadBoolean();
  int afterMeteorOffset = checked((int)stream.Position);
  int cooldownOffset = afterMeteorOffset;
  _ = reader.ReadInt32();
  return (
    windOffset,
    afterWindOffset,
    lanternOffset,
    afterLanternOffset,
    meteorOffset,
    afterMeteorOffset,
    cooldownOffset,
    checked((int)stream.Position));
}

static void VerifyDuplicateChestCoordinatesAreRejected()
{
  WorldGrid world = new(width: 400, height: 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(new WorldMetadata("Duplicate chest", new WorldSeed(919), 400, 300)),
    [],
    [],
    tickNumber: 0,
    chests:
    [
      new ChestPersistentState(1, 100, 100, new ItemStack[ChestComponent.SlotCount], 1),
      new ChestPersistentState(2, 100, 100, new ItemStack[ChestComponent.SlotCount], 1)
    ]);

  try
  {
    using DomeSimulation ignored = new(snapshot);
    throw new InvalidOperationException(
      "Persistence recovery accepted duplicate world-chest coordinates.");
  }
  catch (ArgumentException)
  {
  }
}

static void VerifyImportedCompatibilityRoundTrip(string temporaryDirectory)
{
  LegacyWorldDocument document = new(
    319,
    WldFormatVersion.PointerTableV88ToV319,
    new LegacyWorldMetadata("Imported", 31, 200, 150, 10, 20),
    [new LegacyTile(true, 7, LiquidAmount: 42, LiquidKind: 1)],
    [new LegacyChest(3, 4, "Imported chest", [new LegacyChestItem(2, 1, 0)])],
    [new LegacySign(5, 6, "Imported sign")],
    [new LegacyNpc(1, "Guide", 7, 8, false, 0, 0)],
    [new LegacyTileEntity(1, 7, 9, 10, [], isOpaque: false),
      new LegacyTileEntity(2, 250, 11, 12, [0xAA], isOpaque: true)],
    [new LegacySectionDiagnostic(9, 42, "Preserved diagnostic", [0xBB])]);
  CompatibilityWorldSnapshot compatibility = WldToCompatibilityProjection.Project(document);

  DomeSimulationSnapshot imported = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(31),
    strictImport: false);
  if (imported.NpcStates.Count != 1 || imported.NpcStates[0].GivenName != "Guide")
  {
    throw new InvalidOperationException("Compatibility NPC given name was dropped during projection.");
  }
  if (imported.Chests.Count != 1 || imported.Signs.Count != 1 ||
      imported.TileEntities.Count != 2 || imported.OpaqueCompatibilityRecords.Count != 2)
  {
    throw new InvalidOperationException("Compatibility state was not projected into the Dome snapshot.");
  }

  using MemoryStream state = new();
  DomeStatePersistenceFormat.Write(state, imported);
  state.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(state);
  using DomeSimulation simulation = new(restored);
  IReadOnlyList<Terraria.Dome.Simulation.WorldObjects.ChestSnapshot> chests =
    simulation.CreateChestSnapshots();
  if (chests.Count != 1 || simulation.CreateSignSnapshots().Count != 1 ||
      simulation.CreateTileEntitySnapshots().Count != 2 ||
      restored.OpaqueCompatibilityRecords.Count != 2 ||
      restored.Chests[0].Name != "Imported chest" ||
      simulation.CreateChestPersistentSnapshots().Single().Name != "Imported chest" ||
      simulation.GetChest(chests[0].ChestId).GetSlot(0) !=
      new Terraria.Dome.Simulation.Items.ItemStack(1, 2))
  {
    throw new InvalidOperationException("Imported object state did not survive persistence.");
  }

  try
  {
    _ = CompatibilityToDomeProjection.Project(compatibility, new WorldSeed(31), strictImport: true);
    throw new InvalidOperationException("Strict compatibility projection accepted unsupported state.");
  }
  catch (InvalidOperationException exception) when (
    exception.Message.Contains("unsupported", StringComparison.OrdinalIgnoreCase))
  {
  }
}

static PlayerPersistentState CreatePersistentState()
{
  PlayerPersistentItem[] items = new PlayerPersistentItem[PlayerPersistentState.ItemSlotCount];
  for (int slotId = 0; slotId < items.Length; slotId++)
  {
    items[slotId] = new PlayerPersistentItem(slotId, 0, 0, 0, false, false);
  }

  items[0] = new PlayerPersistentItem(
    0,
    7,
    1,
    1,
    true,
    false,
    VariantId: 6,
    Dye: 7,
    Paint: 8,
    NameOverride: "Persistent item");
  items[10] = new PlayerPersistentItem(10, 23, 2, 999, false, true);
  items[11] = new PlayerPersistentItem(11, 1, 7, 4, false, false);
  return new PlayerPersistentState(
    "2eecdeea-c45e-456f-8244-75ec32da6172",
    new PlayerPersistentProfile(
      "Persistent",
      HairColor: new PlayerPersistentColor(1, 2, 3),
      SkinColor: new PlayerPersistentColor(4, 5, 6)),
    120,
    200,
    80,
    200,
    [new PlayerPersistentBuff(190)],
    selectedLoadout: 1,
    accessoryVisibility: 3,
    items);
}

static void VerifyWorldTimeRatePersistenceRoundTrip()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new(
    "Time-rate persistence",
    new WorldSeed(32),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    progression: new WorldProgressionState(invasionType: 1, invasionSize: 5, invasionX: 205),
    worldTimeRate: new WorldTimeRateSnapshot(5));
  using MemoryStream currentStream = new();
  DomeStatePersistenceFormat.Write(currentStream, snapshot);
  byte[] currentBytes = currentStream.ToArray();
  currentStream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(currentStream);
  if (!restored.WorldTimeRate.IsAvailable || restored.WorldTimeRate.Rate != 5)
  {
    throw new InvalidOperationException("V27 persistence did not retain the world time rate.");
  }

  using DomeSimulation restarted = new(restored);
  restarted.Tick(new SimulationInputBatch());
  if (restarted.CreateWorldProgressionSnapshot().InvasionX != 200 ||
      restarted.CreateWorldTimeRateSnapshot().Rate != 5)
  {
    throw new InvalidOperationException("The restored world time rate did not drive invasion travel.");
  }

  byte[] versionTwentySix = currentBytes[..^(sizeof(bool) * 2 + sizeof(int) + sizeof(double))];
  BitConverter.GetBytes(26).CopyTo(versionTwentySix, sizeof(int));
  using MemoryStream legacyStream = new(versionTwentySix, writable: false);
  DomeSimulationSnapshot legacy = DomeStatePersistenceFormat.Read(legacyStream);
  if (legacy.WorldTimeRate.IsAvailable)
  {
    throw new InvalidOperationException("A V26 state fabricated a contemporary world time rate.");
  }
}

static void VerifyDeterministicBaseGeneration()
{
  WorldGenerationRequest request = new(
    new WorldMetadata("Generated", new WorldSeed(771), 400, 300),
    spawnX: 200,
    surfaceY: 100);
  WorldGenerationPipeline generator = new();
  WorldGrid firstWorld = generator.Generate(request);
  WorldGrid secondWorld = generator.Generate(request);

  ulong firstHash = CalculateWorldHash(firstWorld);
  if (firstHash != CalculateWorldHash(secondWorld))
  {
    throw new InvalidOperationException(
      "Equivalent generation requests produced different worlds.");
  }

  WorldGenerationRequest alternateRequest = new(
    new WorldMetadata("Generated", new WorldSeed(772), 400, 300),
    request.SpawnX,
    request.SurfaceY,
    request.SeedVariant,
    request.RandomStreamVersion,
    request.GenerationId);
  WorldGrid alternateSeed = generator.Generate(alternateRequest);
  if (firstHash == CalculateWorldHash(alternateSeed))
  {
    throw new InvalidOperationException("The base generator ignored the requested seed.");
  }

  bool hasBottomGround = Enumerable.Range(0, firstWorld.Width)
    .Any(x => firstWorld.GetTile(x, firstWorld.Height - 1).IsActive);
  if (!hasBottomGround ||
      firstWorld.GetTile(request.SpawnX, request.SurfaceY).IsActive ||
      firstWorld.GetTile(request.SpawnX, request.SurfaceY + 6).IsActive)
  {
    throw new InvalidOperationException("Base generation did not create ground and clear spawn.");
  }
}

static void VerifyTerrainBaseClearsConfiguredSpawn()
{
  WorldGenerationRequest request = new(
    new WorldMetadata("Terrain spawn", new WorldSeed(770), 400, 300),
    spawnX: 200,
    surfaceY: 100);
  WorldGrid world = new(request.Metadata.Width, request.Metadata.Height);
  WorldGenerationStateComponent state = new(1);
  List<TileChangeCommand> commands = new();
  new TerrainBaseSystem().AppendLegacyCommands(
    world.CreateSnapshot(request.Metadata),
    request,
    new TerrainProfileComponent(request.SurfaceY, request.RockLayerY, request.Metadata.Height - 1),
    ref state,
    commands);
  if (!new TileChangeCommitSystem().TryCommit(
        world,
        commands,
        out TileChangeCommitResult commitResult) ||
      commitResult.AppliedCount != commands.Count)
  {
    throw new InvalidOperationException("The standalone Terrain base stage could not be committed.");
  }

  for (int y = request.SurfaceY; y <= request.SurfaceY + 7; y++)
  {
    for (int x = request.SpawnX - 4; x <= request.SpawnX + 4; x++)
    {
      if (world.GetTile(x, y).IsActive)
      {
        throw new InvalidOperationException(
          $"Terrain base did not clear the configured spawn tile ({x}, {y}).");
      }
    }
  }

  Console.WriteLine("PASS: Terrain base owns deterministic spawn clearing");
}

static ulong CalculateWorldHash(WorldGrid world)
{
  const ulong offsetBasis = 14695981039346656037UL;
  const ulong prime = 1099511628211UL;
  ulong hash = offsetBasis;
  for (int y = 0; y < world.Height; y++)
  {
    for (int x = 0; x < world.Width; x++)
    {
      WorldTile tile = world.GetTile(x, y);
      hash ^= tile.IsActive ? 1UL : 0UL;
      hash *= prime;
      hash ^= tile.Type;
      hash *= prime;
    }
  }

  return hash;
}

static void VerifyInvalidPayloadDoesNotProduceCandidate(string temporaryDirectory)
{
  string path = Path.Combine(temporaryDirectory, "invalid.dome");
  File.WriteAllBytes(path, [0x44, 0x4F, 0x4D, 0x45, 0x01]);

  WorldSaveCoordinator coordinator = new();
  WorldRecoveryResult recovery = coordinator.TryLoad(path);
  if (recovery.IsSuccess || recovery.Snapshot is not null)
  {
    throw new InvalidOperationException(
      "Truncated persistence data produced a mutable world candidate.");
  }
}

static void VerifyUnsupportedVersionDoesNotProduceCandidate(string temporaryDirectory)
{
  string path = Path.Combine(temporaryDirectory, "unsupported-version.dome");
  File.WriteAllBytes(path, [0x44, 0x4F, 0x4D, 0x57, 0x02, 0x00, 0x00, 0x00]);

  WorldRecoveryResult recovery = new WorldSaveCoordinator().TryLoad(path);
  if (recovery.IsSuccess || recovery.Snapshot is not null)
  {
    throw new InvalidOperationException(
      "An unsupported format version produced a world candidate.");
  }
}

static void VerifyInvalidDimensionsDoNotProduceCandidate(string temporaryDirectory)
{
  string path = Path.Combine(temporaryDirectory, "invalid-dimensions.dome");
  File.WriteAllBytes(path,
  [
    0x44, 0x4F, 0x4D, 0x57,
    0x01, 0x00, 0x00, 0x00,
    0xC8, 0x00, 0x00, 0x00,
    0x2C, 0x01, 0x00, 0x00
  ]);

  WorldRecoveryResult recovery = new WorldSaveCoordinator().TryLoad(path);
  if (recovery.IsSuccess || recovery.Snapshot is not null)
  {
    throw new InvalidOperationException("Invalid world dimensions produced a world candidate.");
  }
}

static void VerifyFractionalWorldClockPersistenceRoundTrip()
{
  WorldGrid world = new(width: 400, height: 300);
  WorldMetadata metadata = new("Fractional clock", new WorldSeed(28), 400, 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    worldClock: new WorldClockSnapshot(0, 1.5d, true, false, 1));
  using MemoryStream stream = new();
  DomeStatePersistenceFormat.Write(stream, snapshot);
  stream.Position = 0;
  DomeSimulationSnapshot restored = DomeStatePersistenceFormat.Read(stream);
  if (restored.Clock.TimeOfDay != 1.5d)
  {
    throw new InvalidOperationException(
      "V28 persistence did not retain fractional authoritative world time.");
  }
}

static void VerifyExistingFileIsReplacedAtomically(string temporaryDirectory)
{
  string path = Path.Combine(temporaryDirectory, "replace-existing.dome");
  File.WriteAllBytes(path, [1, 2, 3]);
  WorldGrid world = new(width: 400, height: 300);
  world.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 12));
  WorldGridSnapshot snapshot = world.CreateSnapshot(
    new WorldMetadata("Replace existing", new WorldSeed(2), 400, 300));

  new WorldSaveCoordinator().Save(path, snapshot);
  WorldRecoveryResult recovery = new WorldSaveCoordinator().TryLoad(path);
  if (!recovery.IsSuccess || recovery.Snapshot?.GetTile(10, 10) !=
      new WorldTile(IsActive: true, Type: 12) ||
      Directory.GetFiles(temporaryDirectory, ".replace-existing.dome.*.tmp").Length != 0)
  {
    throw new InvalidOperationException("The existing world file was not atomically replaced.");
  }
}

static void VerifyWorldRollingBackups(string temporaryDirectory)
{
  string path = Path.Combine(temporaryDirectory, "rolling-backups.dome");
  WorldSaveCoordinator coordinator = new();
  for (int index = 0; index < 3; index++)
  {
    WorldGrid world = new(width: 400, height: 300);
    world.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: (ushort)(20 + index)));
    coordinator.Save(
      path,
      world.CreateSnapshot(new WorldMetadata("Rolling backups", new WorldSeed(2), 400, 300)),
      rollingBackupsCountToKeep: 2);
  }

  WorldRecoveryResult oldest = coordinator.TryLoad(path + ".bak2");
  WorldRecoveryResult latestBackup = coordinator.TryLoad(path + ".bak");
  if (!oldest.IsSuccess || oldest.Snapshot?.GetTile(10, 10).Type != 20 ||
      !latestBackup.IsSuccess || latestBackup.Snapshot?.GetTile(10, 10).Type != 21 ||
      File.Exists(path + ".bak3"))
  {
    throw new InvalidOperationException("World rolling backups did not retain bounded generations.");
  }

  string noBackupPath = Path.Combine(temporaryDirectory, "no-backups.dome");
  WorldGrid noBackupWorld = new(width: 400, height: 300);
  WorldGridSnapshot noBackupSnapshot = noBackupWorld.CreateSnapshot(
    new WorldMetadata("No backups", new WorldSeed(3), 400, 300));
  coordinator.Save(noBackupPath, noBackupSnapshot, rollingBackupsCountToKeep: 0);
  coordinator.Save(noBackupPath, noBackupSnapshot, rollingBackupsCountToKeep: 0);
  if (File.Exists(noBackupPath + ".bak"))
  {
    throw new InvalidOperationException("A zero-backup policy created a backup file.");
  }

  try
  {
    coordinator.Save(noBackupPath, noBackupSnapshot, rollingBackupsCountToKeep: -1);
    throw new InvalidOperationException("A negative backup retention count was accepted.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }
}

static void VerifyNpcProjectileCursorServerLifecycle(string temporaryDirectory)
{
  string path = Path.Combine(temporaryDirectory, "npc-projectile-cursors.bin");
  using (DomeServer first = new(new WorldGrid(4200, 1200)))
  {
    first.ConfigureNpcProjectileCursorPersistence(path);
    first.Dispose();
  }

  if (!File.Exists(path))
  {
    throw new InvalidOperationException(
      "DomeServer did not persist the configured NPC projectile cursor sidecar.");
  }

  using (DomeServer second = new(new WorldGrid(4200, 1200)))
  {
    second.ConfigureNpcProjectileCursorPersistence(path);
    second.Dispose();
  }

  IReadOnlyList<NpcProjectileCursorAccountState> restored =
    new NpcProjectileCursorSaveCoordinator().Load(path);
  if (restored.Count != 0)
  {
    throw new InvalidOperationException(
      "An empty server lifecycle unexpectedly persisted a cursor account.");
  }
}

static void VerifyNpcProjectileCursorCrashRecovery(string temporaryDirectory)
{
  string path = Path.Combine(temporaryDirectory, "npc-projectile-recovery.bin");
  NpcProjectileCursorAccountState state = new(
    "22222222-2222-2222-2222-222222222222",
    [new NpcProjectileCursorEntry(9, 4)]);
  NpcProjectileCursorSaveCoordinator coordinator = new();
  coordinator.Save(path, [state]);
  File.Copy(path, path + ".tmp", overwrite: true);
  File.WriteAllBytes(path, [0x01, 0x02, 0x03]);
  IReadOnlyList<NpcProjectileCursorAccountState> recovered = coordinator.Load(path);
  if (recovered.Count != 1 || recovered[0].Entries[0].Revision != 4 || File.Exists(path + ".tmp"))
  {
    throw new InvalidOperationException(
      "NPC projectile cursor coordinator did not promote a valid recovery candidate.");
  }

  File.WriteAllBytes(path, [0x01, 0x02, 0x03]);
  File.WriteAllBytes(path + ".tmp", [0x04, 0x05]);
  try
  {
    _ = coordinator.Load(path);
    throw new InvalidOperationException(
      "NPC projectile cursor coordinator accepted two invalid recovery candidates.");
  }
  catch (InvalidDataException)
  {
  }
}

static void VerifyNpcProjectileCursorCompaction()
{
  NpcProjectileCursorSaveCoordinator coordinator = new();
  IReadOnlyList<NpcProjectileCursorAccountState> compacted = coordinator.Compact(
    [new NpcProjectileCursorAccountState(
      "33333333-3333-3333-3333-333333333333",
      [new NpcProjectileCursorEntry(1, 1), new NpcProjectileCursorEntry(2, 2)])],
    [new NpcProjectileReplicationSnapshot(
      1,
      3,
      new NpcHandle(7),
      new SimulationVector(1.0f, 1.0f),
      default,
      10,
      0,
      false,
      2,
      new WorldSectionCoordinates(0, 0),
      1,
      TombstoneReason: ProjectileTombstoneReason.Expired,
      TombstoneRetainedUntilTick: 20,
      DefinitionKnockback: 1.0f,
      DefinitionOriginalDamage: 10)],
    currentTick: 10);
  if (compacted.Count != 1 || compacted[0].Entries.Count != 1 ||
      compacted[0].Entries[0].ReplicationId != 1)
  {
    throw new InvalidOperationException(
      "NPC projectile cursor compaction did not preserve only retained state.");
  }
}

static void VerifyNpcProjectileCursorRestartReplay(string temporaryDirectory)
{
  string path = Path.Combine(temporaryDirectory, "npc-projectile-replay.bin");
  NpcProjectileCursorSaveCoordinator coordinator = new();
  string accountUuid = "44444444-4444-4444-4444-444444444444";
  NpcProjectileCursorAccountState initial = new(
    accountUuid,
    [new NpcProjectileCursorEntry(10, 7), new NpcProjectileCursorEntry(20, 9)]);
  coordinator.Save(path, [initial]);

  IReadOnlyList<NpcProjectileCursorAccountState> loaded = coordinator.Load(path);
  IReadOnlyList<NpcProjectileCursorAccountState> replayed = coordinator.Compact(
    loaded,
    [new NpcProjectileReplicationSnapshot(
      10,
      3,
      new NpcHandle(8),
      new SimulationVector(1.0f, 1.0f),
      default,
      10,
      0,
      false,
      8,
      new WorldSectionCoordinates(0, 0),
      10,
      TombstoneReason: ProjectileTombstoneReason.Expired,
      TombstoneRetainedUntilTick: 30,
      DefinitionKnockback: 1.0f,
      DefinitionOriginalDamage: 10)],
    currentTick: 20);
  coordinator.Save(path, replayed);

  File.Copy(path, path + ".tmp", overwrite: true);
  File.WriteAllBytes(path, [0x0A, 0x0B]);
  IReadOnlyList<NpcProjectileCursorAccountState> recovered = coordinator.Load(path);
  if (recovered.Count != 1 || recovered[0].AccountUuid != accountUuid ||
      recovered[0].Entries.Count != 1 || recovered[0].Entries[0].ReplicationId != 10 ||
      recovered[0].Entries[0].Revision != 7 || File.Exists(path + ".tmp"))
  {
    throw new InvalidOperationException(
      "NPC projectile cursor restart replay did not preserve the compacted active account state.");
  }
}

static void VerifyFailedReplacementPreservesExistingTarget(string temporaryDirectory)
{
  string targetDirectory = Path.Combine(temporaryDirectory, "existing-target");
  Directory.CreateDirectory(targetDirectory);
  WorldGrid world = new(width: 400, height: 300);
  WorldGridSnapshot snapshot = world.CreateSnapshot(
    new WorldMetadata("Atomic replacement", new WorldSeed(1), 400, 300));

  try
  {
    new WorldSaveCoordinator().Save(targetDirectory, snapshot);
    throw new InvalidOperationException("Persistence unexpectedly replaced a directory target.");
  }
  catch (Exception exception) when (exception is IOException ||
                                   exception is UnauthorizedAccessException)
  {
  }

  if (!Directory.Exists(targetDirectory))
  {
    throw new InvalidOperationException(
      "A failed atomic replacement modified the existing target.");
  }
}
