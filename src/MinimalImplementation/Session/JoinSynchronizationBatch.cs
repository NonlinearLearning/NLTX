using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Session;

/// <summary>
/// A provider-owned, already validated set of packets for one player join.
/// </summary>
public sealed class JoinSynchronizationBatch
{
  public JoinSynchronizationBatch(
    long worldGeneration,
    StatusTextSizePacket progress,
    IReadOnlyCollection<JoinBatchPacket> packets)
  {
    if (worldGeneration <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldGeneration));
    }

    ArgumentNullException.ThrowIfNull(progress);
    ArgumentNullException.ThrowIfNull(packets);
    if (packets.Count == 0)
    {
      throw new ArgumentException(
        "A join batch must contain at least one real synchronization packet.",
        nameof(packets));
    }

    WorldGeneration = worldGeneration;
    Progress = progress;
    Packets = Array.AsReadOnly(packets.ToArray());
  }

  public long WorldGeneration { get; }

  public StatusTextSizePacket Progress { get; }

  public IReadOnlyList<JoinBatchPacket> Packets { get; }
}

/// <summary>One typed packet supplied by a world or domain snapshot producer.</summary>
public sealed record JoinBatchPacket
{
  public JoinBatchPacket(byte messageId, object packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    MessageId = messageId;
    Packet = packet;
  }

  public byte MessageId { get; }

  public object Packet { get; }
}
