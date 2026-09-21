namespace Terraria.WorldSession.Runtime;

public enum WorldTimeSkipDirection
{
  Dawn,
  Dusk
}

public readonly record struct WorldTimeSkipCommand(WorldTimeSkipDirection Direction);
