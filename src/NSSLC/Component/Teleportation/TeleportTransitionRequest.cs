using System;
using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.Teleportation;

public readonly record struct TeleportTransitionRequest(
  Guid CommandId,
  EntityReference SourceEndpoint,
  EntityReference DestinationEndpoint,
  EntityReference Subject,
  PortalSubjectKind SubjectKind,
  TileCoordinate SourcePosition,
  TileCoordinate DestinationPosition,
  TeleportSource Source,
  int CooldownTicks,
  uint ExpectedSourceRevision,
  uint ExpectedDestinationRevision,
  uint ExpectedSubjectRevision,
  bool BlockPlayerTeleportation = false,
  int Style = 0,
  int ExtraInfo = 0,
  long? StartedAtTick = null);
