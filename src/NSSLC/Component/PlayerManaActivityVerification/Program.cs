using Terraria.Player;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertEqual<T>(T expected, T actual, string message)
{
  if (!EqualityComparer<T>.Default.Equals(expected, actual))
  {
    throw new InvalidOperationException(
      $"{message} Expected '{expected}', actual '{actual}'.");
  }
}

static void AssertNear(float expected, float actual, string message)
{
  if (MathF.Abs(expected - actual) > 0.00001f)
  {
    throw new InvalidOperationException(
      $"{message} Expected '{expected}', actual '{actual}'.");
  }
}

ManaActivityPolicyDefinition policy = ManaActivityPolicyDefinition.Default;
AssertEqual(300, policy.ManaSickTime,
  "The mana sickness duration must preserve the Version4 policy default.");
AssertNear(0.25f, policy.ManaSickLessDamage,
  "The mana sickness damage reduction must preserve the Version4 policy default.");
AssertEqual(300, policy.AfkTimeNeededForNoWormSpawns,
  "The worm-spawn AFK threshold must preserve the Version4 policy default.");
AssertEqual(10800, policy.AfkTimeNeededForNoLuckyStars,
  "The lucky-star AFK threshold must preserve the Version4 policy default.");

AssertNear(0.125f, policy.CalculateManaSickReduction(true, 150),
  "Half-duration mana sickness must produce half the maximum reduction.");
AssertNear(0.25f, policy.CalculateManaSickReduction(true, 300),
  "Full-duration mana sickness must produce the maximum reduction.");
AssertNear(0.25f, policy.CalculateManaSickReduction(true, 600),
  "Mana sickness reduction must be capped at the policy maximum.");
AssertNear(0f, policy.CalculateManaSickReduction(true, -1),
  "Negative buff time must not produce mana sickness reduction.");
AssertNear(0f, policy.CalculateManaSickReduction(false, 300),
  "An inactive mana sickness buff must not produce reduction.");

var manaComponent = new PlayerManaActivityComponent();
var regenComponent = new PlayerManaRegenModifierComponent();
var activityQuery = new PlayerActivityQuery();
var system = new ManaStatusSystem(
  manaComponent,
  regenComponent,
  policy,
  activityQuery);

system.RebuildManaStatus(new PlayerManaStatusInput(
  ManaSickBuffActive: true,
  ManaSickBuffTime: 150,
  ManaRegenBonus: 25,
  ManaRegenDelayBonus: 1f));
Assert(manaComponent.ManaSick,
  "An active mana sickness input must commit the sickness capability.");
AssertNear(0.125f, manaComponent.ManaSickReduction,
  "The system must commit the policy-calculated sickness reduction.");
AssertEqual(25, regenComponent.ManaRegenBonus,
  "The system must commit the aggregate mana regeneration bonus.");
AssertNear(1f, regenComponent.ManaRegenDelayBonus,
  "The system must commit the aggregate mana regeneration delay bonus.");

system.RebuildManaStatus(new PlayerManaStatusInput(
  ManaSickBuffActive: true,
  ManaSickBuffTime: 150,
  ManaRegenBonus: 25,
  ManaRegenDelayBonus: 1f));
AssertEqual(25, regenComponent.ManaRegenBonus,
  "Replaying an aggregate status input must not accumulate the regen bonus twice.");
AssertNear(1f, regenComponent.ManaRegenDelayBonus,
  "Replaying an aggregate status input must not accumulate delay bonus twice.");

system.ResetEffects();
Assert(!manaComponent.ManaSick,
  "ResetEffects must clear the transient mana sickness capability.");
AssertNear(0f, manaComponent.ManaSickReduction,
  "ResetEffects must clear the derived sickness reduction.");
AssertEqual(0, regenComponent.ManaRegenBonus,
  "ResetEffects must clear the mana regeneration bonus.");
AssertNear(0f, regenComponent.ManaRegenDelayBonus,
  "ResetEffects must clear the mana regeneration delay bonus.");

PlayerActivityInput stillInput = new(
  IsConsideredStandingStill: true,
  IsItemAnimationActive: false,
  HeldItemIsKite: false,
  HasMovementInput: false,
  HasControlInput: false,
  HasAttackInput: false,
  HasMouseItem: false,
  IsPetting: false,
  IsSitting: false,
  IsSleeping: false,
  IsPaused: false,
  IsDead: false);
system.AdvanceActivity(stillInput);
system.AdvanceActivity(stillInput);
AssertEqual(2, manaComponent.AfkCounter,
  "A qualifying stationary tick must increment the normal AFK counter.");
AssertEqual(2, manaComponent.AfkCounterForKiting,
  "A qualifying stationary tick must increment the kiting AFK counter.");

PlayerActivityInput mouseItemInput = stillInput with { HasMouseItem = true };
system.AdvanceActivity(mouseItemInput);
AssertEqual(3, manaComponent.AfkCounter,
  "A mouse item must not clear the normal stationary AFK counter.");
AssertEqual(0, manaComponent.AfkCounterForKiting,
  "A mouse item must clear the kiting AFK counter.");

PlayerActivityInput movementInput = stillInput with { HasMovementInput = true };
system.AdvanceActivity(movementInput);
AssertEqual(0, manaComponent.AfkCounter,
  "Movement input must clear the normal AFK counter.");
AssertEqual(0, manaComponent.AfkCounterForKiting,
  "Movement input must clear the kiting AFK counter.");

system.AdvanceActivity(stillInput);
PlayerActivityInput attackInput = stillInput with { HasAttackInput = true };
system.AdvanceActivity(attackInput);
AssertEqual(0, manaComponent.AfkCounter,
  "Attack input must clear the normal AFK counter.");
AssertEqual(0, manaComponent.AfkCounterForKiting,
  "Attack input must clear the kiting AFK counter.");

system.AdvanceActivity(stillInput);
PlayerActivityInput pausedInput = stillInput with { IsPaused = true };
system.AdvanceActivity(pausedInput);
AssertEqual(0, manaComponent.AfkCounter,
  "Paused ticks must clear the normal AFK counter.");
AssertEqual(0, manaComponent.AfkCounterForKiting,
  "Paused ticks must clear the kiting AFK counter.");

system.AdvanceActivity(stillInput);
PlayerActivityInput deadInput = stillInput with { IsDead = true };
system.AdvanceActivity(deadInput);
AssertEqual(0, manaComponent.AfkCounter,
  "Dead ticks must clear the normal AFK counter.");
AssertEqual(0, manaComponent.AfkCounterForKiting,
  "Dead ticks must clear the kiting AFK counter.");

system.RebuildManaStatus(new PlayerManaStatusInput(
  ManaSickBuffActive: true,
  ManaSickBuffTime: 300,
  ManaRegenBonus: 10,
  ManaRegenDelayBonus: 0.5f));
system.AdvanceActivity(stillInput);
system.ResetForLifecycle();
Assert(!manaComponent.ManaSick,
  "Lifecycle reset must clear mana sickness.");
AssertEqual(0, manaComponent.AfkCounter,
  "Lifecycle reset must clear the normal AFK counter.");
AssertEqual(0, manaComponent.AfkCounterForKiting,
  "Lifecycle reset must clear the kiting AFK counter.");
AssertEqual(0, regenComponent.ManaRegenBonus,
  "Lifecycle reset must clear the mana regeneration bonus.");

PlayerActivityDecision beforeQuery = activityQuery.Evaluate(stillInput);
PlayerActivityDecision afterQuery = activityQuery.Evaluate(stillInput);
AssertEqual(beforeQuery, afterQuery,
  "PlayerActivityQuery must be deterministic for the same explicit input.");
AssertEqual(0, manaComponent.AfkCounter,
  "PlayerActivityQuery must not mutate the activity component.");

Console.WriteLine(
  "PASS: player mana sickness, regeneration modifiers, AFK activity and lifecycle semantics");
