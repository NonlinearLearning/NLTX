using System;
using Terraria.Dome.Simulation.Movement.Components;
using Terraria.Dome.Simulation.Npc.Components;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcHomeResult(
  float HorizontalVelocity,
  float VerticalVelocity,
  int Facing,
  bool IsAtHome,
  int RemainingTimeoutTicks);

public sealed class NpcHomeSystem
{
  private const float HomeArrivalDistance = 0.1f;
  private const float DefaultHomeSpeed = 0.5f;

  public NpcHomeResult Advance(
    ref NpcHomeComponent home,
    ref MovementIntentComponent movementIntent,
    SimulationVector position,
    bool isDayTime)
  {
    return Advance(
      ref home,
      ref movementIntent,
      position,
      isDayTime,
      DefaultHomeSpeed);
  }

  public NpcHomeResult Advance(
    ref NpcHomeComponent home,
    ref MovementIntentComponent movementIntent,
    SimulationVector position,
    bool isDayTime,
    float speed)
  {
    if (speed < 0.0f || !float.IsFinite(speed))
    {
      throw new ArgumentOutOfRangeException(nameof(speed));
    }

    if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(position));
    }

    movementIntent.HasNpcIntent = true;
    movementIntent.NpcHorizontalVelocity = 0.0f;
    movementIntent.NpcVerticalVelocity = 0.0f;
    movementIntent.HorizontalDirection = 0;

    if (!isDayTime || home.IsHomeless)
    {
      home.ReturnTimeoutTicks = Math.Max(0, home.ReturnTimeoutTicks - 1);
      return new(0.0f, 0.0f, 0, false, home.ReturnTimeoutTicks);
    }

    float homeX = home.HomeTileX;
    float homeY = home.HomeTileY;
    float horizontalDistance = homeX - position.X;
    float verticalDistance = homeY - position.Y;
    bool isAtHome = MathF.Abs(horizontalDistance) <= HomeArrivalDistance &&
      MathF.Abs(verticalDistance) <= HomeArrivalDistance;
    if (isAtHome || speed == 0.0f)
    {
      home.ReturnTimeoutTicks = 0;
      return new(0.0f, 0.0f, 0, true, 0);
    }

    int facing = MathF.Sign(horizontalDistance) switch
    {
      > 0 => 1,
      < 0 => -1,
      _ => 0
    };
    home.ReturnTimeoutTicks = Math.Max(0, home.ReturnTimeoutTicks - 1);
    movementIntent.NpcHorizontalVelocity = facing * speed;
    movementIntent.NpcVerticalVelocity = 0.0f;
    movementIntent.HorizontalDirection = facing;
    return new(
      facing * speed,
      0.0f,
      facing,
      false,
      home.ReturnTimeoutTicks);
  }
}
