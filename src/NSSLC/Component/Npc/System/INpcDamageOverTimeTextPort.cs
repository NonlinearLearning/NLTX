namespace Terraria.Npc;

public interface INpcDamageOverTimeTextPort
{
  // Expected delivery failures return false so authoritative damage resolution can continue.
  bool TryPublish(in NpcDamageOverTimeTextIntent intent);
}
