using System.Collections.Immutable;
using System.Numerics;
using EntityEcs.Components;
using Terraria.SpatialSimulation;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static SpatialGeometrySnapshot Geometry(
  long revision,
  float x,
  float y,
  float width,
  float height)
{
  return new SpatialGeometrySnapshot(
    revision,
    new Vector2(x, y),
    new Vector2(width, height));
}

Assert(
  SpatialCollisionQuery.CheckAabb(
    Geometry(1, 0, 0, 10, 10),
    Geometry(1, 5, 5, 10, 10)),
  "Overlapping AABBs must return true.");

Assert(
  !SpatialCollisionQuery.CheckAabb(
    Geometry(1, 0, 0, 10, 10),
    Geometry(1, 10, 0, 10, 10)),
  "AABBs that only touch at an edge must return false.");

Assert(
  !SpatialCollisionQuery.CheckAabb(
    Geometry(1, 0, 0, 10, 10),
    Geometry(1, 20, 0, 10, 10)),
  "Disjoint AABBs must return false.");

const long subjectId = 100;
const long overlappingEntityId = 200;
SpatialGeometrySnapshot subjectGeometry = Geometry(7, 0, 0, 16, 16);
SpatialEntitySnapshot[] entities =
[
  new SpatialEntitySnapshot(subjectId, subjectGeometry),
  new SpatialEntitySnapshot(overlappingEntityId, Geometry(7, 4, 4, 4, 4)),
  new SpatialEntitySnapshot(201, Geometry(7, 4, 4, 4, 4), isActive: false),
  new SpatialEntitySnapshot(202, Geometry(7, 4, 4, 4, 4), collides: false),
  new SpatialEntitySnapshot(203, Geometry(7, 16, 0, 4, 4)),
];
SpatialTileSnapshot[] blockingTiles =
[
  new SpatialTileSnapshot(
    0,
    0,
    exists: true,
    isActive: true,
    blocksMovement: true,
    isSolid: true,
    isSolidTop: false,
    isHalfBrick: false,
    slope: 0,
    liquidAmount: 0),
];
SpatialCollisionSnapshot blockingSnapshot = new(
  7,
  subjectGeometry,
  subjectId,
  entities,
  blockingTiles);
SpatialContactSnapshot blockingResult = SpatialContactQuery.Evaluate(blockingSnapshot);
Assert(
  blockingResult.EntityContacts.SequenceEqual([new SpatialEntityContact(overlappingEntityId)]),
  "Only the active overlapping entity must be reported; the subject and edge touch are excluded.");
Assert(
  blockingResult.TileContacts.Length == 1 &&
    blockingResult.TileContacts[0].BlocksMovement &&
    !blockingResult.TileContacts[0].IsWet,
  "An overlapping blocking tile must produce a blocking tile contact.");

SpatialContactSnapshot emptyResult = SpatialContactQuery.Evaluate(
  new SpatialCollisionSnapshot(7, subjectGeometry, subjectId, [], []));
Assert(
  !emptyResult.HasEntityContact && !emptyResult.HasTileContact &&
    !emptyResult.IsWet && !emptyResult.IsLavaWet &&
    !emptyResult.IsHoneyWet && !emptyResult.IsShimmerWet,
  "An empty spatial snapshot must produce no contact flags.");

SpatialTileSnapshot[] liquidTiles =
[
  new SpatialTileSnapshot(0, 0, true, true, false, false, false, false, 0, 128, LiquidKind.Water),
  new SpatialTileSnapshot(0, 0, true, true, false, false, false, false, 0, 128, LiquidKind.Lava),
  new SpatialTileSnapshot(0, 0, true, true, false, false, false, false, 0, 128, LiquidKind.Honey),
  new SpatialTileSnapshot(0, 0, true, true, false, false, false, false, 0, 128, LiquidKind.Shimmer),
];
SpatialContactSnapshot liquidResult = SpatialContactQuery.Evaluate(
  new SpatialCollisionSnapshot(7, subjectGeometry, subjectId, [], liquidTiles));
Assert(
  liquidResult.IsWet && liquidResult.IsLavaWet &&
    liquidResult.IsHoneyWet && liquidResult.IsShimmerWet,
  "Overlapping liquid geometry must preserve the aggregate liquid flags.");

SpatialTileSnapshot halfBrick = new(
  2,
  3,
  exists: true,
  isActive: true,
  blocksMovement: true,
  isSolid: true,
  isSolidTop: false,
  isHalfBrick: true,
  slope: 0,
  liquidAmount: 128);
SpatialGeometrySnapshot halfBrickCollision = halfBrick.CollisionGeometry(7);
SpatialGeometrySnapshot halfBrickLiquid = halfBrick.LiquidGeometry(7);
Assert(
  halfBrickCollision.Position == new Vector2(32, 56) &&
    halfBrickCollision.Size == new Vector2(16, 8),
  "Half-brick collision geometry must use Terraria tile coordinates.");
Assert(
  halfBrickLiquid.Position == new Vector2(32, 56) &&
    halfBrickLiquid.Size == new Vector2(16, 8),
  "Liquid surface geometry must preserve the 128-level full-tile surface.");

SpatialTileSnapshot slope = new(
  0,
  0,
  exists: true,
  isActive: true,
  blocksMovement: true,
  isSolid: true,
  isSolidTop: false,
  isHalfBrick: false,
  slope: 1,
  liquidAmount: 0);
SpatialContactSnapshot slopeResult = SpatialContactQuery.Evaluate(
  new SpatialCollisionSnapshot(7, subjectGeometry, subjectId, [], [slope]));
Assert(
  slopeResult.HasUnsupportedSlopeFacts && slopeResult.TileContacts.Length == 0,
  "Unsupported slope facts must remain explicit instead of becoming a false flat-tile hit.");

SpatialCollisionSnapshot stableSnapshot = new(
  7,
  subjectGeometry,
  subjectId,
  entities,
  liquidTiles);
int entityCountBefore = stableSnapshot.Entities.Length;
int tileCountBefore = stableSnapshot.Tiles.Length;
SpatialContactSnapshot firstStableResult = SpatialContactQuery.Evaluate(stableSnapshot);
SpatialContactSnapshot secondStableResult = SpatialContactQuery.Evaluate(stableSnapshot);
Assert(
  firstStableResult.EntityContacts.SequenceEqual(secondStableResult.EntityContacts) &&
    firstStableResult.TileContacts.SequenceEqual(secondStableResult.TileContacts) &&
    firstStableResult.IsWet == secondStableResult.IsWet &&
    firstStableResult.IsLavaWet == secondStableResult.IsLavaWet &&
    firstStableResult.IsHoneyWet == secondStableResult.IsHoneyWet &&
    firstStableResult.IsShimmerWet == secondStableResult.IsShimmerWet,
  "Repeated evaluation of one snapshot must be stable.");
Assert(
  stableSnapshot.Entities.Length == entityCountBefore &&
    stableSnapshot.Tiles.Length == tileCountBefore,
  "Query evaluation must not mutate the input snapshot collections.");

SpatialTileSnapshot lowLiquid = new(
  0,
  0,
  exists: true,
  isActive: true,
  blocksMovement: false,
  isSolid: false,
  isSolidTop: false,
  isHalfBrick: false,
  slope: 0,
  liquidAmount: 1);
Assert(
  lowLiquid.LiquidGeometry(7).HasArea &&
    lowLiquid.LiquidGeometry(7).Size == new Vector2(16, 1),
  "A minimal liquid level must preserve the one-pixel legacy liquid surface.");

Console.WriteLine("PASS: SpatialSimulation core query cases");
