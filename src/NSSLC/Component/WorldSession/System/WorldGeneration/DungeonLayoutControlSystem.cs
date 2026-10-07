using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Commits dungeon layout facts without taking ownership of external dungeon-record fields.
/// </summary>
public static class DungeonLayoutControlSystem
{
  public static void Commit(
    DungeonLayoutControlComponent component,
    in DungeonLayoutSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (snapshot.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Dungeon layout cannot be committed to another generation.",
        nameof(snapshot));
    }

    ArgumentNullException.ThrowIfNull(snapshot.Records);
    if (snapshot.CurrentDungeon < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    component.ReplaceState(
      snapshot.TLeft,
      snapshot.TRight,
      snapshot.TTop,
      snapshot.TBottom,
      snapshot.TRooms,
      snapshot.LAltarX,
      snapshot.LAltarY,
      snapshot.Records,
      snapshot.CurrentDungeon);
  }
}
