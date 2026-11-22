using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Item;

namespace NSSLC.Server.Content.Prototypes;

/// <summary>Projects validated item-use prototypes into the ECS runtime catalog.</summary>
internal static class ItemUseDefinitionPrototypeLoader
{
  public static ItemUseDefinitionCatalog Load(
      PrototypeManager prototypeManager,
      string directory)
  {
    ArgumentNullException.ThrowIfNull(prototypeManager);
    IReadOnlyList<ItemUseDefinitionPrototype> prototypes =
        prototypeManager.LoadDirectory<ItemUseDefinitionPrototype>(directory);
    ItemUseDefinitionCatalog catalog = new();
    for (int index = 0; index < prototypes.Count; index++)
    {
      ItemUseDefinitionPrototype prototype = prototypes[index];
      ItemId itemId = ParseItemId(
          prototype.Id,
          directory,
          index,
          nameof(prototype.Id));
      ItemId useAmmo = ParseItemId(
          prototype.UseAmmo,
          directory,
          index,
          nameof(prototype.UseAmmo));
      if (useAmmo == ItemId.None && prototype.ConsumeAmmo)
      {
        throw new InvalidDataException(
            $"Prototype '{prototype.Id}' at '{directory}' consumes ammunition " +
            "but useAmmo is None.");
      }

      ItemUseDefinition definition = new(
          itemId,
          prototype.UseTime,
          prototype.UseAnimation,
          prototype.AutoReuse,
          prototype.Channel,
          useAmmo,
          prototype.ConsumeAmmo,
          prototype.ManaCost);
      if (!definition.IsValid)
      {
        throw new InvalidDataException(
            $"Prototype '{prototype.Id}' at '{directory}' has invalid use timing or mana cost.");
      }

      catalog.Register(definition);
    }

    if (catalog.Count == 0)
    {
      throw new InvalidDataException(
          $"Prototype directory '{directory}' does not define any item-use entries.");
    }

    catalog.Seal();
    return catalog;
  }

  private static ItemId ParseItemId(
      string value,
      string directory,
      int index,
      string fieldName)
  {
    if (!Enum.TryParse(value, ignoreCase: false, out ItemId itemId) ||
        !ItemIdRules.IsKnown(itemId))
    {
      throw new InvalidDataException(
          $"Prototype directory '{directory}' entry {index} field '{fieldName}' " +
          $"contains unknown item id '{value}'.");
    }

    return itemId;
  }
}

