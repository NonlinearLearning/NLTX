using System;
using Terraria.Npc.Network;

namespace Terraria.Npc;

public static class NpcBuffEffectConsumerSystem
{
  public static NpcBuffEffectConsumerResult Consume(
    NpcBuffStateUpdateResult update,
    NpcInstanceId npcInstanceId,
    int npcLegacySlot,
    INpcBuffSlotSyncPacketPort syncPacketPort,
    INpcWaterPerishableEffectPort waterPerishableEffectPort)
  {
    ArgumentNullException.ThrowIfNull(update);
    ArgumentNullException.ThrowIfNull(syncPacketPort);
    ArgumentNullException.ThrowIfNull(waterPerishableEffectPort);
    if (!npcInstanceId.IsValid)
    {
      throw new ArgumentException(
        "Buff effects require the target NPC identity.",
        nameof(npcInstanceId));
    }

    if (npcLegacySlot < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(npcLegacySlot));
    }

    int buffSlotSyncRequests = 0;
    int waterPerishableCleanupRequests = 0;
    ReadOnlyMemory<NpcBuffStateUpdateResult.EffectIntent> effects = update.Effects;
    for (int index = 0; index < effects.Length; index++)
    {
      NpcBuffStateUpdateResult.EffectIntent effect = effects.Span[index];
      switch (effect.Kind)
      {
        case NpcBuffStateUpdateResult.EffectKind.BuffSlotSyncRequested:
          syncPacketPort.SendBuffSlotSyncPacket(npcLegacySlot);
          buffSlotSyncRequests++;
          break;
        case NpcBuffStateUpdateResult.EffectKind.WaterPerishableCleanupRequested:
          waterPerishableEffectPort.TryRemovingWaterPerishableEffects(
            npcInstanceId,
            isInLava: false);
          waterPerishableCleanupRequests++;
          break;
        default:
          throw new ArgumentOutOfRangeException(
            nameof(update),
            effect.Kind,
            "Unknown NPC buff effect intent.");
      }
    }

    return new NpcBuffEffectConsumerResult(
      buffSlotSyncRequests,
      waterPerishableCleanupRequests);
  }
}
