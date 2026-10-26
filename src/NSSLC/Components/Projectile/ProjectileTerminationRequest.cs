using System;
using Terraria.Relationships;

namespace Terraria.Projectile;

/// <summary>One tick-scoped request to terminate a projectile runtime handle.</summary>
public readonly record struct ProjectileTerminationRequest
{
  public ProjectileTerminationRequest(
      EntityRuntimeHandle handle,
      ProjectileTerminationReason reason,
      long tick,
      long requestId)
  {
    if (!handle.IsValid || !Enum.IsDefined(reason) || tick < 0 || requestId < 0)
    {
      throw new ArgumentException("A valid projectile termination request is required.");
    }

    Handle = handle;
    Reason = reason;
    Tick = tick;
    RequestId = requestId;
  }

  public EntityRuntimeHandle Handle { get; }
  public ProjectileTerminationReason Reason { get; }
  public long Tick { get; }
  public long RequestId { get; }
}
