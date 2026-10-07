namespace Terraria.Npc;

public interface INpcEyeOfCthulhuRandomPort
{
  int Next(int maxExclusive);

  int Next(int minInclusive, int maxExclusive)
  {
    return Next(maxExclusive - minInclusive) + minInclusive;
  }
}
