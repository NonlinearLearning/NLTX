using Terraria.Player.Progression;
using Terraria.Projectile;
using Terraria.Relationships;

var component = new PlayerMinionCapacityComponent();
Assert(component.MaxMinions == 1, "Capacity should start with one available minion.");
Assert(component.NumMinions == 0, "Capacity should start with no active minions.");
Assert(component.SlotsMinions == 0, "Capacity should start with no occupied slots.");
Assert(RemainingMinionCapacityQuery.Get(component) == 1, "Remaining capacity should be one slot.");

var owner = new EntityReference(Guid.NewGuid(), EntityReferenceScope.Player);
var projectile = new ProjectileIdentityComponent(
  new EntityReference(Guid.NewGuid(), EntityReferenceScope.Projectile),
  identity: 7);
var first = new SubmitMinionCapacityDeltaCommand(
  owner,
  projectile,
  MinionCountDelta: 1,
  SlotDelta: 0.5f,
  SourceRevision: 1,
  IdempotencyToken: Guid.NewGuid());

var system = new PlayerMinionCapacityCommitSystem(component);
var firstResult = system.Apply(first);
Assert(firstResult == PlayerMinionCapacityCommitStatus.Committed, "First delta should commit.");
Assert(component.SlotsMinions == 0.5f, "First delta should update occupied slots.");

var duplicateResult = system.Apply(first);
Assert(duplicateResult == PlayerMinionCapacityCommitStatus.AlreadyApplied, "Duplicate delta should be idempotent.");
Assert(component.SlotsMinions == 0.5f, "Duplicate delta must not update occupied slots.");

var countInput = new SubmitMinionCapacityDeltaCommand(
  owner,
  projectile,
  MinionCountDelta: 1,
  SlotDelta: 0.5f,
  SourceRevision: 2,
  IdempotencyToken: Guid.NewGuid());
var countResult = system.Apply(countInput);
Assert(countResult == PlayerMinionCapacityCommitStatus.Committed, "A second minion should commit.");
Assert(component.NumMinions == 2, "A positive count delta should update the aggregate count.");
Assert(component.SlotsMinions == 1f, "A positive slot delta should fill the capacity.");
Assert(!RemainingMinionCapacityQuery.HasCapacity(component), "A full capacity should not admit another minion.");

var rejected = new SubmitMinionCapacityDeltaCommand(
  owner,
  projectile,
  MinionCountDelta: 1,
  SlotDelta: -2f,
  SourceRevision: 3,
  IdempotencyToken: Guid.NewGuid());
var rejectedResult = system.Apply(rejected);
Assert(rejectedResult == PlayerMinionCapacityCommitStatus.RejectedInvalidDelta, "Negative capacity must be rejected.");

system.Reset();
Assert(component.MaxMinions == 1, "Reset should restore the default maximum.");
Assert(component.NumMinions == 0, "Reset should clear the active count.");
Assert(component.SlotsMinions == 0, "Reset should clear occupied slots.");

Console.WriteLine("P05 C03 verifier passed.");

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
