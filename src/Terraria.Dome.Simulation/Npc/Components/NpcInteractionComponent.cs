using System;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Components;

public struct NpcInteractionComponent
{
  public PlayerHandle Player { get; private set; }
  public NpcHandle Npc { get; private set; }
  public NpcInteractionKind Kind { get; private set; }
  public string SessionId { get; private set; }
  public SimulationVector TargetPosition { get; private set; }
  public long LastInteractionTick { get; private set; }
  public bool HasActiveInteraction { get; private set; }

  public void Apply(NpcInteractionCommand command, long tick)
  {
    if (!command.Player.IsValid || !command.Npc.IsValid || !Enum.IsDefined(command.Kind) ||
        string.IsNullOrWhiteSpace(command.SessionId) || tick < 0 ||
        !float.IsFinite(command.TargetPosition.X) || !float.IsFinite(command.TargetPosition.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(command));
    }

    Player = command.Player;
    Npc = command.Npc;
    Kind = command.Kind;
    SessionId = command.SessionId;
    TargetPosition = command.TargetPosition;
    LastInteractionTick = tick;
    HasActiveInteraction = true;
  }

  public bool TryExpire(long currentTick, long timeoutTicks)
  {
    if (currentTick < 0 || timeoutTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(currentTick));
    }

    if (!HasActiveInteraction || currentTick - LastInteractionTick <= timeoutTicks)
    {
      return false;
    }

    HasActiveInteraction = false;
    return true;
  }
}
