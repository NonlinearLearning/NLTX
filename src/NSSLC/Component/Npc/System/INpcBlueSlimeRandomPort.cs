namespace Terraria.Npc;

public interface INpcBlueSlimeRandomPort
{
  int Next(int maxExclusive);

  int Next(int minInclusive, int maxExclusive);

  int GetRandomVoiceItem();
}
