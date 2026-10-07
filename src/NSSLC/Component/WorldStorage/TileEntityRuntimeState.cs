using Terraria.Relationships;

namespace Terraria.WorldStorage;

/// <summary>
/// A short-lived value projection of a currently resolved TileEntity runtime entity.
/// </summary>
public readonly record struct TileEntityRuntimeState(
  TileEntityId Id,
  TileEntityTypeId Type,
  TileCoordinate Anchor,
  EntityReference RuntimeReference,
  short NpcIndex,
  byte LogicCheck,
  bool LogicOn,
  int LogicSensorCountedData);
