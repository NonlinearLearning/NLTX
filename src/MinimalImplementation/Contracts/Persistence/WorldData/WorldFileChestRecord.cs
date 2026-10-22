using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// A persisted chest and its fixed inventory slot sequence.
/// </summary>
public sealed class WorldFileChestRecord
{
  public WorldFileChestRecord(
    int x,
    int y,
    string name,
    IReadOnlyList<WorldFileChestItem> items)
  {
    ArgumentNullException.ThrowIfNull(name);
    ArgumentNullException.ThrowIfNull(items);
    if (items.Count > 1000)
    {
      throw new ArgumentOutOfRangeException(nameof(items));
    }

    X = x;
    Y = y;
    Name = name;
    List<WorldFileChestItem> copiedItems = new(items.Count);
    for (int index = 0; index < items.Count; index++)
    {
      copiedItems.Add(items[index] ?? throw new ArgumentException(
        "A chest inventory cannot contain a null item.", nameof(items)));
    }

    Items = Array.AsReadOnly(copiedItems.ToArray());
  }

  public int X { get; }

  public int Y { get; }

  public string Name { get; }

  public IReadOnlyList<WorldFileChestItem> Items { get; }
}
