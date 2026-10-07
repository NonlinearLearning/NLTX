using Terraria.Items;

namespace Terraria.WorldStorage;

public sealed record WorldChestSnapshot {
  public TileCoordinate Anchor { get; }
  public string Name { get; }
  public IReadOnlyList<ItemState> Items { get; }

  public WorldChestSnapshot(TileCoordinate anchor, string name, IReadOnlyList<ItemState> items) {
    Anchor = anchor;
    Name = name ?? throw new ArgumentNullException(nameof(name));
    Items = Array.AsReadOnly(items.ToArray());
  }
}
