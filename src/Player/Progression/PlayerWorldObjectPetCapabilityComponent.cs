namespace Terraria.Player.Progression;

// status: implemented-core; integration-blocked; not-verified
// source-members: petFlagDirtiestBlock, petFlagBoulderPet, petFlagRainbowBoulderPet,
// petFlagAxeFairyPet
public sealed class PlayerWorldObjectPetCapabilityComponent
{
  public bool PetFlagDirtiestBlock { get; internal set; }

  public bool PetFlagBoulderPet { get; internal set; }

  public bool PetFlagRainbowBoulderPet { get; internal set; }

  public bool PetFlagAxeFairyPet { get; internal set; }
}
