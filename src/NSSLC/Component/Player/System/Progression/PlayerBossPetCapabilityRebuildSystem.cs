namespace Terraria.Player.Progression;

public static class PlayerBossPetCapabilityRebuildSystem
{
  // The caller resolves current-tick inputs; pet spawning and projectile lifecycle stay separate.
  public static void Rebuild(
    in PlayerBossPetCapabilityRebuildInput input,
    PlayerBossPetCapabilityComponent capability)
  {
    ArgumentNullException.ThrowIfNull(capability);

    capability.PetFlagKingSlimePet = input.PetFlagKingSlimePet;
    capability.PetFlagEyeOfCthulhuPet = input.PetFlagEyeOfCthulhuPet;
    capability.PetFlagEaterOfWorldsPet = input.PetFlagEaterOfWorldsPet;
    capability.PetFlagBrainOfCthulhuPet = input.PetFlagBrainOfCthulhuPet;
    capability.PetFlagSkeletronPet = input.PetFlagSkeletronPet;
    capability.PetFlagQueenBeePet = input.PetFlagQueenBeePet;
    capability.PetFlagDestroyerPet = input.PetFlagDestroyerPet;
    capability.PetFlagTwinsPet = input.PetFlagTwinsPet;
    capability.PetFlagSkeletronPrimePet = input.PetFlagSkeletronPrimePet;
    capability.PetFlagPlanteraPet = input.PetFlagPlanteraPet;
    capability.PetFlagGolemPet = input.PetFlagGolemPet;
    capability.PetFlagDukeFishronPet = input.PetFlagDukeFishronPet;
    capability.PetFlagLunaticCultistPet = input.PetFlagLunaticCultistPet;
    capability.PetFlagMoonLordPet = input.PetFlagMoonLordPet;
    capability.PetFlagFairyQueenPet = input.PetFlagFairyQueenPet;
    capability.PetFlagQueenSlimePet = input.PetFlagQueenSlimePet;
  }

  public static void Reset(PlayerBossPetCapabilityComponent capability)
  {
    Rebuild(default, capability);
  }
}
