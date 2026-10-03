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

var vital = new PlayerVitalStateComponent();
var defense = new PlayerCombatDefenseModifierComponent();
var mitigation = new PlayerDamageMitigationInputComponent();
var dodge = new PlayerDodgeAndImmunityStateComponent();
var dodgeSystem = new PlayerDodgeCommitSystem(dodge);
var proc = new PlayerCombatProcSystem(new PlayerCombatProcStateComponent());
var telemetry = new PlayerDpsTelemetrySystem(
  new PlayerDpsTelemetryComponent(),
  new VerificationClock(DateTimeOffset.UtcNow));
var system = new PlayerCombatResolutionSystem(
  vital,
  defense,
  mitigation,
  dodge,
  dodgeSystem,
  proc,
  telemetry);

AssertEqual(
  67,
  PlayerDamageMitigationQuery.Evaluate(
    new PlayerDamageMitigationInput(
      DamageAmount: 100,
      Defense: 10,
      Endurance: 0.25f,
      Critical: false)),
  "The pure mitigation query should apply defense and endurance deterministically.");

DateTimeOffset committedAt = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
Guid sourceId = Guid.Parse("81000000-0000-0000-0000-000000000001");
Guid eventId = Guid.Parse("82000000-0000-0000-0000-000000000001");
PlayerCombatResolutionResult applied = system.Resolve(
  new PlayerCombatResolutionCommand(
    EventId: eventId,
    SourceId: sourceId,
    SourceRevision: 1,
    DamageAmount: 10,
    Critical: false,
    Dodgeable: true,
    HasGeneralImmunity: false,
    SourceCooldownActive: false,
    CooldownTicks: 0,
    CommittedAt: committedAt));
Assert(applied.Applied, "An eligible hit should commit once.");
AssertEqual(10, applied.FinalDamage, "The default player defense should leave ten damage.");
AssertEqual(90, vital.StatLife, "The accepted hit should be the only life writer.");
Assert(applied.ProcPublished, "An accepted hit should publish a committed proc fact.");
Assert(applied.TelemetryPublished, "An accepted hit should publish a committed damage fact.");

PlayerCombatResolutionResult duplicate = system.Resolve(
  new PlayerCombatResolutionCommand(
    EventId: eventId,
    SourceId: sourceId,
    SourceRevision: 1,
    DamageAmount: 10,
    Critical: false,
    Dodgeable: true,
    HasGeneralImmunity: false,
    SourceCooldownActive: false,
    CooldownTicks: 0,
    CommittedAt: committedAt));
Assert(!duplicate.Applied, "A duplicate event must not commit a second hit.");
AssertEqual(
  PlayerDamageEligibilityRejectionReason.DuplicateEvent,
  duplicate.RejectionReason,
  "Duplicate hits should expose a stable rejection reason.");
AssertEqual(90, vital.StatLife, "A duplicate hit must not change life.");

Guid immuneEventId = Guid.Parse("82000000-0000-0000-0000-000000000002");
PlayerCombatResolutionResult immune = system.Resolve(
  new PlayerCombatResolutionCommand(
    EventId: immuneEventId,
    SourceId: sourceId,
    SourceRevision: 2,
    DamageAmount: 20,
    Critical: false,
    Dodgeable: true,
    HasGeneralImmunity: true,
    SourceCooldownActive: false,
    CooldownTicks: 0,
    CommittedAt: committedAt));
Assert(!immune.Applied, "General immunity must reject before the commit point.");
AssertEqual(
  PlayerDamageEligibilityRejectionReason.GeneralImmunity,
  immune.RejectionReason,
  "The eligibility reason must be preserved.");
AssertEqual(90, vital.StatLife, "A rejected hit must not change life.");

Guid dodgeActivationId = Guid.Parse("83000000-0000-0000-0000-000000000001");
Assert(
  dodgeSystem.ActivateShadowDodge(
    new PlayerDodgeActivationCommand(
      dodgeActivationId,
      SourceRevision: 3,
      ShadowDodgeTimer: 5,
      AnimationTicks: 1)),
  "A valid dodge activation should arm the dodge state.");

Guid dodgedEventId = Guid.Parse("82000000-0000-0000-0000-000000000003");
PlayerCombatResolutionResult dodged = system.Resolve(
  new PlayerCombatResolutionCommand(
    EventId: dodgedEventId,
    SourceId: sourceId,
    SourceRevision: 4,
    DamageAmount: 50,
    Critical: true,
    Dodgeable: true,
    HasGeneralImmunity: false,
    SourceCooldownActive: false,
    CooldownTicks: 0,
    CommittedAt: committedAt));
Assert(dodged.Dodged, "Shadow dodge must reject before the life commit.");
AssertEqual(
  PlayerDamageEligibilityRejectionReason.ShadowDodge,
  dodged.RejectionReason,
  "Shadow dodge should expose its own rejection reason.");
AssertEqual(90, vital.StatLife, "A dodged hit must not change life.");
Assert(!dodge.ShadowDodge, "A committed dodge should be consumed exactly once.");

Guid lethalEventId = Guid.Parse("82000000-0000-0000-0000-000000000004");
PlayerCombatResolutionResult critical = system.Resolve(
  new PlayerCombatResolutionCommand(
    EventId: lethalEventId,
    SourceId: sourceId,
    SourceRevision: 5,
    DamageAmount: 50,
    Critical: true,
    Dodgeable: false,
    HasGeneralImmunity: false,
    SourceCooldownActive: false,
    CooldownTicks: 0,
    CommittedAt: committedAt));
Assert(critical.Applied, "A later eligible critical hit should commit.");
AssertEqual(100, critical.FinalDamage, "Critical damage should double the deterministic base amount.");
Assert(critical.Killed, "A hit that reaches zero life should expose the lethal result.");
AssertEqual(0, vital.StatLife, "The lethal hit should clamp life at zero.");

var statusSlots = new PlayerBuffSlotsComponent();
var statusImmunity = new PlayerBuffImmunityComponent(512);
var statusDefinitions = new List<PlayerStatusEffectDefinition>
{
  new(
    new ContentId<BuffDefinition>(1),
    isDebuff: false),
  new(
    new ContentId<BuffDefinition>(2),
    isDebuff: true),
  new(
    new ContentId<BuffDefinition>(3),
    isDebuff: false,
    additiveTimeCap: 20),
};
for (int type = 10; type < 52; type++)
{
  statusDefinitions.Add(
    new PlayerStatusEffectDefinition(
      new ContentId<BuffDefinition>(type),
      isDebuff: true));
}

var statusCatalog = new PlayerStatusEffectCatalog(statusDefinitions);
var statusSystem = new PlayerStatusEffectSystem(
  statusSlots,
  statusImmunity,
  statusCatalog);

PlayerStatusEffectResult statusAdded = statusSystem.Apply(
  new PlayerStatusEffectApplyCommand(
    new ContentId<BuffDefinition>(1),
    DurationTicks: 10));
Assert(statusAdded.Applied, "A known status should occupy the first free slot.");
AssertEqual(0, statusAdded.SlotIndex, "The first status should use slot zero.");

PlayerStatusEffectResult statusRefreshed = statusSystem.Apply(
  new PlayerStatusEffectApplyCommand(
    new ContentId<BuffDefinition>(1),
    DurationTicks: 5));
Assert(statusRefreshed.Refreshed, "A duplicate status should refresh its existing slot.");
AssertEqual(10, statusRefreshed.RemainingTicks, "A shorter refresh must not reduce time.");

PlayerStatusEffectResult additiveStatus = statusSystem.Apply(
  new PlayerStatusEffectApplyCommand(
    new ContentId<BuffDefinition>(3),
    DurationTicks: 15));
Assert(additiveStatus.Applied, "A second known status should be accepted.");
PlayerStatusEffectResult additiveRefresh = statusSystem.Apply(
  new PlayerStatusEffectApplyCommand(
    new ContentId<BuffDefinition>(3),
    DurationTicks: 15));
AssertEqual(20, additiveRefresh.RemainingTicks, "Additive refresh must honor its catalog cap.");

statusImmunity.SetImmunity(new ContentId<BuffDefinition>(2), isImmune: true);
PlayerStatusEffectResult immuneStatus = statusSystem.Apply(
  new PlayerStatusEffectApplyCommand(
    new ContentId<BuffDefinition>(2),
    DurationTicks: 5));
AssertEqual(
  PlayerStatusEffectRejectionReason.Immune,
  immuneStatus.RejectionReason,
  "An immune status must be rejected before slot mutation.");
AssertEqual(2, statusSystem.Snapshot().ActiveCount, "Rejected status application must not add a slot.");
statusImmunity.SetImmunity(new ContentId<BuffDefinition>(2), isImmune: false);

for (int type = 10; type < 52; type++)
{
  PlayerStatusEffectResult fill = statusSystem.Apply(
    new PlayerStatusEffectApplyCommand(
      new ContentId<BuffDefinition>(type),
      DurationTicks: 5));
  Assert(fill.Applied, $"Status type {type} should fill an available slot.");
}

PlayerStatusEffectResult evicted = statusSystem.Apply(
  new PlayerStatusEffectApplyCommand(
    new ContentId<BuffDefinition>(2),
    DurationTicks: 7));
Assert(evicted.Applied && evicted.Evicted, "A full table should evict the first known non-debuff.");
AssertEqual(
  PlayerBuffSlotsComponent.MaximumSlotCount - 1,
  evicted.SlotIndex,
  "Eviction should compact slots and append the replacement at the trailing free slot.");

var tickingSlots = new PlayerBuffSlotsComponent();
var tickingSystem = new PlayerStatusEffectSystem(
  tickingSlots,
  new PlayerBuffImmunityComponent(8),
  new PlayerStatusEffectCatalog(
    [new PlayerStatusEffectDefinition(new ContentId<BuffDefinition>(1), isDebuff: false)]));
_ = tickingSystem.Apply(
  new PlayerStatusEffectApplyCommand(
    new ContentId<BuffDefinition>(1),
    DurationTicks: 2));
PlayerStatusEffectTickResult tickOne = tickingSystem.Tick(
  new PlayerStatusEffectTickInput(DecrementTimers: true));
AssertEqual(0, tickOne.ExpiredEffects.Count, "A two-tick status should survive its first tick.");
PlayerStatusEffectTickResult tickTwo = tickingSystem.Tick(
  new PlayerStatusEffectTickInput(DecrementTimers: true));
AssertEqual(1, tickTwo.ExpiredEffects.Count, "A status should report expiry at zero ticks.");
AssertEqual(0, tickTwo.ActiveCount, "Expired statuses must be removed from the active snapshot.");

var manaVital = new PlayerVitalStateComponent();
manaVital.StatMana = 0;
manaVital.StatManaMax2 = 20;
var manaState = new PlayerManaRegenStateComponent();
manaState.ManaRegenCount = 119;
var manaModifiers = new PlayerManaRegenModifierComponent();
var manaSystem = new PlayerManaRegenSystem(
  manaVital,
  manaState,
  manaModifiers);
PlayerManaRegenResult manaTick = manaSystem.Tick(
  new PlayerManaRegenInput(
    IsConsideredStandingStill: false,
    IsGrappling: false,
    UsedArcaneCrystal: false,
    ManaRegenBuff: false));
Assert(manaTick.Applied, "A valid mana cap should allow a regen tick.");
AssertEqual(1, manaTick.ManaAfter, "The count threshold should commit one mana point.");
AssertEqual(0, manaTick.ManaRegenCountAfter, "A committed mana point should consume 120 count units.");

manaState.ManaRegenDelay = 2f;
manaState.ManaRegenCount = 0;
PlayerManaRegenResult delayedManaTick = manaSystem.Tick(
  new PlayerManaRegenInput(
    IsConsideredStandingStill: false,
    IsGrappling: false,
    UsedArcaneCrystal: false,
    ManaRegenBuff: false));
AssertEqual(1f, delayedManaTick.ManaRegenDelayAfter, "Delay should decrease by one frame.");
AssertEqual(0, delayedManaTick.ManaRegen, "Mana regen stays zero while delay remains positive.");

manaVital.StatManaMax2 = 0;
PlayerManaRegenResult invalidMana = manaSystem.Tick(
  new PlayerManaRegenInput(
    IsConsideredStandingStill: false,
    IsGrappling: false,
    UsedArcaneCrystal: false,
    ManaRegenBuff: false));
AssertEqual(
  PlayerManaRegenFailureReason.InvalidManaCap,
  invalidMana.FailureReason,
  "An invalid mana cap must be returned as an explicit failure.");

manaVital.StatManaMax2 = 20;
manaState.ManaRegenDelay = float.NaN;
PlayerManaRegenResult invalidRegenState = manaSystem.Tick(
  new PlayerManaRegenInput(
    IsConsideredStandingStill: false,
    IsGrappling: false,
    UsedArcaneCrystal: false,
    ManaRegenBuff: false));
AssertEqual(
  PlayerManaRegenFailureReason.InvalidRegenState,
  invalidRegenState.FailureReason,
  "Non-finite regen state must be returned as an explicit failure.");

PlayerLifeRegenResult healingRegen = PlayerLifeRegenQuery.Evaluate(
  new PlayerLifeRegenInput(
    LifeRegen: 120,
    LifeRegenCount: 0,
    LifeRegenTime: 0f,
    StatLife: 90,
    StatLifeMax2: 100,
    SoulDrain: 0,
    Statuses: PlayerLifeRegenStatusFlags.None));
Assert(healingRegen.Computed, "A valid life regen input should produce a result.");
AssertEqual(
  1,
  healingRegen.HealingTicks,
  "One positive regen threshold should request one heal point.");
AssertEqual(
  0,
  healingRegen.LifeRegenCountAfter,
  "A consumed heal threshold should leave no remainder.");
Assert(
  healingRegen.RequiresExternalLifeCommit,
  "Healing must be handed to a later life commit owner.");

PlayerLifeRegenResult poisonedRegen = PlayerLifeRegenQuery.Evaluate(
  new PlayerLifeRegenInput(
    LifeRegen: 10,
    LifeRegenCount: 0,
    LifeRegenTime: 10f,
    StatLife: 90,
    StatLifeMax2: 100,
    SoulDrain: 0,
    Statuses: PlayerLifeRegenStatusFlags.Poisoned));
AssertEqual(
  -4,
  poisonedRegen.LifeRegen,
  "Poison must clear positive regen before applying its penalty.");
AssertEqual(
  1f,
  poisonedRegen.LifeRegenTimeAfter,
  "Poison must reset the timer before the frame increment.");
AssertEqual(
  -4,
  poisonedRegen.LifeRegenCountAfter,
  "Poison damage should accumulate in the regen counter.");

PlayerLifeRegenResult ordinaryDamage = PlayerLifeRegenQuery.Evaluate(
  new PlayerLifeRegenInput(
    LifeRegen: 0,
    LifeRegenCount: -240,
    LifeRegenTime: 0f,
    StatLife: 90,
    StatLifeMax2: 100,
    SoulDrain: 0,
    Statuses: PlayerLifeRegenStatusFlags.None));
AssertEqual(
  2,
  ordinaryDamage.LifeDamageTotal,
  "Two negative regen thresholds should request two damage points.");
AssertEqual(
  1,
  ordinaryDamage.DamageCommitCount,
  "Two ordinary damage points fit in one capped commit.");
AssertEqual(
  0,
  ordinaryDamage.LifeRegenCountAfter,
  "Consumed damage thresholds should leave no remainder.");
Assert(
  !ordinaryDamage.UsesSpecialDamageRule,
  "Ordinary damage should use the general regen rule.");

PlayerLifeRegenResult burnedDamage = PlayerLifeRegenQuery.Evaluate(
  new PlayerLifeRegenInput(
    LifeRegen: 0,
    LifeRegenCount: -600,
    LifeRegenTime: 0f,
    StatLife: 90,
    StatLifeMax2: 100,
    SoulDrain: 0,
    Statuses: PlayerLifeRegenStatusFlags.Burned));
AssertEqual(
  5,
  burnedDamage.LifeDamageTotal,
  "Burned status must use the five-point special damage request.");
AssertEqual(
  1,
  burnedDamage.DamageCommitCount,
  "One special damage threshold should produce one request.");
Assert(
  burnedDamage.UsesSpecialDamageRule,
  "Burned status must select the special damage rule.");
AssertEqual(
  -60,
  burnedDamage.LifeRegenCountAfter,
  "The special threshold must leave the source remainder.");

PlayerLifeRegenResult restingRegen = PlayerLifeRegenQuery.Evaluate(
  new PlayerLifeRegenInput(
    LifeRegen: 0,
    LifeRegenCount: 0,
    LifeRegenTime: 4000f,
    StatLife: 90,
    StatLifeMax2: 100,
    SoulDrain: 0,
    Statuses: PlayerLifeRegenStatusFlags.Resting));
AssertEqual(
  3603f,
  restingRegen.LifeRegenTimeAfter,
  "Resting time must be added after the non-ShinyStone 3600-frame cap.");

Console.WriteLine("PASS: player combat, status-slot, life-regen and mana-regen core semantics");

sealed class VerificationClock : IPlayerTelemetryClock
{
  public VerificationClock(DateTimeOffset utcNow)
  {
    UtcNow = utcNow;
  }

  public DateTimeOffset UtcNow { get; }
}
