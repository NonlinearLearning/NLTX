using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Housing;
using Terraria.WorldGeneration.Systems;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertEqual<T>(T expected, T actual, string message)
{
  Assert(EqualityComparer<T>.Default.Equals(expected, actual),
    $"{message} Expected {expected}, got {actual}.");
}

MemoryHousingSource validSource = new();
validSource.Set(new TilePosition(15, 15), new HousingTileSample(
  false, false, false, 0, true, true, false, false, false, false, false));
validSource.Set(new TilePosition(16, 15), new HousingTileSample(
  false, false, false, 0, true, false, true, false, false, false, false));
validSource.Set(new TilePosition(17, 15), new HousingTileSample(
  false, false, false, 0, true, false, false, true, false, false, false));
validSource.Set(new TilePosition(18, 15), new HousingTileSample(
  false, false, false, 0, true, false, false, false, true, false, false));

HousingValidationDefinition definition = new(
  maxRoomTiles: 256,
  maxRoomSize: 32,
  minimumRoomTiles: 60,
  tileTypeCount: 700);
bool valid = HousingValidationSystem.TryEvaluateRoom(
  new TilePosition(16, 16),
  32,
  32,
  definition,
  validSource,
  out HousingRoomEvaluationResult validResult);
Assert(valid, "A bounded room with all requirements is valid.");
Assert(validResult.CanSpawn, "Valid room returns CanSpawn.");
AssertEqual(121, validResult.RoomTileCount, "The 11 by 11 room is fully visited.");
AssertEqual(HousingRoomValidationFailure.None, validResult.Failure, "Valid room failure code.");
Console.WriteLine("PASS: bounded room flood fill and requirements");

MemoryHousingSource incompleteSource = new();
bool incomplete = HousingValidationSystem.TryEvaluateRoom(
  new TilePosition(16, 16),
  32,
  32,
  definition,
  incompleteSource,
  out HousingRoomEvaluationResult incompleteResult);
Assert(!incomplete, "A room without furniture requirements is rejected.");
AssertEqual(
  HousingRoomValidationFailure.MissingRequirement,
  incompleteResult.Failure,
  "Missing furniture is reported after scanning.");
Console.WriteLine("PASS: missing room requirement is reported");

bool edge = HousingValidationSystem.TryEvaluateRoom(
  new TilePosition(2, 2),
  32,
  32,
  definition,
  validSource,
  out HousingRoomEvaluationResult edgeResult);
Assert(!edge, "A room too close to the world edge is rejected.");
AssertEqual(
  HousingRoomValidationFailure.TooCloseToWorldEdge,
  edgeResult.Failure,
  "World edge failure is explicit.");
Console.WriteLine("PASS: world edge guard");

sealed class MemoryHousingSource : IHousingTileSource
{
  private readonly Dictionary<TilePosition, HousingTileSample> _overrides = new();

  public void Set(TilePosition position, HousingTileSample sample)
  {
    _overrides[position] = sample;
  }

  public HousingTileSample ReadTile(TilePosition position)
  {
    if (_overrides.TryGetValue(position, out HousingTileSample sample))
    {
      return sample;
    }

    bool boundary = position.X <= 11 || position.X >= 21 ||
      position.Y <= 11 || position.Y >= 21;
    return boundary
      ? new HousingTileSample(true, true, false, 0, false, false, false, false, false, false, false)
      : new HousingTileSample(false, false, false, 0, true, false, false, false, false, false, false);
  }
}
