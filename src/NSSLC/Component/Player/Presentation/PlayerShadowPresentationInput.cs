using System.Numerics;

namespace Terraria.Player.Presentation;

public readonly record struct PlayerShadowPresentationInput(
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
  PlayerAdvancedShadowSlot? AdvancedShadow = null,
  bool AddAdvancedShadow = false);
