using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.WorldObjects.Placement;

const short placementX = 2104;
const short placementY = 1;
const short worldWidth = 4200;
const short worldHeight = 1200;
const short carrierProjectileType = 43;
const short signObjectType = 85;
const int placementStyle = 17;
const long placementSequence = 174001;
const string placementText = "loopback-placement";

VerifyPlacementFixtureStateTransitions();
VerifyAutomaticStyle17Placement();
VerifyAutomaticStyle17PlacementRetry();

WorldGrid world = new(worldWidth, worldHeight);
for (int x = 0; x < worldWidth; x++)
{
  _ = world.TrySetTile(x, 0, new WorldTile(true, 1));
  _ = world.TrySetTile(x, placementY + 2, new WorldTile(true, 1));
}

WorldMetadata metadata = new(
  "Placement Loopback Verification",
  new WorldSeed(1456),
  worldWidth,
  worldHeight);
DomeSimulationSnapshot snapshot;
using (DomeSimulation setup = new(world))
{
  snapshot = setup.CreatePersistenceSnapshot(metadata);
}

using DomeServer server = new(snapshot);
server.Start();
using TcpClient actorClient = new();
using TcpClient observerClient = new();
await actorClient.ConnectAsync("127.0.0.1", server.Port);
await observerClient.ConnectAsync("127.0.0.1", server.Port);
NetworkStream actor = actorClient.GetStream();
NetworkStream observer = observerClient.GetStream();
byte actorSlot = await ActivateAsync(actor, placementX, placementY, "PlacementActor");
byte observerSlot = await ActivateAsync(observer, placementX, placementY, "PlacementObserver");
await observer.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(
  new PlayerControlIntent(
    observerSlot,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
  placementX * TerrariaWorldCoordinates.PixelsPerTile,
  placementY * TerrariaWorldCoordinates.PixelsPerTile));
await Task.Delay(TimeSpan.FromMilliseconds(100));

if (!await server.PrepareVerificationSignPlacementAsync(
      actorSlot,
      placementSequence,
      placementX,
      placementY))
{
  throw new InvalidOperationException("The placement projectile was not prepared.");
}

using CancellationTokenSource activeTimeout = new(TimeSpan.FromSeconds(5));
bool activeProjectileSeen = false;
try
{
  while (!activeTimeout.IsCancellationRequested && !activeProjectileSeen)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(observer, activeTimeout.Token));
    if (frame.MessageId != TerrariaMessageId.SyncProjectile)
    {
      continue;
    }

    ProjectileSyncPacket projectile = TerrariaPacketCodec.DecodeProjectileSync(
      TerrariaFrameCodec.Encode(frame));
    activeProjectileSeen = projectile.ProjectileType == carrierProjectileType;
  }
}
catch (OperationCanceledException)
{
}

if (!activeProjectileSeen)
{
  throw new InvalidOperationException("The prepared projectile was not observed on TCP.");
}

WorldObjectPlacementResult placement = await server.QueueVerificationSignPlacementAsync(
  actorSlot,
  placementSequence,
  placementX,
  placementY,
  placementStyle,
  direction: 1,
  placementText);
if (!placement.Committed)
{
  throw new InvalidOperationException(
    $"The placement commit was rejected: {placement.FailureCode}.");
}

int frameOrder = 0;
int objectOrder = -1;
int signOrder = -1;
int killOrder = -1;
using CancellationTokenSource placementTimeout = new(TimeSpan.FromSeconds(5));
try
{
  while (!placementTimeout.IsCancellationRequested && killOrder < 0)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(observer, placementTimeout.Token));
    frameOrder++;
    if (frame.MessageId == TerrariaMessageId.ObjectPlacement)
    {
      ObjectPlacementPacket objectPlacement = TerrariaPacketCodec.DecodeObjectPlacement(
        TerrariaFrameCodec.Encode(frame));
      if (objectPlacement.TileX == placementX && objectPlacement.TileY == placementY &&
          objectPlacement.ObjectType == signObjectType && objectPlacement.Style == placementStyle &&
          objectPlacement.DirectionRight)
      {
        objectOrder = frameOrder;
      }
    }
    else if (frame.MessageId == TerrariaMessageId.OpenSignResponse)
    {
      SignUpdateIntent sign = TerrariaPacketCodec.DecodeSignUpdate(
        TerrariaFrameCodec.Encode(frame));
      if (sign.TileX == placementX && sign.TileY == placementY &&
          sign.Text == placementText && sign.PlayerSlot == observerSlot)
      {
        signOrder = frameOrder;
      }
    }
    else if (frame.MessageId == TerrariaMessageId.KillProjectile &&
             objectOrder >= 0 && signOrder >= 0)
    {
      ClientProjectileTermination termination =
        TerrariaPacketCodec.DecodeClientProjectileTermination(TerrariaFrameCodec.Encode(frame));
      if (termination.Owner == actorSlot)
      {
        killOrder = frameOrder;
      }
    }
  }
}
catch (OperationCanceledException)
{
}

bool footprintCommitted = true;
for (int y = placementY; y < placementY + 2; y++)
{
  for (int x = placementX; x < placementX + 2; x++)
  {
    WorldTile tile = server.World.GetTile(x, y);
    footprintCommitted &= tile.IsActive && tile.Type == signObjectType;
  }
}

IReadOnlyList<SignSnapshot> signs = server.CreateSignSnapshots();
if (objectOrder < 0 || signOrder < 0 || killOrder < 0 ||
    objectOrder >= signOrder || signOrder >= killOrder || !footprintCommitted ||
    signs.Count != 1 || signs[0].TileX != placementX || signs[0].TileY != placementY ||
    signs[0].Text != placementText)
{
  throw new InvalidOperationException(
    $"Placement frames were not ordered object/sign/tombstone: " +
    $"Object={objectOrder}, Sign={signOrder}, Kill={killOrder}, " +
    $"Footprint={footprintCommitted}, Signs={signs.Count}, " +
    $"Fault={server.SimulationFault?.GetType().Name}:{server.SimulationFault?.Message}.");
}

Console.WriteLine(
  $"PASS: TCP placement delivered object/sign/tombstone order " +
  $"({objectOrder} < {signOrder} < {killOrder})");

static async Task<byte> ActivateAsync(NetworkStream stream, short spawnX, short spawnY, string name)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlot = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(stream, CancellationToken.None));
  byte playerSlot = userSlot.Payload.Span[0];
  await stream.WriteAsync(TerrariaPacketCodec.Encode(CreateProfile(playerSlot, name)));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    $"00000000-0000-0000-0000-{playerSlot:x12}")));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(spawnX, spawnY, 0)));
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  for (int index = 0; index < 15; index++)
  {
    _ = await ReadFrameAsync(stream, CancellationToken.None);
  }

  _ = await ReadFrameAsync(stream, CancellationToken.None);
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    playerSlot, spawnX, spawnY, 0, 0, 0, 0, 0)));
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  return playerSlot;
}

static PlayerProfilePacket CreateProfile(byte playerSlot, string name)
{
  TerrariaColor color = new(0, 0, 0);
  return new PlayerProfilePacket(
    playerSlot,
    0,
    0,
    0.0f,
    0,
    name,
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
    0);
}

static void VerifyPlacementFixtureStateTransitions()
{
  const int originX = 2104;
  const int originY = 1;
  const long sequence = 174002;
  WorldGrid world = new(worldWidth, worldHeight);
  using DomeSimulation simulation = new(world);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(originX, originY));

  if (!simulation.PrepareProjectileSignPlacementFixture(
        owner,
        sequence,
        originX,
        originY,
        out WorldObjectPlacementFailureCode prepareFailure) ||
      prepareFailure != WorldObjectPlacementFailureCode.None)
  {
      throw new InvalidOperationException(
      $"The retry fixture could not prepare its projectile: {prepareFailure}.");
  }

  if (!world.TrySetTile(originX, originY + 2, new WorldTile(true, 1)) ||
      !world.TrySetTile(originX + 1, originY + 2, new WorldTile(true, 1)))
  {
    throw new InvalidOperationException("The retry fixture could not create bottom support.");
  }

  if (!world.TrySetTile(originX + 1, originY, new WorldTile(true, 1)))
  {
    throw new InvalidOperationException("The retry fixture could not create its conflict tile.");
  }

  WorldObjectPlacementResult rejected = simulation.TryCommitProjectileSignPlacementFixture(
    owner,
    sequence,
    originX,
    originY,
    style: 17,
    direction: 1,
    signText: "retry");
  IReadOnlyList<ProjectileReplicationSnapshot> rejectedProjectiles =
    simulation.CreateProjectileReplicationSnapshots();
  if (rejected.Committed || rejected.FailureCode != WorldObjectPlacementFailureCode.OccupiedTile ||
      simulation.CreateSignSnapshots().Count != 0 ||
      rejectedProjectiles.Count != 1 || !rejectedProjectiles[0].IsActive)
  {
    throw new InvalidOperationException(
      "A rejected placement did not preserve its active projectile without world mutation.");
  }

  if (!world.TrySetTile(originX + 1, originY, new WorldTile()))
  {
    throw new InvalidOperationException("The retry fixture could not clear its conflict tile.");
  }

  WorldObjectPlacementResult committed = simulation.TryCommitProjectileSignPlacementFixture(
    owner,
    sequence,
    originX,
    originY,
    style: 17,
    direction: 1,
    signText: "retry");
  IReadOnlyList<ProjectileReplicationSnapshot> committedProjectiles =
    simulation.CreateProjectileReplicationSnapshots();
  if (!committed.Committed || simulation.CreateSignSnapshots().Count != 1 ||
      committedProjectiles.Count != 1 || !committedProjectiles[0].IsActive ||
      simulation.CreateWorldObjectPlacementEvents().Count != 1)
  {
    throw new InvalidOperationException(
      "A placement retry did not commit exactly once for its prepared projectile.");
  }

  WorldObjectPlacementResult duplicate = simulation.TryCommitProjectileSignPlacementFixture(
    owner,
    sequence,
    originX,
    originY,
    style: 17,
    direction: 1,
    signText: "retry");
  IReadOnlyList<ProjectileReplicationSnapshot> duplicateProjectiles =
    simulation.CreateProjectileReplicationSnapshots();
  if (duplicate.Committed ||
      duplicate.FailureCode != WorldObjectPlacementFailureCode.DuplicateSequence ||
      simulation.CreateSignSnapshots().Count != 1 ||
      duplicateProjectiles.Count != 1 || !duplicateProjectiles[0].IsActive ||
      simulation.CreateWorldObjectPlacementEvents().Count != 1)
  {
    throw new InvalidOperationException(
      "A duplicate placement sequence created additional projectile or sign state.");
  }
}

static void VerifyAutomaticStyle17Placement()
{
  const int projectileX = 10;
  const int projectileY = 10;
  const int supportY = 30;
  const int maximumTicks = 60;
  const string signText = "automatic-style17";

  WorldGrid world = new(400, 300);
  for (int x = 0; x < 200; x++)
  {
    if (!world.TrySetTile(x, supportY, new WorldTile(true, 1)))
    {
      throw new InvalidOperationException(
        "The automatic placement fixture could not create its floor.");
    }
  }

  using DomeSimulation simulation = new(world);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(projectileX, projectileY));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    projectileX,
    projectileY,
    Facing: 1,
    Damage: 0,
    LifetimeTicks: 3600,
    ProjectileType: 43,
    MiscText: signText));

  for (int tick = 0; tick < maximumTicks && simulation.CreateSignSnapshots().Count == 0; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  IReadOnlyList<SignSnapshot> signs = simulation.CreateSignSnapshots();
  IReadOnlyList<WorldObjectPlacementCommittedEvent> events =
    simulation.CreateWorldObjectPlacementEvents();
  IReadOnlyList<ProjectileReplicationSnapshot> projectiles =
    simulation.CreateProjectileReplicationSnapshots();
  if (signs.Count != 1 || events.Count != 1 || projectiles.Count != 1 ||
      projectiles[0].IsActive ||
      projectiles[0].TombstoneReason != ProjectileTombstoneReason.Expired)
  {
    throw new InvalidOperationException(
      $"Style-17 collision did not commit one sign and linked tombstone: " +
      $"signs={signs.Count}, events={events.Count}, projectiles={projectiles.Count}, " +
      $"active={projectiles.Count == 1 && projectiles[0].IsActive}, " +
      $"reason={(projectiles.Count == 1
        ? projectiles[0].TombstoneReason
        : ProjectileTombstoneReason.None)}.");
  }

  SignSnapshot sign = signs[0];
  WorldObjectPlacementCommittedEvent placement = events[0];
  if (sign.TileY != supportY - 2 || sign.Text != signText ||
      placement.Request.ObjectType != WorldObjectPlacementRequest.SignObjectType ||
      placement.Request.Style != 0 || placement.Request.Direction != 1 ||
      placement.Request.SignText != signText || placement.Footprint.Count != 4 ||
      placement.ProjectileTombstoneReason != ProjectileTombstoneReason.Expired ||
      placement.SectionVersion <= 0 ||
      !placement.SectionVersions.TryGetValue(sign.Section, out long sectionVersion) ||
      sectionVersion != placement.SectionVersion ||
      placement.ProjectileIdentity != projectiles[0].Identity ||
      placement.ProjectileUuid != projectiles[0].ProjectileUuid)
  {
    throw new InvalidOperationException(
      $"Style-17 automatic placement did not preserve its derived request: " +
      $"origin=({sign.TileX},{sign.TileY}), style={placement.Request.Style}, " +
      $"direction={placement.Request.Direction}, footprint={placement.Footprint.Count}, " +
      $"sectionVersion={placement.SectionVersion}, projectileIdentity=" +
      $"{placement.ProjectileIdentity}.");
  }

  for (int row = 0; row < 2; row++)
  {
    for (int column = 0; column < 2; column++)
    {
      WorldTile tile = world.GetTile(sign.TileX + column, sign.TileY + row);
      if (!tile.IsActive || tile.Type != WorldObjectPlacementRequest.SignObjectType ||
          tile.FrameX != column * 18 || tile.FrameY != row * 18)
      {
        throw new InvalidOperationException(
          $"Style-17 automatic placement wrote an invalid footprint at " +
          $"({sign.TileX + column},{sign.TileY + row}).");
      }
    }
  }

  Console.WriteLine("PASS: style-17 collision enqueues and commits automatic sign placement");
}

static void VerifyAutomaticStyle17PlacementRetry()
{
  const int projectileX = 10;
  const int projectileY = 10;
  const int supportY = 30;
  const int expectedImpactOriginX = 65;

  WorldGrid world = new(400, 300);
  for (int x = 0; x < 200; x++)
  {
    if (!world.TrySetTile(x, supportY, new WorldTile(true, 1)))
    {
    throw new InvalidOperationException(
      "The automatic retry fixture could not create its floor.");
    }
  }

  if (!world.TrySetTile(expectedImpactOriginX, supportY - 2, new WorldTile(true, 1)))
  {
    throw new InvalidOperationException(
      "The automatic retry fixture could not create its conflict.");
  }

  using DomeSimulation simulation = new(world);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(projectileX, projectileY));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    projectileX,
    projectileY,
    Facing: 1,
    Damage: 0,
    LifetimeTicks: 3600,
    ProjectileType: 43,
    MiscText: "automatic-retry"));

  for (int tick = 0; tick < 100 && simulation.CreateWorldObjectPlacementEvents().Count == 0;
       tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  IReadOnlyList<ProjectileReplicationSnapshot> rejectedProjectiles =
    simulation.CreateProjectileReplicationSnapshots();
  if (simulation.CreateWorldObjectPlacementEvents().Count != 0 ||
      rejectedProjectiles.Count != 1 || !rejectedProjectiles[0].IsActive ||
      simulation.CreateSignSnapshots().Count != 0)
  {
    throw new InvalidOperationException(
      "An occupied style-17 footprint did not preserve the active projectile for retry.");
  }

  if (!world.TrySetTile(expectedImpactOriginX, supportY - 2, new WorldTile()))
  {
    throw new InvalidOperationException(
      "The automatic retry fixture could not clear its conflict.");
  }

  for (int tick = 0; tick < 3 && simulation.CreateSignSnapshots().Count == 0; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  IReadOnlyList<SignSnapshot> signs = simulation.CreateSignSnapshots();
  IReadOnlyList<WorldObjectPlacementCommittedEvent> events =
    simulation.CreateWorldObjectPlacementEvents();
  IReadOnlyList<ProjectileReplicationSnapshot> committedProjectiles =
    simulation.CreateProjectileReplicationSnapshots();
  bool committedProjectileActive = committedProjectiles.Count == 1 &&
    committedProjectiles[0].IsActive;
  if (signs.Count != 1 || events.Count != 1 || committedProjectiles.Count != 1 ||
      committedProjectiles[0].IsActive ||
      committedProjectiles[0].TombstoneReason != ProjectileTombstoneReason.Expired)
  {
    throw new InvalidOperationException(
      $"A cleared style-17 footprint did not commit on retry: signs={signs.Count}, " +
      $"events={events.Count}, active={committedProjectileActive}, " +
      $"reason={(committedProjectiles.Count == 1
        ? committedProjectiles[0].TombstoneReason
        : ProjectileTombstoneReason.None)}.");
  }

  Console.WriteLine(
    "PASS: style-17 rejected placement refreshes its version and retries atomically");
}

static async Task<byte[]> ReadFrameAsync(NetworkStream stream, CancellationToken cancellationToken)
{
  byte[] prefix = new byte[2];
  await stream.ReadExactlyAsync(prefix, cancellationToken);
  int frameLength = prefix[0] | prefix[1] << 8;
  byte[] frame = new byte[frameLength];
  prefix.CopyTo(frame, 0);
  await stream.ReadExactlyAsync(frame.AsMemory(2), cancellationToken);
  return frame;
}
