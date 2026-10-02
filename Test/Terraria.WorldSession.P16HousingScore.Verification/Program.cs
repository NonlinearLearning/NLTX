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

HousingRoomEvaluationResult invalidRoom = CreateRoom(canSpawn: false);
HousingRoomScoreSnapshot invalid = HousingRoomScoreSystem.ScoreRoom(
  invalidRoom,
  120,
  140,
  new MemoryHousingScoreSource(),
  _ => false);
AssertEqual(-1, invalid.HighScore, "An invalid room has no score.");
Assert(!invalid.HasBestCandidate, "An invalid room has no best candidate.");
Console.WriteLine("PASS: invalid room is not scored");

MemoryHousingScoreSource bestSource = CreateTwoCandidateSource();
bestSource.Set(new TilePosition(12, 62), new HousingRoomScoreTileSample(
  true, false, false, false, false, false, 0, false, 0));
bestSource.Set(new TilePosition(10, 62), new HousingRoomScoreTileSample(
  true, false, false, true, false, false, 0, false, 0));
HousingRoomScoreSnapshot best = HousingRoomScoreSystem.ScoreRoom(
  CreateRoom(),
  120,
  140,
  bestSource,
  IsInsideRoom);
AssertEqual(14, best.BestX, "The candidate with the highest score is selected.");
AssertEqual(64, best.BestY, "The selected candidate keeps its floor coordinate.");
HousingRoomScoreCandidate firstCandidate = best.Candidates.Single(candidate => candidate.X == 12);
AssertEqual(5, firstCandidate.Score, "Chest and center-column deductions apply in source order.");
HousingRoomScoreCandidate bestCandidate = best.Candidates.Single(candidate => candidate.X == 14);
AssertEqual(50, bestCandidate.Score, "Unmodified candidate keeps the base score.");
Assert(bestCandidate.IsNewHighScore, "The selected candidate records the high-score effect.");
Console.WriteLine("PASS: best candidate and ordered chest/furniture scoring");

MemoryHousingScoreSource balanceSource = CreateSingleCandidateSource();
balanceSource.Set(new TilePosition(50, 30), new HousingRoomScoreTileSample(
  true, false, false, false, false, false, -60, false, 0));
balanceSource.Set(new TilePosition(50, 16), new HousingRoomScoreTileSample(
  true, false, false, false, false, false, -90, false, 0));
HousingRoomScoreSnapshot balance = HousingRoomScoreSystem.ScoreRoom(
  CreateRoom(),
  120,
  140,
  balanceSource,
  IsInsideRoom);
AssertEqual(0, balance.HighScore, "A qualifying evil balance removes the best score.");
AssertEqual(-10, balance.Candidates.Single().Score,
  "The tested-area penalty applies while the row beyond Version4's vertical bound is excluded.");
Console.WriteLine("PASS: good/evil penalty and asymmetric tested bounds");

HousingRoomScoreSnapshot shared = HousingRoomScoreSystem.ScoreRoom(
  CreateRoom(),
  120,
  140,
  CreateSingleCandidateSource(),
  IsInsideRoom,
  sharedRoomX: 13);
AssertEqual(1, shared.HighScore, "A nearby shared-room candidate is capped at one point.");
AssertEqual(12, shared.BestX, "A positive shared-room score remains eligible.");
Console.WriteLine("PASS: shared-room proximity rule");

MemoryHousingScoreSource blockedSource = CreateSingleCandidateSource();
blockedSource.Set(new TilePosition(12, 61), new HousingRoomScoreTileSample(
  true, true, false, false, false, false, 0, false, 0));
HousingRoomScoreSnapshot blocked = HousingRoomScoreSystem.ScoreRoom(
  CreateRoom(),
  120,
  140,
  blockedSource,
  IsInsideRoom);
AssertEqual(0, blocked.Candidates.Count,
  "The delegated collision query excludes candidates with solid headroom.");
Assert(!blocked.HasBestCandidate, "A blocked room has no best candidate.");
Console.WriteLine("PASS: collision policy blocks occupied headroom");

static HousingRoomEvaluationResult CreateRoom(bool canSpawn = true) => new(
  canSpawn,
  canSpawn ? HousingRoomValidationFailure.None : HousingRoomValidationFailure.MissingRequirement,
  81,
  new TilePosition(10, 60),
  new TilePosition(18, 68),
  new HousingRoomRequirementResult(true, true, true, true, canSpawn),
  false,
  false);

static MemoryHousingScoreSource CreateTwoCandidateSource()
{
  MemoryHousingScoreSource source = new();
  SetFloor(source, new[] { 11, 12, 13, 14, 15 }, forbiddenExcept: new[] { 12, 14 });
  return source;
}

static MemoryHousingScoreSource CreateSingleCandidateSource()
{
  MemoryHousingScoreSource source = new();
  SetFloor(source, new[] { 11, 12, 13 }, forbiddenExcept: new[] { 12 });
  return source;
}

static void SetFloor(
  MemoryHousingScoreSource source,
  IEnumerable<int> floorXs,
  IEnumerable<int> forbiddenExcept)
{
  HashSet<int> candidates = forbiddenExcept.ToHashSet();
  foreach (int x in floorXs)
  {
    source.Set(new TilePosition(x, 64), new HousingRoomScoreTileSample(
      true,
      true,
      false,
      false,
      false,
      false,
      0,
      !candidates.Contains(x),
      0));
  }
}

static bool IsInsideRoom(TilePosition position) =>
  position.X >= 10 && position.X <= 18 && position.Y >= 60 && position.Y <= 68;

sealed class MemoryHousingScoreSource : IHousingRoomScoreTileSource
{
  private readonly Dictionary<TilePosition, HousingRoomScoreTileSample> _tiles = new();

  public void Set(TilePosition position, HousingRoomScoreTileSample sample)
  {
    _tiles[position] = sample;
  }

  public HousingRoomScoreTileSample ReadTile(TilePosition position) =>
    _tiles.TryGetValue(position, out HousingRoomScoreTileSample sample)
      ? sample
      : default;

  public bool HasSolidTiles(TilePosition minimum, TilePosition maximum)
  {
    for (int x = minimum.X; x <= maximum.X; x++)
    {
      for (int y = minimum.Y; y <= maximum.Y; y++)
      {
        if (ReadTile(new TilePosition(x, y)).IsSolidActive)
        {
          return true;
        }
      }
    }

    return false;
  }
}
