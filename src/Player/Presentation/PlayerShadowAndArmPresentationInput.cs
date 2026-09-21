using System.Numerics;

namespace Terraria.Player.Presentation;

public readonly record struct PlayerShadowAndArmPresentationInput(
  long Tick,
  Vector2 Position,
  float GfxOffY,
  float FullRotation,
  Vector2 FullRotationOrigin,
  int Direction,
  bool CursorItemIconReversed = false,
  int RunSoundDelay = 0,
  bool SkipAnimatingValuesInPlayerFrame = false,
  bool ResetSocialShadow = false,
  PlayerCompositeArmSnapshot? FrontArm = null,
  PlayerCompositeArmSnapshot? BackArm = null,
  PlayerAdvancedShadowSlot? AdvancedShadow = null,
  bool AddAdvancedShadow = false);
