namespace Terraria.WorldGeneration.Dungeon.Layout;

public sealed class DungeonLayoutGraphWorkState
{
  private readonly Dictionary<int, RoomEntry> _rooms = new();
  private readonly Dictionary<int, HallLine> _halls = new();
  private readonly Dictionary<int, DungeonControlLineComponent> _controlLines = new();

  public IReadOnlyDictionary<int, RoomEntry> Rooms => _rooms;

  public IReadOnlyDictionary<int, HallLine> Halls => _halls;

  public IReadOnlyDictionary<int, DungeonControlLineComponent> ControlLines => _controlLines;

  public HallwayCalculatorState Calculator { get; } = new();

  public void AddRoom(RoomEntry entry)
  {
    ArgumentNullException.ThrowIfNull(entry);
    AddUnique(_rooms, entry.EntryId, entry, "room entry");
  }

  public void AddHall(HallLine hallLine)
  {
    ArgumentNullException.ThrowIfNull(hallLine);
    AddUnique(_halls, hallLine.LineId, hallLine, "hall line");
  }

  public void AddControlLine(DungeonControlLineComponent controlLine)
  {
    ArgumentNullException.ThrowIfNull(controlLine);
    AddUnique(_controlLines, controlLine.Index, controlLine, "control line");
  }

  public void Clear()
  {
    _rooms.Clear();
    _halls.Clear();
    _controlLines.Clear();
    Calculator.Replace(Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>(), 0f, 0f);
  }

  private static void AddUnique<T>(
    IDictionary<int, T> target,
    int key,
    T value,
    string label)
  {
    if (!target.TryAdd(key, value))
    {
      throw new InvalidOperationException($"The {label} '{key}' is already registered.");
    }
  }
}
