using System;
using System.Collections.Generic;
using System.IO;
using Terraria.WorldGeneration.Systems;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Preserves Version4's count, NPC type, X, Y room record layout.
/// </summary>
public static class TownHousingRegistryPersistenceAdapter
{
  private const int MaximumRoomAssignments = 100_000;

  public static void Save(BinaryWriter writer, TownHousingRegistryComponent component)
  {
    ArgumentNullException.ThrowIfNull(writer);
    ArgumentNullException.ThrowIfNull(component);
    EnsureNpcTypeKeyMode(component);

    IReadOnlyList<KeyValuePair<TownHousingResidentKey, TilePosition>> assignments =
      TownHousingRegistrySystem.GetRoomAssignmentsSnapshot(component);
    writer.Write(assignments.Count);
    foreach (KeyValuePair<TownHousingResidentKey, TilePosition> entry in assignments)
    {
      writer.Write(entry.Key.NpcType);
      writer.Write(entry.Value.X);
      writer.Write(entry.Value.Y);
    }
  }

  public static void Load(BinaryReader reader, TownHousingRegistryComponent component)
  {
    ArgumentNullException.ThrowIfNull(reader);
    ArgumentNullException.ThrowIfNull(component);
    int count = reader.ReadInt32();
    if (count < 0 || count > MaximumRoomAssignments)
    {
      throw new InvalidDataException("Town housing room count is outside the supported range.");
    }

    var assignments = new List<KeyValuePair<TownHousingResidentKey, TilePosition>>(count);
    var residents = new HashSet<TownHousingResidentKey>();
    for (int index = 0; index < count; index++)
    {
      var resident = new TownHousingResidentKey(reader.ReadInt32());
      var room = new TilePosition(reader.ReadInt32(), reader.ReadInt32());
      if (!residents.Add(resident))
      {
        throw new InvalidDataException("Duplicate town housing resident keys.");
      }

      assignments.Add(new KeyValuePair<TownHousingResidentKey, TilePosition>(resident, room));
    }

    TownHousingRegistrySystem.ReplaceRoomAssignments(component, assignments);
  }

  private static void EnsureNpcTypeKeyMode(TownHousingRegistryComponent component)
  {
    if (component.ResidentKeyMode is TownHousingKeyMode.Unresolved or TownHousingKeyMode.Type)
    {
      TownHousingRegistrySystem.UseNpcTypeKeys(component);
      return;
    }

    throw new InvalidOperationException(
      "Town housing persistence requires the Version4 NPC-type key mode.");
  }
}
