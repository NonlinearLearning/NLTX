namespace Terraria.Dome.Simulation.WorldModel;

public enum WorldClockTransitionKind
{
  Dawn,
  Dusk
}

public readonly record struct WorldClockTransition(
  WorldClockTransitionKind Kind,
  long TickNumber,
  double TimeOfDay,
  byte MoonPhase);
