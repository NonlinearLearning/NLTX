using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects.Placement;

namespace Terraria.Dome.Server.Replication;

public sealed class ObjectPlacementReplicationAssembler
{
  private readonly HashSet<(long Sequence, long SectionVersion)> _publishedRevisions = new();

  public bool TryProject(
    WorldObjectPlacementCommittedEvent placement,
    out ObjectPlacementReplicationFrame frame)
  {
    ArgumentNullException.ThrowIfNull(placement);
    if (!_publishedRevisions.Add((placement.Sequence, placement.SectionVersion)))
    {
      frame = default;
      return false;
    }

    frame = new ObjectPlacementReplicationFrame(
      placement.Sequence,
      placement.Request.OriginX,
      placement.Request.OriginY,
      placement.Request.ObjectType,
      placement.Request.Style,
      placement.Request.Direction,
      placement.Footprint,
      placement.Sign,
      placement.ProjectileTombstoneReason,
      placement.SectionVersion,
      placement.SectionVersions,
      placement.ProjectileIdentity,
      placement.ProjectileUuid);
    return true;
  }

  public bool TryEncodeObjectPlacement(
    WorldObjectPlacementCommittedEvent placement,
    out byte[] frame)
  {
    if (placement.Request.ObjectType == 0 || placement.Request.Style < 0 ||
        placement.Request.ObjectType > short.MaxValue ||
        placement.Request.Style > short.MaxValue ||
        placement.Request.OriginX < short.MinValue ||
        placement.Request.OriginX > short.MaxValue ||
        placement.Request.OriginY < short.MinValue ||
        placement.Request.OriginY > short.MaxValue ||
        !TryProject(placement, out ObjectPlacementReplicationFrame projected))
    {
      frame = Array.Empty<byte>();
      return false;
    }

    frame = TerrariaPacketCodec.EncodeObjectPlacement(new ObjectPlacementPacket(
      (short)projected.OriginX,
      (short)projected.OriginY,
      (short)projected.ObjectType,
      (short)projected.Style,
      Alternate: 0,
      Random: 0,
      DirectionRight: projected.Direction > 0));
    return true;
  }

  public bool TryEncodeOrderedFrames(
    WorldObjectPlacementCommittedEvent placement,
    byte playerSlot,
    out IReadOnlyList<byte[]> frames)
  {
    if (!TryProject(placement, out ObjectPlacementReplicationFrame projected) ||
        projected.OriginX < short.MinValue || projected.OriginX > short.MaxValue ||
        projected.OriginY < short.MinValue || projected.OriginY > short.MaxValue ||
        projected.ObjectType > short.MaxValue || projected.Style < 0 ||
        projected.Style > short.MaxValue)
    {
      frames = Array.Empty<byte[]>();
      return false;
    }

    List<byte[]> ordered =
    [
      TerrariaPacketCodec.EncodeObjectPlacement(new ObjectPlacementPacket(
        (short)projected.OriginX,
        (short)projected.OriginY,
        (short)projected.ObjectType,
        (short)projected.Style,
        Alternate: 0,
        Random: 0,
        DirectionRight: projected.Direction > 0))
    ];
    if (projected.Sign is Terraria.Dome.Simulation.WorldObjects.SignSnapshot sign)
    {
      if (sign.SignId < short.MinValue || sign.SignId > short.MaxValue ||
          sign.TileX < short.MinValue || sign.TileX > short.MaxValue ||
          sign.TileY < short.MinValue || sign.TileY > short.MaxValue ||
          sign.Text.Length > 100)
      {
        frames = Array.Empty<byte[]>();
        return false;
      }

      ordered.Add(TerrariaPacketCodec.EncodeSignState(new SignReplicationSnapshot(
        sign.SignId,
        checked((short)sign.TileX),
        checked((short)sign.TileY),
        sign.Text,
        playerSlot,
        SuppressOpenSign: false,
        sign.Revision)));
    }

    frames = ordered;
    return true;
  }

  public void Clear()
  {
    _publishedRevisions.Clear();
  }
}

public readonly record struct ObjectPlacementReplicationFrame(
  long Sequence,
  int OriginX,
  int OriginY,
  ushort ObjectType,
  int Style,
  int Direction,
  IReadOnlyList<WorldObjectTileMutation> Footprint,
  Terraria.Dome.Simulation.WorldObjects.SignSnapshot? Sign,
  Terraria.Dome.Simulation.ProjectileTombstoneReason ProjectileTombstoneReason,
  long SectionVersion,
  IReadOnlyDictionary<WorldSectionCoordinates, long> SectionVersions,
  int ProjectileIdentity,
  Guid? ProjectileUuid);
