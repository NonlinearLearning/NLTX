namespace Terraria.Dome.Simulation.Npc.Commands;

public readonly record struct NpcGameEventFirstClearCommand(int GameEventId, long Sequence)
{
  public bool IsValid => GameEventId >= 0 && Sequence >= 0;

  public bool SchedulesLanternNight => GameEventId is not (4 or 21 or 22);
}
