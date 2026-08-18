using System;
using System.Collections.Generic;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldCompatibility.Model;

public readonly record struct CompatibilityChestItem(short Stack, int NetId, byte Prefix);

public sealed class CompatibilityChestSnapshot
{
  public CompatibilityChestSnapshot(
    int x,
    int y,
    string name,
    IReadOnlyList<CompatibilityChestItem> items)
  {
    X = x;
    Y = y;
    Name = name ?? throw new ArgumentNullException(nameof(name));
    Items = new CompatibilityReadOnlyList<CompatibilityChestItem>(items ??
      throw new ArgumentNullException(nameof(items)));
  }

  public IReadOnlyList<CompatibilityChestItem> Items { get; }

  public string Name { get; }

  public int X { get; }

  public int Y { get; }

  internal static CompatibilityChestSnapshot From(LegacyChest chest)
  {
    ArgumentNullException.ThrowIfNull(chest);
    CompatibilityChestItem[] items = new CompatibilityChestItem[chest.Items.Count];
    for (int index = 0; index < items.Length; index++)
    {
      LegacyChestItem item = chest.Items[index];
      items[index] = new CompatibilityChestItem(item.Stack, item.NetId, item.Prefix);
    }

    return new CompatibilityChestSnapshot(chest.X, chest.Y, chest.Name, items);
  }
}
