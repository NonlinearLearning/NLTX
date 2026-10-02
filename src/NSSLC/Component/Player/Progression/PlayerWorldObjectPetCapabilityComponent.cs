namespace Terraria.Player.Progression;

// status: local-rebuild-verified; production-integration: unknown
// source-members: petFlagDirtiestBlock, petFlagBoulderPet, petFlagRainbowBoulderPet,
// petFlagAxeFairyPet
public sealed class PlayerWorldObjectPetCapabilityComponent
{
  public bool PetFlagDirtiestBlock { get; internal set; }

  public bool PetFlagBoulderPet { get; internal set; }

  public bool PetFlagRainbowBoulderPet { get; internal set; }

  public bool PetFlagAxeFairyPet { get; internal set; }
}
