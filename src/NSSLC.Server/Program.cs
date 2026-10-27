using System.Net;
using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Communication;
using Terraria.Diagnostics;
using Terraria.Entity;
using Terraria.Movement;
using Terraria.Physics;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.World;

namespace NSSLC.Server;

internal static class Program
{
  private const int WorldWidth = 256;
  private const int WorldHeight = 128;
  private const float TickDeltaTime = 1f / 60f;
  private const int ListenPort = 7777;

  public static async Task<int> Main()
  {
    using ArchWorld ecsWorld = ArchWorld.Create();
    WorldRuntime worldRuntime = new(WorldWidth, WorldHeight);
    EntityRuntimeRegistry entities = new(worldScope: 1);
    ProjectileRuntimeRegistry projectiles = new(entities);
    CombatAttackDefinitionCatalog attackDefinitions = CreateAttackDefinitions();
    CombatAttackIntentBuffer attackIntents = new();
    CombatDamageRequestBuffer damageRequests = new();
    PhysicsContactBuffer contacts = new();
    ProjectileDefinitionCatalog projectileDefinitions = new();
    projectileDefinitions.Seal();
    ProjectileLifecycleSystem projectileLifecycle = new(
        ecsWorld,
        projectileDefinitions,
        projectiles);
    ProjectileHitResolutionSystem projectileHits = new(ecsWorld, entities);
    PhysicsContactCaptureSystem physicsContacts = new(
        ecsWorld,
        worldRuntime,
        entities,
        contacts);
    WorldTickSystem worldTick = new(ecsWorld, worldRuntime);
    WorldInteractionSystem worldInteractions = new(ecsWorld, worldRuntime);
    Dictionary<SessionEpoch, EntityRuntimeHandle> sessions = [];
    object lifecycleGate = new();

    CommunicationHostRuntime? communication = null;
    communication = new(
        ecsWorld,
        new IPEndPoint(IPAddress.Loopback, ListenPort),
        onSessionOpened: epoch =>
        {
          lock (lifecycleGate)
          {
            EntityRuntimeHandle handle = CreatePlayer(ecsWorld, entities);
            if (!communication!.Ingress.TryBindPlayer(epoch, handle))
            {
              entities.Remove(handle, out ArchEntity entity);
              if (ecsWorld.IsAlive(entity))
              {
                ecsWorld.Destroy(entity);
              }

              throw new InvalidOperationException(
                  "The new communication session could not bind its player entity.");
            }

            sessions.Add(epoch, handle);
            ServerLog.Info("ecs.session.player_bound", $"epoch={epoch.Value}");
          }

          return ValueTask.CompletedTask;
        },
        onSessionClosed: closed =>
        {
          lock (lifecycleGate)
          {
            if (sessions.Remove(closed.Epoch, out EntityRuntimeHandle handle) &&
                entities.Remove(handle, out ArchEntity entity) && ecsWorld.IsAlive(entity))
            {
              ecsWorld.Destroy(entity);
            }

            ServerLog.Info(
                "ecs.session.player_unbound",
                $"epoch={closed.Epoch.Value} reason={ServerLog.Quote(closed.Reason)}");
          }

          return ValueTask.CompletedTask;
        });

    IReadOnlyList<EcsServerSystemRegistration> registrations = BuildRegistrations(
        ecsWorld,
        entities,
        attackIntents,
        attackDefinitions,
        damageRequests,
        physicsContacts,
        projectileHits,
        projectileLifecycle,
        communication,
        worldTick,
        worldInteractions);
    using EcsServer server = EcsServer.Create(ecsWorld, registrations);
    server.Initialize();

    using CancellationTokenSource shutdown = new();
    ConsoleCancelEventHandler cancel = (_, args) =>
    {
      args.Cancel = true;
      shutdown.Cancel();
    };
    Console.CancelKeyPress += cancel;
    try
    {
      await communication!.StartAsync(shutdown.Token).ConfigureAwait(false);
      ServerLog.Info(
          "ecs.server.ready",
          $"listen=127.0.0.1:{ListenPort} world={WorldWidth}x{WorldHeight}");
      await RunTicksAsync(server, shutdown.Token).ConfigureAwait(false);
    }
    catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
    {
      ServerLog.Info("ecs.server.canceled", "The ECS server shutdown was requested.");
    }
    catch (Exception exception)
    {
      ServerLog.Error("ecs.server.failed", exception);
      return 1;
    }
    finally
    {
      Console.CancelKeyPress -= cancel;
      await communication!.StopAsync().ConfigureAwait(false);
      ServerLog.Info("ecs.server.stopped", $"lastTick={server.LastTick}");
    }

    await communication!.DisposeAsync().ConfigureAwait(false);

    return 0;
  }

  private static IReadOnlyList<EcsServerSystemRegistration> BuildRegistrations(
      ArchWorld world,
      EntityRuntimeRegistry entities,
      CombatAttackIntentBuffer attackIntents,
      CombatAttackDefinitionCatalog attackDefinitions,
      CombatDamageRequestBuffer damageRequests,
      PhysicsContactCaptureSystem physicsContacts,
      ProjectileHitResolutionSystem projectileHits,
      ProjectileLifecycleSystem projectileLifecycle,
      CommunicationHostRuntime communication,
      WorldTickSystem worldTick,
      WorldInteractionSystem worldInteractions)
  {
    List<EcsServerSystemRegistration> registrations = [];
    registrations.AddRange(CommunicationSystemRegistration.CreateGame(
        world,
        entities,
        attackIntents,
        attackDefinitions,
        communication.OutboundPort));
    registrations.AddRange(CombatSystemRegistration.Create(
        world,
        entities,
        entities,
        physicsContacts,
        projectileHits,
        projectileLifecycle,
        attackDefinitions,
        damageRequests,
        attackIntents: attackIntents));
    registrations.Add(new EcsServerSystemRegistration(EcsServerPhase.Simulation, worldTick));
    registrations.Add(new EcsServerSystemRegistration(EcsServerPhase.Commit, worldInteractions));
    return registrations.AsReadOnly();
  }

  private static EntityRuntimeHandle CreatePlayer(
      ArchWorld world,
      EntityRuntimeRegistry entities)
  {
    EntityUuid id = EntityUuid.Create();
    EntityRuntimeHandle handle = entities.Allocate(id);
    ArchEntity entity = world.Create(
        new EntityIdentityComponent(id),
        new EntityFactionComponent(Faction.Friendly),
        new LifeComponent(current: 100, maximum: 100),
        new LocationComponent(Vector2.Zero),
        Terraria.Movement.VelocityComponent.Stationary,
        new ColliderComponent(new Vector2(16f, 32f)),
        new EntityCollisionCapabilityComponent(Enabled: true, Layer: 1, Mask: uint.MaxValue),
        MovementIntent.Stop(tick: 0),
        new WorldInteractionCapabilityComponent(
            WorldInteractionPermission.ReadTile |
            WorldInteractionPermission.BreakTile |
            WorldInteractionPermission.PlaceTile),
        WorldInteractionRequestComponent.ReadTile(
            requestId: 0,
            tick: 0,
            coordinate: new WorldTileCoordinate(0, 0)),
        new WorldInteractionResultComponent());
    try
    {
      entities.Register(handle, entity);
      return handle;
    }
    catch
    {
      if (world.IsAlive(entity))
      {
        world.Destroy(entity);
      }

      throw;
    }
  }

  private static CombatAttackDefinitionCatalog CreateAttackDefinitions()
  {
    CombatAttackDefinitionCatalog definitions = new();
    definitions.Register(new CombatAttackDefinition(
        new CombatAttackDefinitionId("player.default"),
        new DamagePayload(baseDamage: 10, type: DamageType.Melee)));
    definitions.Seal();
    return definitions;
  }

  private static async Task RunTicksAsync(EcsServer server, CancellationToken cancellationToken)
  {
    using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(1000d / 60d));
    long tick = 0;
    while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
    {
      server.Update(tick++, TickDeltaTime);
    }
  }
}
