namespace Terraria.WorldSession.Seeds;

public interface IWorldRandomPort
{
  int Next(int exclusiveUpperBound);

  double NextDouble();
}
