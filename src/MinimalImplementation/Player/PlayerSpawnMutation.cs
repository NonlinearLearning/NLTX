using System;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

public sealed record PlayerSpawnMutation(
  OwnerCommitResult Result,
  PlayerSpawnPacket? Packet)
{
  public static PlayerSpawnMutation Rejected(
    ProtocolErrorCode errorCode,
    string detail)
  {
    return new PlayerSpawnMutation(
      OwnerCommitResult.Rejected(errorCode, detail),
      null);
  }

  public static PlayerSpawnMutation Applied(
    CommitVersion version,
    PlayerSpawnPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    return new PlayerSpawnMutation(
      OwnerCommitResult.Applied(version),
      packet);
  }
}
