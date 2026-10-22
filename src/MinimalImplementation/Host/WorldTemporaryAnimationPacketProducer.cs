using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

/// <summary>Encodes one authoritative world temporary-animation broadcast.</summary>
public sealed class WorldTemporaryAnimationPacketProducer
{
  private readonly Version4WireProfile _wireProfile;

  public WorldTemporaryAnimationPacketProducer(Version4WireProfile wireProfile)
  {
    _wireProfile = wireProfile ?? throw new ArgumentNullException(nameof(wireProfile));
  }

  public OutboundPacketIntent CreateIntent(
      WorldTemporaryAnimationIntent intent,
      IReadOnlyCollection<SessionEpoch> recipients,
      CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(recipients);
    if (intent.WorldEpoch == 0)
    {
      throw new ArgumentException(
        "A temporary-animation intent requires a non-zero world epoch.",
        nameof(intent));
    }

    if (!intent.Version.IsValid)
    {
      throw new ArgumentException(
        "A temporary-animation intent requires a valid captured world version.",
        nameof(intent));
    }

    if (!commitVersion.IsValid)
    {
      throw new ArgumentException(
        "A temporary-animation broadcast requires a valid commit version.",
        nameof(commitVersion));
    }

    if (intent.Version != commitVersion)
    {
      throw new ArgumentException(
        "The temporary-animation commit version must match the captured world version.",
        nameof(commitVersion));
    }

    if (recipients.Count == 0)
    {
      throw new ArgumentException(
        "A temporary-animation broadcast requires at least one recipient.",
        nameof(recipients));
    }

    var targetSet = new HashSet<SessionEpoch>();
    var targetEpochs = new List<SessionEpoch>(recipients.Count);
    foreach (SessionEpoch recipient in recipients)
    {
      if (!recipient.IsValid)
      {
        throw new ArgumentException(
          "Every temporary-animation recipient must have a valid session epoch.",
          nameof(recipients));
      }

      if (!targetSet.Add(recipient))
      {
        throw new ArgumentException(
          "A temporary-animation broadcast cannot contain duplicate recipients.",
          nameof(recipients));
      }

      targetEpochs.Add(recipient);
    }

    if (targetEpochs.Count == 0)
    {
      throw new ArgumentException(
        "A temporary-animation broadcast requires at least one recipient.",
        nameof(recipients));
    }

    if (intent.Coordinate.X < short.MinValue || intent.Coordinate.X > short.MaxValue ||
        intent.Coordinate.Y < short.MinValue || intent.Coordinate.Y > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(
        nameof(intent),
        intent.Coordinate,
        "Temporary-animation coordinates must fit the packet's signed Int16 fields.");
    }

    var packet = new TemporaryAnimationPacket
    {
      AnimationType = intent.AnimationType,
      TileType = intent.TileType,
      X = (short)intent.Coordinate.X,
      Y = (short)intent.Coordinate.Y
    };
    Version4EncodedPacket encoded = _wireProfile.Encode(
      WireDirection.ServerToClient,
      packet);
    return new OutboundPacketIntent(
      targetEpochs,
      ignoreEpoch: null,
      encoded.MessageId,
      encoded.Payload,
      commitVersion);
  }
}
