using System.Numerics;

namespace Terraria.Npc;

public sealed class NpcMovementTickStateComponent
{
  public Vector2 OldPosition { get; private set; }

  public Vector2 OldVelocity { get; private set; }

  public bool CollideX { get; private set; }

  public bool CollideY { get; private set; }

  public bool Wet { get; private set; }

  public bool ShimmerWet { get; private set; }

  public bool HoneyWet { get; private set; }

  public bool NoGravity { get; private set; }

  public bool NoTileCollide { get; private set; }

  public void BeginTick(
    Vector2 position,
    Vector2 velocity,
    bool wet,
    bool shimmerWet,
    bool honeyWet)
  {
    OldPosition = position;
    OldVelocity = velocity;
    CollideX = false;
    CollideY = false;
    Wet = wet;
    ShimmerWet = shimmerWet;
    HoneyWet = honeyWet;
    NoGravity = false;
    NoTileCollide = false;
  }

  public void CommitPhysicsFlags(bool noGravity, bool noTileCollide)
  {
    NoGravity = noGravity;
    NoTileCollide = noTileCollide;
  }

  public void CommitCollision(bool collideX, bool collideY)
  {
    CollideX = collideX;
    CollideY = collideY;
  }
}
