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
using Terraria.Dome.Simulation.StatusEffects.Components;
using ArchWorld = Arch.Core.World;

using EntityEcs.Components;
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
    if (replicationId <= 0 || replicationId == int.MaxValue ||
        !definitions.TryGet(command.DefinitionId, out NpcDefinition definition) ||
        !Enum.IsDefined(command.Source) ||
        !float.IsFinite(command.Position.X) || !float.IsFinite(command.Position.Y) ||
        !float.IsFinite(command.DifficultyScale) || command.DifficultyScale <= 0.0f ||
        command.ReleaseOwner < 0 || command.ReleaseOwner > byte.MaxValue ||
        command.ReleaseVariant is < 0 or > 4 ||
        command.GivenName is not null && command.GivenName.Length > NpcComponents.NpcGivenNameComponent.MaximumLength)
    {
      failureReason = "NPC definition or replication identity is invalid.";
      return false;
    }

    float scaledHealth = definition.MaximumHealth * command.DifficultyScale;
    if (!float.IsFinite(scaledHealth) || scaledHealth > int.MaxValue)
    {
      failureReason = "NPC difficulty scale produces an unrepresentable health value.";
      return false;
    }

    if (worldGrid is not null &&
        command.Source != NpcComponents.NpcSpawnSource.TileEntity &&
        IsOccupied(worldGrid, command.Position, definition))
    {
      failureReason = "NPC spawn space is outside the world or occupied.";
      return false;
    }

    int maximumHealth = Math.Max(1, (int)MathF.Max(1.0f, scaledHealth));
    Entity entity = world.Create(
      new NpcTagComponent(),
      new NpcComponents.NpcDefinitionComponent(
        definition.DefinitionId,
        definition.NetId,
        definition.Faction,
        definition.Category),
      new NpcComponents.NpcAuthorityComponent(
        definition.AiStyle,
        definition.IsImmortal,
        definition.AlwaysReplicate,
        definition.TakenDamageMultiplier,
        definition.NpcSlotCost,
        definition.IsTrapImmune,
        definition.IsLavaImmune),
      new LocationComponent(command.Position.X, command.Position.Y),
      new VelocityComponent(0.0f, 0.0f),
      new DirectionComponent(-1),
      new ColliderComponent(definition.ColliderWidth, definition.ColliderHeight),
      new PhysicsStateComponent { IsGrounded = command.Position.Y <= 0.0f },
      new HealthComponent(maximumHealth, maximumHealth),
      new DefenseComponent(definition.Defense),
      new NpcComponents.NpcCombatStateComponent(
        damage: definition.Damage,
        defense: definition.Defense,
        maximumHealth: maximumHealth,
        takenDamageMultiplier: definition.TakenDamageMultiplier,
        immortal: definition.IsImmortal,
        coldDamage: definition.IsColdDamage,
        friendly: definition.Faction == NpcFaction.Town,
        knockBackResist: definition.KnockBackResist,
        trapImmune: definition.IsTrapImmune,
        lavaImmune: definition.IsLavaImmune,
        doesNotTakeDamageFromHostiles: command.DoesNotTakeDamageFromHostiles,
        reflectsProjectiles: false),
      new NpcComponents.NpcMovementStateComponent(),
      new ImmunityComponent(),
      new MovementIntentComponent(),
      new Terraria.Dome.Simulation.Components.NpcTargetComponent(),
      new NpcAiStateComponent(1.0f),
      new Terraria.Dome.Simulation.Npc.Components.NpcTargetComponent(),
      new NpcComponents.NpcBehaviorStateComponent(
        definition.BehaviorId,
        new NpcComponents.NpcChaseState(1.0f, 0.0f),
        new NpcComponents.NpcTownHomeState(default, true, 0),
        doesNotTakeDamageFromHostiles: command.DoesNotTakeDamageFromHostiles),
      new NpcComponents.NpcSpawnStateComponent(
        command.Source,
        command.DifficultyScale,
        command.ReleaseOwner,
        command.Source == NpcComponents.NpcSpawnSource.Statue || command.ReleaseVariant > 2),
      new NpcComponents.NpcLifecycleComponent(
        isActive: true,
        timeLeft: 750,
        canBeReplaced: command.CanBeReplaced,
        doesNotCountMe: command.DoesNotCountMe,
        homelessDespawn: command.HomelessDespawn),
      new NpcComponents.NpcGivenNameComponent(command.GivenName),
      new BuffCollectionComponent(NpcLegacyFieldPolicy.MaximumBuffs),
      new NpcComponents.NpcInteractionComponent(),
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
