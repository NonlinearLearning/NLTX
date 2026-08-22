namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public enum TrainingDummyActivationDecisionReason
{
  PlayerNearby = 0,
  AlreadyLinked = 1,
  NpcSlotsFull = 2,
  NoPlayerNearby = 3
}

public readonly record struct TrainingDummyActivationDecisionResult(
  bool ShouldActivate,
  TrainingDummyActivationDecisionReason Reason);
