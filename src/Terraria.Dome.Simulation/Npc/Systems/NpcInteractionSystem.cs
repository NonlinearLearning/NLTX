using System;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Systems;

public sealed class NpcInteractionSystem
{
  private const int TownNpcAiStyle = 7;

  public bool CanTalk(NpcDefinition definition, float verticalVelocity)
  {
    return CanTalk(
      definition.IsLikeTownNpc,
      definition.AiStyle,
      verticalVelocity,
      definition.IsTownPet);
  }

  public bool CanTalk(
    bool isLikeTownNpc,
    int aiStyle,
    float verticalVelocity,
    bool isTownPet)
  {
    return CanBeTalkedTo(isLikeTownNpc, aiStyle, verticalVelocity) && !isTownPet;
  }

  public bool CanBeTalkedTo(
    bool isLikeTownNpc,
    int aiStyle,
    float verticalVelocity)
  {
    return isLikeTownNpc && aiStyle == TownNpcAiStyle &&
      float.IsFinite(verticalVelocity) && verticalVelocity == 0.0f;
  }

  public bool CanBeTalkedTo(NpcDefinition definition, float verticalVelocity)
  {
    return CanBeTalkedTo(
      definition.IsLikeTownNpc,
      definition.AiStyle,
      verticalVelocity);
  }

  public void Apply(
    ref NpcInteractionComponent interaction,
    NpcInteractionCommand command,
    long tick)
  {
    interaction.Apply(command, tick);
  }

  public bool TryExpire(
    ref NpcInteractionComponent interaction,
    long currentTick,
    long timeoutTicks)
  {
    return interaction.TryExpire(currentTick, timeoutTicks);
  }

  public bool TryCreate(
    PlayerHandle player,
    NpcHandle npc,
    NpcInteractionKind kind,
    string sessionId,
    SimulationVector playerPosition,
    SimulationVector npcPosition,
    float maximumRange,
    bool playerIsActive,
    bool npcIsActive,
    out NpcInteractionCommand command,
    NpcFaction faction)
  {
    command = default;
    if (!player.IsValid || !npc.IsValid || !Enum.IsDefined(kind) ||
        faction != NpcFaction.Town ||
        string.IsNullOrWhiteSpace(sessionId) || !playerIsActive || !npcIsActive ||
        !IsFinite(playerPosition) || !IsFinite(npcPosition) ||
        !float.IsFinite(maximumRange) || maximumRange <= 0.0f)
    {
      return false;
    }

    float deltaX = playerPosition.X - npcPosition.X;
    float deltaY = playerPosition.Y - npcPosition.Y;
    if (deltaX * deltaX + deltaY * deltaY > maximumRange * maximumRange)
    {
      return false;
    }

    command = new NpcInteractionCommand(player, npc, kind, sessionId, npcPosition);
    return true;
  }

  private static bool IsFinite(SimulationVector value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }
}
