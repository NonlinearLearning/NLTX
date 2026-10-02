using EntityEcs.Components;
using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

public enum LeashedAnchorCommandKind : byte
{
  Respawn,
  Despawn,
  InsertItem,
  DropItem
}

public readonly record struct LeashedAnchorCommand(
  LeashedAnchorCommandKind Kind,
  TileEntityId AnchorId,
  TileCoordinate Position,
  EntityId? RuntimeEntityId,
  int? ItemType);

