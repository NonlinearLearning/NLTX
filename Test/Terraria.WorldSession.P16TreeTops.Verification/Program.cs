using System.IO;
using Terraria.WorldGeneration.Terrain.TreeTops;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertEqual(int expected, int actual, string message)
{
  Assert(expected == actual, $"{message} Expected {expected}, got {actual}.");
}

WorldTreeTopsStateComponent state = new();
for (int areaId = 0; areaId < state.AreaCount; areaId++)
{
  state.SetTreeStyle(areaId, areaId * 17 - 3);
}

using MemoryStream saveStream = new();
using (BinaryWriter writer = new(saveStream, System.Text.Encoding.UTF8, leaveOpen: true))
{
  WorldTreeTopsPersistenceAdapter.Save(writer, state);
}

AssertEqual(4 + state.AreaCount * sizeof(int), (int)saveStream.Length, "Save payload length.");
saveStream.Position = 0;
using (BinaryReader reader = new(saveStream, System.Text.Encoding.UTF8, leaveOpen: true))
{
  AssertEqual(state.AreaCount, reader.ReadInt32(), "Saved area count.");
  for (int areaId = 0; areaId < state.AreaCount; areaId++)
  {
    AssertEqual(areaId * 17 - 3, reader.ReadInt32(), "Saved style order.");
  }
}
Console.WriteLine("PASS: Version4 TreeTops save layout");

WorldTreeTopsStateComponent partialState = new();
for (int areaId = 0; areaId < partialState.AreaCount; areaId++)
{
  partialState.SetTreeStyle(areaId, 100 + areaId);
}

using MemoryStream partialStream = new();
using (BinaryWriter writer = new(partialStream, System.Text.Encoding.UTF8, leaveOpen: true))
{
  writer.Write(3);
  writer.Write(7);
  writer.Write(8);
  writer.Write(9);
}

partialStream.Position = 0;
using (BinaryReader reader = new(partialStream, System.Text.Encoding.UTF8, leaveOpen: true))
{
  WorldTreeTopsPersistenceAdapter.Load(reader, 211, partialState);
}

AssertEqual(7, partialState.GetTreeStyle(0), "First loaded style.");
AssertEqual(8, partialState.GetTreeStyle(1), "Second loaded style.");
AssertEqual(9, partialState.GetTreeStyle(2), "Third loaded style.");
AssertEqual(103, partialState.GetTreeStyle(3), "Unspecified styles remain unchanged.");
Console.WriteLine("PASS: Version4 TreeTops counted load");

using MemoryStream legacyStream = new();
using (BinaryReader reader = new(legacyStream, System.Text.Encoding.UTF8, leaveOpen: true))
{
  WorldTreeTopsPersistenceAdapter.Load(reader, 210, partialState);
}

AssertEqual(7, partialState.GetTreeStyle(0), "Pre-211 load leaves current state unchanged.");
Console.WriteLine("PASS: Version4 pre-211 TreeTops fallback no-op");

partialState.SetTreeStyle(0, -1);
partialState.SetTreeStyle(1, 255);
partialState.SetTreeStyle(2, 256);
partialState.SetTreeStyle(3, 257);
byte[] payload = WorldTreeTopsNetworkProjection.CreateSyncPayload(partialState);
AssertEqual(WorldTreeTopsStateComponent.TreeTopsAreaCount, payload.Length, "Sync payload length.");
AssertEqual(255, payload[0], "Negative style wraps to one byte.");
AssertEqual(255, payload[1], "Byte maximum is preserved.");
AssertEqual(0, payload[2], "256 wraps to zero.");
AssertEqual(1, payload[3], "257 wraps to one.");
Console.WriteLine("PASS: Version4 TreeTops sync byte projection");

bool found = WorldTreeTopsAreaQuery.TryGetAreaId(
  true,
  53,
  25,
  true,
  null,
  out int oceanAreaId);
Assert(found, "Ocean sand should resolve to a TreeTops area.");
AssertEqual(WorldTreeTopsAreaId.Ocean, oceanAreaId, "Ocean area id.");

found = WorldTreeTopsAreaQuery.TryGetAreaId(
  true,
  477,
  150,
  false,
  new[] { 50, 100, 150 },
  out int forestAreaId);
Assert(found, "Forest tree should resolve to a TreeTops area.");
AssertEqual(WorldTreeTopsAreaId.Forest4, forestAreaId, "Right forest band.");

WorldTreeTopsAreaQuery.GetTileCoordinates(32f, 16f, out int tileX, out int tileY);
AssertEqual(2, tileX, "World X to tile X conversion.");
AssertEqual(2, tileY, "World Y to tile Y conversion.");

found = WorldTreeTopsAreaQuery.TryGetAreaId(
  false,
  70,
  0,
  false,
  null,
  out _);
Assert(!found, "Inactive tiles must not select a TreeTops area.");
Console.WriteLine("PASS: Version4 TreeTops tile-area selection");

WorldTreeTopsStateComponent randomizedState = new();
SequenceTreeTopsRandomSource forestRandom = new(0, 1);
bool randomized = WorldTreeTopsSystem.TryRandomizeTreeStyleForTile(
    randomizedState,
    forestRandom,
    true,
    2,
    25,
    false,
    new[] { 30, 60, 90 },
    out WorldTreeTopsStyleChangeResult forestChange);
Assert(randomized, "Supported active tree tile should randomize a style.");
Assert(forestChange.Changed, "Randomized style must change the current value.");
AssertEqual(0, forestChange.PreviousStyle, "Previous forest style.");
AssertEqual(1, forestChange.CurrentStyle, "Current forest style.");
AssertEqual(
  1,
  randomizedState.GetTreeStyle(WorldTreeTopsAreaId.Forest1),
  "Committed forest style.");
AssertEqual(2, forestRandom.RequestedBounds.Count, "Same-style random result must retry.");
AssertEqual(6, forestRandom.RequestedBounds[0], "Forest style count.");

SequenceTreeTopsRandomSource snowRandom = new(13);
WorldTreeTopsStyleChangeResult snowChange =
  WorldTreeTopsSystem.RandomizeTreeStyle(
    randomizedState,
    snowRandom,
    WorldTreeTopsAreaId.Snow);
AssertEqual(7, snowChange.CurrentStyle, "Snow style list mapping.");
AssertEqual(14, snowRandom.RequestedBounds[0], "Snow style list count.");
Console.WriteLine("PASS: Version4 TreeTops random style mutation");

SequenceTreeTopsRandomSource unusedRandom = new();
int priorStyle = randomizedState.GetTreeStyle(WorldTreeTopsAreaId.Forest2);
randomized = WorldTreeTopsSystem.TryRandomizeTreeStyleForTile(
  randomizedState,
  unusedRandom,
  false,
  2,
  40,
  false,
  new[] { 30, 60, 90 },
  out _);
Assert(!randomized, "Inactive tile must not randomize a tree style.");
AssertEqual(
  priorStyle,
  randomizedState.GetTreeStyle(WorldTreeTopsAreaId.Forest2),
  "Inactive tile leaves state unchanged.");
AssertEqual(0, unusedRandom.RequestedBounds.Count, "Inactive tile does not consume randomness.");
Console.WriteLine("PASS: Version4 TreeTops inactive-tile no-op");

sealed class SequenceTreeTopsRandomSource : IWorldTreeTopsRandomSource
{
  private readonly Queue<int> _values;

  public SequenceTreeTopsRandomSource(params int[] values)
  {
    _values = new Queue<int>(values);
  }

  public List<int> RequestedBounds { get; } = new();

  public int Next(int exclusiveUpperBound)
  {
    RequestedBounds.Add(exclusiveUpperBound);
    return _values.Dequeue();
  }
}
