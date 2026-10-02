namespace Terraria.Player.Progression;

public static class PlayerLegacyPetCapabilityRebuildSystem
{
  // Garden-gnome state has a separate network and luck lifecycle from these tick flags.
  public static void Rebuild(
    in PlayerLegacyPetCapabilityRebuildInput input,
    PlayerLegacyPetCapabilityComponent capability)
  {
    ArgumentNullException.ThrowIfNull(capability);

    capability.SuspiciousLookingTentacle = input.SuspiciousLookingTentacle;
    capability.CrimsonHeart = input.CrimsonHeart;
    capability.LightOrb = input.LightOrb;
    capability.BlueFairy = input.BlueFairy;
    capability.RedFairy = input.RedFairy;
    capability.GreenFairy = input.GreenFairy;
    capability.Bunny = input.Bunny;
    capability.Turtle = input.Turtle;
    capability.Eater = input.Eater;
    capability.Penguin = input.Penguin;
    capability.HasGardenGnomeNearby = input.HasGardenGnomeNearby;
    capability.MagicLantern = input.MagicLantern;
    capability.Rabid = input.Rabid;
    capability.Sunflower = input.Sunflower;
    capability.WellFed = input.WellFed;
    capability.Puppy = input.Puppy;
    capability.Grinch = input.Grinch;
    capability.MiniMinotaur = input.MiniMinotaur;
    capability.BlackCat = input.BlackCat;
    capability.Spider = input.Spider;
    capability.Squashling = input.Squashling;
  }

  public static void ResetTickFlags(PlayerLegacyPetCapabilityComponent capability)
  {
    ArgumentNullException.ThrowIfNull(capability);

    capability.SuspiciousLookingTentacle = false;
    capability.CrimsonHeart = false;
    capability.LightOrb = false;
    capability.BlueFairy = false;
    capability.RedFairy = false;
    capability.GreenFairy = false;
    capability.Bunny = false;
    capability.Turtle = false;
    capability.Eater = false;
    capability.Penguin = false;
    capability.MagicLantern = false;
    capability.Rabid = false;
    capability.Sunflower = false;
    capability.WellFed = false;
    capability.Puppy = false;
    capability.Grinch = false;
    capability.MiniMinotaur = false;
    capability.BlackCat = false;
    capability.Spider = false;
    capability.Squashling = false;
  }
}
