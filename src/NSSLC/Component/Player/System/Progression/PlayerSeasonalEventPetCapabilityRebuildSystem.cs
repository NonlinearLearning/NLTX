namespace Terraria.Player.Progression;

public static class PlayerSeasonalEventPetCapabilityRebuildSystem
{
  // The caller resolves current-tick inputs; pet entity lifecycle remains a separate owner.
  public static void Rebuild(
    in PlayerSeasonalEventPetCapabilityRebuildInput input,
    PlayerSeasonalEventPetCapabilityComponent capability)
  {
    ArgumentNullException.ThrowIfNull(capability);

    capability.PetFlagDD2Gato = input.PetFlagDD2Gato;
    capability.PetFlagDD2Ghost = input.PetFlagDD2Ghost;
    capability.PetFlagDD2Dragon = input.PetFlagDD2Dragon;
    capability.PetFlagPumpkingPet = input.PetFlagPumpkingPet;
    capability.PetFlagEverscreamPet = input.PetFlagEverscreamPet;
    capability.PetFlagIceQueenPet = input.PetFlagIceQueenPet;
    capability.PetFlagMartianPet = input.PetFlagMartianPet;
    capability.PetFlagDD2OgrePet = input.PetFlagDD2OgrePet;
    capability.PetFlagDD2BetsyPet = input.PetFlagDD2BetsyPet;
  }

  public static void Reset(PlayerSeasonalEventPetCapabilityComponent capability)
  {
    Rebuild(default, capability);
  }
}
