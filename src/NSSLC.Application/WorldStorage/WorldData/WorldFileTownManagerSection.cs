using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Saved town NPC room assignments from the WorldFile TownManager section.
/// </summary>
public sealed class WorldFileTownManagerSection
{
  public const string SectionId = "world.town-manager";

  public WorldFileTownManagerSection(IReadOnlyList<WorldFileTownRoomRecord> rooms)
  {
    ArgumentNullException.ThrowIfNull(rooms);
    if (rooms.Count > 100_000)
    {
      throw new ArgumentOutOfRangeException(nameof(rooms));
    }

    List<WorldFileTownRoomRecord> copiedRooms = new(rooms.Count);
    for (int index = 0; index < rooms.Count; index++)
    {
      copiedRooms.Add(rooms[index] ?? throw new ArgumentException(
        "A town manager section cannot contain a null room record.", nameof(rooms)));
    }

    Rooms = Array.AsReadOnly(copiedRooms.ToArray());
  }

  public IReadOnlyList<WorldFileTownRoomRecord> Rooms { get; }

  public static WorldFileTownManagerSection Empty =>
    new(Array.Empty<WorldFileTownRoomRecord>());
}
