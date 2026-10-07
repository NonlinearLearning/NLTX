namespace Terraria.Player.Environment;

[System.Flags]
public enum PlayerSunScorchEffectRequest : ushort
{
  None = 0,
  RefreshArmorFrameAndAchievement = 1 << 0,
  ClearBuffImmunity = 1 << 1,
  SpawnVampireOnFireParticle = 1 << 2,
  AddOnFireBuff = 1 << 3,
  AddCursedInfernoBuff = 1 << 4,
  AddShadowFlameBuff = 1 << 5,
  Dismount = 1 << 6,
  ClearWings = 1 << 7,
  ClearRocketBoots = 1 << 8
}
