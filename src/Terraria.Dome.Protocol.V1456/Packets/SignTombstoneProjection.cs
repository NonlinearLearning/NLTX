using System;
using Terraria.Dome.Simulation.WorldObjects.Sign;

namespace Terraria.Dome.Protocol.V1456.Packets;

public enum SignTombstoneProjectionStatus
{
  Deferred = 1,
  Rejected = 2
}

public readonly record struct SignTombstoneProjection(
  int SignId,
  long Revision,
  SignTombstoneReason Reason,
  SignTombstoneProjectionStatus Status,
  string Detail)
{
  public static SignTombstoneProjection Create(SignTombstoneSnapshot tombstone)
  {
    if (tombstone.SignId < 0 || tombstone.SignId > short.MaxValue || tombstone.Revision < 0 ||
        !Enum.IsDefined(tombstone.Reason))
    {
      return new SignTombstoneProjection(
        tombstone.SignId,
        tombstone.Revision,
        tombstone.Reason,
        SignTombstoneProjectionStatus.Rejected,
        "The V1456 sign wire shape cannot represent this tombstone.");
    }

    return new SignTombstoneProjection(
      tombstone.SignId,
      tombstone.Revision,
      tombstone.Reason,
      SignTombstoneProjectionStatus.Deferred,
      "V1456 message 47 has no deletion shape; " +
      "retain tombstone until a versioned extension exists.");
  }
}
