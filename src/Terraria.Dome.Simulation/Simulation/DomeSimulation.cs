using System;
using System.Collections.Generic;
using Arch.Buffer;
using Arch.Core;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Movement.Systems;
using Terraria.Dome.Simulation.Physics.Systems;
using Terraria.Dome.Simulation.Player.Systems;
using Terraria.Dome.Simulation.Tick;

namespace Terraria.Dome.Simulation;

public sealed class DomeSimulation : IDisposable
{
  private const float PlayerSpeed = 3.0f;
  private const float GravityPerTick = -1.0f;
  private const float JumpSpeed = 4.0f;
  private const float ProjectileSpeed = 4.0f;
  private const int ProjectileDamage = 10;
  private const int ProjectileLifetimeTicks = 30;
  private readonly SimulationCommandQueue _commands = new();
  private readonly Dictionary<NpcHandle, Entity> _npcs = new();
  private readonly Dictionary<PlayerHandle, Entity> _players = new();
  private readonly GroundCollisionSystem _groundCollisionSystem = new();
  private readonly MovementSystem _movementSystem = new();
  private readonly PlayerControlSystem _playerControlSystem = new();
  private readonly PlayerGravitySystem _playerGravitySystem = new();
  private readonly PlayerInputApplySystem _playerInputApplySystem = new();
  private readonly QueryDescription _npcMovementQuery = new QueryDescription()
    .WithAll<NpcTagComponent, TransformComponent, VelocityComponent>();
  private readonly QueryDescription _playerMovementQuery = new QueryDescription()
    .WithAll<PlayerTagComponent, TransformComponent, VelocityComponent>();
  private readonly QueryDescription _projectileQuery = new QueryDescription()
    .WithAll<ProjectileTagComponent, TransformComponent, VelocityComponent,
      ProjectileDamageComponent, ProjectileLifetimeComponent>();
  private int _nextPlayerHandle;
  private int _nextNpcHandle;
  private bool _disposed;

  public DomeSimulation()
  {
    _nextNpcHandle = 1;
    _nextPlayerHandle = 1;
    World = World.Create();
  }

  public World World { get; }
  public long TickNumber { get; private set; }

  public PlayerHandle CreatePlayer(SimulationVector spawn)
  {
    ThrowIfDisposed();

    PlayerHandle player = new(_nextPlayerHandle);
    _nextPlayerHandle++;
    Entity entity = World.Create(
      new PlayerTagComponent(),
      new TransformComponent(spawn.X, spawn.Y),
      new VelocityComponent(0.0f, 0.0f),
      new FacingComponent(1),
      new ColliderComponent(1.0f, 2.0f),
      new PhysicsStateComponent { IsGrounded = spawn.Y <= 0.0f },
      new HealthComponent(100, 100),
      new PlayerInputComponent(),
      new PlayerControlStateComponent());
    _players.Add(player, entity);
    return player;
  }

  public NpcHandle CreateNpc(SimulationVector spawn)
  {
    ThrowIfDisposed();

    NpcHandle npc = new(_nextNpcHandle);
    _nextNpcHandle++;
    Entity entity = World.Create(
      new NpcTagComponent(),
      new TransformComponent(spawn.X, spawn.Y),
      new VelocityComponent(0.0f, 0.0f),
      new FacingComponent(-1),
      new ColliderComponent(1.0f, 2.0f),
      new PhysicsStateComponent { IsGrounded = spawn.Y <= 0.0f },
      new HealthComponent(100, 100),
      new NpcTargetComponent(),
      new NpcAiStateComponent(1.0f));
    _npcs.Add(npc, entity);
    return npc;
  }

  public SimulationSnapshot CreateSnapshot()
  {
    ThrowIfDisposed();

    List<PlayerSnapshot> players = new(_players.Count);
    foreach (KeyValuePair<PlayerHandle, Entity> entry in _players)
    {
      TransformComponent transform = World.Get<TransformComponent>(entry.Value);
      VelocityComponent velocity = World.Get<VelocityComponent>(entry.Value);
      FacingComponent facing = World.Get<FacingComponent>(entry.Value);
      PhysicsStateComponent physics = World.Get<PhysicsStateComponent>(entry.Value);
      HealthComponent health = World.Get<HealthComponent>(entry.Value);
      players.Add(new PlayerSnapshot(
        entry.Key,
        new SimulationVector(transform.X, transform.Y),
        new SimulationVector(velocity.X, velocity.Y),
        facing.Horizontal,
        physics.IsGrounded,
        health.Current));
    }

    List<NpcSnapshot> npcs = new(_npcs.Count);
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      TransformComponent transform = World.Get<TransformComponent>(entry.Value);
      HealthComponent health = World.Get<HealthComponent>(entry.Value);
      NpcTargetComponent target = World.Get<NpcTargetComponent>(entry.Value);
      npcs.Add(new NpcSnapshot(
        entry.Key,
        new SimulationVector(transform.X, transform.Y),
        health.Current,
        target.HasTarget));
    }

    List<ProjectileSnapshot> projectiles = new();
    World.Query(
      in _projectileQuery,
      (Entity entity, ref TransformComponent transform,
        ref ProjectileLifetimeComponent lifetime) =>
      {
        projectiles.Add(new ProjectileSnapshot(
          new SimulationVector(transform.X, transform.Y),
          lifetime.RemainingTicks));
      });

    return new SimulationSnapshot(TickNumber, players, npcs, projectiles);
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    World.Dispose();
  }

  public void Tick(SimulationInputBatch inputBatch)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(inputBatch);

    _playerInputApplySystem.Apply(World, _players, inputBatch);
    _playerControlSystem.Apply(World, _players.Values);
    _playerGravitySystem.Apply(World, _players.Values);
    _movementSystem.Apply(World, in _playerMovementQuery);
    _groundCollisionSystem.Apply(World, _players.Values);
    SelectNpcTargets();
    ApplyNpcAi();
    _movementSystem.Apply(World, in _npcMovementQuery);
    RequestProjectiles();
    MoveProjectiles();
    DetectProjectileHits();
    CommitCommands();
    TickNumber++;
  }

  private void ApplyPlayerControl()
  {
    foreach (Entity entity in _players.Values)
    {
      ref PlayerInputComponent input = ref World.Get<PlayerInputComponent>(entity);
      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(entity);
      ref FacingComponent facing = ref World.Get<FacingComponent>(entity);
      float direction = 0.0f;

      if (input.MoveLeft && !input.MoveRight)
      {
        direction = -1.0f;
      }
      else if (input.MoveRight && !input.MoveLeft)
      {
        direction = 1.0f;
      }

      velocity.X = direction * PlayerSpeed;
      if (direction != 0.0f)
      {
        facing.Horizontal = direction > 0.0f ? 1 : -1;
      }

      PhysicsStateComponent physics = World.Get<PhysicsStateComponent>(entity);
      if (input.Jump && physics.IsGrounded)
      {
        velocity.Y = JumpSpeed;
      }
    }
  }

  private void CommitCommands()
  {
    for (int index = 0; index < _commands.SpawnProjectileCommands.Count; index++)
    {
      SpawnProjectileCommand command = _commands.SpawnProjectileCommands[index];
      float horizontalVelocity = command.Facing * ProjectileSpeed;
      World.Create(
        new ProjectileTagComponent(),
        new TransformComponent(command.X, command.Y),
        new VelocityComponent(horizontalVelocity, 0.0f),
        new ColliderComponent(0.5f, 0.5f),
        new ProjectileOwnerComponent(command.Owner),
        new ProjectileDamageComponent(command.Damage),
        new ProjectileLifetimeComponent(command.LifetimeTicks));
    }

    for (int index = 0; index < _commands.DamageCommands.Count; index++)
    {
      DamageCommand command = _commands.DamageCommands[index];
      ref HealthComponent health = ref World.Get<HealthComponent>(command.Target);
      health.Current -= command.Amount;
      if (health.Current < 0)
      {
        health.Current = 0;
      }
    }

    using CommandBuffer commandBuffer = new();
    for (int index = 0; index < _commands.DespawnEntityCommands.Count; index++)
    {
      DespawnEntityCommand command = _commands.DespawnEntityCommands[index];
      commandBuffer.Destroy(command.Target);
    }

    commandBuffer.Playback(World);
    _commands.Clear();
  }

  private void DetectProjectileHits()
  {
    World.Query(
      in _projectileQuery,
      (Entity projectileEntity, ref TransformComponent projectileTransform,
        ref ColliderComponent projectileCollider,
        ref ProjectileDamageComponent damage) =>
      {
        foreach (Entity npcEntity in _npcs.Values)
        {
          HealthComponent npcHealth = World.Get<HealthComponent>(npcEntity);
          if (npcHealth.Current <= 0)
          {
            continue;
          }

          TransformComponent npcTransform = World.Get<TransformComponent>(npcEntity);
          ColliderComponent npcCollider = World.Get<ColliderComponent>(npcEntity);
          if (!Overlaps(projectileTransform, projectileCollider, npcTransform, npcCollider))
          {
            continue;
          }

          _commands.Enqueue(new DamageCommand(projectileEntity, npcEntity, damage.Amount));
          _commands.Enqueue(new DespawnEntityCommand(projectileEntity));
          break;
        }
      });
  }

  private void ApplyPlayerInputs(SimulationInputBatch inputBatch)
  {
    foreach (Entity entity in _players.Values)
    {
      ref PlayerInputComponent input = ref World.Get<PlayerInputComponent>(entity);
      input = new PlayerInputComponent();
    }

    for (int index = 0; index < inputBatch.Inputs.Count; index++)
    {
      PlayerInput supplied = inputBatch.Inputs[index];
      if (!_players.TryGetValue(supplied.Player, out Entity entity))
      {
        throw new ArgumentException("Input references an unknown player.", nameof(inputBatch));
      }

      ref PlayerInputComponent input = ref World.Get<PlayerInputComponent>(entity);
      input.MoveLeft = supplied.MoveLeft;
      input.MoveRight = supplied.MoveRight;
      input.Jump = supplied.Jump;
      input.Fire = supplied.Fire;
    }
  }

  private void MovePlayers()
  {
    World.Query(
      in _playerMovementQuery,
      (Entity entity, ref TransformComponent transform, ref VelocityComponent velocity) =>
      {
        transform.X += velocity.X;
        transform.Y += velocity.Y;
      });
  }

  private void ApplyPlayerGravity()
  {
    foreach (Entity entity in _players.Values)
    {
      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(entity);
      velocity.Y += GravityPerTick;
    }
  }

  private void ResolvePlayerGroundCollision()
  {
    foreach (Entity entity in _players.Values)
    {
      ref TransformComponent transform = ref World.Get<TransformComponent>(entity);
      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(entity);
      ref PhysicsStateComponent physics = ref World.Get<PhysicsStateComponent>(entity);
      if (transform.Y > 0.0f)
      {
        physics.IsGrounded = false;
        continue;
      }

      transform.Y = 0.0f;
      velocity.Y = 0.0f;
      physics.IsGrounded = true;
    }
  }

  private void SelectNpcTargets()
  {
    foreach (Entity npcEntity in _npcs.Values)
    {
      TransformComponent npcTransform = World.Get<TransformComponent>(npcEntity);
      Entity closestPlayer = default;
      float closestDistanceSquared = float.MaxValue;
      bool hasTarget = false;

      foreach (Entity playerEntity in _players.Values)
      {
        HealthComponent playerHealth = World.Get<HealthComponent>(playerEntity);
        if (playerHealth.Current <= 0)
        {
          continue;
        }

        TransformComponent playerTransform = World.Get<TransformComponent>(playerEntity);
        float horizontalDistance = playerTransform.X - npcTransform.X;
        float verticalDistance = playerTransform.Y - npcTransform.Y;
        float distanceSquared = horizontalDistance * horizontalDistance +
          verticalDistance * verticalDistance;
        if (distanceSquared < closestDistanceSquared)
        {
          closestDistanceSquared = distanceSquared;
          closestPlayer = playerEntity;
          hasTarget = true;
        }
      }

      ref NpcTargetComponent target = ref World.Get<NpcTargetComponent>(npcEntity);
      target.HasTarget = hasTarget;
      target.Target = closestPlayer;
    }
  }

  private void ApplyNpcAi()
  {
    foreach (Entity npcEntity in _npcs.Values)
    {
      NpcTargetComponent target = World.Get<NpcTargetComponent>(npcEntity);
      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(npcEntity);
      ref FacingComponent facing = ref World.Get<FacingComponent>(npcEntity);
      NpcAiStateComponent ai = World.Get<NpcAiStateComponent>(npcEntity);

      if (!target.HasTarget)
      {
        velocity.X = 0.0f;
        continue;
      }

      TransformComponent npcTransform = World.Get<TransformComponent>(npcEntity);
      TransformComponent targetTransform = World.Get<TransformComponent>(target.Target);
      float horizontalDistance = targetTransform.X - npcTransform.X;
      if (MathF.Abs(horizontalDistance) < ai.ChaseSpeed)
      {
        velocity.X = 0.0f;
        continue;
      }

      velocity.X = horizontalDistance > 0.0f ? ai.ChaseSpeed : -ai.ChaseSpeed;
      facing.Horizontal = velocity.X > 0.0f ? 1 : -1;
    }
  }

  private void MoveNpcs()
  {
    World.Query(
      in _npcMovementQuery,
      (Entity entity, ref TransformComponent transform, ref VelocityComponent velocity) =>
      {
        transform.X += velocity.X;
        transform.Y += velocity.Y;
      });
  }

  private void MoveProjectiles()
  {
    World.Query(
      in _projectileQuery,
      (Entity projectileEntity, ref TransformComponent transform,
        ref VelocityComponent velocity, ref ProjectileLifetimeComponent lifetime) =>
      {
        transform.X += velocity.X;
        transform.Y += velocity.Y;
        lifetime.RemainingTicks--;
        if (lifetime.RemainingTicks <= 0)
        {
          _commands.Enqueue(new DespawnEntityCommand(projectileEntity));
        }
      });
  }

  private static bool Overlaps(
    TransformComponent firstTransform,
    ColliderComponent firstCollider,
    TransformComponent secondTransform,
    ColliderComponent secondCollider)
  {
    return firstTransform.X < secondTransform.X + secondCollider.Width &&
      firstTransform.X + firstCollider.Width > secondTransform.X &&
      firstTransform.Y < secondTransform.Y + secondCollider.Height &&
      firstTransform.Y + firstCollider.Height > secondTransform.Y;
  }

  private void RequestProjectiles()
  {
    foreach (Entity playerEntity in _players.Values)
    {
      PlayerInputComponent input = World.Get<PlayerInputComponent>(playerEntity);
      ref PlayerControlStateComponent control = ref World.Get<PlayerControlStateComponent>(playerEntity);
      if (control.FireCooldownTicks > 0)
      {
        control.FireCooldownTicks--;
      }

      if (!input.Fire || control.FireCooldownTicks > 0)
      {
        continue;
      }

      TransformComponent transform = World.Get<TransformComponent>(playerEntity);
      FacingComponent facing = World.Get<FacingComponent>(playerEntity);
      _commands.Enqueue(new SpawnProjectileCommand(
        playerEntity,
        transform.X,
        transform.Y + 0.75f,
        facing.Horizontal,
        ProjectileDamage,
        ProjectileLifetimeTicks));
      control.FireCooldownTicks = 10;
    }
  }

  private void ThrowIfDisposed()
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(DomeSimulation));
    }
  }
}
