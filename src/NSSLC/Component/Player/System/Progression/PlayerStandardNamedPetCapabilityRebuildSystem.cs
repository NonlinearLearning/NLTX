namespace Terraria.Player.Progression;

public static class PlayerStandardNamedPetCapabilityRebuildSystem
{
  // The caller resolves inputs; pet spawning and projectile lifecycle stay with their owners.
  public static void Rebuild(
    in PlayerStandardNamedPetCapabilityRebuildInput input,
    PlayerStandardNamedPetCapabilityComponent capability)
  {
    ArgumentNullException.ThrowIfNull(capability);

    capability.PetFlagUpbeatStar = input.PetFlagUpbeatStar;
    capability.PetFlagSugarGlider = input.PetFlagSugarGlider;
    capability.PetFlagBabyShark = input.PetFlagBabyShark;
    capability.PetFlagLilHarpy = input.PetFlagLilHarpy;
    capability.PetFlagFennecFox = input.PetFlagFennecFox;
    capability.PetFlagGlitteryButterfly = input.PetFlagGlitteryButterfly;
    capability.PetFlagBabyImp = input.PetFlagBabyImp;
    capability.PetFlagBabyRedPanda = input.PetFlagBabyRedPanda;
    capability.PetFlagPlantero = input.PetFlagPlantero;
    capability.PetFlagDynamiteKitten = input.PetFlagDynamiteKitten;
    capability.PetFlagBabyWerewolf = input.PetFlagBabyWerewolf;
    capability.PetFlagShadowMimic = input.PetFlagShadowMimic;
    capability.PetFlagVoltBunny = input.PetFlagVoltBunny;
  }

  public static void Reset(PlayerStandardNamedPetCapabilityComponent capability)
  {
    Rebuild(default, capability);
  }
}
