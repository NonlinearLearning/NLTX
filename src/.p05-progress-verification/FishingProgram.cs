using Terraria.Player.Progression;

var snapshot = new PlayerFishingCapabilitySnapshotComponent();
var input = new PlayerFishingCapabilityRebuildInput(
  FishingSkill: 50,
  CratePotion: true,
  SonarPotion: true,
  AccFishingLine: true,
  AccFishingBobber: false,
  AccTackleBox: true,
  AccLavaFishing: true);

PlayerFishingCapabilityRebuildSystem.Rebuild(input, ref snapshot);

Assert(snapshot.FishingSkill == 50, "Fishing skill should be rebuilt from explicit input.");
Assert(snapshot.CratePotion, "Crate potion should be rebuilt.");
Assert(snapshot.SonarPotion, "Sonar potion should be rebuilt.");
Assert(snapshot.AccFishingLine, "Fishing line accessory should be rebuilt.");
Assert(!snapshot.AccFishingBobber, "Bobber accessory should preserve false input.");
Assert(snapshot.AccTackleBox, "Tackle box accessory should be rebuilt.");
Assert(snapshot.AccLavaFishing, "Lava fishing capability should be rebuilt.");
Assert(FishingCapabilityQuery.HasCratePotion(snapshot), "Crate query should read the snapshot.");
Assert(FishingCapabilityQuery.CanFishInLava(snapshot), "Lava query should read the snapshot.");

PlayerFishingCapabilityRebuildSystem.Reset(ref snapshot);

Assert(snapshot.FishingSkill == 0, "Reset should clear fishing skill.");
Assert(!FishingCapabilityQuery.HasCratePotion(snapshot), "Reset should clear crate capability.");
Assert(!FishingCapabilityQuery.CanFishInLava(snapshot), "Reset should clear lava capability.");

Console.WriteLine("P05 C02 verifier passed.");

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
