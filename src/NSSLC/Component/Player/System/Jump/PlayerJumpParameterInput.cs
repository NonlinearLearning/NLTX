namespace Terraria.Player.Jump;

public readonly record struct PlayerJumpParameterInput(
  int JumpHeight,
  float JumpSpeed,
  bool MountActive,
  int MountJumpHeight,
  float MountJumpSpeed,
  bool JumpBoost,
  bool EmpressBrooch,
  bool FrogLegJumpBoost,
  bool MoonLordLegs,
  bool WereWolf,
  bool PortableStoolInUse,
  bool Sticky,
  bool Dazed);
