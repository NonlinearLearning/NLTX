namespace Terraria.Player.Mount;

public readonly record struct MountMovementDefinition(
  int FlightTimeMax,
  bool UsesHover,
  float RunSpeed,
  float DashSpeed,
  float SwimSpeed,
  float Acceleration,
  float JumpSpeed,
  int JumpHeight,
  float FallDamage,
  int ExtraFall,
  float FatigueMax,
  bool ConstantJump,
  bool BlockExtraJumps,
  bool IsMinecart,
  bool CanRideMinecartTracks,
  bool CanUseWings,
  int WalkingGraceTimeMax,
  bool DismountsOnItemUse)
{
  public void Validate()
  {
    if (FlightTimeMax < 0 || JumpHeight < 0 || ExtraFall < 0 ||
      WalkingGraceTimeMax < 0 || FatigueMax < 0f ||
      float.IsNaN(RunSpeed) || float.IsNaN(DashSpeed) ||
      float.IsNaN(SwimSpeed) || float.IsNaN(Acceleration) ||
      float.IsNaN(JumpSpeed) || float.IsNaN(FallDamage))
    {
      throw new ArgumentOutOfRangeException(nameof(FlightTimeMax));
    }
  }
}
