using Terraria.Player.Progression;

var capability = new PlayerWorldObjectPetCapabilityComponent();
var input = new PlayerWorldObjectPetCapabilityRebuildInput
{
  PetFlagDirtiestBlock = true,
  PetFlagBoulderPet = true,
  PetFlagRainbowBoulderPet = true,
  PetFlagAxeFairyPet = true,
};

PlayerWorldObjectPetCapabilityRebuildSystem.Rebuild(input, capability);
Assert(
  capability.PetFlagDirtiestBlock &&
    capability.PetFlagBoulderPet &&
    capability.PetFlagRainbowBoulderPet &&
    capability.PetFlagAxeFairyPet,
  "Rebuild should copy all four world-object pet flags.");

PlayerWorldObjectPetCapabilityRebuildSystem.Reset(capability);
Assert(
  !capability.PetFlagDirtiestBlock &&
    !capability.PetFlagBoulderPet &&
    !capability.PetFlagRainbowBoulderPet &&
    !capability.PetFlagAxeFairyPet,
  "Reset should clear all four world-object pet flags.");

Console.WriteLine("PASS: world-object pet rebuild and reset.");

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
