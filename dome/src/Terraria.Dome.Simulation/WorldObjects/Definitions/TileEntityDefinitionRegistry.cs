using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public static class TileEntityDefinitionRegistry
{
  private static readonly TileEntityDefinition[] DefinitionsByType =
  [
    new(0, "TrainingDummy"),
    new(1, "ItemFrame"),
    new(2, "LogicSensor"),
    new(3, "DisplayDoll"),
    new(4, "WeaponsRack"),
    new(5, "HatRack"),
    new(6, "FoodPlatter"),
    new(7, "TeleportationPylon"),
    new(8, "DeadCellsDisplayJar"),
    new(9, "KiteAnchor"),
    new(10, "CritterAnchor")
  ];

  private static readonly IReadOnlyList<TileEntityDefinition> ReadOnlyDefinitions =
    Array.AsReadOnly(DefinitionsByType);

  public static IReadOnlyList<TileEntityDefinition> Definitions => ReadOnlyDefinitions;

  public static bool TryGet(byte type, out TileEntityDefinition definition)
  {
    if (type >= DefinitionsByType.Length)
    {
      definition = default;
      return false;
    }

    definition = DefinitionsByType[type];
    return true;
  }
}
