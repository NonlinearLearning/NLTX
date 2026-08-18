using System;
using System.Collections.Generic;

namespace Terraria.WorldFile.V319.Model;

public sealed class LegacyChest
{
  public LegacyChest(int x, int y, string name, IReadOnlyList<LegacyChestItem> items)
  {
    X = x;
    Y = y;
    Name = name ?? throw new ArgumentNullException(nameof(name));
    Items = new LegacyReadOnlyList<LegacyChestItem>(items ??
      throw new ArgumentNullException(nameof(items)));
  }

  public IReadOnlyList<LegacyChestItem> Items { get; }

  public string Name { get; }

  public int X { get; }

  public int Y { get; }
}
