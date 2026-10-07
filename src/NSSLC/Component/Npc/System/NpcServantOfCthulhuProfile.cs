using System.Numerics;

namespace Terraria.Npc;

public static class NpcServantOfCthulhuProfile
{
  private const int ServantOfCthulhuTypeId = 5;
  private const int ServantOfCthulhuNetId = 5;
  private const int ServantOfCthulhuAiStyle = 5;
  private const float TargetGridSize = 8f;
  private const float TargetSpeed = 5f;
  private const float TargetAcceleration = 0.03f;

  public static bool CanHandle(int typeId, int netId, int aiStyle)
  {
    return typeId == ServantOfCthulhuTypeId &&
      netId == ServantOfCthulhuNetId &&
      aiStyle == ServantOfCthulhuAiStyle;
  }

  public static NpcServantOfCthulhuProfileResult Evaluate(
    in NpcServantOfCthulhuProfileInput input)
  {
    if (!CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Servant of Cthulhu profile requires type=5, netID=5, and aiStyle=5.");
    }

    Vector2 npcCenter = input.Position + new Vector2(input.Width * 0.5f, input.Height * 0.5f);
    float targetX = RoundToTargetGrid(input.TargetCenter.X) - RoundToTargetGrid(npcCenter.X);
    float targetY = RoundToTargetGrid(input.TargetCenter.Y) - RoundToTargetGrid(npcCenter.Y);
    float targetDistance = MathF.Sqrt(targetX * targetX + targetY * targetY);
    if (targetDistance > 0f)
    {
      float scale = TargetSpeed / targetDistance;
      targetX *= scale;
      targetY *= scale;
    }
    else
    {
      targetX = input.Velocity.X;
      targetY = input.Velocity.Y;
    }

    Vector2 velocity = new(
      Accelerate(input.Velocity.X, targetX),
      Accelerate(input.Velocity.Y, targetY));
    return new NpcServantOfCthulhuProfileResult(IsSupported: true, velocity);
  }

  private static float RoundToTargetGrid(float coordinate)
  {
    return (int)(coordinate / TargetGridSize) * TargetGridSize;
  }

  private static float Accelerate(float velocity, float targetVelocity)
  {
    if (velocity < targetVelocity)
    {
      velocity += TargetAcceleration;
      if (velocity < 0f && targetVelocity > 0f)
      {
        velocity += TargetAcceleration;
      }
    }
    else if (velocity > targetVelocity)
    {
      velocity -= TargetAcceleration;
      if (velocity > 0f && targetVelocity < 0f)
      {
        velocity -= TargetAcceleration;
      }
    }

    return velocity;
  }
}
