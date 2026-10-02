using System;
using System.Collections.Generic;
using Terraria.Npc;

namespace Terraria.Npc.Queries;

public static class NpcSoulDrainEligibilityQuery
{
  private const int LegacyPlayerCount = 255;
  private const int SoulDrainItemType = 3006;
  private const float SoulDrainRange = 1100.0f;

  public static NpcSoulDrainEligibilityResult Evaluate(
    in NpcSoulDrainEligibilityInput input)
  {
    if (!input.IsSoulDrainActive)
    {
      return new NpcSoulDrainEligibilityResult(
        Succeeded: true,
        FailureReason: NpcSoulDrainEligibilityFailureReason.None,
        EligibleLegacyPlayerSlots: Array.Empty<int>());
    }

    if (input.PlayerSnapshot is null)
    {
      return Rejected(NpcSoulDrainEligibilityFailureReason.PlayerSnapshotRequired);
    }

    if (input.PlayerSnapshot.Count != LegacyPlayerCount)
    {
      return Rejected(NpcSoulDrainEligibilityFailureReason.PlayerSnapshotLengthMismatch);
    }

    var eligibleSlots = new List<int>();
    for (int playerSlot = 0; playerSlot < LegacyPlayerCount; playerSlot++)
    {
      NpcSoulDrainPlayerSnapshot player = input.PlayerSnapshot[playerSlot];
      if (!player.IsActive || player.IsDead)
      {
        continue;
      }

      if (!((input.NpcCenter - player.Position).Length() < SoulDrainRange))
      {
        continue;
      }

      if (player.SelectedItemType != SoulDrainItemType || player.ItemAnimation <= 0)
      {
        continue;
      }

      eligibleSlots.Add(playerSlot);
    }

    return new NpcSoulDrainEligibilityResult(
      Succeeded: true,
      FailureReason: NpcSoulDrainEligibilityFailureReason.None,
      EligibleLegacyPlayerSlots: eligibleSlots.ToArray());
  }

  private static NpcSoulDrainEligibilityResult Rejected(
    NpcSoulDrainEligibilityFailureReason reason)
  {
    return new NpcSoulDrainEligibilityResult(
      Succeeded: false,
      FailureReason: reason,
      EligibleLegacyPlayerSlots: Array.Empty<int>());
  }
}
