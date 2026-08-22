using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public static class TrainingDummyActivationDecisionQuery
{
  public static TrainingDummyActivationDecisionResult Evaluate(
    TrainingDummyTileEntityState entity,
    IReadOnlyList<TrainingDummyPlayerHitboxSnapshot> players,
    bool npcSlotsFull)
  {
    if (entity.NpcId >= 0)
    {
      return new(false, TrainingDummyActivationDecisionReason.AlreadyLinked);
    }

    if (npcSlotsFull)
    {
      return new(false, TrainingDummyActivationDecisionReason.NpcSlotsFull);
    }

    for (int index = 0; index < players.Count; index++)
    {
      if (TrainingDummyActivationEligibilityQuery.IsPlayerInRange(entity, players[index]))
      {
        return new(true, TrainingDummyActivationDecisionReason.PlayerNearby);
      }
    }

    return new(false, TrainingDummyActivationDecisionReason.NoPlayerNearby);
  }
}
