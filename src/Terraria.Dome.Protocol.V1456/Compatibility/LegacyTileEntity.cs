using System.Collections.Generic;

namespace Terraria.Dome.Protocol.V1456.Compatibility;

public abstract record LegacyTileEntity(int EntityId, short TileX, short TileY);

public sealed record LegacyTrainingDummyTileEntity(
  int EntityId,
  short TileX,
  short TileY,
  short NpcId) : LegacyTileEntity(EntityId, TileX, TileY);

public sealed record LegacyItemTileEntity(
  LegacyTileEntityItemKind Kind,
  int EntityId,
  short TileX,
  short TileY,
  LegacyTileEntityItem Item) : LegacyTileEntity(EntityId, TileX, TileY);

public sealed record LegacyHatRackTileEntity(
  int EntityId,
  short TileX,
  short TileY,
  LegacyTileEntityItem? Item0,
  LegacyTileEntityItem? Item1,
  LegacyTileEntityItem? Dye0,
  LegacyTileEntityItem? Dye1) : LegacyTileEntity(EntityId, TileX, TileY);

public sealed record LegacyDisplayDollTileEntity(
  int EntityId,
  short TileX,
  short TileY,
  IReadOnlyList<LegacyTileEntityItem?> Equipment,
  IReadOnlyList<LegacyTileEntityItem?> Dyes,
  LegacyTileEntityItem? Misc,
  byte Pose) : LegacyTileEntity(EntityId, TileX, TileY);

public sealed record LegacyTeleportationPylonTileEntity(
  int EntityId,
  short TileX,
  short TileY) : LegacyTileEntity(EntityId, TileX, TileY);
