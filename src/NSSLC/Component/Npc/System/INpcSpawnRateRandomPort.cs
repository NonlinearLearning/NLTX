namespace Terraria.Npc;

public interface INpcSpawnRateRandomPort
{
  int Next(int exclusiveUpperBound);

  int RollOnlyBadLuckExtreme(float luck, int range);
}
