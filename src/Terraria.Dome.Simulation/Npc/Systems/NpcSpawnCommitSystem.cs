using System;
using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Movement.Components;
using Terraria.Dome.Simulation.Npc.Commands;
using NpcComponents = Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.WorldModel;
using ArchWorld = Arch.Core.World;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcSpawnCommitResult(Entity Entity, int ReplicationId);

public sealed class NpcSpawnCommitSystem
{
  private const float TileBoundaryEpsilon = 0.0001f;

  public bool TryCommit(
    ArchWorld world,
    NpcDefinitionRegistry definitions,
    SpawnNpcCommand command,
    int replicationId,
    out NpcSpawnCommitResult result,
    out string failureReason)
  {
    return TryCommit(
      world,
      worldGrid: null,
      definitions,
      command,
      replicationId,
      out result,
      out failureReason);
  }

  public bool TryCommit(
    ArchWorld world,
    WorldGrid? worldGrid,
    NpcDefinitionRegistry definitions,
    SpawnNpcCommand command,
    int replicationId,
    out NpcSpawnCommitResult result,
    out string failureReason)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(definitions);
    result = default;
    failureReason = string.Empty;
    if (replicationId <= 0 || !definitions.TryGet(command.DefinitionId, out NpcDefinition definition))
    {
      failureReason = "NPC definition or replication identity is invalid.";
      return false;
    }

    if (command.DifficultyScale <= 0.0f)
    {
      failureReason = "NPC difficulty scale must be positive.";
      return false;
    }

    if (worldGrid is not null && IsOccupied(worldGrid, command.Position, definition))
    {
      failureReason = "NPC spawn space is outside the world or occupied.";
      return false;
    }

    int maximumHealth = checked((int)MathF.Max(1.0f, definition.MaximumHealth * command.DifficultyScale));
    Entity entity = world.Create(
      new NpcTagComponent(),
      new NpcComponents.NpcDefinitionComponent(
        definition.DefinitionId,
        definition.NetId,
        definition.Faction,
        definition.Category),
      new TransformComponent(command.Position.X, command.Position.Y),
      new VelocityComponent(0.0f, 0.0f),
      new FacingComponent(-1),
      new ColliderComponent(definition.ColliderWidth, definition.ColliderHeight),
      new PhysicsStateComponent { IsGrounded = command.Position.Y <= 0.0f },
      new HealthComponent(maximumHealth, maximumHealth),
      new DefenseComponent(definition.Defense),
      new ImmunityComponent(),
      new MovementIntentComponent(),
      new Terraria.Dome.Simulation.Components.NpcTargetComponent(),
      new NpcAiStateComponent(1.0f),
      new Terraria.Dome.Simulation.Npc.Components.NpcTargetComponent(),
      new NpcComponents.NpcBehaviorStateComponent(
        definition.BehaviorId,
        new NpcComponents.NpcChaseState(1.0f, 0.0f),
        new NpcComponents.NpcTownHomeState(default, true, 0)),
      new NpcComponents.NpcSpawnStateComponent(
        command.Source,
        command.DifficultyScale,
        command.ReleaseOwner,
        command.Source == NpcComponents.NpcSpawnSource.Statue),
      new NpcComponents.NpcLifecycleComponent(isActive: true, timeLeft: 750),
      new NpcComponents.NpcReplicationComponent(replicationId, revision: 1));
    result = new NpcSpawnCommitResult(entity, replicationId);
    return true;
  }

  private static bool IsOccupied(
    WorldGrid world,
    SimulationVector position,
    NpcDefinition definition)
  {
    int firstX = (int)MathF.Floor(position.X);
    int firstY = (int)MathF.Floor(position.Y);
    int lastX = (int)MathF.Floor(position.X + definition.ColliderWidth - TileBoundaryEpsilon);
    int lastY = (int)MathF.Floor(position.Y + definition.ColliderHeight - TileBoundaryEpsilon);
    for (int y = firstY; y <= lastY; y++)
    {
      for (int x = firstX; x <= lastX; x++)
      {
        if (!world.Contains(x, y) || world.GetTile(x, y).IsActive)
        {
          return true;
        }
      }
    }

    return false;
  }
}
