using System.Numerics;

namespace Terraria.Npc;

public sealed class NpcImmediateEffectStateComponent
{
  public int DespawnEncouragementTicks { get; private set; }

  public int DustCount { get; private set; }

  public bool NetworkUpdateRequested { get; private set; }

  public bool JumpRequested { get; private set; }

  public float JumpVelocityY { get; private set; }

  public bool DoorOpenRequested { get; private set; }

  public int DoorTileX { get; private set; }

  public int DoorTileY { get; private set; }

  public int DoorDirection { get; private set; }

  public bool HomeTeleportRequested { get; private set; }

  public bool HomeTeleportSucceeded { get; private set; }

  public bool HomeTeleportFailed { get; private set; }

  public int HomeTeleportCandidateOffset { get; private set; }

  public NpcTaskFailureReason HomeTeleportFailureReason { get; private set; }

  public Vector2 HomeTeleportPosition { get; private set; }

  public bool HousingRevalidationFailed { get; private set; }

  public bool HousingRegistrySynchronized { get; private set; }

  public Vector2 LastDustPosition { get; private set; }

  public Vector2 LastDustVelocity { get; private set; }

  public void BeginTick()
  {
    ClearPending();
  }

  public void DiscardPendingEffects()
  {
    ClearPending();
  }

  private void ClearPending()
  {
    DespawnEncouragementTicks = 0;
    DustCount = 0;
    NetworkUpdateRequested = false;
    JumpRequested = false;
    JumpVelocityY = 0f;
    DoorOpenRequested = false;
    DoorTileX = 0;
    DoorTileY = 0;
    DoorDirection = 0;
    HomeTeleportRequested = false;
    HomeTeleportSucceeded = false;
    HomeTeleportFailed = false;
    HomeTeleportCandidateOffset = 0;
    HomeTeleportFailureReason = NpcTaskFailureReason.None;
    HomeTeleportPosition = Vector2.Zero;
    HousingRevalidationFailed = false;
    HousingRegistrySynchronized = false;
    LastDustPosition = Vector2.Zero;
    LastDustVelocity = Vector2.Zero;
  }

  public void CommitDespawnEncouragement(int ticks)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ticks);
    DespawnEncouragementTicks = ticks;
  }

  public void CommitDust(Vector2 position, Vector2 velocity)
  {
    DustCount++;
    LastDustPosition = position;
    LastDustVelocity = velocity;
  }

  public void CommitNetworkUpdate()
  {
    NetworkUpdateRequested = true;
  }

  public void CommitJump(float velocityY)
  {
    JumpRequested = true;
    JumpVelocityY = velocityY;
  }

  public void CommitDoorOpen(int tileX, int tileY, int direction)
  {
    DoorOpenRequested = true;
    DoorTileX = tileX;
    DoorTileY = tileY;
    DoorDirection = direction;
  }

  public void CommitHomeTeleport(Vector2 position, int candidateOffset)
  {
    HomeTeleportRequested = true;
    HomeTeleportSucceeded = true;
    HomeTeleportFailed = false;
    HomeTeleportCandidateOffset = candidateOffset;
    HomeTeleportFailureReason = NpcTaskFailureReason.None;
    HomeTeleportPosition = position;
  }

  public void CommitHomeTeleportFailure(NpcTaskFailureReason reason)
  {
    if (reason == NpcTaskFailureReason.None)
    {
      throw new ArgumentException(
        "A home teleport failure must include a reason.",
        nameof(reason));
    }

    HomeTeleportRequested = true;
    HomeTeleportSucceeded = false;
    HomeTeleportFailed = true;
    HomeTeleportFailureReason = reason;
  }

  public void CommitHousingRevalidationFailure()
  {
    HousingRevalidationFailed = true;
  }

  public void CommitHousingRegistrySynchronization()
  {
    HousingRegistrySynchronized = true;
  }
}
