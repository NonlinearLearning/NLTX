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

var component = new PlayerDodgeAndImmunityStateComponent();
var system = new PlayerDodgeCommitSystem(component);
var itemReference = new ItemEntityRef(
  Guid.Parse("40000000-0000-0000-0000-000000000001"));

Assert(!component.BlackBelt, "Black belt must start inactive.");
Assert(component.BrainOfConfusionItem.IsEmpty,
  "Brain of Confusion must start without an item relation.");
AssertEqual(0, component.BrainOfConfusionDodgeAnimationCounter,
  "Dodge animation must start at zero.");

system.RebuildCapabilities(new PlayerDodgeCapabilityInput(
  BlackBelt: true,
  BrainOfConfusionItem: itemReference));
Assert(component.BlackBelt, "Capability rebuild should record black belt.");
AssertEqual(itemReference, component.BrainOfConfusionItem,
  "Capability rebuild should preserve the typed item relation.");

Guid activationId = Guid.Parse("50000000-0000-0000-0000-000000000001");
var activation = new PlayerDodgeActivationCommand(
  CommandId: activationId,
  SourceRevision: 4,
  ShadowDodgeTimer: 5,
  AnimationTicks: 300);
Assert(system.ActivateShadowDodge(activation),
  "A valid dodge activation should be committed.");
Assert(!system.ActivateShadowDodge(activation),
  "A duplicate dodge activation must be ignored.");
Assert(component.ShadowDodge, "Committed activation should arm shadow dodge.");
AssertEqual(5, component.ShadowDodgeTimer,
  "Committed activation should set the dodge timer.");
AssertEqual(300, component.BrainOfConfusionDodgeAnimationCounter,
  "Committed activation should set the presentation counter.");

var blockedByImmunity = PlayerDamageEligibilityQuery.Evaluate(
  new PlayerDamageEligibilityInput(
    SourceId: Guid.Parse("60000000-0000-0000-0000-000000000001"),
    DamageAmount: 20,
    HasGeneralImmunity: true,
    SourceCooldownActive: false),
  component);
Assert(!blockedByImmunity.IsEligible,
  "General immunity must reject incoming damage before dodge consumption.");
Assert(component.ShadowDodge,
  "Rejected damage must not consume shadow dodge.");

var eligible = PlayerDamageEligibilityQuery.Evaluate(
  new PlayerDamageEligibilityInput(
    SourceId: Guid.Parse("60000000-0000-0000-0000-000000000001"),
    DamageAmount: 20,
    HasGeneralImmunity: false,
    SourceCooldownActive: false),
  component);
Assert(eligible.IsEligible, "A valid damage request should be eligible.");
Assert(eligible.UsesShadowDodge,
  "Eligibility should expose the committed shadow dodge fact.");

var committedDamage = new PlayerDodgeCommitCommand(
  CommandId: Guid.Parse("70000000-0000-0000-0000-000000000001"),
  SourceId: Guid.Parse("60000000-0000-0000-0000-000000000001"),
  SourceRevision: 8,
  DamageAmount: 20,
  IsCommitted: true);
Assert(system.ConsumeCommittedDamage(committedDamage),
  "A committed damage command should consume shadow dodge once.");
Assert(!component.ShadowDodge, "Consumed dodge should no longer be armed.");
Assert(!system.ConsumeCommittedDamage(committedDamage),
  "A duplicate damage command must not consume a second dodge.");

var secondActivation = activation with
{
  CommandId = Guid.Parse("50000000-0000-0000-0000-000000000002"),
  SourceRevision = 5
};
Assert(system.ActivateShadowDodge(secondActivation),
  "A later lifecycle command should be able to arm a new dodge window.");
system.AdvanceTick();
AssertEqual(299, component.BrainOfConfusionDodgeAnimationCounter,
  "Presentation counter should tick independently.");
AssertEqual(4, component.ShadowDodgeTimer,
  "Dodge timer should tick deterministically.");

var projection = new PlayerDodgePresentationProjection();
PlayerDodgePresentationSnapshot beforeProjection = projection.Snapshot(component);
PlayerDodgePresentationSnapshot afterProjection = projection.Snapshot(component);
AssertEqual(beforeProjection, afterProjection,
  "Presentation projection must be pure.");
Assert(component.ShadowDodge,
  "Presentation projection must not write dodge authority.");

system.ResetForLifecycle();
Assert(!component.BlackBelt, "Lifecycle reset should clear capabilities.");
Assert(component.BrainOfConfusionItem.IsEmpty,
  "Lifecycle reset should clear the item relation.");
Assert(!component.ShadowDodge, "Lifecycle reset should clear active dodge.");
AssertEqual(0, component.ShadowDodgeTimer,
  "Lifecycle reset should clear the dodge timer.");

Console.WriteLine("PASS: player dodge eligibility, committed consumption and projection semantics");
