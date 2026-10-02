namespace Terraria.Player.Progression;

public readonly record struct PlayerWorldObjectPetCapabilityRebuildInput
{
  public bool PetFlagDirtiestBlock { get; init; }

  public bool PetFlagBoulderPet { get; init; }

  public bool PetFlagRainbowBoulderPet { get; init; }

  public bool PetFlagAxeFairyPet { get; init; }
}
