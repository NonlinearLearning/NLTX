using Terraria.Player.Luck;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertClose(float expected, float actual, string message)
{
  if (MathF.Abs(expected - actual) > 0.00001f)
  {
    throw new InvalidOperationException(
      $"{message} Expected '{expected}', actual '{actual}'.");
  }
}

var noLuck = new PlayerLuckCalculationInput(
  LadyBugLuckTimeLeft: 0,
  LadyBugGoodLuckTime: 6000,
  LadyBugBadLuckTime: 6000,
  TorchLuck: 0f,
  LuckPotion: 0,
  KiteLuckLevel: 0,
  UsedGalaxyPearl: false,
  LanternsUp: false,
  HasGardenGnomeNearby: false,
  Stinky: false,
  EquipmentBasedLuckBonus: 0f,
  CoinLuck: 0f,
  BrokenMirrorBadLuck: false);
AssertClose(0f, PlayerLuckCalculationQuery.Calculate(noLuck),
  "No luck sources should produce zero.");

AssertClose(0.2f, PlayerLuckCalculationQuery.Calculate(
  noLuck with { LadyBugLuckTimeLeft = 6000 }),
  "Positive ladybug time should use the good-luck duration.");
AssertClose(0.1f, PlayerLuckCalculationQuery.Calculate(
  noLuck with { LadyBugLuckTimeLeft = -3000 }),
  "Negative ladybug time should use the bad-luck duration.");

var factorState = new PlayerLuckAndRescanStateComponent();
var positiveFactors = PlayerLuckSystem.UpdateFactors(
  new PlayerLuckFactorUpdateInput(5, 0.25f, 2),
  factorState);
Assert(positiveFactors.LadyBugLuckTimeLeft == 3,
  "Positive ladybug luck should decay without crossing zero.");
AssertClose(0f, positiveFactors.CoinLuck,
  "Coin luck below the decay floor should reset to zero.");
Assert(factorState.LadyBugLuckTimeLeft == 3,
  "The factor System should commit the ladybug timer.");
AssertClose(0f, factorState.CoinLuck,
  "The factor System should commit the decayed coin luck.");

var negativeFactors = PlayerLuckSystem.UpdateFactors(
  new PlayerLuckFactorUpdateInput(-2, -1f, 3),
  factorState);
Assert(negativeFactors.LadyBugLuckTimeLeft == 0,
  "Negative ladybug luck should clamp at zero when its timer expires.");
AssertClose(-1f, negativeFactors.CoinLuck,
  "Non-positive coin luck should remain unchanged.");

AssertClose(0.025f, PlayerLuckCalculationQuery.Calculate(
  noLuck with { CoinLuck = 0.249f }),
  "Coin luck at the low threshold should use the lower tier.");
AssertClose(0.05f, PlayerLuckCalculationQuery.Calculate(
  noLuck with { CoinLuck = 0.25f }),
  "Coin luck above the low threshold should use the next tier.");
AssertClose(0.175f, PlayerLuckCalculationQuery.Calculate(
  noLuck with { CoinLuck = 249000f }),
  "Coin luck at the maximum threshold should remain in the lower tier.");
AssertClose(0.2f, PlayerLuckCalculationQuery.Calculate(
  noLuck with { CoinLuck = 249001f }),
  "Coin luck above the maximum threshold should use the top tier.");

var allSources = noLuck with
{
  LadyBugLuckTimeLeft = 6000,
  TorchLuck = 0.5f,
  LuckPotion = 3,
  KiteLuckLevel = 3,
  UsedGalaxyPearl = true,
  LanternsUp = true,
  HasGardenGnomeNearby = true,
  Stinky = true,
  EquipmentBasedLuckBonus = 0.07f,
  CoinLuck = 1000f,
  BrokenMirrorBadLuck = true
};
float expected = PlayerLuckCalculationQuery.Calculate(allSources);
AssertClose(0.925f, expected,
  "The calculation should compose every Version4 luck contribution.");
AssertClose(expected, PlayerLuckCalculationQuery.Calculate(allSources),
  "The calculation should be deterministic for an explicit input.");

var state = new PlayerLuckAndRescanStateComponent();
float committedLuck = PlayerLuckSystem.Recalculate(allSources, state);
AssertClose(expected, committedLuck,
  "The System should return the calculated luck.");
AssertClose(expected, state.Luck,
  "The System should commit luck to its state owner.");
Assert(!state.LuckNeedsSync,
  "Luck recalculation must not claim ownership of the sync flag.");

Console.WriteLine(
  "PASS: player luck thresholds, contributions and single-owner commit");
