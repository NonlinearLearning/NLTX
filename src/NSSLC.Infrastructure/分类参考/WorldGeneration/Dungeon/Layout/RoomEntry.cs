using Terraria.WorldGeneration.Dungeon.Rooms;

namespace Terraria.WorldGeneration.Dungeon.Layout;

public sealed class RoomEntry
{
  private readonly List<int> _backLinks = new();
  private readonly List<int> _forwardLinks = new();

  public RoomEntry(
    int entryId,
    DungeonRoomDefinition room,
    float progressAlongSnake)
  {
    if (entryId < 0 || !float.IsFinite(progressAlongSnake) || progressAlongSnake < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(entryId));
    }

    EntryId = entryId;
    Room = room ?? throw new ArgumentNullException(nameof(room));
    ProgressAlongSnake = progressAlongSnake;
  }

  public int EntryId { get; }

  public DungeonRoomDefinition Room { get; }

  public float ProgressAlongSnake { get; }

  public IReadOnlyList<int> BackLinks => _backLinks;

  public IReadOnlyList<int> ForwardLinks => _forwardLinks;

  public void AddBackLink(int entryId)
  {
    AddLink(_backLinks, entryId);
  }

  public void AddForwardLink(int entryId)
  {
    AddLink(_forwardLinks, entryId);
  }

  private void AddLink(List<int> links, int entryId)
  {
    if (entryId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(entryId));
    }

    if (!links.Contains(entryId))
    {
      links.Add(entryId);
    }
  }
}
