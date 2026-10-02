namespace Terraria.Player.Progression;

public static class PlayerCrossoverPetCapabilityRebuildSystem
{
  // Caller supplies same-tick flags; projectile death and lifetime effects remain external.
  public static void Rebuild(
    in PlayerCrossoverPetCapabilityRebuildInput input,
    PlayerCrossoverPetCapabilityComponent capability)
  {
    ArgumentNullException.ThrowIfNull(capability);

    capability.PetFlagBerniePet = input.PetFlagBerniePet;
    capability.PetFlagGlommerPet = input.PetFlagGlommerPet;
    capability.PetFlagDeerclopsPet = input.PetFlagDeerclopsPet;
    capability.PetFlagPigPet = input.PetFlagPigPet;
    capability.PetFlagChesterPet = input.PetFlagChesterPet;
    capability.PetFlagJunimoPet = input.PetFlagJunimoPet;
    capability.PetFlagBlueChickenPet = input.PetFlagBlueChickenPet;
    capability.PetFlagSpiffo = input.PetFlagSpiffo;
    capability.PetFlagCaveling = input.PetFlagCaveling;
    capability.PetFlagDeadCellsSwarmBiter = input.PetFlagDeadCellsSwarmBiter;
    capability.PetFlagPufferfish = input.PetFlagPufferfish;
    capability.PetFlagChillet = input.PetFlagChillet;
    capability.PetFlagChilletIgnis = input.PetFlagChilletIgnis;
  }

  public static void Reset(PlayerCrossoverPetCapabilityComponent capability)
  {
    Rebuild(default, capability);
  }
}
