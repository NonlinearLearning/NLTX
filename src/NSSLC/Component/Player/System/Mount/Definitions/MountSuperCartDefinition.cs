namespace Terraria.Player.Mount;

public readonly record struct MountSuperCartDefinition(
  float RunSpeed,
  float DashSpeed,
  float Acceleration,
  int JumpHeight,
  float JumpSpeed)
{
  public static MountSuperCartDefinition Version4 => new(20f, 20f, 0.1f, 15, 5.15f);
}
