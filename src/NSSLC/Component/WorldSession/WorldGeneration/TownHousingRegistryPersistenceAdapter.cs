using System;
using System.IO;
using Terraria.WorldGeneration.Systems;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Preserves Version4's count, NPC type, X, Y room record layout.
/// </summary>
public static class TownHousingRegistryPersistenceAdapter
{
  public static void Save(BinaryWriter writer, TownHousingRegistryComponent component)
  {
    ArgumentNullException.ThrowIfNull(writer);
    ArgumentNullException.ThrowIfNull(component);
    EnsureNpcTypeKeyMode(component);

    writer.Write(component.RoomsByResidentKey.Count);
    foreach (var entry in component.RoomsByResidentKey)
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
    if (count < 0)
    {
      throw new InvalidDataException("Town housing room count cannot be negative.");
    }

    TownHousingRegistrySystem.Clear(component);
    TownHousingRegistrySystem.UseNpcTypeKeys(component);
    for (int index = 0; index < count; index++)
    {
      TownHousingRegistrySystem.AssignRoom(
        component,
        new TownHousingResidentKey(reader.ReadInt32()),
        new TilePosition(reader.ReadInt32(), reader.ReadInt32()));
    }
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
