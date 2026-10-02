namespace Terraria.WorldGeneration.Dungeon.Layout;

public sealed class HallwayCalculatorState
{
  private readonly List<int> _roomEntryIds = new();
  private readonly List<int> _hallLineIds = new();
  private readonly List<int> _stairwellIds = new();
  private readonly List<int> _controlLineIds = new();

  public IReadOnlyList<int> RoomEntryIds => _roomEntryIds;

  public IReadOnlyList<int> HallLineIds => _hallLineIds;

  public IReadOnlyList<int> StairwellIds => _stairwellIds;

  public IReadOnlyList<int> ControlLineIds => _controlLineIds;

  public float MaxProgressDelta { get; private set; }

  public float AverageLineLength { get; private set; }

  public void Replace(
    IEnumerable<int> roomEntryIds,
    IEnumerable<int> hallLineIds,
    IEnumerable<int> stairwellIds,
    IEnumerable<int> controlLineIds,
    float maxProgressDelta,
    float averageLineLength)
  {
    ArgumentNullException.ThrowIfNull(roomEntryIds);
    ArgumentNullException.ThrowIfNull(hallLineIds);
    ArgumentNullException.ThrowIfNull(stairwellIds);
    ArgumentNullException.ThrowIfNull(controlLineIds);
    if (!float.IsFinite(maxProgressDelta) || maxProgressDelta < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(maxProgressDelta));
    }

    if (!float.IsFinite(averageLineLength) || averageLineLength < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(averageLineLength));
    }

    Replace(_roomEntryIds, roomEntryIds);
    Replace(_hallLineIds, hallLineIds);
    Replace(_stairwellIds, stairwellIds);
    Replace(_controlLineIds, controlLineIds);
    MaxProgressDelta = maxProgressDelta;
    AverageLineLength = averageLineLength;
  }

  private static void Replace(List<int> target, IEnumerable<int> values)
  {
    target.Clear();
    foreach (int value in values)
    {
      if (value < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(values));
      }

      target.Add(value);
    }
  }
}
