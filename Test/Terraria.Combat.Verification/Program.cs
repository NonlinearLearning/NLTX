using Terraria.Combat;
using Terraria.Relationships;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

var attacker = new EntityReference(
  Guid.Parse("10000000-0000-0000-0000-000000000001"),
  EntityReferenceScope.Player);
var target = new EntityReference(
  Guid.Parse("20000000-0000-0000-0000-000000000002"),
  EntityReferenceScope.Npc);

var request = new DamageRequest(
  attacker,
  target,
  Amount: 10,
  Defense: 6,
  Knockback: 2f,
  HitDirection: 1,
  Critical: false,
  CooldownTicks: 20,
  Attribution: new DamageAttribution(DamageSourceKind.Projectile, attacker));

var health = new HealthComponent(current: 20, maximum: 20);
var immunity = new ImmunityComponent(remainingTicks: 0);
var cooldowns = new HitCooldownComponent();
var contributions = new DamageContributionComponent();
var first = DamageResolutionSystem.Resolve(
  request,
  ref health,
  ref immunity,
  cooldowns,
  contributions);

Assert(first.Applied, "The first damage request should apply.");
Assert(first.FinalDamage == 4, "Defense should reduce damage while preserving minimum damage.");
Assert(health.Current == 16, "Health should be reduced by the resolved damage.");
Assert(contributions.GetDamage(attacker) == 4, "Contribution should be recorded before death resolution.");
Assert(cooldowns.GetRemaining(target) == 20, "The source-target cooldown should be armed.");
Assert(immunity.RemainingTicks == 0, "Source-target cooldown must not mutate general immunity.");

var blocked = DamageResolutionSystem.Resolve(
  request,
  ref health,
  ref immunity,
  cooldowns,
  contributions);
Assert(!blocked.Applied, "A source-target cooldown should reject an immediate duplicate hit.");

immunity = new ImmunityComponent(remainingTicks: 3);
cooldowns.Tick();
var immune = DamageResolutionSystem.Resolve(
  request with { CooldownTicks = 0 },
  ref health,
  ref immunity,
  cooldowns,
  contributions);
Assert(!immune.Applied, "General immunity should reject damage.");

immunity = new ImmunityComponent(remainingTicks: 0);
cooldowns.Clear(target);
health = new HealthComponent(current: 4, maximum: 20);
var lethal = DamageResolutionSystem.Resolve(
  request,
  ref health,
  ref immunity,
  cooldowns,
  contributions);
Assert(lethal.Applied && lethal.Killed, "Lethal damage should report a kill.");
var death = DeathResolutionSystem.Resolve(health, lethal.Attribution);
Assert(death.IsDead, "Death resolution should observe zero health.");
Assert(death.Cause.Kind == DamageSourceKind.Projectile, "Death cause should preserve source kind.");
Assert(death.Cause.Source == attacker, "Death cause should preserve source entity.");

Console.WriteLine("PASS: combat resolution, immunity, contribution, and attribution");
