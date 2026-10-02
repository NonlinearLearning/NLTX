using Terraria.Player.Progression;

var capability = new PlayerLegacyPetCapabilityComponent();
var input = new PlayerLegacyPetCapabilityRebuildInput
{
  SuspiciousLookingTentacle = true,
  CrimsonHeart = true,
  LightOrb = true,
  BlueFairy = true,
  RedFairy = true,
  GreenFairy = true,
  Bunny = true,
  Turtle = true,
  Eater = true,
  Penguin = true,
  HasGardenGnomeNearby = true,
  MagicLantern = true,
  Rabid = true,
  Sunflower = true,
  WellFed = true,
  Puppy = true,
  Grinch = true,
  MiniMinotaur = true,
  BlackCat = true,
  Spider = true,
  Squashling = true,
};

PlayerLegacyPetCapabilityRebuildSystem.Rebuild(input, capability);
Assert(AllFlagsSet(capability), "Rebuild should copy all 21 legacy fields.");

PlayerLegacyPetCapabilityRebuildSystem.ResetTickFlags(capability);
Assert(TickFlagsCleared(capability), "Reset should clear all 20 tick flags.");
Assert(
  capability.HasGardenGnomeNearby,
  "Reset should preserve the independently synchronized garden-gnome flag.");

Console.WriteLine("PASS: legacy pet rebuild and tick reset preserve garden-gnome state.");

static bool AllFlagsSet(PlayerLegacyPetCapabilityComponent capability)
{
  return capability.SuspiciousLookingTentacle &&
    capability.CrimsonHeart &&
    capability.LightOrb &&
    capability.BlueFairy &&
    capability.RedFairy &&
    capability.GreenFairy &&
    capability.Bunny &&
    capability.Turtle &&
    capability.Eater &&
    capability.Penguin &&
    capability.HasGardenGnomeNearby &&
    capability.MagicLantern &&
    capability.Rabid &&
    capability.Sunflower &&
    capability.WellFed &&
    capability.Puppy &&
    capability.Grinch &&
    capability.MiniMinotaur &&
    capability.BlackCat &&
    capability.Spider &&
    capability.Squashling;
}

static bool TickFlagsCleared(PlayerLegacyPetCapabilityComponent capability)
{
  return !capability.SuspiciousLookingTentacle &&
    !capability.CrimsonHeart &&
    !capability.LightOrb &&
    !capability.BlueFairy &&
    !capability.RedFairy &&
    !capability.GreenFairy &&
    !capability.Bunny &&
    !capability.Turtle &&
    !capability.Eater &&
    !capability.Penguin &&
    !capability.MagicLantern &&
    !capability.Rabid &&
    !capability.Sunflower &&
    !capability.WellFed &&
    !capability.Puppy &&
    !capability.Grinch &&
    !capability.MiniMinotaur &&
    !capability.BlackCat &&
    !capability.Spider &&
    !capability.Squashling;
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
