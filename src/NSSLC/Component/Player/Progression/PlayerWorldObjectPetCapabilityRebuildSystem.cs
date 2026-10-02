namespace Terraria.Player.Progression;

public static class PlayerWorldObjectPetCapabilityRebuildSystem
{
  // Caller resolves these tick flags; projectile lifetime and death effects stay external.
  public static void Rebuild(
    in PlayerWorldObjectPetCapabilityRebuildInput input,
    PlayerWorldObjectPetCapabilityComponent capability)
  {
    ArgumentNullException.ThrowIfNull(capability);

    capability.PetFlagDirtiestBlock = input.PetFlagDirtiestBlock;
    capability.PetFlagBoulderPet = input.PetFlagBoulderPet;
    capability.PetFlagRainbowBoulderPet = input.PetFlagRainbowBoulderPet;
    capability.PetFlagAxeFairyPet = input.PetFlagAxeFairyPet;
  }

  public static void Reset(PlayerWorldObjectPetCapabilityComponent capability)
  {
    Rebuild(default, capability);
  }
}
