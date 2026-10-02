namespace Terraria.Npc;

public interface INpcWaterPerishableEffectPort
{
  void TryRemovingWaterPerishableEffects(
    NpcInstanceId npcInstanceId,
    bool isInLava);
}
