namespace Terraria.Dome.Simulation.Combat;

public interface IDamageVariationRandom
{
  float NextChance();

  int NextDamageStep();
}
