using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using EntityEcs;
using EntityEcs.Components;
using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.IO;
using NSSLC.WorldGeneration.Utilities;
using Terraria.Content;
using Terraria.Items;
using NSSLC.WorldGeneration.ID;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.Simulation;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.Npc;
using Terraria.Projectile;
using PlayerInventoryItemSnapshot = Terraria.Player.PlayerInventoryItemSnapshot;
using Terraria.Relationships;
using Terraria.Town;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;
using Terraria.WorldStorage;
using RuntimeMain = NSSLC.WorldGeneration.Main;

namespace Terraria.NonAuthoritative.SimulationHost;

internal static class Program
{
  private const int ChestItemProbeSlot = 0;
  private const int ChestItemProbeType = 23;
  private const int ChestItemProbeStack = 7;
  private const int WorldItemPhysicsProbeMinimumTicks = 300;
  private const float WorldItemPhysicsProbeStartHeightAboveSpawnPixels = 160f;
  private sealed record NpcEyeTransformationProbeSetup(
    string Scenario,
    int EyeSlot,
    int? ReleasedLowerSlot,
    int PrefilledNpcCount);

  private static readonly int[] SpatialProbeRequiredNpcNetIds =
  {
    SimulationContentSupportManifest.BlueSlimeNetId,
    SimulationContentSupportManifest.DemonEyeNetId,
    SimulationContentSupportManifest.ZombieNetId,
    SimulationContentSupportManifest.GreenSlimeNetId,
    SimulationContentSupportManifest.GuideNetId,
    SimulationContentSupportManifest.OldManNetId,
    SimulationContentSupportManifest.EyeOfCthulhuNetId,
    SimulationContentSupportManifest.TrainingDummyNetId,
  };

  private static int Main(string[] args)
  {
    using var cancellationSource = new CancellationTokenSource();
    ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
    {
      eventArgs.Cancel = true;
      cancellationSource.Cancel();
    };
    Console.CancelKeyPress += cancelHandler;
    try
    {
      Run(args, cancellationSource);
      return 0;
    }
    catch (OperationCanceledException)
    {
      Console.Error.WriteLine("Simulation canceled.");
      return 130;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
    finally
    {
      Console.CancelKeyPress -= cancelHandler;
    }
  }

  private static void Run(string[] args, CancellationTokenSource cancellationSource)
  {
    ArgumentNullException.ThrowIfNull(cancellationSource);
    CancellationToken cancellationToken = cancellationSource.Token;
    if (args.Length < 2 ||
        !int.TryParse(args[1], out int tickCount) ||
        tickCount < 0 || tickCount > 10_000_000)
    {
      throw new ArgumentException(
        "Usage: <world.wld> <ticks> [--report <report.json>] " +
        "[--world-time-rate <ticks-per-step>] [--players <count>] " +
        "[--seed <integer>] [--input-script <script.json>] " +
        "[--spawn-npc <net-id>]... [--save <world.wld>] " +
        "[--autosave-interval <ticks>] [--cancel-after-ms <milliseconds>] " +
        "[--cancel-after-ticks <ticks>] " +
        "[--liquid-probe <true|false>] " +
        "[--liquid-checksum <true|false>] " +
        "[--inspect-tile-area <x,y>] " +
        "[--pressure-plate-probe <true|false>] " +
        "[--player-root-probe <true|false>] " +
        "[--chest-item-probe <set|inspect>] " +
        "[--world-item-probe <expired|physics|partial-pickup|full-inventory>] " +
        "[--npc-slot-probe <true|false>] " +
        "[--npc-relation-probe <true|false>] " +
        "[--npc-relation-chain-probe <true|false>] " +
        "[--npc-night-probe <true|false>] " +
        "[--npc-eye-scenario <classic-night|expert-night>] " +
        "[--npc-eye-transform-probe " +
        "<expert-summon|phase-one-boundary|phase-two-boundary|capacity-rejection|next-tick>] " +
        "[--npc-home-return-probe <success|blocked|invalid>] " +
        "[--npc-housing-revalidation-probe <valid|invalid>] " +
        "[--npc-despawn-probe <true|false>] [--npc-despawn-probe-net-id <id>] " +
        "[--npc-reset-preflight-probe <true|false>] " +
        "[--npc-task-lifecycle-probe <true|false>] " +
        "[--player-cleanup-preflight-probe <true|false>] " +
        "[--runtime-world-load-rollback <candidate-world.wld>] " +
        "[--spatial-entity-probe <true|false>] " +
        "[--tile-entity-removal-probe <true|false>] " +
        "[--tile-entity-reload-probe <true|false>] " +
        "[--switch-world <world.wld>]...");
    }

    string worldPath = Path.GetFullPath(args[0]);
    if (!File.Exists(worldPath))
    {
      throw new FileNotFoundException("The requested world file does not exist.", worldPath);
    }

    string reportPath = string.Empty;
    int worldTimeRate = 1;
    int randomSeed = 0x4E4C5458;
    int localPlayerCount = 0;
    string savePath = string.Empty;
    int autosaveIntervalTicks = 0;
    int cancelAfterMilliseconds = 0;
    int cancelAfterTicks = 0;
    bool liquidProbeEnabled = false;
    bool liquidChecksumEnabled = false;
    TileCoordinate? inspectTileAreaCoordinate = null;
    bool pressurePlateProbeEnabled = false;
    bool playerRootProbeEnabled = false;
    string? chestItemProbe = null;
    string? worldItemProbe = null;
    bool npcSlotProbeEnabled = false;
    bool npcRelationProbeEnabled = false;
    bool npcRelationChainProbeEnabled = false;
    bool npcNightProbeEnabled = false;
    string? npcEyeScenario = null;
    string? npcEyeTransformProbe = null;
    string? npcHomeReturnProbe = null;
    string? npcHousingRevalidationProbe = null;
    bool npcDespawnProbeEnabled = false;
    int? npcDespawnProbeNetId = null;
    bool npcResetPreflightProbeEnabled = false;
    bool npcTaskLifecycleProbeEnabled = false;
    bool playerCleanupPreflightProbeEnabled = false;
    string runtimeWorldLoadRollbackPath = string.Empty;
    bool spatialEntityProbeEnabled = false;
    bool tileEntityRemovalProbeEnabled = false;
    bool tileEntityReloadProbeEnabled = false;
    string inputScriptPath = string.Empty;
    var npcTypesToSpawn = new List<int>();
    var switchWorldPaths = new List<string>();
    for (int index = 2; index < args.Length; index++)
    {
      string option = args[index];
      if (index + 1 >= args.Length)
      {
        throw new ArgumentException($"Option '{option}' requires a value.");
      }

      string value = args[++index];
      switch (option)
      {
        case "--report":
          reportPath = Path.GetFullPath(value);
          break;
        case "--world-time-rate":
          if (!int.TryParse(value, out worldTimeRate) || worldTimeRate < 0)
          {
            throw new ArgumentException("World time rate must be a non-negative integer.");
          }
          break;
        case "--seed":
          if (!int.TryParse(value, out randomSeed))
          {
            throw new ArgumentException("The simulation seed must be a 32-bit integer.");
          }
          break;
        case "--input-script":
          inputScriptPath = Path.GetFullPath(value);
          break;
        case "--players":
          if (!int.TryParse(value, out localPlayerCount) ||
              localPlayerCount < 0 || localPlayerCount > byte.MaxValue)
          {
            throw new ArgumentException("Local player count must be between 0 and 255.");
          }
          break;
        case "--save":
          savePath = Path.GetFullPath(value);
          break;
        case "--autosave-interval":
          if (!int.TryParse(value, out autosaveIntervalTicks) || autosaveIntervalTicks <= 0)
          {
            throw new ArgumentException(
              "Autosave interval must be a positive integer number of ticks.");
          }
          break;
        case "--cancel-after-ms":
          if (!int.TryParse(value, out cancelAfterMilliseconds) ||
              cancelAfterMilliseconds <= 0)
          {
            throw new ArgumentException(
              "Cancellation delay must be a positive number of milliseconds.");
          }
          break;
        case "--cancel-after-ticks":
          if (!int.TryParse(value, out cancelAfterTicks) || cancelAfterTicks <= 0)
          {
            throw new ArgumentException(
              "Cancellation tick must be a positive number of committed simulation ticks.");
          }
          break;
        case "--liquid-probe":
          if (!bool.TryParse(value, out liquidProbeEnabled))
          {
            throw new ArgumentException("The liquid probe option must be true or false.");
          }
          break;
        case "--liquid-checksum":
          if (!bool.TryParse(value, out liquidChecksumEnabled))
          {
            throw new ArgumentException("The liquid checksum option must be true or false.");
          }
          break;
        case "--inspect-tile-area":
        case "--inspect-liquid-probe":
          string[] coordinates = value.Split(',', StringSplitOptions.TrimEntries);
          if (coordinates.Length != 2 ||
              !int.TryParse(coordinates[0], out int inspectX) ||
              !int.TryParse(coordinates[1], out int inspectY))
          {
            throw new ArgumentException("The liquid probe coordinate must use x,y format.");
          }
          inspectTileAreaCoordinate = new TileCoordinate(inspectX, inspectY);
          break;
        case "--pressure-plate-probe":
          if (!bool.TryParse(value, out pressurePlateProbeEnabled))
          {
            throw new ArgumentException(
              "The pressure-plate probe option must be true or false.");
          }
          break;
        case "--player-root-probe":
          if (!bool.TryParse(value, out playerRootProbeEnabled))
          {
            throw new ArgumentException("The Player root probe option must be true or false.");
          }
          break;
        case "--chest-item-probe":
          chestItemProbe = value.ToLowerInvariant();
          if (chestItemProbe is not ("set" or "inspect"))
          {
            throw new ArgumentException("The chest item probe must be set or inspect.");
          }
          break;
        case "--world-item-probe":
          worldItemProbe = value.ToLowerInvariant();
          if (worldItemProbe is not ("expired" or "physics" or "partial-pickup" or "full-inventory"))
          {
            throw new ArgumentException(
              "The world item probe must be expired, physics, partial-pickup, or full-inventory.");
          }
          break;
        case "--npc-slot-probe":
          if (!bool.TryParse(value, out npcSlotProbeEnabled))
          {
            throw new ArgumentException("The NPC slot probe option must be true or false.");
          }
          break;
        case "--npc-relation-probe":
          if (!bool.TryParse(value, out npcRelationProbeEnabled))
          {
            throw new ArgumentException("The NPC relation probe option must be true or false.");
          }
          break;
        case "--npc-relation-chain-probe":
          if (!bool.TryParse(value, out npcRelationChainProbeEnabled))
          {
            throw new ArgumentException(
              "The NPC relation chain probe option must be true or false.");
          }
          break;
        case "--npc-night-probe":
          if (!bool.TryParse(value, out npcNightProbeEnabled))
          {
            throw new ArgumentException("The NPC night probe option must be true or false.");
          }
          break;
        case "--npc-eye-scenario":
          npcEyeScenario = value.ToLowerInvariant();
          if (npcEyeScenario is not ("classic-night" or "expert-night"))
          {
            throw new ArgumentException(
              "The Eye scenario must be classic-night or expert-night.");
          }
          break;
        case "--npc-eye-transform-probe":
          npcEyeTransformProbe = value.ToLowerInvariant();
          if (npcEyeTransformProbe is not (
                "expert-summon" or
                "phase-one-boundary" or
                "phase-two-boundary" or
                "capacity-rejection" or
                "next-tick"))
          {
            throw new ArgumentException("The Eye transformation probe is not recognized.");
          }
          break;
        case "--npc-home-return-probe":
          npcHomeReturnProbe = value.ToLowerInvariant();
          if (npcHomeReturnProbe is not ("success" or "blocked" or "invalid"))
          {
            throw new ArgumentException(
              "The NPC home return probe must be success, blocked, or invalid.");
          }
          break;
        case "--npc-housing-revalidation-probe":
          npcHousingRevalidationProbe = value.ToLowerInvariant();
          if (npcHousingRevalidationProbe is not ("valid" or "invalid"))
          {
            throw new ArgumentException(
              "The NPC housing revalidation probe must be valid or invalid.");
          }
          break;
        case "--npc-despawn-probe":
          if (!bool.TryParse(value, out npcDespawnProbeEnabled))
          {
            throw new ArgumentException("The NPC despawn probe option must be true or false.");
          }
          break;
        case "--npc-despawn-probe-net-id":
          if (!int.TryParse(value, out int despawnProbeNetId) || despawnProbeNetId <= 0)
          {
            throw new ArgumentException("The NPC despawn probe net id must be a positive integer.");
          }
          npcDespawnProbeNetId = despawnProbeNetId;
          break;
        case "--spatial-entity-probe":
          if (!bool.TryParse(value, out spatialEntityProbeEnabled))
          {
            throw new ArgumentException(
              "The spatial entity probe option must be true or false.");
          }
          break;
        case "--npc-task-lifecycle-probe":
          if (!bool.TryParse(value, out npcTaskLifecycleProbeEnabled))
          {
            throw new ArgumentException("The NPC task lifecycle probe option must be true or false.");
          }
          break;
        case "--npc-reset-preflight-probe":
          if (!bool.TryParse(value, out npcResetPreflightProbeEnabled))
          {
            throw new ArgumentException(
              "The NPC reset preflight probe option must be true or false.");
          }
          break;
        case "--player-cleanup-preflight-probe":
          if (!bool.TryParse(value, out playerCleanupPreflightProbeEnabled))
          {
            throw new ArgumentException(
              "The Player cleanup preflight probe option must be true or false.");
          }
          break;
        case "--runtime-world-load-rollback":
          runtimeWorldLoadRollbackPath = Path.GetFullPath(value);
          break;
        case "--tile-entity-removal-probe":
          if (!bool.TryParse(value, out tileEntityRemovalProbeEnabled))
          {
            throw new ArgumentException(
              "The TileEntity removal probe option must be true or false.");
          }
          break;
        case "--tile-entity-reload-probe":
          if (!bool.TryParse(value, out tileEntityReloadProbeEnabled))
          {
            throw new ArgumentException(
              "The TileEntity reload probe option must be true or false.");
          }
          break;
        case "--spawn-npc":
          if (!int.TryParse(value, out int npcNetId) || npcNetId <= 0)
          {
            throw new ArgumentException("NPC net ids must be positive integers.");
          }
          npcTypesToSpawn.Add(npcNetId);
          break;
        case "--switch-world":
          switchWorldPaths.Add(Path.GetFullPath(value));
          break;
        default:
          throw new ArgumentException($"Unknown simulation option '{option}'.");
      }
    }

    if (autosaveIntervalTicks > 0 && string.IsNullOrWhiteSpace(savePath))
    {
      throw new ArgumentException("--autosave-interval requires --save <world.wld>.");
    }

    if (tileEntityRemovalProbeEnabled && tickCount < 3)
    {
      throw new ArgumentException(
        "The TileEntity removal probe requires at least three simulation ticks.");
    }

    if (tileEntityReloadProbeEnabled &&
        (switchWorldPaths.Count != 1 ||
         string.IsNullOrWhiteSpace(savePath) ||
         !string.Equals(
           savePath,
           switchWorldPaths[0],
           StringComparison.OrdinalIgnoreCase)))
    {
      throw new ArgumentException(
        "The TileEntity reload probe requires one --switch-world target that matches --save.");
    }

    if (playerRootProbeEnabled && localPlayerCount != 1)
    {
      throw new ArgumentException("The Player root probe requires exactly one local player.");
    }

    if (npcResetPreflightProbeEnabled && (localPlayerCount != 1 || tickCount < 1))
    {
      throw new ArgumentException(
        "The NPC reset preflight probe requires exactly one local player and at least one tick.");
    }

    if (playerCleanupPreflightProbeEnabled && (localPlayerCount != 2 || tickCount < 1))
    {
      throw new ArgumentException(
        "The Player cleanup preflight probe requires exactly two local players and at least one tick.");
    }

    if (!string.IsNullOrWhiteSpace(runtimeWorldLoadRollbackPath))
    {
      if (!File.Exists(runtimeWorldLoadRollbackPath))
      {
        throw new FileNotFoundException(
          "The runtime world-load rollback candidate does not exist.",
          runtimeWorldLoadRollbackPath);
      }

      if (tickCount < 1 || localPlayerCount != 1 || switchWorldPaths.Count > 0 ||
          !string.IsNullOrWhiteSpace(savePath) || autosaveIntervalTicks > 0 ||
          cancelAfterMilliseconds > 0 || cancelAfterTicks > 0 ||
          tileEntityRemovalProbeEnabled || tileEntityReloadProbeEnabled ||
          npcResetPreflightProbeEnabled || playerCleanupPreflightProbeEnabled)
      {
        throw new ArgumentException(
          "The runtime world-load rollback probe requires at least one tick, exactly one local " +
          "player, and no switch, save, cancellation, TileEntity, NPC reset, or Player cleanup probes.");
      }
    }

    if (npcDespawnProbeEnabled &&
        (localPlayerCount != 0 || tickCount < RuntimeNpcStore.NaturalDespawnGraceTicks))
    {
      throw new ArgumentException(
        "The NPC despawn probe requires --players 0 and at least 300 ticks.");
    }

    if (npcDespawnProbeNetId.HasValue && !npcDespawnProbeEnabled)
    {
      throw new ArgumentException(
        "--npc-despawn-probe-net-id requires --npc-despawn-probe true.");
    }

    if (npcDespawnProbeEnabled && !npcDespawnProbeNetId.HasValue)
    {
      npcDespawnProbeNetId = SimulationContentSupportManifest.BlueSlimeNetId;
    }

    if (spatialEntityProbeEnabled)
    {
      if (tickCount < 600 || localPlayerCount > 2)
      {
        throw new ArgumentException(
          "The spatial entity probe requires at most two players and 600 ticks.");
      }

      foreach (int requiredNetId in SpatialProbeRequiredNpcNetIds)
      {
        if (!npcTypesToSpawn.Contains(requiredNetId))
        {
          throw new ArgumentException(
            $"The spatial entity probe requires --spawn-npc {requiredNetId}.");
        }
      }

      if (npcTypesToSpawn.Count(netId =>
            netId == SimulationContentSupportManifest.BlueSlimeNetId) < 2)
      {
        throw new ArgumentException(
          "The spatial entity probe requires two --spawn-npc entries for Blue Slime.");
      }
    }

    if (npcNightProbeEnabled && tickCount < 1)
    {
      throw new ArgumentException("The NPC night probe requires at least one simulation tick.");
    }

    if (npcEyeScenario is not null &&
        (tickCount < 1 || localPlayerCount != 1 ||
         npcTypesToSpawn.Count(netId => netId == SimulationContentSupportManifest.EyeOfCthulhuNetId) != 1 ||
         switchWorldPaths.Count > 0 || !string.IsNullOrWhiteSpace(savePath)))
    {
      throw new ArgumentException(
          "The Eye scenario requires one Eye spawn, one local player, no world switch, and no save.");
    }

    if (npcEyeTransformProbe is not null)
    {
      int requiredTicks = npcEyeTransformProbe == "next-tick" ? 2 : 1;
      if (npcEyeScenario != "expert-night" || tickCount != requiredTicks ||
          localPlayerCount != 1 ||
          npcTypesToSpawn.Count(netId =>
            netId == SimulationContentSupportManifest.EyeOfCthulhuNetId) != 1 ||
          switchWorldPaths.Count > 0 || !string.IsNullOrWhiteSpace(savePath) ||
          spatialEntityProbeEnabled || npcSlotProbeEnabled || npcRelationProbeEnabled ||
          npcRelationChainProbeEnabled || npcNightProbeEnabled ||
          npcHomeReturnProbe is not null || npcHousingRevalidationProbe is not null ||
          npcDespawnProbeEnabled || npcResetPreflightProbeEnabled ||
          npcTaskLifecycleProbeEnabled || tileEntityRemovalProbeEnabled)
      {
        throw new ArgumentException(
          "The Eye transformation probe requires expert-night, one player, one Eye, its exact " +
          "tick count, no world switch or save, and no competing NPC probes.");
      }

      if (npcEyeTransformProbe == "next-tick" &&
          (npcTypesToSpawn.Count < 2 ||
           npcTypesToSpawn[0] != SimulationContentSupportManifest.ZombieNetId ||
           npcTypesToSpawn[1] != SimulationContentSupportManifest.EyeOfCthulhuNetId))
      {
        throw new ArgumentException(
          "The next-tick Eye probe requires --spawn-npc 3 before --spawn-npc 4.");
      }

      if (npcEyeTransformProbe == "capacity-rejection" && npcTypesToSpawn.Count != 1)
      {
        throw new ArgumentException(
          "The capacity Eye probe requires exactly one requested Eye NPC.");
      }
    }

    if (npcHomeReturnProbe is not null &&
        (localPlayerCount != 0 || tickCount < NpcGuideHomeReturnProfile.TeleportTriggerCursor))
    {
      throw new ArgumentException(
        "The NPC home return probe requires --players 0 and at least " +
        $"{NpcGuideHomeReturnProfile.TeleportTriggerCursor} simulation ticks.");
    }

    if (npcHousingRevalidationProbe is not null &&
        (localPlayerCount != 0 || tickCount < 1))
    {
      throw new ArgumentException(
        "The NPC housing revalidation probe requires --players 0 and at least one " +
        "simulation tick.");
    }

    if (npcDespawnProbeNetId is int requestedDespawnProbeNetId &&
        !SimulationContentSupportManifest.TryGetNpcNaturalDespawnPolicy(
          requestedDespawnProbeNetId,
          out _))
    {
      throw new ArgumentException(
        $"NPC {requestedDespawnProbeNetId} has no declared natural despawn policy.");
    }

    if (worldItemProbe == "physics" &&
        (localPlayerCount != 0 || tickCount < WorldItemPhysicsProbeMinimumTicks))
    {
      throw new ArgumentException(
        $"The world item physics probe requires --players 0 and at least " +
        $"{WorldItemPhysicsProbeMinimumTicks} ticks.");
    }

    if (chestItemProbe == "set" && string.IsNullOrWhiteSpace(savePath))
    {
      throw new ArgumentException(
        "The chest item set probe requires --save so the owner mutation can be reloaded.");
    }

    if (cancelAfterTicks > 0 &&
        (cancelAfterMilliseconds > 0 || cancelAfterTicks >= tickCount))
    {
      throw new ArgumentException(
        "--cancel-after-ticks must be less than the tick limit and cannot be combined " +
        "with --cancel-after-ms.");
    }

    foreach (string switchWorldPath in switchWorldPaths)
    {
      bool willBeCreatedByThisRun =
        !string.IsNullOrWhiteSpace(savePath) &&
        string.Equals(
          Path.GetFullPath(switchWorldPath),
          Path.GetFullPath(savePath),
          StringComparison.OrdinalIgnoreCase);
      if (!File.Exists(switchWorldPath) && !willBeCreatedByThisRun)
      {
        throw new FileNotFoundException(
          "A requested switch world file does not exist.",
          switchWorldPath);
      }
    }

    if (cancelAfterMilliseconds > 0)
    {
      cancellationSource.CancelAfter(cancelAfterMilliseconds);
    }

    byte[] originalWorldBytes = File.ReadAllBytes(worldPath);
    string originalWorldHash = Convert.ToHexString(SHA256.HashData(originalWorldBytes));
    SimulationInputScript? inputScript = string.IsNullOrWhiteSpace(inputScriptPath)
      ? null
      : SimulationInputScript.Load(inputScriptPath);
    ContentCatalog content = SimulationContentBootstrap.Build();
    var hostIdentityRegistry = new EntityIdentityRegistry();
    var projectileStaticNpcImmunity = new ProjectileStaticNpcImmunityRegistryComponent(
      checked(content.Snapshot.Projectiles.MaximumTypeId + 1),
      RuntimeNpcStore.MaximumNpcCapacity);
    var ioGate = new WorldStorageIoGate();
    var runtimeNpcs = new RuntimeNpcStore(projectileStaticNpcImmunity, hostIdentityRegistry);
    var runtimeNpcStoresBySession = new Dictionary<LoadedWorldSession, RuntimeNpcStore>();
    var projectileStaticNpcImmunitiesBySession =
      new Dictionary<LoadedWorldSession, ProjectileStaticNpcImmunityRegistryComponent>();
    var entityRuntimesBySession = new Dictionary<LoadedWorldSession, EntityRuntime>();
    var loadedSessions = new List<LoadedWorldSession>();
    var failedCandidateNpcEvidenceBySession =
      new Dictionary<LoadedWorldSession, RuntimeNpcCandidateRollbackEvidence>();
    RuntimeNpcRollbackEvidence? previousNpcRollbackEvidence = null;
    bool lateFinalizeFailureArmed = false;
    bool lateFinalizeFailureObserved = false;
    WorldLoadRecoveryResult? loadResult = null;
    Exception? loadException = null;

    RuntimeMain.InitializeHeadlessRuntime();
    using IDisposable mainThreadActionQueue = RuntimeMain.BindMainThreadActionQueue();
    RuntimeMain.dedServ = true;
    RuntimeMain.netMode = 0;
    RuntimeMain.gameMenu = false;
    RuntimeMain.worldPathName = worldPath;
    RuntimeMain.rand = new UnifiedRandom(randomSeed);
    new WorldFileData(Path.GetFileNameWithoutExtension(worldPath)).SetAsActive();

    WorldStorageCoordinatorFactory.RegisterGeneratedWorldLoadHandlerWithRuntimeTileMap(
      ioGate,
      (session, cancellationToken) =>
      {
        if (cancellationToken.IsCancellationRequested)
        {
          return WorldStorageOperationResult.Failed(
            WorldStorageFailure.Create(WorldStorageFailureKind.Canceled));
        }

        try
        {
          runtimeNpcs.Hydrate(session, content);
          runtimeNpcStoresBySession[session] = runtimeNpcs;
          projectileStaticNpcImmunitiesBySession[session] = projectileStaticNpcImmunity;
          return WorldStorageOperationResult.Success;
        }
        catch (InvalidDataException exception)
        {
          return WorldStorageOperationResult.Failed(
            WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, exception.Message));
        }
      },
      static (_, cancellationToken) => cancellationToken.IsCancellationRequested
        ? WorldStorageOperationResult.Failed(
          WorldStorageFailure.Create(WorldStorageFailureKind.Canceled))
        : WorldStorageOperationResult.Success,
      FinalizeLoadedWorld,
      resultObserver: result =>
      {
        loadResult = result;
        if (result.Succeeded &&
            WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession is
              LoadedWorldSession activeSession)
        {
          foreach (LoadedWorldSession staleSession in runtimeNpcStoresBySession.Keys
                     .Where(session => !ReferenceEquals(session, activeSession))
                     .ToArray())
          {
            if (string.IsNullOrWhiteSpace(runtimeWorldLoadRollbackPath))
            {
              runtimeNpcStoresBySession.Remove(staleSession);
              projectileStaticNpcImmunitiesBySession.Remove(staleSession);
              failedCandidateNpcEvidenceBySession.Remove(staleSession);
            }
          }

          previousNpcRollbackEvidence = null;
        }
      },
      exceptionObserver: exception =>
      {
        loadException = exception;
      },
      cancellationTokenProvider: () => cancellationToken,
      resetHostWorldState: session =>
      {
        if (runtimeNpcStoresBySession.TryGetValue(session, out RuntimeNpcStore? candidateStore))
        {
          candidateStore.Reset();
          if (string.IsNullOrWhiteSpace(runtimeWorldLoadRollbackPath))
          {
            runtimeNpcStoresBySession.Remove(session);
            projectileStaticNpcImmunitiesBySession.Remove(session);
            failedCandidateNpcEvidenceBySession.Remove(session);
          }
        }
        else
        {
          runtimeNpcs.Reset();
        }

        return WorldStorageOperationResult.Success;
      },
      sessionFactory: CreateSession,
      reprojectPreviousWorldState: previousSession =>
      {
        if (!runtimeNpcStoresBySession.TryGetValue(
              previousSession,
              out RuntimeNpcStore? previousStore) ||
            !projectileStaticNpcImmunitiesBySession.TryGetValue(
              previousSession,
              out ProjectileStaticNpcImmunityRegistryComponent? previousImmunityRegistry) ||
            previousNpcRollbackEvidence is not RuntimeNpcRollbackEvidence evidence ||
            !HasRuntimeNpcProjectionUnchanged(evidence, previousSession, previousStore))
        {
          return WorldStorageOperationResult.Failed(
            WorldStorageFailure.Create(
              WorldStorageFailureKind.Unknown,
              "The previous session's NPC store, roots, or projection changed before rollback."));
        }

        runtimeNpcs = previousStore;
        projectileStaticNpcImmunity = previousImmunityRegistry;
        return WorldStorageOperationResult.Success;
      });

    LoadedWorldSession CreateSession()
    {
      var createdSession = new LoadedWorldSession(hostIdentityRegistry);
      loadedSessions.Add(createdSession);
      entityRuntimesBySession.Add(createdSession, createdSession.EntityRuntime);
      return createdSession;
    }

    WorldStorageOperationResult FinalizeLoadedWorld(
      LoadedWorldSession finalizedSession,
      CancellationToken finalizeCancellationToken)
    {
      if (finalizeCancellationToken.IsCancellationRequested)
      {
        return WorldStorageOperationResult.Failed(
          WorldStorageFailure.Create(WorldStorageFailureKind.Canceled));
      }

      if (lateFinalizeFailureArmed)
      {
        if (!finalizedSession.IsPublished ||
            !ReferenceEquals(
              WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
              finalizedSession))
        {
          return WorldStorageOperationResult.Failed(
            WorldStorageFailure.Create(
              WorldStorageFailureKind.InvalidData,
              "The controlled rollback fault did not reach a published active candidate."));
        }

        if (!string.IsNullOrWhiteSpace(runtimeWorldLoadRollbackPath))
        {
          if (!runtimeNpcStoresBySession.TryGetValue(
                finalizedSession,
                out RuntimeNpcStore? candidateStore) ||
              !projectileStaticNpcImmunitiesBySession.TryGetValue(
                finalizedSession,
                out ProjectileStaticNpcImmunityRegistryComponent? candidateImmunityRegistry))
          {
            throw new InvalidOperationException(
              "The published rollback candidate has no selected NPC store or immunity registry.");
          }

          failedCandidateNpcEvidenceBySession[finalizedSession] =
            RuntimeWorldLoadRollbackVerification.CaptureFailedCandidateNpcEvidence(
              finalizedSession,
              candidateStore,
              candidateImmunityRegistry);
        }

        lateFinalizeFailureArmed = false;
        lateFinalizeFailureObserved = true;
        return WorldStorageOperationResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.Unknown,
            "Controlled late-finalize failure for the runtime world-load rollback probe."));
      }

      return WorldStorageOperationResult.Success;
    }

    WorldLoadRecoveryResult? LoadRollbackCandidate(string candidateWorldPath)
    {
      loadResult = null;
      loadException = null;
      runtimeNpcs = CreateRuntimeNpcStore(out projectileStaticNpcImmunity);
      RuntimeMain.worldPathName = candidateWorldPath;
      RuntimeMain.rand = new UnifiedRandom(randomSeed);
      new WorldFileData(Path.GetFileNameWithoutExtension(candidateWorldPath)).SetAsActive();
      WorldGen.serverLoadWorldCallBack();
      if (loadException is not null)
      {
        throw new InvalidOperationException(
          "The runtime rollback world-load host failed.",
          loadException);
      }

      return loadResult;
    }

    RuntimeNpcStore CreateRuntimeNpcStore(
      out ProjectileStaticNpcImmunityRegistryComponent immunityRegistry)
    {
      immunityRegistry = new ProjectileStaticNpcImmunityRegistryComponent(
        checked(content.Snapshot.Projectiles.MaximumTypeId + 1),
        RuntimeNpcStore.MaximumNpcCapacity);
      return new RuntimeNpcStore(immunityRegistry, hostIdentityRegistry);
    }

    if (cancellationToken.IsCancellationRequested)
    {
      WriteLoadFailureReport(
        reportPath,
        worldPath,
        tickCount,
        localPlayerCount,
        randomSeed,
        loadResult,
        loadException: null,
        canceled: true);
      throw new OperationCanceledException(cancellationToken);
    }

    WorldGen.serverLoadWorldCallBack();
    if (loadException is not null)
    {
      WriteLoadFailureReport(
        reportPath,
        worldPath,
        tickCount,
        localPlayerCount,
        randomSeed,
        loadResult,
        loadException,
        cancellationToken.IsCancellationRequested);
      throw new InvalidOperationException("The production world-load host failed.", loadException);
    }

    if (loadResult?.Succeeded != true)
    {
      bool canceled = cancellationToken.IsCancellationRequested ||
        loadResult?.Failure.Kind == WorldStorageFailureKind.Canceled;
      WriteLoadFailureReport(
        reportPath,
        worldPath,
        tickCount,
        localPlayerCount,
        randomSeed,
        loadResult,
        loadException: null,
        canceled);
      if (canceled)
      {
        throw new OperationCanceledException(cancellationToken);
      }

      string failure = loadResult is null
        ? "The load handler returned no recovery result."
        : $"Recovery: {loadResult.Failure.Kind}: {loadResult.Failure.Detail}; " +
          $"last load: {loadResult.LastLoadOutcome?.Failure.Kind.ToString() ?? "none"}: " +
          $"{loadResult.LastLoadOutcome?.Failure.Detail ?? "none"}";
      throw new InvalidOperationException(
        $"The world was not published. Load status: " +
        $"{loadResult?.TerminalAction.ToString() ?? "missing"}; {failure}");
    }

    LoadedWorldSession session = WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession ??
      throw new InvalidOperationException(
        "The successful load did not publish an active world session.");
    if (!session.IsPublished)
    {
      WriteLoadFailureReport(
        reportPath,
        worldPath,
        tickCount,
        localPlayerCount,
        randomSeed,
        loadResult,
        loadException: null,
        canceled: false);
      throw new InvalidOperationException(
        $"The world load reported success but did not reach the published state: " +
        $"{loadResult.TerminalAction}.");
    }
    string? initialLiquidStateChecksum = liquidChecksumEnabled
      ? CalculateLiquidStateChecksum(session.Storage.TileMap)
      : null;
    var runtimeItems = new RuntimeItemRegistry(content, session.EntityRuntime);
    if (inspectTileAreaCoordinate is TileCoordinate inspectCoordinate &&
        ((uint)inspectCoordinate.X >= (uint)session.Storage.TileMap.Width ||
         (uint)inspectCoordinate.Y >= (uint)session.Storage.TileMap.Height))
    {
      throw new ArgumentOutOfRangeException(
        nameof(inspectTileAreaCoordinate),
        "The requested liquid probe inspection coordinate is outside the loaded world.");
    }
    _ = RuntimeMain.DrainQueuedMainThreadActions();

    if (npcEyeScenario is not null)
    {
      ConfigureEyeWorldScenario(session, npcEyeScenario);
    }

    if (npcHomeReturnProbe is not null || npcHousingRevalidationProbe == "invalid")
    {
      session.World.TimeWeather.DayTime = false;
      session.World.TimeWeather.Time = 0;
    }

    using var runtimePlayers = new RuntimePlayerStore(
      session.EntityRuntime,
      session.IdentityRegistry);
    runtimePlayers.Initialize(
      session,
      localPlayerCount,
      content,
      runtimeItems,
      inputScript,
      session.IdentityRegistry);
    object? playerRootProbe = playerRootProbeEnabled
      ? RunPlayerRootLifecycleProbe(runtimePlayers)
      : null;
    Vector2[] initialPlayerPositions = runtimePlayers.Players
      .Select(static player => player.Movement.Position)
      .ToArray();
    EntityReference[] initialPlayerReferences = runtimePlayers.Players
      .Select(static player => player.Reference)
      .ToArray();
    TileCoordinate? pressurePlateProbeAnchor = pressurePlateProbeEnabled
      ? PressurePlateProbeInstaller.Install(session, runtimePlayers)
      : null;
    var runtimeWorldItems = new RuntimeWorldItemStore(session, content, runtimeItems);
    Vector2? worldItemProbeInitialPosition = null;
    if (worldItemProbe is not null)
    {
      if (worldItemProbe == "expired")
      {
        if (runtimePlayers.ActiveCount != 0)
        {
          throw new ArgumentException("The expiry probe requires --players 0.");
        }

        runtimeWorldItems.SpawnProbeItem(
          typeId: 23,
          stack: 1,
          position: new Vector2(100f, 100f),
          timeLeft: 1);
      }
      else if (worldItemProbe == "physics")
      {
        Vector2 initialPosition = new(
          session.World.Descriptor.SpawnTileX * 16f,
          session.World.Descriptor.SpawnTileY * 16f -
            WorldItemPhysicsProbeStartHeightAboveSpawnPixels);
        runtimeWorldItems.SpawnProbeItem(
          typeId: 23,
          stack: 1,
          position: initialPosition,
          timeLeft: 6_000);
        worldItemProbeInitialPosition = initialPosition;
      }
      else
      {
        if (runtimePlayers.ActiveCount != 1)
        {
          throw new ArgumentException(
            "The pickup probes require exactly one local player.");
        }

        bool fillEveryInventorySlot = worldItemProbe == "full-inventory";
        runtimePlayers.Players[0].Inventory.PrepareWorldItemPickupProbeInventory(
          fillEveryInventorySlot);
        runtimeWorldItems.SpawnProbeItem(
          typeId: 23,
          stack: 5,
          position: runtimePlayers.Players[0].Movement.Position,
          timeLeft: 6_000);
      }
    }
    TileCoordinate? chestItemProbeAnchor = null;
    if (chestItemProbe is not null)
    {
      IReadOnlyList<WorldChestSnapshot> chests = session.Storage.WorldContainers.CreateSnapshot();
      if (chests.Count == 0 || chests[0].Items.Count <= ChestItemProbeSlot)
      {
        throw new InvalidOperationException(
          "The chest item probe requires a loaded chest with the requested item slot.");
      }

      WorldChestSnapshot chest = chests[0];
      if (chestItemProbe == "set")
      {
        ItemState originalItem = chest.Items[ChestItemProbeSlot];
        int stack = originalItem.Type == ChestItemProbeType &&
          originalItem.Prefix == 0 && originalItem.Stack == ChestItemProbeStack
            ? ChestItemProbeStack + 1
            : ChestItemProbeStack;
        if (!session.Storage.WorldContainers.TrySetChestItem(
              chest.Anchor,
              ChestItemProbeSlot,
              new ItemState(ChestItemProbeType, 0, stack)))
        {
          throw new InvalidOperationException(
            "The chest item owner rejected the probe mutation.");
        }
      }

      chestItemProbeAnchor = chest.Anchor;
    }
    TileEntityRemovalProbe? tileEntityProbe = tileEntityRemovalProbeEnabled
      ? TileEntityRemovalProbeInstaller.Install(session, runtimeNpcs)
      : null;
    RuntimeNpcTrainingDummyBinding? trainingDummyTileEntityBinding = null;
    var naturalNpcSpawns = new RuntimeNpcNaturalSpawnPass(
      session,
      runtimePlayers,
      runtimeNpcs,
      content);
    for (int index = 0; index < npcTypesToSpawn.Count; index++)
    {
      Vector2 spawnPosition = runtimePlayers.ActiveCount > 0
        ? runtimePlayers.Players[0].Movement.Position + new Vector2((index + 1) * 48f, 0f)
        : new Vector2(
          session.World.Descriptor.SpawnTileX * 16f,
          session.World.Descriptor.SpawnTileY * 16f - 40f);
      if (!runtimeNpcs.TrySpawn(
        npcTypesToSpawn[index],
        spawnPosition,
        content,
        session.World.Descriptor.WorldId,
        out _))
      {
        throw new InvalidOperationException(
          $"The requested NPC {npcTypesToSpawn[index]} is unsupported or no NPC slot is " +
          "available.");
      }
    }
    NpcEyeTransformationProbeSetup? npcEyeTransformationProbeSetup =
      npcEyeTransformProbe is null
        ? null
        : ConfigureNpcEyeTransformationProbe(
          npcEyeTransformProbe,
          runtimeNpcs,
          content,
          session);
    if (spatialEntityProbeEnabled)
    {
      IReadOnlyList<RuntimeNpcEntity> spatialProbeNpcs = runtimeNpcs.CreateActiveSnapshot();
      IReadOnlyList<RuntimePlayerEntity> spatialProbePlayers = runtimePlayers.Players;
      EntityRuntime entityRuntime = session.EntityRuntime;
      RuntimeSpatialEntityVerification.AssertLiveParity(
        entityRuntime,
        spatialProbeNpcs,
        spatialProbePlayers,
        SpatialProbeRequiredNpcNetIds,
        SimulationContentSupportManifest.BlueSlimeNetId,
        localPlayerCount);
      RuntimeSpatialEntityVerification.AssertInstanceIsolation(
        entityRuntime,
        spatialProbeNpcs,
        spatialProbePlayers,
        SimulationContentSupportManifest.BlueSlimeNetId,
        localPlayerCount);
      RuntimeSpatialEntityVerification.AssertMovementWriteProtocol(
        entityRuntime,
        spatialProbeNpcs.First(npc =>
          npc.Definition.NetId == SimulationContentSupportManifest.BlueSlimeNetId));
      foreach (RuntimePlayerEntity player in spatialProbePlayers)
      {
        RuntimeSpatialEntityVerification.AssertMovementWriteProtocol(entityRuntime, player);
      }

      if (spatialProbePlayers.Count == 1)
      {
        RuntimeNpcEntity hostileNpc = spatialProbeNpcs.First(npc =>
          npc.IsActive && npc.Definition.Capabilities.Hostile);
        RuntimeSpatialEntityVerification.AssertContactImmunityAndRespawn(
          entityRuntime,
          spatialProbePlayers[0],
          hostileNpc,
          tickNumber: 0);
        RuntimeSpatialEntityVerification.AssertLiveParity(
          entityRuntime,
          runtimeNpcs.CreateActiveSnapshot(),
          runtimePlayers.Players,
          SpatialProbeRequiredNpcNetIds,
          SimulationContentSupportManifest.BlueSlimeNetId,
          localPlayerCount);
      }
    }
    int? actualNpcDespawnProbeNetId = null;
    int? npcDespawnProbeSlot = null;
    EntityReference? npcDespawnProbeReference = null;
    if (npcDespawnProbeEnabled)
    {
      Vector2 probePosition = new(
        session.World.Descriptor.SpawnTileX * 16f,
        session.World.Descriptor.SpawnTileY * 16f - 40f);
      if (!runtimeNpcs.TrySpawn(
            npcDespawnProbeNetId.GetValueOrDefault(),
            probePosition,
            content,
            session.World.Descriptor.WorldId,
            out RuntimeNpcEntity? npcDespawnProbe,
            isNaturallySpawned: true) ||
          npcDespawnProbe is null)
      {
        throw new InvalidOperationException(
          "The natural NPC despawn probe could not allocate its runtime entity.");
      }

      actualNpcDespawnProbeNetId = npcDespawnProbe.Definition.NetId;
      npcDespawnProbeSlot = npcDespawnProbe.Slot.Value;
      if (!runtimeNpcs.TryGetEntityReference(
            npcDespawnProbe.InstanceId,
            out EntityReference npcDespawnProbeEntityReference))
      {
        throw new InvalidOperationException(
          "The natural NPC despawn probe could not resolve its entity root.");
      }
      npcDespawnProbeReference = npcDespawnProbeEntityReference;
    }
    object? npcSlotProbe = npcSlotProbeEnabled
      ? RunNpcSlotProbe(
        runtimeNpcs,
        content,
        session.World.Descriptor.WorldId,
        new Vector2(
          session.World.Descriptor.SpawnTileX * 16f,
          session.World.Descriptor.SpawnTileY * 16f - 40f))
      : null;
    object? npcRelationProbe = npcRelationProbeEnabled
      ? RunNpcRelationProbe(
        runtimeNpcs,
        content,
        session.World.Descriptor.WorldId,
        new Vector2(
          session.World.Descriptor.SpawnTileX * 16f,
          session.World.Descriptor.SpawnTileY * 16f - 40f))
      : null;
    object? npcRelationChainProbe = npcRelationChainProbeEnabled
      ? RunNpcRelationChainProbe(
        runtimeNpcs,
        content,
        session.World.Descriptor.WorldId,
        new Vector2(
          session.World.Descriptor.SpawnTileX * 16f,
          session.World.Descriptor.SpawnTileY * 16f - 40f))
      : null;
    object? npcTaskLifecycleProbe = npcTaskLifecycleProbeEnabled
      ? NpcTaskLifecycleProbe.Run(
        runtimeNpcs, session, content,
        new Vector2(session.World.Descriptor.SpawnTileX * 16f,
                    session.World.Descriptor.SpawnTileY * 16f - 40f))
      : null;
    int? npcNightProbeSlot = null;
    if (npcNightProbeEnabled)
    {
      if (!content.Npcs.TryGetByNetId(
            SimulationContentSupportManifest.GuideNetId,
            out NpcDefinition? guideDefinition) ||
          guideDefinition is null)
      {
        throw new InvalidOperationException(
          "The NPC night probe could not resolve the Guide definition.");
      }

      TownRoomTilePoint homeTile =
        FindGuideHousingRevalidationProbeTile(session, guideDefinition);
      int guideWidth = Math.Max(
        1,
        (int)(guideDefinition.Movement.Width * guideDefinition.Movement.Scale));
      int guideHeight = Math.Max(
        1,
        (int)(guideDefinition.Movement.Height * guideDefinition.Movement.Scale));
      float maximumX = Math.Max(0f, session.Storage.TileMap.Width * 16f - guideWidth);
      Vector2 probePosition = new(
        Math.Clamp(homeTile.X * 16f + 640f, 0f, maximumX),
        Math.Max(0f, homeTile.Y * 16f - guideHeight - 0.1f));
      if (!runtimeNpcs.TrySpawn(
            SimulationContentSupportManifest.GuideNetId,
            probePosition,
            content,
            session.World.Descriptor.WorldId,
            out RuntimeNpcEntity? nightProbeNpc) ||
          nightProbeNpc is null)
      {
        throw new InvalidOperationException("The NPC night probe could not spawn its Guide.");
      }

      nightProbeNpc.CommitHousingRelation(
        isHomeless: false,
        homelessDespawn: false,
        homeTile);
      npcNightProbeSlot = nightProbeNpc.Slot.Value;
    }
    int? npcHomeReturnProbeSlot = null;
    Vector2? npcHomeReturnProbeInitialPosition = null;
    if (npcHomeReturnProbe is not null)
    {
      if (!content.Npcs.TryGetByNetId(
            SimulationContentSupportManifest.GuideNetId,
            out NpcDefinition? guideDefinition) ||
          guideDefinition is null)
      {
        throw new InvalidOperationException(
          "The NPC home return probe could not resolve the Guide definition.");
      }

      TownRoomTilePoint homeTile = npcHomeReturnProbe switch
      {
        "success" or "blocked" =>
          FindGuideHousingRevalidationProbeTile(session, guideDefinition),
        _ => new TownRoomTilePoint(0, 0),
      };
      if (npcHomeReturnProbe == "blocked")
      {
        BlockGuideHomeReturnCandidates(session, homeTile);
      }
      int guideWidth = Math.Max(
        1,
        (int)(guideDefinition.Movement.Width * guideDefinition.Movement.Scale));
      int guideHeight = Math.Max(
        1,
        (int)(guideDefinition.Movement.Height * guideDefinition.Movement.Scale));
      float maximumX = Math.Max(0f, session.Storage.TileMap.Width * 16f - guideWidth);
      Vector2 probePosition = new(
        Math.Clamp(homeTile.X * 16f + 640f, 0f, maximumX),
        Math.Max(0f, homeTile.Y * 16f - guideHeight - 0.1f));
      if (!runtimeNpcs.TrySpawn(
            SimulationContentSupportManifest.GuideNetId,
            probePosition,
            content,
            session.World.Descriptor.WorldId,
            out RuntimeNpcEntity? homeReturnProbeNpc) ||
          homeReturnProbeNpc is null)
      {
        throw new InvalidOperationException(
          "The NPC home return probe could not spawn its Guide.");
      }

      homeReturnProbeNpc.CommitHousingRelation(
        isHomeless: false,
        homelessDespawn: false,
        homeTile);
      npcHomeReturnProbeSlot = homeReturnProbeNpc.Slot.Value;
      npcHomeReturnProbeInitialPosition = homeReturnProbeNpc.Movement.Position;
    }
    int? npcHousingRevalidationProbeSlot = null;
    if (npcHousingRevalidationProbe is not null)
    {
      if (!content.Npcs.TryGetByNetId(
            SimulationContentSupportManifest.GuideNetId,
            out NpcDefinition? guideDefinition) ||
          guideDefinition is null)
      {
        throw new InvalidOperationException(
          "The NPC housing revalidation probe could not resolve the Guide definition.");
      }

      TownRoomTilePoint homeTile = npcHousingRevalidationProbe == "valid"
        ? FindGuideHousingRevalidationProbeTile(session, guideDefinition)
        : new TownRoomTilePoint(0, 0);
      int guideWidth = Math.Max(
        1,
        (int)(guideDefinition.Movement.Width * guideDefinition.Movement.Scale));
      int guideHeight = Math.Max(
        1,
        (int)(guideDefinition.Movement.Height * guideDefinition.Movement.Scale));
      float maximumX = Math.Max(0f, session.Storage.TileMap.Width * 16f - guideWidth);
      Vector2 probePosition = new(
        Math.Clamp(homeTile.X * 16f + 640f, 0f, maximumX),
        Math.Max(0f, homeTile.Y * 16f - guideHeight - 0.1f));
      foreach (RuntimeNpcEntity existingGuide in runtimeNpcs.CreateActiveSnapshot()
                 .Where(npc => npc.Definition.TypeId == guideDefinition.TypeId))
      {
        if (!runtimeNpcs.TryRelease(existingGuide))
        {
          throw new InvalidOperationException(
            "The NPC housing revalidation probe could not isolate the Guide resident key.");
        }
      }

      TownHousingResidentKey resident = new(guideDefinition.TypeId);
      _ = TownHousingRegistrySystem.RemoveResident(session.TownHousing, resident);
      if (!runtimeNpcs.TrySpawn(
            SimulationContentSupportManifest.GuideNetId,
            probePosition,
            content,
            session.World.Descriptor.WorldId,
            out RuntimeNpcEntity? housingProbeNpc) ||
          housingProbeNpc is null)
      {
        throw new InvalidOperationException(
          "The NPC housing revalidation probe could not spawn its Guide.");
      }

      housingProbeNpc.CommitHousingRelation(
        isHomeless: false,
        homelessDespawn: false,
        homeTile);
      if (npcHousingRevalidationProbe == "valid")
      {
        TownHousingRegistrySystem.MarkHomeless(session.TownHousing, resident);
      }
      else
      {
        TownHousingRegistrySystem.AssignRoom(
          session.TownHousing,
          resident,
          new TilePosition(homeTile.X, homeTile.Y));
      }
      npcHousingRevalidationProbeSlot = housingProbeNpc.Slot.Value;
    }
    var initialRuntimeNpcStates = runtimeNpcs.CreateActiveSnapshot()
      .Select(static npc =>
      {
        NpcAiStateComponent ai = npc.CaptureAiState();
        return new
        {
          Slot = npc.Slot.Value,
          NetId = npc.Definition.NetId,
          Action = npc.BehaviorAction,
          Ai0 = ai.State0,
          Ai1 = ai.State1,
          Ai2 = ai.State2,
          Ai3 = ai.State3,
          X = npc.Movement.Position.X,
          Y = npc.Movement.Position.Y,
          npc.IsNaturallySpawned,
        };
      })
      .ToArray();
    var runtimeProjectiles = new RuntimeProjectileStore(
      session,
      content,
      runtimeNpcs,
      runtimePlayers,
      runtimeWorldItems,
      projectileStaticNpcImmunity);
    var playerTickPhase = new ActivePlayerTickPhase(
      runtimePlayers,
      runtimeNpcs,
      runtimeProjectiles);
    var npcTickPhase = new ActiveNpcTickPhase(
      runtimeNpcs,
      runtimePlayers,
      naturalNpcSpawns);
    var tileEntityTickPhase = new ActiveTileEntityTickPhase(
      session,
      runtimeNpcs,
      runtimePlayers,
      content);
    var liquidTickPhase = new ActiveLiquidTickPhase(session, liquidProbeEnabled);
    var kernel = new WorldSimulationKernel(
      session,
      new IWorldSimulationTickPhase[]
      {
        playerTickPhase,
        npcTickPhase,
        new ActiveProjectileTickPhase(runtimeProjectiles),
        new ActiveWorldItemsTickPhase(runtimeWorldItems, runtimePlayers),
        tileEntityTickPhase,
        liquidTickPhase,
      },
      worldTimeRate,
      new ActiveRuntimeProjection(),
      isSessionCurrent: static candidate => ReferenceEquals(
        WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
        candidate));
    WorldSimulationPhase[] expectedPhaseOrder =
    {
      WorldSimulationPhase.CommandDrain,
      WorldSimulationPhase.WorldClock,
      WorldSimulationPhase.SnapshotCommit,
      WorldSimulationPhase.RuntimeProjection,
      WorldSimulationPhase.Player,
      WorldSimulationPhase.Npc,
      WorldSimulationPhase.Projectile,
      WorldSimulationPhase.WorldItems,
      WorldSimulationPhase.TileEntities,
      WorldSimulationPhase.WorldSystems,
      WorldSimulationPhase.TickCommit,
    };
    double initialTime = session.World.TimeWeather.Time;
    bool initialDayTime = session.World.TimeWeather.DayTime;
    Terraria.WorldSession.Components.WorldGameMode initialGameMode =
      session.World.Rules.GameMode;
    bool initialExpertMode = initialGameMode is
      Terraria.WorldSession.Components.WorldGameMode.Expert or
      Terraria.WorldSession.Components.WorldGameMode.Master;
    WorldSaveSnapshotCoordinator? snapshotCoordinator = string.IsNullOrWhiteSpace(savePath)
      ? null
      : CreateSnapshotCoordinator(session, runtimeNpcs, ioGate);
    var backupPolicy = new WorldBackupPolicy(backupsToKeep: 2);
    bool saveCommitted = false;
    string? savedWorldHash = null;
    string? saveFailure = null;
    bool sourceArchiveReplacedBySave = false;
    int autosaveCount = 0;
    long? lastAutosaveTick = null;
    long? lastSavedTick = null;
    long? npcEyeServantLastUpdatedTickAfterFirstTick = null;
    for (int index = 0; index < tickCount; index++)
    {
      kernel.EnqueueCommand(_ => RuntimeMain.DrainQueuedMainThreadActions(
        () => ReferenceEquals(
          WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
          session)));
      WorldSimulationStepResult step = kernel.Step(cancellationToken);
      if (!step.TickCommitted || step.Status == WorldSimulationKernelStatus.Stopped)
      {
        break;
      }

      if (step.TickNumber != index + 1 ||
          !step.PhasesExecuted.SequenceEqual(expectedPhaseOrder))
      {
        throw new InvalidOperationException(
          $"Tick {index + 1} violated the simulation phase or tick commit contract.");
      }

      if (npcEyeTransformProbe == "next-tick" && step.TickNumber == 1)
      {
        RuntimeNpcEntity? spawnedServant = runtimeNpcs.CreateActiveSnapshot()
          .FirstOrDefault(npc =>
            npc.Definition.NetId == SimulationContentSupportManifest.ServantOfCthulhuNetId);
        npcEyeServantLastUpdatedTickAfterFirstTick = spawnedServant?.LastBehaviorUpdatedTick;
      }

      if (spatialEntityProbeEnabled && step.TickNumber == 1)
      {
        RuntimeSpatialEntityVerification.AssertLiveParity(
          session.EntityRuntime,
          runtimeNpcs.CreateActiveSnapshot(),
          runtimePlayers.Players,
          SpatialProbeRequiredNpcNetIds,
          SimulationContentSupportManifest.BlueSlimeNetId,
          localPlayerCount);
      }

      if (tileEntityProbe is TileEntityRemovalProbe removalProbe && step.TickNumber == 1)
      {
        if (!session.Storage.TileEntities.TryGetSnapshot(
              removalProbe.TrainingDummyId,
              out TileEntitySnapshot? trainingDummyEntity) ||
            trainingDummyEntity is null ||
            trainingDummyEntity.NpcIndex < 0 ||
            !runtimeNpcs.TryCaptureTrainingDummyBinding(
              trainingDummyEntity.NpcIndex,
              removalProbe.TrainingDummyAnchor,
              out RuntimeNpcTrainingDummyBinding trainingDummyBinding))
        {
          throw new InvalidOperationException(
            "The TrainingDummy TileEntity did not capture its spawned NPC binding.");
        }

        trainingDummyTileEntityBinding = trainingDummyBinding;
        TileEntityRemovalProbeInstaller.InvalidateAnchor(session, removalProbe);
        TileEntityRemovalProbeInstaller.InvalidateTrainingDummyAnchor(session, removalProbe);
      }

      if (autosaveIntervalTicks > 0 && step.TickNumber % autosaveIntervalTicks == 0)
      {
        if (!ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession, session))
        {
          throw new InvalidOperationException(
            "The active world changed before an autosave; the previous session was not saved.");
        }

        if (!EnsureSourceWorldUnchanged(worldPath, originalWorldHash, sourceArchiveReplacedBySave))
        {
          throw new InvalidOperationException(
            "The simulation modified its source world archive before autosave.");
        }

        string? previousSaveHash = GetFileSha256IfExists(savePath);
        WorldSaveProjection autosave = snapshotCoordinator!.Save(
          savePath,
          backupPolicy,
          CancellationToken.None);
        if (!autosave.Committed)
        {
          saveFailure = $"{autosave.Failure.Kind}: {autosave.Failure.Detail}";
          WriteSaveFailureReport(
            reportPath,
            worldPath,
            savePath,
            session,
            kernel,
            autosave,
            previousSaveHash);
          throw new InvalidOperationException(
            $"The runtime world autosave could not be saved: " +
            $"{autosave.Failure.Kind} {autosave.Failure.Detail}");
        }

        saveCommitted = true;
        lastSavedTick = step.TickNumber;
        autosaveCount++;
        lastAutosaveTick = step.TickNumber;
        sourceArchiveReplacedBySave |= PathsReferToSameFile(savePath, worldPath);
      }

      if (cancelAfterTicks > 0 && step.TickNumber >= cancelAfterTicks)
      {
        cancellationSource.Cancel();
        break;
      }
    }

    if (spatialEntityProbeEnabled)
    {
      foreach (RuntimePlayerEntity player in runtimePlayers.Players)
      {
        RuntimeSpatialEntityVerification.AssertJumpAndLandingObserved(player);
      }
    }

    object? npcEyeTransformationProbeResult = npcEyeTransformationProbeSetup is null
      ? null
      : VerifyNpcEyeTransformationProbe(
        npcEyeTransformationProbeSetup,
        runtimeNpcs,
        kernel.TickNumber,
        npcEyeServantLastUpdatedTickAfterFirstTick);

    bool simulationCanceled = cancellationToken.IsCancellationRequested;
    if (!ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession, session))
    {
      throw new InvalidOperationException(
        "The active world changed during this finite run; the previous session was not saved.");
    }

    if (kernel.Status != WorldSimulationKernelStatus.Stopped)
    {
      kernel.RequestStop();
      _ = kernel.Step();
    }

    bool npcPhaseValidationApplicable = kernel.TickNumber > 0;
    bool? runtimeNpcsUpdatedAtFinalTick = npcPhaseValidationApplicable
      ? WereNpcPhaseCandidatesUpdatedAtTick(
          runtimeNpcs,
          npcTickPhase.NpcReferencesAtUpdateStart,
          npcTickPhase.NpcUpdateTickNumberAtUpdateStart,
          kernel.TickNumber)
      : null;
    if (runtimeNpcsUpdatedAtFinalTick == false)
    {
      throw new InvalidOperationException(
        $"An NPC eligible for the final NPC phase missed tick {kernel.TickNumber}.");
    }

    bool sourceFileUnchanged = EnsureSourceWorldUnchanged(
      worldPath,
      originalWorldHash,
      sourceArchiveReplacedBySave);
    if (!sourceFileUnchanged)
    {
      throw new InvalidOperationException(
        "The simulation modified its source world archive before save.");
    }

    if (snapshotCoordinator is not null && lastSavedTick != kernel.TickNumber)
    {
      string? previousSaveHash = GetFileSha256IfExists(savePath);
      WorldSaveProjection save = snapshotCoordinator.Save(
        savePath,
        backupPolicy,
        CancellationToken.None);
      saveCommitted = save.Committed;
      saveFailure = save.Committed
        ? null
        : $"{save.Failure.Kind}: {save.Failure.Detail}";
      if (!saveCommitted)
      {
        WriteSaveFailureReport(
          reportPath,
          worldPath,
          savePath,
          session,
          kernel,
          save,
          previousSaveHash);
        throw new InvalidOperationException(
            $"The runtime world snapshot could not be saved: " +
            $"{save.Failure.Kind} {save.Failure.Detail}");
      }

      lastSavedTick = kernel.TickNumber;
      sourceArchiveReplacedBySave |= PathsReferToSameFile(savePath, worldPath);
    }
    if (saveCommitted)
    {
      savedWorldHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(savePath)));
    }

    if (!string.IsNullOrWhiteSpace(runtimeWorldLoadRollbackPath))
    {
      RuntimePlayerEntity rollbackPlayer = runtimePlayers.Players.Single();
      if (!runtimeProjectiles.TrySpawnArrow(
            projectileType: 1,
            ownerSlot: rollbackPlayer.Slot,
            center: rollbackPlayer.Movement.Position,
            velocity: new Vector2(3.0f, 0.0f),
            damage: 1,
            knockback: 0.0f))
      {
        throw new InvalidOperationException(
          "The runtime world-load rollback probe could not spawn its owned Projectile.");
      }

      previousNpcRollbackEvidence = CaptureRuntimeNpcRollbackEvidence(session, runtimeNpcs);
      RuntimeWorldLoadRollbackReport rollbackReport =
        RuntimeWorldLoadRollbackVerification.Run(
          candidateWorldPath: runtimeWorldLoadRollbackPath,
          loadWorld: LoadRollbackCandidate,
          activeSession: () => WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
          sessions: () => loadedSessions.ToArray(),
          loadGateReleased: () =>
            !NSSLC.WorldGeneration.WorldGen.isGeneratingOrLoadingWorld,
          storeForSession: loadedSession =>
            runtimeNpcStoresBySession.TryGetValue(
              loadedSession,
              out RuntimeNpcStore? store)
                ? store
                : throw new InvalidOperationException(
                  "The loaded session has no mapped RuntimeNpcStore."),
          selectedNpcStore: () => runtimeNpcs,
          runtimeForSession: loadedSession => entityRuntimesBySession[loadedSession],
          candidateNpcEvidenceForSession: loadedSession =>
            failedCandidateNpcEvidenceBySession.TryGetValue(
              loadedSession,
              out RuntimeNpcCandidateRollbackEvidence? evidence)
                ? evidence
                : throw new InvalidOperationException(
                  "The failed candidate NPC evidence was not captured before cleanup."),
          staticNpcImmunityForSession: loadedSession =>
            projectileStaticNpcImmunitiesBySession.TryGetValue(
              loadedSession,
              out ProjectileStaticNpcImmunityRegistryComponent? immunityRegistry)
                ? immunityRegistry
                : throw new InvalidOperationException(
                  "The loaded session has no mapped static NPC immunity registry."),
          lateFinalizeFailureObserved: () => lateFinalizeFailureObserved,
          armLateFinalizeFailure: () => lateFinalizeFailureArmed = true,
          runtimePlayers,
          runtimeItems,
          runtimeProjectiles);
      WriteReport(rollbackReport, reportPath);
      return;
    }

    string? finalLiquidStateChecksum = liquidChecksumEnabled
      ? CalculateLiquidStateChecksum(session.Storage.TileMap)
      : null;
    object? npcResetPreflightProbe = npcResetPreflightProbeEnabled
      ? RunNpcResetPreflightProbe(
        session,
        runtimeNpcs,
        runtimePlayers,
        runtimeItems,
        runtimeProjectiles,
        content)
      : null;
    object? playerCleanupPreflightProbe = playerCleanupPreflightProbeEnabled
      ? RunPlayerCleanupPreflightProbe(
        session,
        runtimeNpcs,
        runtimePlayers,
        runtimeItems,
        content)
      : null;

    object report = new
    {
      Succeeded = !simulationCanceled,
      Canceled = simulationCanceled,
      Mode = "HeadlessFiniteSimulation",
      WorldPath = worldPath,
      session.World.Descriptor.WorldId,
      session.World.Descriptor.SizeX,
      session.World.Descriptor.SizeY,
      LoadStatus = loadResult.TerminalAction,
      LoadAttempts = loadResult.LastLoadOutcome?.Attempts,
      ContentRevision = content.CatalogRevision,
      ContentSource = content.Snapshot.SourceKey,
      ContentSupportManifest = SimulationContentSupportManifest.Version,
      RandomSeed = randomSeed,
      InputScriptPath = inputScript?.SourcePath,
      LocalPlayerCount = runtimePlayers.ActiveCount,
      runtimePlayers.ShotsFired,
      runtimePlayers.ArrowAmmoRemaining,
      PlayerPositions = runtimePlayers.Players.Select(player => new
      {
        player.Slot,
        X = player.Movement.Position.X,
        Y = player.Movement.Position.Y,
      }),
      InitialPlayerPositions = initialPlayerPositions.Select((position, slot) => new
      {
        Slot = slot,
        X = position.X,
        Y = position.Y,
      }),
      InitialPlayerRootReferences = initialPlayerReferences.Select((reference, slot) => new
      {
        Slot = slot,
        Reference = reference,
      }),
      PlayerStates = runtimePlayers.Players.Select(player => new
      {
        player.Slot,
        RootReference = player.Reference,
        player.JumpCount,
        player.LandingCount,
        Life = player.Vitals.Life,
        player.Vitals.EffectiveLifeMaximum,
        Lifecycle = player.Lifecycle.Phase.ToString(),
        player.Lifecycle.RespawnRemainingTicks,
        player.PveDeathCount,
        ArrowAmmo = player.Inventory.CountItem(40),
        Gel = player.Inventory.CountItem(23),
        player.InventorySlotsUsed,
      }),
      PlayerRootProbe = playerRootProbe,
      NpcResetPreflightProbe = npcResetPreflightProbe,
      PlayerCleanupPreflightProbe = playerCleanupPreflightProbe,
      RuntimeNpcStates = runtimeNpcs.CreateActiveSnapshot().Select(npc =>
      {
        RuntimeNpcEntity.NpcMovementTickSnapshot movementTick =
          npc.CaptureMovementTickSnapshot();
        RuntimeNpcEntity.NpcDirectionSnapshot direction = npc.CaptureDirection();
        RuntimeNpcEntity.NpcImmediateEffectSnapshot immediateEffects =
          npc.CaptureImmediateEffects();
        RuntimeNpcEntity.NpcTaskSnapshot task = npc.CaptureTaskSnapshot();
        NpcAiStateComponent ai = npc.CaptureAiState();
        return new
        {
          Slot = npc.Slot.Value,
          NetId = npc.Definition.NetId,
          Action = npc.BehaviorAction,
          Ai0 = ai.State0,
          Ai1 = ai.State1,
          Ai2 = ai.State2,
          Ai3 = ai.State3,
          LocalAi0 = ai.LocalAi0,
          LocalAi1 = ai.LocalAi1,
          LocalAi2 = ai.LocalAi2,
          LocalAi3 = ai.LocalAi3,
          npc.CurrentLife,
          npc.MaximumLife,
          EyeOfCthulhuVelocityX = npc.Definition.NetId ==
            SimulationContentSupportManifest.EyeOfCthulhuNetId
              ? npc.Movement.Velocity.X
              : (float?)null,
          EyeOfCthulhuVelocityY = npc.Definition.NetId ==
            SimulationContentSupportManifest.EyeOfCthulhuNetId
              ? npc.Movement.Velocity.Y
              : (float?)null,
          EyeOfCthulhuTargetSlot = npc.Definition.NetId ==
            SimulationContentSupportManifest.EyeOfCthulhuNetId
              ? npc.CaptureTargetSlot()
              : (int?)null,
          ServantOfCthulhuVelocityX = npc.Definition.NetId ==
            SimulationContentSupportManifest.ServantOfCthulhuNetId
              ? npc.Movement.Velocity.X
              : (float?)null,
          ServantOfCthulhuVelocityY = npc.Definition.NetId ==
            SimulationContentSupportManifest.ServantOfCthulhuNetId
              ? npc.Movement.Velocity.Y
              : (float?)null,
          ServantOfCthulhuTargetSlot = npc.Definition.NetId ==
            SimulationContentSupportManifest.ServantOfCthulhuNetId
              ? npc.CaptureTargetSlot()
              : (int?)null,
          npc.LastBehaviorUpdatedTick,
          X = npc.Movement.Position.X,
          Y = npc.Movement.Position.Y,
          OldX = movementTick.OldPosition.X,
          OldY = movementTick.OldPosition.Y,
          OldVelocityX = movementTick.OldVelocity.X,
          OldVelocityY = movementTick.OldVelocity.Y,
          movementTick.CollideX,
          movementTick.CollideY,
          movementTick.Wet,
          movementTick.NoGravity,
          movementTick.NoTileCollide,
          Direction = direction.Sprite,
          DirectionY = direction.Vertical,
          immediateEffects.DespawnEncouragementTicks,
          immediateEffects.DustCount,
          immediateEffects.NetworkUpdateRequested,
          immediateEffects.JumpRequested,
          immediateEffects.JumpVelocityY,
          immediateEffects.DoorOpenRequested,
          immediateEffects.DoorTileX,
          immediateEffects.DoorTileY,
          immediateEffects.DoorDirection,
          immediateEffects.HomeTeleportRequested,
          immediateEffects.HomeTeleportSucceeded,
          immediateEffects.HomeTeleportFailed,
          immediateEffects.HomeTeleportCandidateOffset,
          HomeTeleportFailureReason = immediateEffects.HomeTeleportFailureReason.ToString(),
          HomeTeleportX = immediateEffects.HomeTeleportPosition.X,
          HomeTeleportY = immediateEffects.HomeTeleportPosition.Y,
          immediateEffects.HousingRevalidationFailed,
          immediateEffects.HousingRegistrySynchronized,
          DustX = immediateEffects.LastDustPosition.X,
          DustY = immediateEffects.LastDustPosition.Y,
          EyeOfCthulhuEffectTrace = npc.CaptureEyeOfCthulhuEffectTrace(),
          Task = task.Kind.ToString(),
          TaskPhase = task.Phase.ToString(),
          TaskCursor = task.Cursor,
          TaskFailureReason = task.FailureReason.ToString(),
          npc.IsNaturallySpawned,
        };
      }),
      PersistedNpcStates = runtimeNpcs.CreatePersistenceSnapshot().Select(static npc => new
      {
        npc.NetId,
        npc.LegacyTypeName,
        npc.IsTownNpc,
        npc.Name,
        npc.X,
        npc.Y,
        npc.Homeless,
        HomeX = npc.Home.X,
        HomeY = npc.Home.Y,
        npc.Variation,
        npc.HomelessDespawn,
      }),
      InitialRuntimeNpcStates = initialRuntimeNpcStates,
      session.IsPublished,
      RecoveryPhase = session.Lifecycle.RecoveryPhase.ToString(),
      TickLimit = tickCount,
      StoppedEarly = kernel.Status == WorldSimulationKernelStatus.Stopped &&
        kernel.TickNumber < tickCount,
      kernel.TickNumber,
      PhaseOrder = expectedPhaseOrder,
      WorldTimeRate = worldTimeRate,
      InitialTime = initialTime,
      InitialDayTime = initialDayTime,
      NpcEyeWorldInput = npcEyeScenario is null
        ? null
        : new
        {
          Scenario = npcEyeScenario,
          GameMode = initialGameMode.ToString(),
          ExpertMode = initialExpertMode,
          DayTime = initialDayTime,
          SecretSeeds = session.World.Rules.SecretSeeds.ToString(),
        },
      NpcEyeTransformationProbe = npcEyeTransformationProbeResult,
      FinalTime = session.World.TimeWeather.Time,
      FinalDayTime = session.World.TimeWeather.DayTime,
      MoonPhase = session.World.TimeWeather.MoonPhase,
      ClockRevision = session.World.TimeWeather.ClockRevision,
      WorldEvents = new
      {
        session.World.TimeWeather.BloodMoon,
        session.World.TimeWeather.Eclipse,
        session.World.TimeWeather.PumpkinMoon,
        session.World.TimeWeather.SnowMoon,
        session.World.TimeWeather.Raining,
        session.World.TimeWeather.RainTime,
        session.World.TimeWeather.SlimeRain,
        session.World.TimeWeather.SlimeRainTime,
        SandstormActive = session.World.TimeWeather.Sandstorm.Happening,
        SandstormTimeLeft = session.World.TimeWeather.Sandstorm.TimeLeft,
        InvasionType = session.World.Progression.Invasion.Type.ToString(),
        InvasionDelay = session.World.Progression.Invasion.Delay,
        InvasionWarningTimer = session.World.Progression.Invasion.WarningTimer,
        InvasionSize = session.World.Progression.Invasion.Size,
        InvasionProgress = session.World.Progression.Invasion.Progress,
        Dd2Ongoing = session.World.Progression.Dd2.Ongoing,
        Dd2SpawnDelay = session.World.Progression.Dd2.TimeLeftUntilSpawningBegins,
        session.World.Progression.Lunar.LunarApocalypseIsUp,
        MoonLordCountdown = session.World.Progression.Lunar.MoonLordCountdown,
      },
      RuntimeNpcCount = runtimeNpcs.ActiveCount,
      SpatialEntityProbeEnabled = spatialEntityProbeEnabled,
      runtimeNpcs.DespawnedCount,
      PressurePlateActivations = playerTickPhase.PressurePlateActivationCount,
      PressurePlateProbeEnabled = pressurePlateProbeEnabled,
      ChestItemProbe = chestItemProbeAnchor is TileCoordinate chestAnchor
        ? CreateChestItemProbeReport(
          session.Storage.WorldContainers,
          chestAnchor,
          ChestItemProbeSlot)
        : null,
      WorldItemProbe = worldItemProbe,
      WorldItemProbeInitialY = worldItemProbeInitialPosition?.Y,
      NpcSlotProbe = npcSlotProbe,
      NpcRelationProbe = npcRelationProbe,
      NpcRelationChainProbe = npcRelationChainProbe,
      NpcTaskLifecycleProbe = npcTaskLifecycleProbe,
      NpcNightProbe = npcNightProbeSlot is int nightProbeSlot &&
        runtimeNpcs.TryGetAt(nightProbeSlot, out RuntimeNpcEntity? nightProbeEntity) &&
        nightProbeEntity is not null
          ? new
          {
            Slot = nightProbeSlot,
            InitialDayTime = initialDayTime,
            FinalDayTime = session.World.TimeWeather.DayTime,
            Task = nightProbeEntity.CaptureTaskSnapshot().Kind.ToString(),
            TaskPhase = nightProbeEntity.CaptureTaskSnapshot().Phase.ToString(),
            TaskCursor = nightProbeEntity.CaptureTaskSnapshot().Cursor,
          }
          : null,
      NpcHomeReturnProbe = npcHomeReturnProbeSlot is int homeReturnProbeSlot &&
        runtimeNpcs.TryGetAt(homeReturnProbeSlot, out RuntimeNpcEntity? homeReturnProbeEntity) &&
        homeReturnProbeEntity is not null
          ? CreateNpcHomeReturnProbeReport(
            npcHomeReturnProbe!,
            homeReturnProbeEntity,
            npcHomeReturnProbeInitialPosition.GetValueOrDefault())
          : null,
      NpcHousingRevalidationProbe = npcHousingRevalidationProbeSlot is int housingProbeSlot &&
        runtimeNpcs.TryGetAt(housingProbeSlot, out RuntimeNpcEntity? housingProbeEntity) &&
        housingProbeEntity is not null
          ? CreateNpcHousingRevalidationProbeReport(
            npcHousingRevalidationProbe!,
            housingProbeEntity,
            session)
          : null,
      NpcDespawnProbeEnabled = npcDespawnProbeEnabled,
      NpcDespawnProbeNetId = actualNpcDespawnProbeNetId,
      NpcDespawnProbePolicy = actualNpcDespawnProbeNetId is int probeNetId &&
        SimulationContentSupportManifest.TryGetNpcNaturalDespawnPolicy(
          probeNetId,
          out SimulationContentSupportManifest.NpcNaturalDespawnPolicy despawnPolicy)
            ? despawnPolicy.ToString()
            : null,
      NpcDespawnProbeSlot = npcDespawnProbeSlot,
      NpcDespawnProbeRemoved = npcDespawnProbeReference is EntityReference despawnProbeReference &&
        !runtimeNpcs.TryResolveEntityReference(despawnProbeReference, out _),
      TileEntityRemovalProbeEnabled = tileEntityProbe is not null,
      TileEntityProbeRemoved = tileEntityProbe is TileEntityRemovalProbe tileEntityProbeValue &&
        !session.Storage.TileEntities.TryGetSnapshot(tileEntityProbeValue.Id, out _),
      TrainingDummyTileEntityBound = trainingDummyTileEntityBinding.HasValue,
      TrainingDummyTileEntityRemoved = tileEntityProbe is TileEntityRemovalProbe trainingDummyRemovalProbe &&
        !session.Storage.TileEntities.TryGetSnapshot(trainingDummyRemovalProbe.TrainingDummyId, out _),
      TrainingDummyNpcReleased = trainingDummyTileEntityBinding is
        RuntimeNpcTrainingDummyBinding releasedTrainingDummyBinding &&
        !runtimeNpcs.TryGetAt(releasedTrainingDummyBinding.Slot.Value, out _),
      TrainingDummyActiveNpcCountRestored = tileEntityProbe is TileEntityRemovalProbe completedTrainingDummyProbe &&
        runtimeNpcs.ActiveCount == completedTrainingDummyProbe.InitialNpcCount,
      TileEntityProbeScheduled = tileEntityProbe is TileEntityRemovalProbe scheduledProbe &&
        session.Storage.TileEntityUpdates.IsScheduled(scheduledProbe.Id),
      TileEntityUpdatePassCount = tileEntityTickPhase.UpdatePassCount,
      RecognizedUnsupportedWiredDeviceTileTypes =
        playerTickPhase.RecognizedUnsupportedWiredDeviceTileTypes
        .Concat(tileEntityTickPhase.RecognizedUnsupportedWiredDeviceTileTypes)
        .Distinct()
        .OrderBy(static tileType => tileType)
        .ToArray(),
      PressurePlateProbeAnchor = pressurePlateProbeAnchor is TileCoordinate plateAnchor
        ? new { plateAnchor.X, plateAnchor.Y }
        : null,
      WiredActuatorToggleCount = playerTickPhase.ActuatorToggleCount,
      PressurePlateAnchors = session.Storage.PressurePlates.Anchors
        .Select(static anchor => new { anchor.X, anchor.Y })
        .ToArray(),
      LiquidTicks = liquidTickPhase.TickCount,
      LiquidCommittedTileMutationCount = liquidTickPhase.CommittedTileMutationCount,
      LiquidStateChangeCount = liquidTickPhase.LiquidStateChangeCount,
      LiquidLastTickMutationCount = liquidTickPhase.LastTickMutationCount,
      LiquidProbeCoordinate = liquidTickPhase.ProbeCoordinate is TileCoordinate probe
        ? new { probe.X, probe.Y }
        : null,
      InitialLiquidStateChecksum = initialLiquidStateChecksum,
      FinalLiquidStateChecksum = finalLiquidStateChecksum,
      TileAreaSample = CreateTileAreaSample(
        session.Storage.TileMap,
        liquidTickPhase.ProbeCoordinate ?? inspectTileAreaCoordinate),
      SpawnedNpcTypes = npcTypesToSpawn,
      NaturalSpawnedNpcTypes = naturalNpcSpawns.CreatedNetIds,
      RuntimeProjectileCount = runtimeProjectiles.ActiveCount,
      runtimeProjectiles.AcceptedNpcHitCount,
      ProjectileTileCollisionCount = runtimeProjectiles.TileCollisionCount,
      RuntimeWorldItemCount = runtimeWorldItems.ActiveCount,
      WorldItemStates = runtimeWorldItems.CreateSnapshot(),
      runtimeWorldItems.NpcDeathDropCount,
      runtimeWorldItems.PickupCount,
      runtimeWorldItems.PartialPickupCount,
      runtimeWorldItems.LastPickupDistanceToPlayerHitbox,
      runtimeWorldItems.ExpiredCount,
      LastTickProjectileUpdateCount = runtimeProjectiles.LastTickResult.UpdateStepCount,
      NpcPhaseUpdateTickNumberAtUpdateStart = npcTickPhase.NpcUpdateTickNumberAtUpdateStart,
      NpcPhaseEligibleNpcCountAtUpdateStart = npcTickPhase.NpcReferencesAtUpdateStart.Count,
      NpcPhaseValidationApplicable = npcPhaseValidationApplicable,
      RuntimeNpcsUpdatedAtFinalTick = runtimeNpcsUpdatedAtFinalTick,
      SourceFileUnchanged = sourceFileUnchanged,
      SourceSha256 = originalWorldHash,
      SavePath = string.IsNullOrWhiteSpace(savePath) ? null : savePath,
      AutosaveIntervalTicks = autosaveIntervalTicks == 0 ? (int?)null : autosaveIntervalTicks,
      AutosaveCount = autosaveCount,
      LastAutosaveTick = lastAutosaveTick,
      SaveCommitted = saveCommitted,
      SaveFailure = saveFailure,
      SavedWorldSha256 = savedWorldHash,
      SnapshotRevision = kernel.CurrentSnapshot?.Revision,
      SnapshotReady = kernel.CurrentSnapshot?.Readiness?.CanUpdateEntities,
      KernelStatus = kernel.Status.ToString(),
    };
    WriteReport(report, reportPath);
    if (simulationCanceled)
    {
      throw new OperationCanceledException(cancellationToken);
    }

    if (switchWorldPaths.Count > 0)
    {
      var switchReports = new List<object>(switchWorldPaths.Count);
      var playerRootSwitchReports = new List<object>(switchWorldPaths.Count);
      LoadedWorldSession previousSession = session;
      foreach (string switchWorldPath in switchWorldPaths)
      {
        var previousPlayerRoots = runtimePlayers.Players.Select(player =>
        {
          if (!player.Inventory.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot bow))
          {
            throw new InvalidOperationException(
              "The Player world-switch probe could not resolve the prior inventory owner.");
          }

          return (
            player.Slot,
            Reference: player.Reference,
            InventoryOwner: player.Inventory,
            Bow: bow.Entity);
        }).ToArray();
        runtimePlayers.Clear();
        bool oldPlayerReferencesRejected = previousPlayerRoots.All(root =>
          !runtimePlayers.TryResolveEntityReference(root.Reference, out _));
        bool oldInventoryOwnersRejected = previousPlayerRoots.All(root =>
          !root.InventoryOwner.TryGetItem(root.Bow, out _));
        if (!oldPlayerReferencesRejected || !oldInventoryOwnersRejected)
        {
          throw new InvalidOperationException(
            "The world switch retained a Player root reference or inventory owner.");
        }

        (LoadedWorldSession switchedSession, object switchReport) = RunWorldSwitchProbe(
          switchWorldPath,
          previousSession,
          runtimeNpcs,
          localPlayerCount,
          worldTimeRate,
          randomSeed,
          content,
          inputScript,
          npcTypesToSpawn,
          tileEntityReloadProbeEnabled,
          (candidate, candidateImmunityRegistry) =>
          {
            previousNpcRollbackEvidence = CaptureRuntimeNpcRollbackEvidence(
              previousSession,
              runtimeNpcs);
            runtimeNpcs = candidate;
            projectileStaticNpcImmunity = candidateImmunityRegistry;
          },
          targetPath =>
          {
            loadResult = null;
            loadException = null;
            RuntimeMain.InitializeHeadlessRuntime();
            RuntimeMain.dedServ = true;
            RuntimeMain.netMode = 0;
            RuntimeMain.gameMenu = false;
            RuntimeMain.worldPathName = targetPath;
            RuntimeMain.rand = new UnifiedRandom(randomSeed);
            new WorldFileData(Path.GetFileNameWithoutExtension(targetPath)).SetAsActive();
            WorldGen.serverLoadWorldCallBack();

            if (loadException is not null)
            {
              WriteLoadFailureReport(
                string.IsNullOrWhiteSpace(reportPath)
                  ? string.Empty
                  : reportPath + ".switch-load-failure.json",
                targetPath,
                tickLimit: 0,
                localPlayerCount,
                randomSeed,
                loadResult,
                loadException,
                canceled: cancellationToken.IsCancellationRequested);
              throw new InvalidOperationException(
                "The world switch load handler failed.",
                loadException);
            }

            if (loadResult?.Succeeded != true)
            {
              WriteLoadFailureReport(
                string.IsNullOrWhiteSpace(reportPath)
                  ? string.Empty
                  : reportPath + ".switch-load-failure.json",
                targetPath,
                tickLimit: 0,
                localPlayerCount,
                randomSeed,
                loadResult,
                loadException: null,
                canceled: cancellationToken.IsCancellationRequested ||
                  loadResult?.Failure.Kind == WorldStorageFailureKind.Canceled);
              throw new InvalidOperationException(
                $"The world switch did not publish its candidate: " +
                $"{loadResult?.Failure.Kind.ToString() ?? "missing result"}: " +
                $"{loadResult?.Failure.Detail ?? "no recovery result was returned"}");
            }
          },
          cancellationToken);
        runtimeItems = new RuntimeItemRegistry(content, switchedSession.EntityRuntime);
        runtimePlayers.Initialize(
          switchedSession,
          localPlayerCount,
          content,
          runtimeItems,
          inputScript,
          runtimeNpcs.IdentityRegistry);
        var replacementPlayerReferences = runtimePlayers.Players.ToDictionary(
          static player => player.Slot,
          static player => player.Reference);
        bool replacementPlayerReferencesChanged = previousPlayerRoots.All(root =>
          replacementPlayerReferences.TryGetValue(root.Slot, out EntityReference newReference) &&
          newReference != root.Reference);
        bool replacementPlayerReferencesResolve = runtimePlayers.Players.All(player =>
          runtimePlayers.TryResolveEntityReference(player.Reference, out RuntimePlayerEntity? resolved) &&
          resolved?.RuntimeHandle == player.RuntimeHandle);
        if (!replacementPlayerReferencesChanged || !replacementPlayerReferencesResolve)
        {
          throw new InvalidOperationException(
            "The world switch did not create fresh, resolvable Player roots.");
        }

        switchReports.Add(switchReport);
        playerRootSwitchReports.Add(new
        {
          PreviousPlayerCount = previousPlayerRoots.Length,
          OldPlayerReferencesRejected = oldPlayerReferencesRejected,
          OldInventoryOwnersRejected = oldInventoryOwnersRejected,
          ReplacementPlayerReferencesChanged = replacementPlayerReferencesChanged,
          ReplacementPlayerReferencesResolve = replacementPlayerReferencesResolve,
        });
        previousSession = switchedSession;
      }

      WriteReport(new
      {
        Succeeded = true,
        Mode = "HeadlessWorldSwitchVerification",
        Switches = switchReports,
        PlayerRootLifecycles = playerRootSwitchReports,
      }, string.IsNullOrWhiteSpace(reportPath) ? string.Empty : reportPath + ".switch.json");
    }
  }

  private static bool WereNpcPhaseCandidatesUpdatedAtTick(
    RuntimeNpcStore npcs,
    IReadOnlyList<EntityReference> candidateReferences,
    long candidateTickNumber,
    long tickNumber)
  {
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(candidateReferences);
    if (tickNumber <= 0 || candidateTickNumber != tickNumber)
    {
      return false;
    }

    foreach (RuntimeNpcEntity npc in npcs.CreateActiveSnapshot())
    {
      if (!npc.IsActive)
      {
        continue;
      }

      if (!npcs.TryGetEntityReference(npc.InstanceId, out EntityReference currentReference))
      {
        return false;
      }

      bool wasEligibleAtPhaseStart = false;
      for (int index = 0; index < candidateReferences.Count; index++)
      {
        if (candidateReferences[index] == currentReference)
        {
          wasEligibleAtPhaseStart = true;
          break;
        }
      }

      if (wasEligibleAtPhaseStart && npc.LastBehaviorUpdatedTick != tickNumber)
      {
        return false;
      }
    }

    return true;
  }

  private static object RunNpcResetPreflightProbe(
    LoadedWorldSession session,
    RuntimeNpcStore npcs,
    RuntimePlayerStore players,
    RuntimeItemRegistry items,
    RuntimeProjectileStore projectiles,
    ContentCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(projectiles);
    ArgumentNullException.ThrowIfNull(catalog);
    if (players.ActiveCount != 1 || npcs.ActiveCount == 0 ||
        !ReferenceEquals(session.EntityRuntime, items.EntityRuntime))
    {
      throw new InvalidOperationException(
        "The NPC reset preflight probe requires NPCs, one Player, and one shared runtime.");
    }

    EntityRuntime runtime = session.EntityRuntime;
    EntityRuntimeId runtimeIdBeforeReset = runtime.RuntimeId;
    RuntimeNpcEntity[] previousNpcs = npcs.CreateActiveSnapshot().ToArray();
    var previousNpcState = previousNpcs.Select(npc =>
      (npc.Definition.NetId, npc.Movement.Position)).ToArray();
    RuntimeNpcEntity borrowedNpc = previousNpcs[^1];
    if (!npcs.TryGetEntityReference(borrowedNpc.InstanceId, out EntityReference borrowedNpcReference))
    {
      throw new InvalidOperationException("The NPC reset probe could not capture an NPC reference.");
    }

    RuntimePlayerEntity player = players.Players[0];
    EntityReference playerReference = player.Reference;
    RuntimeEntityHandle playerHandle = player.RuntimeHandle;
    if (!player.Inventory.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot bowSnapshot))
    {
      throw new InvalidOperationException("The NPC reset probe could not capture the Player item root.");
    }

    ItemEntityRef itemReference = bowSnapshot.Entity;
    RuntimeEntityHandle[] projectileHandlesBefore = runtime.Match<ProjectileIdentityComponent>();
    if (!projectiles.TrySpawnArrow(
          projectileType: 1,
          ownerSlot: player.Slot,
          center: player.Movement.Position,
          velocity: new Vector2(3.0f, 0.0f),
          damage: 1,
          knockback: 0.0f))
    {
      throw new InvalidOperationException("The NPC reset probe could not create a Projectile root.");
    }

    var projectileHandlesBeforeSet = new HashSet<RuntimeEntityHandle>(projectileHandlesBefore);
    RuntimeEntityHandle[] newProjectileHandles = runtime.Match<ProjectileIdentityComponent>()
      .Where(handle => !projectileHandlesBeforeSet.Contains(handle))
      .ToArray();
    if (newProjectileHandles.Length != 1 ||
        !runtime.TryGetReference(
          newProjectileHandles[0],
          EntityReferenceScope.Projectile,
          out EntityReference projectileReference) ||
        !runtime.TryCapture<ProjectileIdentityComponent, EntityReference>(
          newProjectileHandles[0],
          static identity => identity.OwnerReference,
          out EntityReference projectileOwnerReference) ||
        projectileOwnerReference != playerReference)
    {
      throw new InvalidOperationException(
        "The NPC reset probe could not capture the Projectile root and its Player owner.");
    }

    bool resetBorrowCallbackRan = false;
    bool resetRejectedBorrow = false;
    try
    {
      bool inspected = runtime.TryInspect<NpcEntityIdentityComponent>(
        borrowedNpc.RuntimeHandle,
        (in NpcEntityIdentityComponent identity) =>
        {
          resetBorrowCallbackRan = identity.InstanceId.IsValid;
          npcs.Reset();
        });
      if (!inspected)
      {
        throw new InvalidOperationException("The NPC reset probe could not borrow its NPC root.");
      }
    }
    catch (InvalidOperationException)
    {
      resetRejectedBorrow = true;
    }

    if (!resetBorrowCallbackRan || !resetRejectedBorrow ||
        npcs.ActiveCount != previousNpcs.Length ||
        runtime.RuntimeId != runtimeIdBeforeReset ||
        !npcs.TryResolveEntityReference(borrowedNpcReference, out RuntimeEntityHandle resolvedNpc) ||
        resolvedNpc != borrowedNpc.RuntimeHandle ||
        !runtime.TryGetStatus(borrowedNpc.RuntimeHandle, out EntityRuntimeStatus npcStatus) ||
        npcStatus != EntityRuntimeStatus.Running ||
        !players.TryResolveEntityReference(playerReference, out RuntimePlayerEntity? resolvedPlayer) ||
        resolvedPlayer?.RuntimeHandle != playerHandle ||
        !runtime.TryResolve(itemReference.Reference, out RuntimeEntityHandle resolvedItem) ||
        !items.TryGet(itemReference, out _) ||
        !runtime.TryResolve(projectileReference, out RuntimeEntityHandle resolvedProjectile) ||
        resolvedProjectile != newProjectileHandles[0])
    {
      throw new InvalidOperationException(
        "A borrowed NPC reset must reject before changing any NPC or shared-world roots.");
    }

    npcs.Reset();
    if (npcs.ActiveCount != 0 ||
        npcs.TryResolveEntityReference(borrowedNpcReference, out _) ||
        runtime.TryGetStatus(borrowedNpc.RuntimeHandle, out _) ||
        runtime.RuntimeId != runtimeIdBeforeReset ||
        !players.TryResolveEntityReference(playerReference, out resolvedPlayer) ||
        resolvedPlayer?.RuntimeHandle != playerHandle ||
        !runtime.TryResolve(itemReference.Reference, out resolvedItem) ||
        !items.TryGet(itemReference, out _) ||
        !runtime.TryResolve(projectileReference, out resolvedProjectile) ||
        resolvedProjectile != newProjectileHandles[0])
    {
      throw new InvalidOperationException(
        "A released NPC borrow must allow reset without invalidating other world roots.");
    }

    RuntimeNpcEntity?[] replacements = new RuntimeNpcEntity?[previousNpcState.Length];
    for (int index = 0; index < previousNpcState.Length; index++)
    {
      (int netId, Vector2 position) = previousNpcState[index];
      if (!npcs.TrySpawn(
            netId,
            position,
            catalog,
            session.World.Descriptor.WorldId,
            out RuntimeNpcEntity? replacement) ||
          replacement is null)
      {
        throw new InvalidOperationException(
          "The NPC reset probe could not restore the previous NPC count after reset.");
      }

      replacements[index] = replacement;
    }

    RuntimeNpcEntity? reusedRuntimeSlot = replacements
      .FirstOrDefault(npc => npc?.RuntimeHandle.LocalIndex == borrowedNpc.RuntimeHandle.LocalIndex);
    if (reusedRuntimeSlot is null ||
        reusedRuntimeSlot.RuntimeHandle.Generation == borrowedNpc.RuntimeHandle.Generation ||
        runtime.TryGetStatus(borrowedNpc.RuntimeHandle, out _) ||
        runtime.TryResolve(borrowedNpcReference, out _) ||
        runtime.RuntimeId != runtimeIdBeforeReset ||
        npcs.ActiveCount != previousNpcs.Length ||
        !players.TryResolveEntityReference(playerReference, out resolvedPlayer) ||
        resolvedPlayer?.RuntimeHandle != playerHandle ||
        !runtime.TryResolve(itemReference.Reference, out resolvedItem) ||
        !items.TryGet(itemReference, out _) ||
        !runtime.TryResolve(projectileReference, out resolvedProjectile) ||
        resolvedProjectile != newProjectileHandles[0])
    {
      throw new InvalidOperationException(
        "Reset must preserve the world runtime and non-NPC roots while rejecting stale generations.");
    }

    return new
    {
      BorrowedResetRejectedWithoutWrites = true,
      ReleasedBorrowAllowedRetry = true,
      NpcRootsRestoredAfterReset = npcs.ActiveCount,
      PlayerReferencePreserved = playerReference,
      ItemReferencePreserved = itemReference,
      ProjectileReferencePreserved = projectileReference,
      WorldRuntimeIdPreserved = runtimeIdBeforeReset,
      PreviousNpcGeneration = borrowedNpc.RuntimeHandle.Generation,
      ReusedNpcLocalIndex = reusedRuntimeSlot.RuntimeHandle.LocalIndex,
      ReusedNpcGeneration = reusedRuntimeSlot.RuntimeHandle.Generation,
      StaleNpcReferenceRejected = true,
    };
  }

  private static object RunPlayerCleanupPreflightProbe(
    LoadedWorldSession session,
    RuntimeNpcStore npcs,
    RuntimePlayerStore players,
    RuntimeItemRegistry items,
    ContentCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(catalog);
    if (players.ActiveCount != 2 || npcs.ActiveCount == 0 ||
        !ReferenceEquals(session.EntityRuntime, items.EntityRuntime))
    {
      throw new InvalidOperationException(
        "The Player cleanup probe requires NPCs, two Players, and one shared runtime.");
    }

    EntityRuntime runtime = session.EntityRuntime;
    EntityRuntimeId runtimeId = runtime.RuntimeId;
    RuntimePlayerEntity firstPlayer = players.Players[0];
    RuntimePlayerEntity secondPlayer = players.Players[1];
    EntityReference firstPlayerReference = firstPlayer.Reference;
    RuntimeEntityHandle firstPlayerHandle = firstPlayer.RuntimeHandle;
    EntityReference secondPlayerReference = secondPlayer.Reference;
    RuntimeEntityHandle secondPlayerHandle = secondPlayer.RuntimeHandle;
    RuntimeNpcEntity npc = npcs.CreateActiveSnapshot()[0];
    if (!npcs.TryGetEntityReference(npc.InstanceId, out EntityReference npcReference) ||
        !firstPlayer.Inventory.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot firstBow) ||
        !firstPlayer.Inventory.TryGetItemAtSlot(54, out PlayerInventoryItemSnapshot firstArrows) ||
        !secondPlayer.Inventory.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot secondBow))
    {
      throw new InvalidOperationException(
        "The Player cleanup probe could not capture its Player, NPC, and Item references.");
    }

    ItemEntityRef[] firstItems = { firstBow.Entity, firstArrows.Entity };
    int entityCount = runtime.EntityCount;

    void AssertCleanupStatePreserved(string operation)
    {
      if (players.ActiveCount != 2 ||
          firstPlayer.Reference != firstPlayerReference ||
          secondPlayer.Reference != secondPlayerReference ||
          runtime.EntityCount != entityCount ||
          runtime.RuntimeId != runtimeId ||
          !players.TryResolveEntityReference(firstPlayerReference, out RuntimePlayerEntity? resolvedFirst) ||
          resolvedFirst?.RuntimeHandle != firstPlayerHandle ||
          !players.TryResolveEntityReference(secondPlayerReference, out RuntimePlayerEntity? resolvedSecond) ||
          resolvedSecond?.RuntimeHandle != secondPlayerHandle ||
          !npcs.TryResolveEntityReference(npcReference, out RuntimeEntityHandle resolvedNpc) ||
          resolvedNpc != npc.RuntimeHandle)
      {
        throw new InvalidOperationException(
          $"Rejected Player {operation} changed a Player or NPC root.");
      }

      foreach (ItemEntityRef itemReference in firstItems.Append(secondBow.Entity))
      {
        if (!runtime.TryResolve(itemReference.Reference, out _) ||
            !items.TryGet(itemReference, out _))
        {
          throw new InvalidOperationException(
            $"Rejected Player {operation} changed an inventory item root.");
        }
      }

      if (!firstPlayer.Inventory.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot currentBow) ||
          currentBow.Entity != firstBow.Entity ||
          !firstPlayer.Inventory.TryGetItemAtSlot(54, out PlayerInventoryItemSnapshot currentArrows) ||
          currentArrows.Entity != firstArrows.Entity)
      {
        throw new InvalidOperationException(
          $"Rejected Player {operation} changed inventory slots.");
      }
    }

    bool initializeBorrowEntered = false;
    bool initializeRejected = false;
    if (!runtime.TryResolve(firstArrows.Entity.Reference, out RuntimeEntityHandle arrowItemHandle) ||
        !runtime.TryInspect<ItemStackComponent>(
          arrowItemHandle,
          (in ItemStackComponent stack) =>
          {
            initializeBorrowEntered = stack.Quantity > 0;
            try
            {
              players.Initialize(
                session,
                playerCount: 2,
                catalog,
                items,
                inputScript: null,
                identityRegistry: session.IdentityRegistry);
            }
            catch (InvalidOperationException)
            {
              initializeRejected = true;
            }
          }) ||
        !initializeBorrowEntered || !initializeRejected)
    {
      throw new InvalidOperationException(
        "Player initialization must reject a borrowed inventory item before replacing roots.");
    }
    AssertCleanupStatePreserved("Initialize");

    bool clearBorrowEntered = false;
    bool clearRejected = false;
    if (!runtime.TryInspect<Terraria.Player.PlayerInventorySlotsComponent>(
          firstPlayerHandle,
          (in Terraria.Player.PlayerInventorySlotsComponent slots) =>
          {
            clearBorrowEntered = slots.MainInventorySlots[0] == firstBow.Entity;
            try
            {
              players.Clear();
            }
            catch (InvalidOperationException)
            {
              clearRejected = true;
            }
          }) ||
        !clearBorrowEntered || !clearRejected)
    {
      throw new InvalidOperationException(
        "Player Clear must reject a borrowed Player root before releasing inventory.");
    }
    AssertCleanupStatePreserved("Clear");

    bool disposeBorrowEntered = false;
    bool disposeRejected = false;
    if (!runtime.TryInspect<Terraria.Player.PlayerInventorySlotsComponent>(
          secondPlayerHandle,
          (in Terraria.Player.PlayerInventorySlotsComponent slots) =>
          {
            disposeBorrowEntered = slots.MainInventorySlots[0] == secondBow.Entity;
            try
            {
              players.Dispose();
            }
            catch (InvalidOperationException)
            {
              disposeRejected = true;
            }
          }) ||
        !disposeBorrowEntered || !disposeRejected)
    {
      throw new InvalidOperationException(
        "Player Dispose must reject a borrowed Player root without marking the store disposed.");
    }
    AssertCleanupStatePreserved("Dispose");

    bool destroyRootBorrowEntered = false;
    bool destroyDuringRootBorrow = true;
    if (!runtime.TryInspect<Terraria.Player.PlayerInventorySlotsComponent>(
          firstPlayerHandle,
          (in Terraria.Player.PlayerInventorySlotsComponent slots) =>
          {
            destroyRootBorrowEntered = slots.MainInventorySlots[0] == firstBow.Entity;
            destroyDuringRootBorrow = players.TryDestroyPlayer(firstPlayer.Slot);
          }) ||
        !destroyRootBorrowEntered || destroyDuringRootBorrow)
    {
      throw new InvalidOperationException(
        "TryDestroyPlayer must refuse a borrowed Player root without changing its inventory.");
    }
    AssertCleanupStatePreserved("TryDestroyPlayer root preflight");

    bool destroyItemBorrowEntered = false;
    bool destroyDuringItemBorrow = true;
    if (!runtime.TryInspect<ItemStackComponent>(
          arrowItemHandle,
          (in ItemStackComponent stack) =>
          {
            destroyItemBorrowEntered = stack.Quantity > 0;
            destroyDuringItemBorrow = players.TryDestroyPlayer(firstPlayer.Slot);
          }) ||
        !destroyItemBorrowEntered || destroyDuringItemBorrow)
    {
      throw new InvalidOperationException(
        "TryDestroyPlayer must refuse a borrowed inventory item before clearing any slot.");
    }
    AssertCleanupStatePreserved("TryDestroyPlayer inventory preflight");

    if (!players.TryDestroyPlayer(firstPlayer.Slot) ||
        players.ActiveCount != 1 ||
        players.TryResolveEntityReference(firstPlayerReference, out _) ||
        npcs.TryResolveEntityReference(npcReference, out RuntimeEntityHandle retainedNpc) == false ||
        retainedNpc != npc.RuntimeHandle ||
        firstItems.Any(itemReference =>
          runtime.TryResolve(itemReference.Reference, out _) || items.TryGet(itemReference, out _)) ||
        !players.TryResolveEntityReference(secondPlayerReference, out RuntimePlayerEntity? retainedSecond) ||
        retainedSecond?.RuntimeHandle != secondPlayerHandle)
    {
      throw new InvalidOperationException(
        "Released Player cleanup must remove only its own root and inventory items.");
    }

    if (!players.TryCreatePlayerAtSlot(firstPlayer.Slot) ||
        !players.TryGetPlayerAtSlot(firstPlayer.Slot, out RuntimePlayerEntity? replacementPlayer) ||
        replacementPlayer is null ||
        replacementPlayer.RuntimeHandle.LocalIndex != firstPlayerHandle.LocalIndex ||
        replacementPlayer.RuntimeHandle.Generation == firstPlayerHandle.Generation ||
        players.TryResolveEntityReference(firstPlayerReference, out _) ||
        !players.TryResolveEntityReference(secondPlayerReference, out retainedSecond) ||
        retainedSecond?.RuntimeHandle != secondPlayerHandle ||
        !npcs.TryResolveEntityReference(npcReference, out retainedNpc) ||
        retainedNpc != npc.RuntimeHandle ||
        runtime.RuntimeId != runtimeId)
    {
      throw new InvalidOperationException(
        "Recreated Player roots must advance generation while preserving other domain roots.");
    }

    return new
    {
      InitializeRejectedBorrowedItemWithoutWrites = true,
      ClearRejectedBorrowedPlayerWithoutWrites = true,
      DisposeRejectedBorrowedPlayerWithoutDisposingStore = true,
      TryDestroyRejectedBorrowedPlayerAndItem = true,
      ReleasedBorrowAllowedDestroyRetry = true,
      PlayerRootRecreatedWithNewGeneration = true,
      NpcRootPreserved = true,
      WorldRuntimeIdPreserved = runtimeId,
      PreviousPlayerGeneration = firstPlayerHandle.Generation,
      ReplacementPlayerGeneration = replacementPlayer.RuntimeHandle.Generation,
    };
  }

  private static object RunPlayerRootLifecycleProbe(RuntimePlayerStore players)
  {
    ArgumentNullException.ThrowIfNull(players);
    if (players.ActiveCount != 1)
    {
      throw new InvalidOperationException("The Player root probe requires one active player.");
    }

    RuntimePlayerEntity originalPlayer = players.Players[0];
    EntityReference oldReference = originalPlayer.Reference;
    RuntimePlayerInventoryOwner oldInventoryOwner = originalPlayer.Inventory;
    if (!oldInventoryOwner.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot originalBow))
    {
      throw new InvalidOperationException("The Player root probe could not resolve the original bow.");
    }

    int slot = originalPlayer.Slot;
    RuntimeEntityHandle oldHandle = originalPlayer.RuntimeHandle;
    if (!players.TryDestroyPlayer(slot))
    {
      throw new InvalidOperationException("The Player root probe could not terminate its player.");
    }

    bool oldReferenceRejected =
      !players.TryResolveEntityReference(oldReference, out _);
    bool oldInventoryOwnerRejected =
      !oldInventoryOwner.TryGetItem(originalBow.Entity, out _);
    if (!players.TryCreatePlayerAtSlot(slot))
    {
      throw new InvalidOperationException("The Player root probe could not recreate its player.");
    }

    RuntimePlayerEntity replacementPlayer = players.Players.Single();
    EntityReference newReference = replacementPlayer.Reference;
    bool newReferenceResolved =
      players.TryResolveEntityReference(newReference, out RuntimePlayerEntity? resolvedPlayer) &&
      resolvedPlayer?.RuntimeHandle == replacementPlayer.RuntimeHandle;
    bool handleChanged = oldHandle != replacementPlayer.RuntimeHandle;
    bool rootReferenceChanged = oldReference != newReference;
    int freshArrowCount = replacementPlayer.Inventory.CountItem(40);
    if (!oldReferenceRejected ||
        !oldInventoryOwnerRejected ||
        !newReferenceResolved ||
        !handleChanged ||
        !rootReferenceChanged ||
        freshArrowCount != 99)
    {
      throw new InvalidOperationException("The Player root recreation probe failed its identity checks.");
    }

    return new
    {
      Slot = slot,
      OldReference = oldReference,
      NewReference = newReference,
      OldReferenceRejected = oldReferenceRejected,
      OldInventoryOwnerRejected = oldInventoryOwnerRejected,
      NewReferenceResolved = newReferenceResolved,
      HandleChanged = handleChanged,
      RootReferenceChanged = rootReferenceChanged,
      FreshArrowCount = freshArrowCount,
    };
  }

  private static object RunNpcRelationProbe(
    RuntimeNpcStore npcs,
    ContentCatalog content,
    int worldId,
    Vector2 position)
  {
    int initialActiveCount = npcs.ActiveCount;
    RuntimeNpcEntity? parent = null;
    RuntimeNpcEntity? child = null;
    RuntimeNpcEntity? replacementParent = null;
    RuntimeNpcEntity? lethalParent = null;
    RuntimeNpcEntity? lethalChild = null;
    var capacityFillers = new List<RuntimeNpcEntity>();
    RuntimeNpcStore.NpcParentRelationBinding binding = default;
    RuntimeNpcStore.NpcParentRelationBinding lethalBinding = default;
    bool attached = false;
    bool parentHitApplied = false;
    bool parentLifeOwnerMatched = false;
    bool parentLifeChanged = false;
    bool childMirroredParentLife = false;
    bool parentHitWasNonlethal = false;
    bool parentReleased = false;
    bool relationDetachedAfterParentRelease = false;
    bool parentSlotReused = false;
    bool replacementHasFreshIdentity = false;
    bool childHitAfterParentReleaseAppliedLocally = false;
    bool replacementUnaffectedByChildHit = false;
    bool childReleased = false;
    bool replacementReleased = false;
    bool lethalParentHitApplied = false;
    bool lethalParentDropOwnerMatched = false;
    bool lethalParentReleasedWithChild = false;
    bool relationReferenceMatched = false;
    bool partialPairRejectedAtCapacity = false;
    bool capacityCountRestoredAfterRejection = false;
    bool capacityFillersReleased = false;
    try
    {
      if (!npcs.TrySpawnParentChild(
            SimulationContentSupportManifest.ZombieNetId,
            SimulationContentSupportManifest.BlueSlimeNetId,
            position,
            attachedAtTick: 0,
            content,
            worldId,
            out binding))
      {
        throw new InvalidOperationException(
          "The NPC relation probe could not create its parent and child pair.");
      }

      if (!npcs.TryGetAt(binding.ParentSlot, out parent) ||
          parent is null ||
          !npcs.TryGetAt(binding.ChildSlot, out child) ||
          child is null)
      {
        throw new InvalidOperationException(
          "The NPC relation probe could not resolve its generated pair.");
      }

      attached = child.TryCaptureParentRelation(
          out RuntimeNpcEntity.NpcParentRelationSnapshot relation) &&
        relation.ParentInstanceId == binding.ParentInstanceId &&
        relation.ParentLegacySlot == parent.Slot &&
        relation.AttachedAtTick == binding.AttachedAtTick;
      relationReferenceMatched = child.TryCaptureParentRelation(
          out RuntimeNpcEntity.NpcParentRelationSnapshot relationWithReference) &&
        relationWithReference.ParentReference == binding.ParentReference &&
        relationWithReference.ParentReference.Scope == EntityReferenceScope.Npc;
      attached &= relationReferenceMatched;
      if (!attached)
      {
        throw new InvalidOperationException(
          "The generated child did not expose the expected parent relation.");
      }

      int parentLifeBeforeHit = parent.CurrentLife;
      uint parentGenerationBeforeRelease = parent.SlotGeneration;
      npcs.AdvanceDamageTrackingTo(tickNumber: 0);
      RuntimeNpcProjectileTargetSnapshot childTarget = npcs
        .CreateProjectileTargetSnapshot()
        .Single(target => target.Reference == binding.ChildReference);
      if (!npcs.TryApplyProjectileHit(
            childTarget,
            damage: 10,
            ownerSlot: 0,
            tickNumber: 0,
            knockback: 0f,
            hitDirection: 1,
            out NpcStrikeResult parentHit,
            out RuntimeNpcStore.NpcDeathDropSnapshot deathDrop))
      {
        throw new InvalidOperationException(
          "The parent relation probe could not resolve its child projectile target.");
      }

      parentHitApplied = parentHit.CombatResult.Applied;
      parentLifeOwnerMatched = parentHit.CombatResult.LifeOwnerInstanceId ==
        binding.ParentInstanceId;
      parentLifeChanged = parent.CurrentLife < parentLifeBeforeHit;
      childMirroredParentLife = child.CurrentLife == parent.CurrentLife &&
        child.MaximumLife == parent.MaximumLife;
      parentHitWasNonlethal = !parentHit.CombatResult.DeathTransitioned &&
        deathDrop == default;
      if (!parentHitApplied || !parentLifeOwnerMatched || !parentLifeChanged ||
          !childMirroredParentLife || !parentHitWasNonlethal)
      {
        throw new InvalidOperationException(
          "A projectile hit on a child did not update and mirror the resolved parent's life.");
      }

      parentReleased = npcs.TryRelease(parent);
      relationDetachedAfterParentRelease =
        !child.TryCaptureParentRelation(out _);
      bool replacementSpawned = npcs.TrySpawn(
        SimulationContentSupportManifest.ZombieNetId,
        position,
        content,
        worldId,
        out replacementParent) && replacementParent is not null;
      if (replacementSpawned)
      {
        parentSlotReused = replacementParent!.Slot.Value == binding.ParentSlot &&
          replacementParent.SlotGeneration > parentGenerationBeforeRelease;
        replacementHasFreshIdentity =
          replacementParent.InstanceId != binding.ParentInstanceId;
      }

      if (!replacementSpawned || !parentReleased || !relationDetachedAfterParentRelease ||
          !parentSlotReused || !replacementHasFreshIdentity)
      {
        throw new InvalidOperationException(
          "Releasing a parent did not detach its child before safe same-slot parent reuse.");
      }

      int replacementLifeBeforeChildHit = replacementParent!.CurrentLife;
      int childLifeBeforeLocalHit = child.CurrentLife;
      childTarget = npcs.CreateProjectileTargetSnapshot()
        .Single(target => target.Reference == binding.ChildReference);
      if (!npcs.TryApplyProjectileHit(
            childTarget,
            damage: 10,
            ownerSlot: 0,
            tickNumber: 0,
            knockback: 0f,
            hitDirection: 1,
            out NpcStrikeResult detachedChildHit,
            out _))
      {
        throw new InvalidOperationException(
          "The detached child projectile target could not be resolved after parent slot reuse.");
      }

      childHitAfterParentReleaseAppliedLocally = detachedChildHit.CombatResult.Applied &&
        detachedChildHit.CombatResult.LifeOwnerInstanceId is null &&
        child.CurrentLife < childLifeBeforeLocalHit;
      replacementUnaffectedByChildHit =
        replacementParent.CurrentLife == replacementLifeBeforeChildHit;
      childReleased = npcs.TryRelease(child);
      replacementReleased = npcs.TryRelease(replacementParent);
      if (!childHitAfterParentReleaseAppliedLocally || !replacementUnaffectedByChildHit ||
          !childReleased || !replacementReleased)
      {
        throw new InvalidOperationException(
          "A reused parent slot received a stale child hit or the relation probe leaked an NPC.");
      }

      if (!npcs.TrySpawnParentChild(
            SimulationContentSupportManifest.ZombieNetId,
            SimulationContentSupportManifest.BlueSlimeNetId,
            position,
            attachedAtTick: 0,
            content,
            worldId,
            out lethalBinding) ||
          !npcs.TryGetAt(lethalBinding.ParentSlot, out lethalParent) ||
          lethalParent is null ||
          !npcs.TryGetAt(lethalBinding.ChildSlot, out lethalChild) ||
          lethalChild is null)
      {
        throw new InvalidOperationException(
          "The NPC relation probe could not create its lethal parent and child pair.");
      }

      Vector2 expectedDropPosition = lethalParent.Movement.Position;
      RuntimeNpcProjectileTargetSnapshot lethalChildTarget = npcs
        .CreateProjectileTargetSnapshot()
        .Single(target => target.Reference == lethalBinding.ChildReference);
      if (!npcs.TryApplyProjectileHit(
            lethalChildTarget,
            damage: int.MaxValue,
            ownerSlot: 0,
            tickNumber: 0,
            knockback: 0f,
            hitDirection: 1,
            out NpcStrikeResult lethalParentHit,
            out RuntimeNpcStore.NpcDeathDropSnapshot lethalDeathDrop))
      {
        throw new InvalidOperationException(
          "The NPC relation probe could not resolve its lethal child projectile target.");
      }

      lethalParentHitApplied = lethalParentHit.CombatResult.Applied &&
        lethalParentHit.CombatResult.DeathTransitioned &&
        lethalParentHit.CombatResult.LifeOwnerInstanceId == lethalBinding.ParentInstanceId;
      lethalParentDropOwnerMatched =
        lethalDeathDrop.NetId == SimulationContentSupportManifest.ZombieNetId &&
        lethalDeathDrop.Position == expectedDropPosition;
      lethalParentReleasedWithChild =
        !npcs.TryGetEntityReference(lethalBinding.ParentInstanceId, out _) &&
        !npcs.TryGetEntityReference(lethalBinding.ChildInstanceId, out _) &&
        npcs.ActiveCount == initialActiveCount;
      if (!lethalParentHitApplied || !lethalParentDropOwnerMatched ||
          !lethalParentReleasedWithChild)
      {
        throw new InvalidOperationException(
          "A lethal child hit did not release the parent group and select the parent's death drop.");
      }

      int capacityProbeCount = RuntimeNpcStore.MaximumNpcCapacity - 1;
      while (npcs.ActiveCount < capacityProbeCount)
      {
        if (!npcs.TrySpawn(
              SimulationContentSupportManifest.BlueSlimeNetId,
              position,
              content,
              worldId,
              out RuntimeNpcEntity? filler) ||
            filler is null)
        {
          throw new InvalidOperationException(
            "The NPC relation capacity probe could not reserve its filler slots.");
        }

        capacityFillers.Add(filler);
      }

      partialPairRejectedAtCapacity = !npcs.TrySpawnParentChild(
        SimulationContentSupportManifest.ZombieNetId,
        SimulationContentSupportManifest.BlueSlimeNetId,
        position,
        attachedAtTick: 1,
        content,
        worldId,
        out _);
      capacityCountRestoredAfterRejection =
        npcs.ActiveCount == capacityProbeCount;
      foreach (RuntimeNpcEntity filler in capacityFillers)
      {
        if (!npcs.TryRelease(filler))
        {
          throw new InvalidOperationException(
            "The NPC relation capacity probe could not release a filler slot.");
        }
      }
      capacityFillersReleased = npcs.ActiveCount == initialActiveCount;
      if (!partialPairRejectedAtCapacity || !capacityCountRestoredAfterRejection ||
          !capacityFillersReleased)
      {
        throw new InvalidOperationException(
          "A partial parent-child spawn failure leaked an NPC at the capacity boundary.");
      }

      return new
      {
        ParentNetId = SimulationContentSupportManifest.ZombieNetId,
        ChildNetId = SimulationContentSupportManifest.BlueSlimeNetId,
        ParentSlot = binding.ParentSlot,
        ChildSlot = binding.ChildSlot,
        ParentInstanceId = binding.ParentInstanceId.Value,
        ChildInstanceId = binding.ChildInstanceId.Value,
        ParentReference = binding.ParentReference,
        ChildReference = binding.ChildReference,
        AttachedAtTick = binding.AttachedAtTick,
        Attached = attached,
        RelationReferenceMatched = relationReferenceMatched,
        ParentHitApplied = parentHitApplied,
        ParentLifeOwnerMatched = parentLifeOwnerMatched,
        ParentLifeChanged = parentLifeChanged,
        ChildMirroredParentLife = childMirroredParentLife,
        ParentHitWasNonlethal = parentHitWasNonlethal,
        ParentReleased = parentReleased,
        RelationDetachedAfterParentRelease = relationDetachedAfterParentRelease,
        ParentSlotReused = parentSlotReused,
        ReplacementHasFreshIdentity = replacementHasFreshIdentity,
        ChildHitAfterParentReleaseAppliedLocally = childHitAfterParentReleaseAppliedLocally,
        ReplacementUnaffectedByChildHit = replacementUnaffectedByChildHit,
        ChildReleased = childReleased,
        ReplacementReleased = replacementReleased,
        LethalParentHitApplied = lethalParentHitApplied,
        LethalParentDropOwnerMatched = lethalParentDropOwnerMatched,
        LethalParentReleasedWithChild = lethalParentReleasedWithChild,
        PartialPairRejectedAtCapacity = partialPairRejectedAtCapacity,
        CapacityCountRestoredAfterRejection = capacityCountRestoredAfterRejection,
        CapacityFillersReleased = capacityFillersReleased,
        CapacityProbeFillerCount = capacityFillers.Count,
        InitialActiveCount = initialActiveCount,
        FinalActiveCount = npcs.ActiveCount,
      };
    }
    finally
    {
      if (parent is not null && npcs.TryGetAt(parent.Slot.Value, out RuntimeNpcEntity? currentParent) &&
          ReferenceEquals(parent, currentParent))
      {
        _ = npcs.TryRelease(parent);
      }

      if (child is not null && npcs.TryGetAt(child.Slot.Value, out RuntimeNpcEntity? currentChild) &&
          ReferenceEquals(child, currentChild))
      {
        _ = npcs.TryRelease(child);
      }

      if (replacementParent is not null &&
          npcs.TryGetAt(replacementParent.Slot.Value, out RuntimeNpcEntity? currentReplacement) &&
          ReferenceEquals(replacementParent, currentReplacement))
      {
        _ = npcs.TryRelease(replacementParent);
      }

      if (lethalParent is not null &&
          npcs.TryGetAt(lethalParent.Slot.Value, out RuntimeNpcEntity? currentLethalParent) &&
          ReferenceEquals(lethalParent, currentLethalParent))
      {
        _ = npcs.TryRelease(lethalParent);
      }

      if (lethalChild is not null &&
          npcs.TryGetAt(lethalChild.Slot.Value, out RuntimeNpcEntity? currentLethalChild) &&
          ReferenceEquals(lethalChild, currentLethalChild))
      {
        _ = npcs.TryRelease(lethalChild);
      }

      foreach (RuntimeNpcEntity filler in capacityFillers)
      {
        if (npcs.TryGetAt(filler.Slot.Value, out RuntimeNpcEntity? currentFiller) &&
            ReferenceEquals(filler, currentFiller))
        {
          _ = npcs.TryRelease(filler);
        }
      }
    }
  }

  private static object RunNpcRelationChainProbe(
    RuntimeNpcStore npcs,
    ContentCatalog content,
    int worldId,
    Vector2 position)
  {
    int initialActiveCount = npcs.ActiveCount;
    RuntimeNpcEntity? root = null;
    RuntimeNpcEntity? middle = null;
    RuntimeNpcEntity? tail = null;
    RuntimeNpcEntity? replacementRoot = null;
    var capacityFillers = new List<RuntimeNpcEntity>();
    RuntimeNpcStore.NpcRelationChainBinding binding = default;
    bool attached = false;
    bool referencesStable = false;
    bool tailHitApplied = false;
    bool tailHitResolvedToRoot = false;
    bool rootLifeChanged = false;
    bool rootDeathReleasedChain = false;
    bool rootSlotReused = false;
    bool replacementHasFreshIdentity = false;
    bool staleTailRejectedAfterReuse = false;
    bool middleNodeReleased = false;
    bool middleReleaseDetachedTail = false;
    bool detachedTailHitAppliedLocally = false;
    bool rootUnaffectedByDetachedTailHit = false;
    bool severedChainActiveCountRestored = false;
    bool partialChainRejectedAtCapacity = false;
    bool capacityCountRestoredAfterRejection = false;
    bool capacityFillersReleased = false;
    RuntimeNpcProjectileTargetSnapshot staleTailTarget = default;
    try
    {
      var chainNetIds = new[]
      {
        SimulationContentSupportManifest.ZombieNetId,
        SimulationContentSupportManifest.BlueSlimeNetId,
        SimulationContentSupportManifest.BlueSlimeNetId,
      };
      if (!npcs.TrySpawnParentChildChain(
            chainNetIds,
            position,
            attachedAtTick: 2,
            content,
            worldId,
            out binding) ||
          binding.References.Count != chainNetIds.Length)
      {
        throw new InvalidOperationException(
          "The NPC relation chain probe could not create its three-node chain.");
      }

      if (!npcs.TryGetAt(binding.Slots[0], out root) ||
          root is null ||
          !npcs.TryGetAt(binding.Slots[1], out middle) ||
          middle is null ||
          !npcs.TryGetAt(binding.Slots[2], out tail) ||
          tail is null)
      {
        throw new InvalidOperationException(
          "The NPC relation chain probe could not resolve all generated nodes.");
      }

      attached = true;
      referencesStable = true;
      for (int index = 1; index < binding.InstanceIds.Count; index++)
      {
        RuntimeNpcEntity child = index == 1 ? middle : tail;
        if (!child.TryCaptureParentRelation(
              out RuntimeNpcEntity.NpcParentRelationSnapshot relation) ||
            relation.ParentInstanceId != binding.InstanceIds[index - 1] ||
            relation.ParentReference != binding.References[index - 1] ||
            relation.AttachedAtTick != binding.AttachedAtTick)
        {
          attached = false;
          referencesStable = false;
          break;
        }
      }

      if (!attached || !referencesStable)
      {
        throw new InvalidOperationException(
          "The generated relation chain did not expose stable adjacent parent references.");
      }

      int rootLifeBeforeHit = root.CurrentLife;
      npcs.AdvanceDamageTrackingTo(tickNumber: 0);
      RuntimeNpcProjectileTargetSnapshot tailTarget = npcs
        .CreateProjectileTargetSnapshot()
        .Single(target => target.Reference == binding.References[^1]);
      if (!npcs.TryApplyProjectileHit(
            tailTarget,
            damage: 10,
            ownerSlot: 0,
            tickNumber: 0,
            knockback: 0f,
            hitDirection: 1,
            out NpcStrikeResult tailHit,
            out _))
      {
        throw new InvalidOperationException(
          "The relation chain probe could not resolve its tail projectile target.");
      }

      tailHitApplied = tailHit.CombatResult.Applied;
      tailHitResolvedToRoot = tailHit.CombatResult.LifeOwnerInstanceId ==
        binding.InstanceIds[0];
      rootLifeChanged = root.CurrentLife < rootLifeBeforeHit;
      staleTailTarget = tailTarget;
      if (!tailHitApplied || !tailHitResolvedToRoot || !rootLifeChanged)
      {
        throw new InvalidOperationException(
          "A tail projectile hit did not resolve to the chain root life owner.");
      }

      Vector2 expectedDropPosition = root.Movement.Position;
      uint rootGenerationBeforeRelease = root.SlotGeneration;
      if (!npcs.TryApplyProjectileHit(
            tailTarget,
            damage: int.MaxValue,
            ownerSlot: 0,
            tickNumber: 0,
            knockback: 0f,
            hitDirection: 1,
            out NpcStrikeResult lethalHit,
            out RuntimeNpcStore.NpcDeathDropSnapshot deathDrop))
      {
        throw new InvalidOperationException(
          "The relation chain probe could not resolve its lethal tail projectile target.");
      }

      rootDeathReleasedChain = lethalHit.CombatResult.DeathTransitioned &&
        lethalHit.CombatResult.LifeOwnerInstanceId == binding.InstanceIds[0] &&
        deathDrop.NetId == SimulationContentSupportManifest.ZombieNetId &&
        deathDrop.Position == expectedDropPosition &&
        !npcs.TryGetEntityReference(binding.InstanceIds[0], out _) &&
        !npcs.TryGetEntityReference(binding.InstanceIds[1], out _) &&
        !npcs.TryGetEntityReference(binding.InstanceIds[2], out _) &&
        npcs.ActiveCount == initialActiveCount;
      if (!rootDeathReleasedChain)
      {
        throw new InvalidOperationException(
          "A lethal root-owned tail hit did not release the complete relation chain.");
      }

      if (!npcs.TrySpawn(
            SimulationContentSupportManifest.ZombieNetId,
            position,
            content,
            worldId,
            out replacementRoot) ||
          replacementRoot is null)
      {
        throw new InvalidOperationException(
          "The relation chain probe could not allocate a same-slot replacement root.");
      }

      rootSlotReused = replacementRoot.Slot.Value == binding.Slots[0] &&
        replacementRoot.SlotGeneration > rootGenerationBeforeRelease;
      replacementHasFreshIdentity = replacementRoot.InstanceId != binding.InstanceIds[0];
      staleTailRejectedAfterReuse = !npcs.TryApplyProjectileHit(
        staleTailTarget,
        damage: 10,
        ownerSlot: 0,
        tickNumber: 4,
        knockback: 0f,
        hitDirection: 1,
        out _,
        out _);
      if (!rootSlotReused || !replacementHasFreshIdentity || !staleTailRejectedAfterReuse)
      {
        throw new InvalidOperationException(
          "A reused root slot retained a stale relation-chain target reference.");
      }

      if (!npcs.TryRelease(replacementRoot))
      {
        throw new InvalidOperationException(
          "The relation chain probe could not release its replacement root.");
      }
      replacementRoot = null;

      root = null;
      middle = null;
      tail = null;
      if (!npcs.TrySpawnParentChildChain(
            chainNetIds,
            position,
            attachedAtTick: 4,
            content,
            worldId,
            out RuntimeNpcStore.NpcRelationChainBinding severedBinding) ||
          !npcs.TryGetAt(severedBinding.Slots[0], out root) ||
          root is null ||
          !npcs.TryGetAt(severedBinding.Slots[1], out middle) ||
          middle is null ||
          !npcs.TryGetAt(severedBinding.Slots[2], out tail) ||
          tail is null)
      {
        throw new InvalidOperationException(
          "The relation chain probe could not create a chain for middle-node release.");
      }

      int severedRootLifeBeforeTailHit = root.CurrentLife;
      int tailLifeBeforeSeveredHit = tail.CurrentLife;
      NpcInstanceId severedMiddleId = middle.InstanceId;
      middleNodeReleased = npcs.TryRelease(middle);
      if (!middleNodeReleased)
      {
        throw new InvalidOperationException(
          "The relation chain probe could not release the middle node.");
      }
      middle = null;
      middleReleaseDetachedTail = middleNodeReleased &&
        !npcs.TryGetEntityReference(severedMiddleId, out _) &&
        !tail.TryCaptureParentRelation(out _);
      npcs.AdvanceDamageTrackingTo(tickNumber: 0);
      RuntimeNpcProjectileTargetSnapshot severedTailTarget = npcs
        .CreateProjectileTargetSnapshot()
        .Single(target => target.Reference == severedBinding.References[^1]);
      if (!npcs.TryApplyProjectileHit(
            severedTailTarget,
            damage: 10,
            ownerSlot: 0,
            tickNumber: 0,
            knockback: 0f,
            hitDirection: 1,
            out NpcStrikeResult severedTailHit,
            out _))
      {
        throw new InvalidOperationException(
          "The detached tail projectile target could not be resolved after middle-node release.");
      }

      detachedTailHitAppliedLocally = severedTailHit.CombatResult.Applied &&
        severedTailHit.CombatResult.LifeOwnerInstanceId is null &&
        tail.CurrentLife < tailLifeBeforeSeveredHit;
      rootUnaffectedByDetachedTailHit = root.CurrentLife == severedRootLifeBeforeTailHit;
      bool severedRootReleased = npcs.TryRelease(root);
      bool severedTailReleased = npcs.TryRelease(tail);
      severedChainActiveCountRestored = severedRootReleased && severedTailReleased &&
        npcs.ActiveCount == initialActiveCount;
      if (!middleNodeReleased || !middleReleaseDetachedTail ||
          !detachedTailHitAppliedLocally || !rootUnaffectedByDetachedTailHit ||
          !severedChainActiveCountRestored)
      {
        throw new InvalidOperationException(
          "Releasing a middle relation node did not detach and isolate its tail child.");
      }
      root = null;
      tail = null;

      int capacityProbeCount = RuntimeNpcStore.MaximumNpcCapacity - 2;
      while (npcs.ActiveCount < capacityProbeCount)
      {
        if (!npcs.TrySpawn(
              SimulationContentSupportManifest.BlueSlimeNetId,
              position,
              content,
              worldId,
              out RuntimeNpcEntity? filler) ||
            filler is null)
        {
          throw new InvalidOperationException(
            "The relation chain capacity probe could not reserve its filler slots.");
        }

        capacityFillers.Add(filler);
      }

      partialChainRejectedAtCapacity = !npcs.TrySpawnParentChildChain(
        chainNetIds,
        position,
        attachedAtTick: 5,
        content,
        worldId,
        out _);
      capacityCountRestoredAfterRejection = npcs.ActiveCount == capacityProbeCount;
      foreach (RuntimeNpcEntity filler in capacityFillers)
      {
        if (!npcs.TryRelease(filler))
        {
          throw new InvalidOperationException(
            "The relation chain capacity probe could not release a filler slot.");
        }
      }
      capacityFillersReleased = npcs.ActiveCount == initialActiveCount;
      if (!partialChainRejectedAtCapacity || !capacityCountRestoredAfterRejection ||
          !capacityFillersReleased)
      {
        throw new InvalidOperationException(
          "A partial relation-chain spawn failure leaked one or more NPC nodes.");
      }

      return new
      {
        NetIds = chainNetIds,
        References = binding.References,
        InstanceIds = binding.InstanceIds.Select(instanceId => instanceId.Value).ToArray(),
        Slots = binding.Slots,
        AttachedAtTick = binding.AttachedAtTick,
        Attached = attached,
        ReferencesStable = referencesStable,
        TailHitApplied = tailHitApplied,
        TailHitResolvedToRoot = tailHitResolvedToRoot,
        RootLifeChanged = rootLifeChanged,
        RootDeathReleasedChain = rootDeathReleasedChain,
        RootSlotReused = rootSlotReused,
        ReplacementHasFreshIdentity = replacementHasFreshIdentity,
        StaleTailRejectedAfterReuse = staleTailRejectedAfterReuse,
        MiddleNodeReleased = middleNodeReleased,
        MiddleReleaseDetachedTail = middleReleaseDetachedTail,
        DetachedTailHitAppliedLocally = detachedTailHitAppliedLocally,
        RootUnaffectedByDetachedTailHit = rootUnaffectedByDetachedTailHit,
        SeveredChainActiveCountRestored = severedChainActiveCountRestored,
        PartialChainRejectedAtCapacity = partialChainRejectedAtCapacity,
        CapacityCountRestoredAfterRejection = capacityCountRestoredAfterRejection,
        CapacityFillersReleased = capacityFillersReleased,
        CapacityProbeFillerCount = capacityFillers.Count,
        InitialActiveCount = initialActiveCount,
        FinalActiveCount = npcs.ActiveCount,
      };
    }
    finally
    {
      if (replacementRoot is not null &&
          npcs.TryGetAt(replacementRoot.Slot.Value, out RuntimeNpcEntity? currentReplacement) &&
          ReferenceEquals(replacementRoot, currentReplacement))
      {
        _ = npcs.TryRelease(replacementRoot);
      }

      if (root is not null &&
          npcs.TryGetAt(root.Slot.Value, out RuntimeNpcEntity? currentRoot) &&
          ReferenceEquals(root, currentRoot))
      {
        _ = npcs.TryRelease(root);
      }

      if (middle is not null &&
          npcs.TryGetAt(middle.Slot.Value, out RuntimeNpcEntity? currentMiddle) &&
          ReferenceEquals(middle, currentMiddle))
      {
        _ = npcs.TryRelease(middle);
      }

      if (tail is not null &&
          npcs.TryGetAt(tail.Slot.Value, out RuntimeNpcEntity? currentTail) &&
          ReferenceEquals(tail, currentTail))
      {
        _ = npcs.TryRelease(tail);
      }

      foreach (RuntimeNpcEntity filler in capacityFillers)
      {
        if (npcs.TryGetAt(filler.Slot.Value, out RuntimeNpcEntity? currentFiller) &&
            ReferenceEquals(filler, currentFiller))
        {
          _ = npcs.TryRelease(filler);
        }
      }
    }
  }

  private static object RunNpcSlotProbe(
    RuntimeNpcStore npcs,
    ContentCatalog content,
    int worldId,
    Vector2 position)
  {
    int initialActiveCount = npcs.ActiveCount;
    if (initialActiveCount >= RuntimeNpcStore.MaximumNpcCapacity)
    {
      throw new InvalidOperationException(
        "The NPC slot probe requires at least one free slot before it starts.");
    }

    var spawned = new List<RuntimeNpcEntity>();
    int countAtCapacity = 0;
    int releasedSlot = -1;
    uint releasedGeneration = 0;
    ulong releasedInstanceId = 0;
    EntityReference releasedEntityReference = EntityReference.None;
    Guid releasedEntityUuid = Guid.Empty;
    int reusedSlot = -1;
    uint reusedGeneration = 0;
    ulong reusedInstanceId = 0;
    bool overflowSpawnRejected = false;
    bool releasedSlotWasEmpty = false;
    bool staleHandleRejectedAfterReuse = false;
    bool staleProjectileTargetRejectedAfterReuse = false;
    bool reusedNpcUnaffectedByStaleProjectileTarget = false;
    bool staleHealthComponentRejectedAfterRelease = false;
    bool staleBehaviorComponentRejectedAfterRelease = false;
    bool reusedAiStateResetAfterReuse = false;
    bool instanceIdChangedAfterReuse = false;
    bool releasedProjectionRejected = false;
    bool reusedRootChanged = false;
    bool staleRootRejectedAfterReuse = false;
    try
    {
      while (npcs.TrySpawn(
               SimulationContentSupportManifest.BlueSlimeNetId,
               position,
               content,
               worldId,
               out RuntimeNpcEntity? allocated) &&
             allocated is not null)
      {
        spawned.Add(allocated);
      }

      countAtCapacity = npcs.ActiveCount;
      overflowSpawnRejected = !npcs.TrySpawn(
        SimulationContentSupportManifest.BlueSlimeNetId,
        position,
        content,
        worldId,
        out RuntimeNpcEntity? overflowNpc);
      if (overflowNpc is not null)
      {
        throw new InvalidOperationException(
          "The NPC owner returned an entity for a rejected capacity allocation.");
      }

      if (countAtCapacity != RuntimeNpcStore.MaximumNpcCapacity ||
          !overflowSpawnRejected || spawned.Count == 0)
      {
        throw new InvalidOperationException(
          "The NPC owner did not reach capacity and reject the overflow allocation.");
      }

      RuntimeNpcEntity releasedNpc = spawned[0];
      releasedSlot = releasedNpc.Slot.Value;
      releasedGeneration = releasedNpc.SlotGeneration;
      NpcInstanceId releasedInstanceIdentity = releasedNpc.InstanceId;
      releasedInstanceId = releasedInstanceIdentity.Value;
      releasedNpc.CommitAiState(
        new NpcAiStateComponent(
          releasedNpc.Definition.Spawn.AiStyle,
          11f,
          12f,
          13f,
          14f,
          0,
          21f,
          22f,
          23f,
          24f),
        action: 3);
      if (!npcs.TryGetEntityReference(releasedInstanceIdentity, out releasedEntityReference))
      {
        throw new InvalidOperationException("The NPC projection did not resolve to its entity root.");
      }

      releasedEntityUuid = releasedEntityReference.EntityId.Value;
      RuntimeNpcProjectileTargetSnapshot releasedProjectileTarget =
        npcs.CreateProjectileTargetSnapshot().Single(target =>
          target.Reference == releasedEntityReference);
      if (!npcs.TryRelease(releasedNpc))
      {
        throw new InvalidOperationException("The NPC owner did not release the selected slot.");
      }

      try
      {
        _ = releasedNpc.CurrentLife;
      }
      catch (InvalidOperationException)
      {
        staleHealthComponentRejectedAfterRelease = true;
      }

      try
      {
        _ = releasedNpc.CaptureAiState();
      }
      catch (InvalidOperationException)
      {
        staleBehaviorComponentRejectedAfterRelease = true;
      }

      releasedSlotWasEmpty = !npcs.TryGetAt(releasedSlot, out _);
      releasedProjectionRejected =
        !npcs.TryGetEntityReference(releasedInstanceIdentity, out _);
      if (!releasedSlotWasEmpty || !releasedProjectionRejected ||
          !npcs.TrySpawn(
            SimulationContentSupportManifest.BlueSlimeNetId,
            position,
            content,
            worldId,
            out RuntimeNpcEntity? reusedNpc) ||
          reusedNpc is null)
      {
        throw new InvalidOperationException(
          "The NPC owner did not reuse its released slot.");
      }

      spawned.Add(reusedNpc);
      reusedSlot = reusedNpc.Slot.Value;
      reusedGeneration = reusedNpc.SlotGeneration;
      reusedInstanceId = reusedNpc.InstanceId.Value;
      instanceIdChangedAfterReuse =
        reusedNpc.InstanceId != releasedInstanceIdentity;
      if (!npcs.TryGetEntityReference(reusedNpc.InstanceId, out EntityReference reusedReference))
      {
        throw new InvalidOperationException("A reused NPC projection did not resolve to its new entity root.");
      }

      reusedRootChanged = reusedReference.EntityId != releasedEntityReference.EntityId;
      staleRootRejectedAfterReuse = !npcs.TryResolveEntityReference(releasedEntityReference, out _);
      staleHandleRejectedAfterReuse = !npcs.TryRelease(releasedNpc);
      NpcAiStateComponent reusedAiState = reusedNpc.CaptureAiState();
      reusedAiStateResetAfterReuse = reusedNpc.BehaviorAction == 0 &&
        reusedAiState.State0 == 0f && reusedAiState.State1 == 0f &&
        reusedAiState.State2 == 0f && reusedAiState.State3 == 0f &&
        reusedAiState.LocalAi0 == 0f && reusedAiState.LocalAi1 == 0f &&
        reusedAiState.LocalAi2 == 0f && reusedAiState.LocalAi3 == 0f;
      int reusedNpcLifeBeforeStaleTarget = reusedNpc.CurrentLife;
      staleProjectileTargetRejectedAfterReuse = !npcs.TryApplyProjectileHit(
        releasedProjectileTarget,
        damage: 1,
        ownerSlot: 0,
        tickNumber: 1,
        knockback: 0f,
        hitDirection: 0,
        out _,
        out _);
      reusedNpcUnaffectedByStaleProjectileTarget =
        reusedNpc.CurrentLife == reusedNpcLifeBeforeStaleTarget;
      if (reusedSlot != releasedSlot || reusedGeneration <= releasedGeneration ||
          !staleHandleRejectedAfterReuse || !instanceIdChangedAfterReuse ||
          !reusedRootChanged || !staleRootRejectedAfterReuse ||
          !staleProjectileTargetRejectedAfterReuse || !reusedNpcUnaffectedByStaleProjectileTarget ||
          !staleHealthComponentRejectedAfterRelease ||
          !staleBehaviorComponentRejectedAfterRelease || !reusedAiStateResetAfterReuse)
      {
        throw new InvalidOperationException(
          "NPC slot reuse did not advance the generation, issue a new identity, or reject stale handles and targets.");
      }
    }
    finally
    {
      foreach (RuntimeNpcEntity npc in spawned)
      {
        _ = npcs.TryRelease(npc);
      }
    }

    int finalActiveCount = npcs.ActiveCount;
    if (finalActiveCount != initialActiveCount)
    {
      throw new InvalidOperationException(
        "The NPC slot probe did not restore the runtime owner's original active count.");
    }

    object trainingDummyBindingProbe = RunNpcTrainingDummyBindingProbe(
      content,
      npcs.IdentityRegistry,
      worldId,
      position);
    object resetProbe = RunNpcInstanceIdentityResetProbe(
      content,
      npcs.IdentityRegistry,
      worldId,
      position);

    return new
    {
      InitialActiveCount = initialActiveCount,
      Capacity = RuntimeNpcStore.MaximumNpcCapacity,
      CountAtCapacity = countAtCapacity,
      OverflowSpawnRejected = overflowSpawnRejected,
      ReleasedSlot = releasedSlot,
      ReleasedGeneration = releasedGeneration,
      ReleasedInstanceId = releasedInstanceId,
      ReleasedEntityUuid = releasedEntityUuid,
      ReleasedSlotWasEmpty = releasedSlotWasEmpty,
      ReleasedProjectionRejected = releasedProjectionRejected,
      ReusedSlot = reusedSlot,
      ReusedGeneration = reusedGeneration,
      ReusedInstanceId = reusedInstanceId,
      InstanceIdChangedAfterReuse = instanceIdChangedAfterReuse,
      ReusedRootChanged = reusedRootChanged,
      StaleRootRejectedAfterReuse = staleRootRejectedAfterReuse,
      StaleHandleRejectedAfterReuse = staleHandleRejectedAfterReuse,
      StaleHealthComponentRejectedAfterRelease = staleHealthComponentRejectedAfterRelease,
      StaleBehaviorComponentRejectedAfterRelease = staleBehaviorComponentRejectedAfterRelease,
      StaleProjectileTargetRejectedAfterReuse = staleProjectileTargetRejectedAfterReuse,
      ReusedNpcUnaffectedByStaleProjectileTarget = reusedNpcUnaffectedByStaleProjectileTarget,
      ReusedAiStateResetAfterReuse = reusedAiStateResetAfterReuse,
      TrainingDummyBindingProbe = trainingDummyBindingProbe,
      ResetProbe = resetProbe,
      FinalActiveCount = finalActiveCount,
    };
  }

  private static object RunNpcTrainingDummyBindingProbe(
    ContentCatalog content,
    EntityIdentityRegistry identityRegistry,
    int worldId,
    Vector2 position)
  {
    var immunityRegistry = new ProjectileStaticNpcImmunityRegistryComponent(
      checked(content.Snapshot.Projectiles.MaximumTypeId + 1),
      RuntimeNpcStore.MaximumNpcCapacity);
    var trainingDummyStore = new RuntimeNpcStore(immunityRegistry, identityRegistry);
    RuntimeNpcTrainingDummyBinding binding = default;
    bool bindingCreated = false;
    bool released = false;
    RuntimeNpcEntity? replacement = null;
    try
    {
      if (!trainingDummyStore.TrySpawnTrainingDummy(
            10,
            20,
            content,
            worldId,
            out binding))
      {
        throw new InvalidOperationException(
          "The training dummy binding probe could not spawn its NPC.");
      }
      bindingCreated = true;

      bool bindingCaptured = trainingDummyStore.TryCaptureTrainingDummyBinding(
        binding.Slot.Value,
        binding.Anchor,
        out RuntimeNpcTrainingDummyBinding capturedBinding);
      bool capturedRootMatches = bindingCaptured &&
        capturedBinding.Reference == binding.Reference &&
        capturedBinding.RuntimeHandle == binding.RuntimeHandle &&
        capturedBinding.SlotGeneration == binding.SlotGeneration;
      released = trainingDummyStore.TryReleaseTrainingDummy(binding);
      bool staleBindingRejectedAfterRelease =
        !trainingDummyStore.TryReleaseTrainingDummy(binding);
      if (!trainingDummyStore.TrySpawn(
            SimulationContentSupportManifest.BlueSlimeNetId,
            position,
            content,
            worldId,
            out replacement) ||
          replacement is null)
      {
        throw new InvalidOperationException(
          "The training dummy binding probe could not reuse its released slot.");
      }

      bool staleBindingRejectedAfterReuse =
        !trainingDummyStore.TryReleaseTrainingDummy(binding);
      bool replacementUnaffected = trainingDummyStore.TryGetAt(
        replacement.Slot.Value,
        out RuntimeNpcEntity? current) &&
        ReferenceEquals(current, replacement);
      bool slotReused = replacement.Slot == binding.Slot &&
        replacement.SlotGeneration > binding.SlotGeneration;
      if (!bindingCaptured || !capturedRootMatches || !released ||
          !staleBindingRejectedAfterRelease || !staleBindingRejectedAfterReuse ||
          !replacementUnaffected || !slotReused)
      {
        throw new InvalidOperationException(
          "The training dummy binding did not retain root identity or reject stale slot reuse.");
      }

      return new
      {
        BindingCaptured = bindingCaptured,
        CapturedRootMatches = capturedRootMatches,
        Released = released,
        StaleBindingRejectedAfterRelease = staleBindingRejectedAfterRelease,
        StaleBindingRejectedAfterReuse = staleBindingRejectedAfterReuse,
        ReplacementUnaffected = replacementUnaffected,
        SlotReused = slotReused,
      };
    }
    finally
    {
      if (replacement is not null)
      {
        _ = trainingDummyStore.TryRelease(replacement);
      }
      if (bindingCreated && !released)
      {
        _ = trainingDummyStore.TryReleaseTrainingDummy(binding);
      }
      trainingDummyStore.Dispose();
    }
  }

  private static object RunNpcInstanceIdentityResetProbe(
    ContentCatalog content,
    EntityIdentityRegistry identityRegistry,
    int worldId,
    Vector2 position)
  {
    var immunityRegistry = new ProjectileStaticNpcImmunityRegistryComponent(
      checked(content.Snapshot.Projectiles.MaximumTypeId + 1),
      RuntimeNpcStore.MaximumNpcCapacity);
    var resetProbeStore = new RuntimeNpcStore(immunityRegistry, identityRegistry);
    if (!resetProbeStore.TrySpawn(
          SimulationContentSupportManifest.BlueSlimeNetId,
          position,
          content,
          worldId,
          out RuntimeNpcEntity? beforeReset) ||
        beforeReset is null)
    {
      throw new InvalidOperationException("The isolated NPC identity probe could not spawn its initial entity.");
    }

    NpcInstanceId beforeResetId = beforeReset.InstanceId;
    if (!resetProbeStore.TryGetEntityReference(beforeResetId, out EntityReference beforeResetReference))
    {
      throw new InvalidOperationException("The pre-reset NPC projection did not resolve to its entity root.");
    }

    int slot = beforeReset.Slot.Value;
    uint beforeResetGeneration = beforeReset.SlotGeneration;
    resetProbeStore.Reset();
    bool staleHandleRejected = !resetProbeStore.TryRelease(beforeReset);
    bool staleRootRejected = !resetProbeStore.TryResolveEntityReference(beforeResetReference, out _);
    if (!resetProbeStore.TrySpawn(
          SimulationContentSupportManifest.BlueSlimeNetId,
          position,
          content,
          worldId,
          out RuntimeNpcEntity? afterReset) ||
        afterReset is null)
    {
      throw new InvalidOperationException("The isolated NPC identity probe could not spawn after reset.");
    }

    try
    {
      bool identityChanged = afterReset.InstanceId != beforeResetId;
      bool sameSlotReused = afterReset.Slot.Value == slot;
      bool rootChanged = resetProbeStore.TryGetEntityReference(
        afterReset.InstanceId,
        out EntityReference afterResetReference) &&
        afterResetReference.EntityId != beforeResetReference.EntityId;
      if (!sameSlotReused || !identityChanged || !rootChanged ||
          !staleRootRejected || !staleHandleRejected)
      {
        throw new InvalidOperationException(
          "NPC reset reused an instance identity or accepted a stale entity handle.");
      }

      return new
      {
        Slot = slot,
        BeforeResetGeneration = beforeResetGeneration,
        AfterResetGeneration = afterReset.SlotGeneration,
        BeforeResetInstanceId = beforeResetId.Value,
        AfterResetInstanceId = afterReset.InstanceId.Value,
        BeforeResetEntityUuid = beforeResetReference.EntityId.Value,
        AfterResetEntityUuid = afterResetReference.EntityId.Value,
        SameSlotReused = sameSlotReused,
        InstanceIdChanged = identityChanged,
        RootChanged = rootChanged,
        StaleRootRejected = staleRootRejected,
        StaleHandleRejected = staleHandleRejected,
      };
    }
    finally
    {
      _ = resetProbeStore.TryRelease(afterReset);
    }
  }

  private static (LoadedWorldSession Session, object Report) RunWorldSwitchProbe(
    string worldPath,
    LoadedWorldSession previousSession,
    RuntimeNpcStore previousNpcStore,
    int localPlayerCount,
    int worldTimeRate,
    int randomSeed,
    ContentCatalog content,
    SimulationInputScript? inputScript,
    IReadOnlyList<int> npcTypesToSpawn,
    bool tileEntityReloadProbeEnabled,
    Action<RuntimeNpcStore, ProjectileStaticNpcImmunityRegistryComponent>
      selectRuntimeNpcStore,
    Action<string> loadWorld,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(previousSession);
    ArgumentNullException.ThrowIfNull(previousNpcStore);
    ArgumentNullException.ThrowIfNull(content);
    ArgumentNullException.ThrowIfNull(selectRuntimeNpcStore);
    ArgumentNullException.ThrowIfNull(loadWorld);

    cancellationToken.ThrowIfCancellationRequested();
    int previousWorldId = previousSession.World.Descriptor.WorldId;
    RuntimeTileEntityReloadVerification.CapturedState? tileEntityReloadCapture =
      tileEntityReloadProbeEnabled
        ? RuntimeTileEntityReloadVerification.Capture(previousSession)
        : null;
    RuntimeNpcEntity[] previousNpcs = previousNpcStore.CreateActiveSnapshot().ToArray();
    if (previousNpcs.Length == 0)
    {
      throw new InvalidOperationException(
        "The world-switch identity probe requires an NPC in the previous runtime store.");
    }

    ulong[] previousNpcInstanceIds = previousNpcs
      .Select(static npc => npc.InstanceId.Value)
      .ToArray();
    EntityReference[] previousNpcReferences = previousNpcs
      .Select(npc => previousNpcStore.TryGetEntityReference(
          npc.InstanceId,
          out EntityReference reference)
        ? reference
        : throw new InvalidOperationException("A previous-world NPC projection has no entity root."))
      .ToArray();
    var staticNpcImmunity = new ProjectileStaticNpcImmunityRegistryComponent(
      checked(content.Snapshot.Projectiles.MaximumTypeId + 1),
      RuntimeNpcStore.MaximumNpcCapacity);
    var runtimeNpcs = new RuntimeNpcStore(staticNpcImmunity, previousNpcStore.IdentityRegistry);
    selectRuntimeNpcStore(runtimeNpcs, staticNpcImmunity);
    LoadedWorldSession session;
    try
    {
      loadWorld(worldPath);

      session = WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession ??
        throw new InvalidOperationException("The world switch completed without an active session.");
      if (ReferenceEquals(session, previousSession) || !session.IsPublished)
      {
        throw new InvalidOperationException(
          "A world switch must publish a fresh session before runtime owners are created.");
      }
    }
    catch
    {
      runtimeNpcs.Dispose();
      throw;
    }
    previousNpcStore.Dispose();
    _ = RuntimeMain.DrainQueuedMainThreadActions();

    var switchedItemRegistry = new RuntimeItemRegistry(content, session.EntityRuntime);
    var players = new RuntimePlayerStore(session.EntityRuntime, session.IdentityRegistry);
    players.Initialize(session, localPlayerCount, content, switchedItemRegistry, inputScript);
    var worldItems = new RuntimeWorldItemStore(session, content, switchedItemRegistry);
    var naturalSpawns = new RuntimeNpcNaturalSpawnPass(
      session,
      players,
      runtimeNpcs,
      content);
    for (int index = 0; index < npcTypesToSpawn.Count; index++)
    {
      Vector2 spawnPosition = players.ActiveCount > 0
        ? players.Players[0].Movement.Position + new Vector2((index + 1) * 48f, 0f)
        : new Vector2(
          session.World.Descriptor.SpawnTileX * 16f,
          session.World.Descriptor.SpawnTileY * 16f - 40f);
      if (!runtimeNpcs.TrySpawn(
        npcTypesToSpawn[index],
        spawnPosition,
        content,
        session.World.Descriptor.WorldId,
        out _))
      {
        throw new InvalidOperationException(
          $"The requested NPC {npcTypesToSpawn[index]} could not be spawned after world switch.");
      }
    }

    var projectiles = new RuntimeProjectileStore(
      session,
      content,
      runtimeNpcs,
      players,
      worldItems,
      staticNpcImmunity);
    var liquidPhase = new ActiveLiquidTickPhase(session);
    var tileEntityPhase = new ActiveTileEntityTickPhase(
      session,
      runtimeNpcs,
      players,
      content);
    var npcTickPhase = new ActiveNpcTickPhase(runtimeNpcs, players, naturalSpawns);
    var phases = new IWorldSimulationTickPhase[]
    {
      new ActivePlayerTickPhase(players, runtimeNpcs, projectiles),
      npcTickPhase,
      new ActiveProjectileTickPhase(projectiles),
      new ActiveWorldItemsTickPhase(worldItems, players),
      tileEntityPhase,
      liquidPhase,
    };
    var kernel = new WorldSimulationKernel(
      session,
      phases,
      worldTimeRate,
      new ActiveRuntimeProjection(),
      candidate => ReferenceEquals(
        WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
        candidate));
    kernel.EnqueueCommand(_ => RuntimeMain.DrainQueuedMainThreadActions(
      () => ReferenceEquals(
        WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
        session)));
    WorldSimulationStepResult step = kernel.Step(cancellationToken);
    if (!step.TickCommitted)
    {
      throw new InvalidOperationException(
        "The switched world did not commit its first simulation tick.");
    }

    kernel.RequestStop();
    _ = kernel.Step();
    bool npcPhaseCandidatesUpdatedAtCommittedTick =
      WereNpcPhaseCandidatesUpdatedAtTick(
        runtimeNpcs,
        npcTickPhase.NpcReferencesAtUpdateStart,
        npcTickPhase.NpcUpdateTickNumberAtUpdateStart,
        kernel.TickNumber);
    if (!npcPhaseCandidatesUpdatedAtCommittedTick)
    {
      throw new InvalidOperationException(
        $"A switched-world NPC eligible for the NPC phase missed tick {kernel.TickNumber}.");
    }

    RuntimeTileEntityReloadReport? tileEntityReload = tileEntityReloadCapture is null
      ? null
      : RuntimeTileEntityReloadVerification.Evaluate(
        tileEntityReloadCapture,
        session,
        runtimeNpcs,
        tileEntityPhase.UpdatePassCount);

    ulong[] switchedNpcInstanceIds = runtimeNpcs.CreateActiveSnapshot()
      .Select(static npc => npc.InstanceId.Value)
      .ToArray();
    EntityReference[] switchedNpcReferences = runtimeNpcs.CreateActiveSnapshot()
      .Select(npc => runtimeNpcs.TryGetEntityReference(
          npc.InstanceId,
          out EntityReference reference)
        ? reference
        : throw new InvalidOperationException("A switched-world NPC projection has no entity root."))
      .ToArray();
    Guid[] previousNpcRoots = previousNpcReferences
      .Select(static reference => reference.EntityId.Value)
      .ToArray();
    Guid[] switchedNpcRoots = switchedNpcReferences
      .Select(static reference => reference.EntityId.Value)
      .ToArray();
    bool npcInstanceIdentitiesDidNotOverlap =
      !previousNpcInstanceIds.Intersect(switchedNpcInstanceIds).Any();
    bool npcRootsDidNotOverlap = !previousNpcRoots.Intersect(switchedNpcRoots).Any();
    bool previousNpcReferencesRejected = previousNpcReferences.All(reference =>
      !runtimeNpcs.TryResolveEntityReference(reference, out _));
    bool previousNpcHandleRejected = !runtimeNpcs.TryRelease(previousNpcs[0]);
    if (!npcInstanceIdentitiesDidNotOverlap || !npcRootsDidNotOverlap ||
        !previousNpcReferencesRejected || !previousNpcHandleRejected)
    {
      throw new InvalidOperationException(
        "A world switch reused an NPC identity or accepted a previous-world handle/reference.");
    }

    var report = new Dictionary<string, object?>
    {
      ["PreviousWorldId"] = previousWorldId,
      ["WorldPath"] = worldPath,
      ["WorldId"] = session.World.Descriptor.WorldId,
      ["SizeX"] = session.World.Descriptor.SizeX,
      ["SizeY"] = session.World.Descriptor.SizeY,
      ["SessionIdentityChanged"] = !ReferenceEquals(previousSession, session),
      ["PreviousSessionIsActive"] = ReferenceEquals(
        WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
        previousSession),
      ["NewSessionIsActive"] = ReferenceEquals(
        WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
        session),
      ["IsPublished"] = session.IsPublished,
      ["LocalPlayerCount"] = players.ActiveCount,
      ["RuntimeNpcCount"] = runtimeNpcs.ActiveCount,
      ["RuntimeNpcNetIds"] = runtimeNpcs.CreateActiveSnapshot()
        .Select(static npc => npc.Definition.NetId)
        .ToArray(),
      ["PreviousNpcInstanceIds"] = previousNpcInstanceIds,
      ["SwitchedNpcInstanceIds"] = switchedNpcInstanceIds,
      ["NpcInstanceIdentitiesDidNotOverlap"] = npcInstanceIdentitiesDidNotOverlap,
      ["PreviousNpcRoots"] = previousNpcRoots,
      ["SwitchedNpcRoots"] = switchedNpcRoots,
      ["NpcRootsDidNotOverlap"] = npcRootsDidNotOverlap,
      ["PreviousNpcReferencesRejected"] = previousNpcReferencesRejected,
      ["PreviousNpcHandleRejected"] = previousNpcHandleRejected,
      ["PressurePlateAnchors"] = session.Storage.PressurePlates.Anchors
        .Select(static anchor => new { anchor.X, anchor.Y })
        .ToArray(),
      ["LiquidTicks"] = liquidPhase.TickCount,
      ["LiquidCommittedTileMutationCount"] = liquidPhase.CommittedTileMutationCount,
      ["LiquidStateChangeCount"] = liquidPhase.LiquidStateChangeCount,
      ["LiquidLastTickMutationCount"] = liquidPhase.LastTickMutationCount,
      ["TickNumber"] = kernel.TickNumber,
      ["TickCommitted"] = step.TickCommitted,
      ["NpcPhaseUpdateTickNumberAtUpdateStart"] =
        npcTickPhase.NpcUpdateTickNumberAtUpdateStart,
      ["NpcPhaseEligibleNpcCountAtUpdateStart"] = npcTickPhase.NpcReferencesAtUpdateStart.Count,
      ["NpcPhaseCandidatesUpdatedAtCommittedTick"] = npcPhaseCandidatesUpdatedAtCommittedTick,
      ["PhaseOrder"] = step.PhasesExecuted,
    };
    if (tileEntityReload is not null)
    {
      report.Add("TileEntityReload", tileEntityReload);
    }

    return (session, report);
  }

  private static void ConfigureEyeWorldScenario(
    LoadedWorldSession session,
    string scenario)
  {
    Terraria.WorldSession.Components.WorldSessionRestoreState owner = session.World;
    Terraria.WorldSession.Components.WorldDescriptorState descriptor = owner.Descriptor;
    Terraria.WorldSession.Components.WorldRulesState rules = owner.Rules;
    Terraria.WorldSession.Components.WorldAppearanceStateComponent appearance = owner.Appearance;
    Terraria.WorldSession.Components.WorldTimeWeatherState timeWeather = owner.TimeWeather;
    bool expertMode = scenario == "expert-night";
    Terraria.WorldSession.Components.WorldGameMode gameMode = expertMode
      ? Terraria.WorldSession.Components.WorldGameMode.Expert
      : Terraria.WorldSession.Components.WorldGameMode.Classic;

    Terraria.WorldSession.Components.WorldSessionRestoreSystem.ApplyHeader(
      owner,
      new Terraria.WorldSession.Components.WorldDescriptorSnapshotValue(
        descriptor.WorldId,
        descriptor.UniqueId,
        descriptor.Name,
        descriptor.SeedText,
        descriptor.WorldGeneratorVersion,
        descriptor.SizeX,
        descriptor.SizeY,
        descriptor.Bounds,
        descriptor.SurfaceLayer,
        descriptor.RockLayer,
        descriptor.SpawnTileX,
        descriptor.SpawnTileY,
        descriptor.DungeonTileX,
        descriptor.DungeonTileY),
      gameMode,
      rules.SecretSeeds);
    Terraria.WorldSession.Components.WorldSessionRestoreSystem.ApplyEnvironment(
      owner: owner,
      moonType: appearance.MoonType,
      treeX: appearance.TreeX,
      treeStyle: appearance.TreeStyle,
      caveBackX: appearance.CaveBackX,
      caveBackStyle: appearance.CaveBackStyle,
      iceBackStyle: appearance.IceBackStyle,
      jungleBackStyle: appearance.JungleBackStyle,
      hellBackStyle: appearance.HellBackStyle,
      spawnTileX: descriptor.SpawnTileX,
      spawnTileY: descriptor.SpawnTileY,
      worldSurface: descriptor.SurfaceLayer,
      rockLayer: descriptor.RockLayer,
      time: 0,
      dayTime: false,
      moonPhase: timeWeather.MoonPhase,
      bloodMoon: timeWeather.BloodMoon,
      eclipse: timeWeather.Eclipse,
      dungeonX: descriptor.DungeonTileX,
      dungeonY: descriptor.DungeonTileY,
      crimson: rules.WorldEvil == Terraria.WorldSession.Components.WorldEvilType.Crimson);

    // Keep the legacy runtime projection aligned after committing authoritative session state.
    RuntimeMain.GameMode = (int)owner.Rules.GameMode;
    RuntimeMain.expertMode = owner.Rules.GameMode is
      Terraria.WorldSession.Components.WorldGameMode.Expert or
      Terraria.WorldSession.Components.WorldGameMode.Master;
    RuntimeMain.masterMode =
      owner.Rules.GameMode == Terraria.WorldSession.Components.WorldGameMode.Master;
    RuntimeMain.dayTime = owner.TimeWeather.DayTime;
    RuntimeMain.time = owner.TimeWeather.Time;
  }

  private static NpcEyeTransformationProbeSetup ConfigureNpcEyeTransformationProbe(
    string scenario,
    RuntimeNpcStore npcs,
    ContentCatalog content,
    LoadedWorldSession session)
  {
    RuntimeNpcEntity eye = npcs.CreateActiveSnapshot().Single(npc =>
      npc.Definition.NetId == SimulationContentSupportManifest.EyeOfCthulhuNetId);
    int? releasedLowerSlot = null;
    if (scenario == "next-tick")
    {
      if (npcs.ActiveCount != 2)
      {
        throw new InvalidOperationException(
          "The next-tick Eye probe requires only its Zombie and Eye NPCs before setup.");
      }

      RuntimeNpcEntity zombie = npcs.CreateActiveSnapshot()
        .Where(npc => npc.Definition.NetId == SimulationContentSupportManifest.ZombieNetId)
        .OrderBy(npc => npc.Slot.Value)
        .FirstOrDefault() ??
        throw new InvalidOperationException(
          "The next-tick Eye probe could not find its earlier Zombie slot.");
      if (zombie.Slot.Value >= eye.Slot.Value || !npcs.TryRelease(zombie))
      {
        throw new InvalidOperationException(
          "The next-tick Eye probe could not free a lower NPC slot.");
      }

      releasedLowerSlot = zombie.Slot.Value;
    }
    else if (npcs.ActiveCount != 1)
    {
      throw new InvalidOperationException(
        "The Eye transformation probe requires the Eye to be the only active NPC before setup.");
    }

    (float ai0, float ai1, float ai2) = scenario switch
    {
      "expert-summon" or "capacity-rejection" or "next-tick" => (1f, 19f, 0.095f),
      "phase-one-boundary" => (1f, 99f, 0.495f),
      "phase-two-boundary" => (2f, 99f, 0.005f),
      _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
    };
    NpcAiStateComponent aiState = new(
      eye.Definition.Spawn.AiStyle,
      ai0,
      ai1,
      ai2,
      state3: 0f,
      timer: 0);
    eye.CommitAiState(in aiState, eye.BehaviorAction);

    int prefilledNpcCount = 0;
    if (scenario == "capacity-rejection")
    {
      if (npcs.ActiveCount != 1)
      {
        throw new InvalidOperationException(
          "The capacity Eye probe requires the Eye to be the only active NPC before filling.");
      }

      while (npcs.TrySpawn(
        SimulationContentSupportManifest.TrainingDummyNetId,
        Vector2.Zero,
        content,
        session.World.Descriptor.WorldId,
        out _))
      {
        prefilledNpcCount++;
      }
    }

    return new NpcEyeTransformationProbeSetup(
      scenario,
      eye.Slot.Value,
      releasedLowerSlot,
      prefilledNpcCount);
  }

  private static object VerifyNpcEyeTransformationProbe(
    NpcEyeTransformationProbeSetup setup,
    RuntimeNpcStore npcs,
    long finalTickNumber,
    long? servantLastUpdatedTickAfterFirstTick)
  {
    RuntimeNpcEntity eye = npcs.CreateActiveSnapshot().Single(npc =>
      npc.Definition.NetId == SimulationContentSupportManifest.EyeOfCthulhuNetId);
    NpcAiStateComponent aiState = eye.CaptureAiState();
    RuntimeNpcEntity[] servants = npcs.CreateActiveSnapshot()
      .Where(npc =>
        npc.Definition.NetId == SimulationContentSupportManifest.ServantOfCthulhuNetId)
      .ToArray();
    IReadOnlyList<string> effects = eye.CaptureEyeOfCthulhuEffectTrace();
    RuntimeNpcEntity? servant = servants.FirstOrDefault();

    bool passed = setup.Scenario switch
    {
      "expert-summon" => aiState.State0 == 1f && aiState.State1 == 20f &&
        servants.Length == 1 && servant!.LastBehaviorUpdatedTick == finalTickNumber &&
        MathF.Abs(servant.Movement.Velocity.Length() - 5f) < 0.001f,
      "phase-one-boundary" => aiState.State0 == 2f && aiState.State1 == 0f &&
        MathF.Abs(aiState.State2 - 0.5f) < 0.000001f && servants.Length == 1 &&
        effects.Count(effect => effect.StartsWith("SpawnGore(", StringComparison.Ordinal)) == 6 &&
        effects.Any(effect => effect.StartsWith("PlaySound(3,", StringComparison.Ordinal)) &&
        effects.Any(effect => effect.StartsWith("PlaySound(15,", StringComparison.Ordinal)),
      "phase-two-boundary" => aiState.State0 == 3f && aiState.State1 == 0f &&
        aiState.State2 == 0f && servants.Length == 1 &&
        effects.All(effect => !effect.StartsWith("PlaySound(3,", StringComparison.Ordinal)) &&
        !effects.Any(effect => effect.StartsWith("SpawnGore(", StringComparison.Ordinal)),
      "capacity-rejection" => servants.Length == 0 && npcs.ActiveCount == RuntimeNpcStore.MaximumNpcCapacity &&
        effects.Contains("ServantSpawnRejected:Capacity"),
      "next-tick" => servant is not null && servant.Slot.Value < eye.Slot.Value &&
        servantLastUpdatedTickAfterFirstTick != finalTickNumber &&
        servant.LastBehaviorUpdatedTick == finalTickNumber &&
        finalTickNumber == 2,
      _ => false,
    };
    if (!passed)
    {
      throw new InvalidOperationException(
        $"The Eye transformation host probe '{setup.Scenario}' did not satisfy its result checks.");
    }

    return new
    {
      setup.Scenario,
      Passed = passed,
      setup.EyeSlot,
      setup.ReleasedLowerSlot,
      setup.PrefilledNpcCount,
      FinalTickNumber = finalTickNumber,
      ServantSlot = servant?.Slot.Value,
      ServantLastUpdatedTickAfterFirstTick = servantLastUpdatedTickAfterFirstTick,
      ServantLastUpdatedTick = servant?.LastBehaviorUpdatedTick,
      Ai0 = aiState.State0,
      Ai1 = aiState.State1,
      Ai2 = aiState.State2,
      Ai3 = aiState.State3,
      ServantCount = servants.Length,
      EyeEffectTrace = effects,
    };
  }

  private static void WriteLoadFailureReport(
    string reportPath,
    string worldPath,
    int tickLimit,
    int playerCount,
    int randomSeed,
    WorldLoadRecoveryResult? loadResult,
    Exception? loadException,
    bool canceled)
  {
    WriteReport(new
    {
      Succeeded = false,
      Canceled = canceled,
      Mode = "HeadlessFiniteSimulation",
      WorldPath = worldPath,
      TickLimit = tickLimit,
      TickNumber = 0,
      LocalPlayerCount = playerCount,
      RandomSeed = randomSeed,
      LoadStatus = loadResult?.TerminalAction.ToString() ?? "NoResult",
      LoadAttempts = loadResult?.LoadAttemptCount,
      LastLoadStatus = loadResult?.LastLoadOutcome?.Status.ToString(),
      LastLoadFailureKind = loadResult?.LastLoadOutcome?.Failure.Kind.ToString(),
      LastLoadFailureDetail = loadResult?.LastLoadOutcome?.Failure.Detail,
      FailureKind = loadResult?.Failure.Kind.ToString(),
      FailureDetail = loadResult?.Failure.Detail,
      CleanupFailureKind = loadResult?.CleanupFailure.Kind.ToString(),
      CleanupFailureDetail = loadResult?.CleanupFailure.Detail,
      Exception = loadException?.ToString(),
    }, reportPath);
  }

  private static void WriteSaveFailureReport(
    string reportPath,
    string worldPath,
    string savePath,
    LoadedWorldSession session,
    WorldSimulationKernel kernel,
    WorldSaveProjection save,
    string? previousSaveHash)
  {
    string? currentSaveHash = GetFileSha256IfExists(savePath);
    WriteReport(new
    {
      Succeeded = false,
      Canceled = save.Failure.Kind == WorldStorageFailureKind.Canceled,
      Mode = "HeadlessFiniteSimulation",
      WorldPath = worldPath,
      session.World.Descriptor.WorldId,
      TickNumber = kernel.TickNumber,
      IsPublished = session.IsPublished,
      SavePath = savePath,
      SaveCommitted = save.Committed,
      SaveFailureKind = save.Failure.Kind.ToString(),
      SaveFailureDetail = save.Failure.Detail,
      SaveStages = save.Stages,
      PreviousSaveSha256 = previousSaveHash,
      CurrentSaveSha256 = currentSaveHash,
      PreviousSavePreserved = previousSaveHash is not null &&
        StringComparer.Ordinal.Equals(previousSaveHash, currentSaveHash),
      SnapshotRevision = kernel.CurrentSnapshot?.Revision,
      KernelStatus = kernel.Status.ToString(),
    }, reportPath);
  }

  private static void WriteReport(object report, string reportPath)
  {
    string json = JsonSerializer.Serialize(report, new JsonSerializerOptions
    {
      WriteIndented = true,
    });
    if (!string.IsNullOrWhiteSpace(reportPath))
    {
      string? reportDirectory = Path.GetDirectoryName(reportPath);
      if (!string.IsNullOrWhiteSpace(reportDirectory))
      {
        Directory.CreateDirectory(reportDirectory);
      }
      File.WriteAllText(reportPath, json);
    }

    Console.WriteLine(json);
  }

  private static string CalculateLiquidStateChecksum(TileMapStore tileMap)
  {
    ArgumentNullException.ThrowIfNull(tileMap);
    TileMapSnapshot snapshot = tileMap.CreateSnapshot();
    const ulong prime = 1099511628211UL;
    ulong hash = 14695981039346656037UL;
    for (int shift = 0; shift < 32; shift += 8)
    {
      hash = unchecked((hash ^ (byte)(snapshot.Width >> shift)) * prime);
      hash = unchecked((hash ^ (byte)(snapshot.Height >> shift)) * prime);
    }

    for (int x = 0; x < snapshot.Width; x++)
    {
      for (int y = 0; y < snapshot.Height; y++)
      {
        TileCellState tile = snapshot.GetTile(x, y);
        byte liquidType = tile.LiquidAmount > 0 ? tile.LiquidType : (byte)0;
        hash = unchecked((hash ^ tile.LiquidAmount) * prime);
        hash = unchecked((hash ^ liquidType) * prime);
      }
    }

    return hash.ToString("X16", System.Globalization.CultureInfo.InvariantCulture);
  }

  private static TownRoomTilePoint FindGuideHomeReturnProbeTile(
    LoadedWorldSession session,
    NpcDefinition guideDefinition)
  {
    int width = Math.Max(
      1,
      (int)(guideDefinition.Movement.Width * guideDefinition.Movement.Scale));
    int height = Math.Max(
      1,
      (int)(guideDefinition.Movement.Height * guideDefinition.Movement.Scale));
    var collisionQuery = new RuntimeNpcHomeReturnCollisionQuery(session);
    int preferredX = session.World.Descriptor.SpawnTileX;
    int preferredY = session.World.Descriptor.SpawnTileY;
    int minimumY = Math.Max(10, preferredY - 20);
    int maximumY = Math.Min(session.Storage.TileMap.Height - 41, preferredY + 20);
    for (int distance = 0; distance <= 40; distance++)
    {
      foreach (int tileX in new[] { preferredX - distance, preferredX + distance })
      {
        for (int tileY = minimumY; tileY <= maximumY; tileY++)
        {
          if (NpcHomeReturnDestinationQuery.TryFindDestination(
                tileX,
                tileY,
                width,
                height,
                collisionQuery,
                out _))
          {
            return new TownRoomTilePoint(tileX, tileY);
          }
        }
      }
    }

    throw new InvalidOperationException(
      "The NPC home return success probe could not find an open home area.");
  }

  private static TownRoomTilePoint FindGuideHousingRevalidationProbeTile(
    LoadedWorldSession session,
    NpcDefinition guideDefinition)
  {
    int width = Math.Max(
      1,
      (int)(guideDefinition.Movement.Width * guideDefinition.Movement.Scale));
    int height = Math.Max(
      1,
      (int)(guideDefinition.Movement.Height * guideDefinition.Movement.Scale));
    var collisionQuery = new RuntimeNpcHomeReturnCollisionQuery(session);
    int roomX = Math.Clamp(
      session.World.Descriptor.SpawnTileX + 32,
      20,
      session.Storage.TileMap.Width - 20);
    int roomY = Math.Clamp(
      session.World.Descriptor.SpawnTileY - 20,
      20,
      session.Storage.TileMap.Height - 20);
    InstallHousingRevalidationProbeRoom(roomX, roomY);
    if (WorldGen.IsHousingRoomValidAt(
          roomX + 2,
          roomY + 1,
          session.World.Descriptor.SizeX,
          session.World.Descriptor.SizeY) &&
        NpcHomeReturnDestinationQuery.TryFindDestination(
          roomX + 2,
          roomY + 2,
          width,
          height,
          collisionQuery,
          out _))
    {
      return new TownRoomTilePoint(roomX + 2, roomY + 2);
    }

    var resident = new TownHousingResidentKey(guideDefinition.TypeId);
    if (TownHousingRegistrySystem.TryGetRoom(session.TownHousing, resident, out TilePosition assignedRoom) &&
        WorldGen.IsHousingRoomValidAt(
          assignedRoom.X,
          assignedRoom.Y - 1,
          session.World.Descriptor.SizeX,
          session.World.Descriptor.SizeY) &&
        NpcHomeReturnDestinationQuery.TryFindDestination(
          assignedRoom.X,
          assignedRoom.Y,
          width,
          height,
          collisionQuery,
          out _))
    {
      return new TownRoomTilePoint(assignedRoom.X, assignedRoom.Y);
    }

    foreach (KeyValuePair<TownHousingResidentKey, TilePosition> assignment in
             TownHousingRegistrySystem.GetRoomAssignmentsSnapshot(session.TownHousing))
    {
      TilePosition room = assignment.Value;
      if (WorldGen.IsHousingRoomValidAt(
            room.X,
            room.Y - 1,
            session.World.Descriptor.SizeX,
            session.World.Descriptor.SizeY) &&
          NpcHomeReturnDestinationQuery.TryFindDestination(
            room.X,
            room.Y,
            width,
            height,
            collisionQuery,
            out _))
      {
        return new TownRoomTilePoint(room.X, room.Y);
      }
    }

    int preferredX = session.World.Descriptor.SpawnTileX;
    int preferredY = session.World.Descriptor.SpawnTileY;
    int minimumY = Math.Max(10, preferredY - 40);
    int maximumY = Math.Min(session.Storage.TileMap.Height - 41, preferredY + 40);
    for (int distance = 0; distance <= 80; distance++)
    {
      foreach (int tileX in new[] { preferredX - distance, preferredX + distance })
      {
        for (int tileY = minimumY; tileY <= maximumY; tileY++)
        {
          if (WorldGen.IsHousingRoomValidAt(
                tileX,
                tileY - 1,
                session.World.Descriptor.SizeX,
                session.World.Descriptor.SizeY) &&
              NpcHomeReturnDestinationQuery.TryFindDestination(
                tileX,
                tileY,
                width,
                height,
                collisionQuery,
                out _))
          {
            return new TownRoomTilePoint(tileX, tileY);
          }
        }
      }
    }

    throw new InvalidOperationException(
      "The NPC housing revalidation probe could not find a valid housing room.");
  }

  private static void InstallHousingRevalidationProbeRoom(int roomX, int roomY)
  {
    const int roomWidth = 10;
    const int roomHeight = 6;
    for (int x = roomX - 1; x <= roomX + roomWidth; x++)
    {
      for (int y = roomY - 1; y <= roomY + roomHeight; y++)
      {
        Tile tile = RuntimeMain.tile[x, y];
        tile.ClearEverything();
        tile.wall = WallID.Wood;
        bool boundary = x == roomX - 1 || x == roomX + roomWidth ||
          y == roomY - 1 || y == roomY + roomHeight;
        if (boundary)
        {
          tile.type = 1;
          tile.active(active: true);
        }
        else
        {
          tile.type = 0;
          tile.active(active: false);
        }
      }
    }

    SetProbeRoomTile(roomX + 1, roomY + 1, TileID.Torches);
    SetProbeRoomTile(roomX + 3, roomY + 1, TileID.Tables);
    SetProbeRoomTile(roomX + 5, roomY + 1, TileID.Chairs);
    SetProbeRoomTile(roomX + 7, roomY, TileID.ClosedDoor);
  }

  private static void SetProbeRoomTile(int x, int y, ushort type)
  {
    Tile tile = RuntimeMain.tile[x, y];
    tile.type = type;
    tile.active(active: true);
    tile.frameX = 0;
    tile.frameY = 0;
  }

  private static void BlockGuideHomeReturnCandidates(
    LoadedWorldSession session,
    TownRoomTilePoint homeTile)
  {
    for (int x = homeTile.X - 1; x <= homeTile.X + 1; x++)
    {
      for (int y = homeTile.Y - 3; y <= homeTile.Y - 2; y++)
      {
        session.Storage.TileMap.CommitTile(
          x,
          y,
          new TileCellState
          {
            Type = 1,
            TileHeader = 0x20,
          });
      }
    }
  }

  private static object CreateNpcHomeReturnProbeReport(
    string scenario,
    RuntimeNpcEntity npc,
    Vector2 initialPosition)
  {
    RuntimeNpcEntity.NpcImmediateEffectSnapshot effects = npc.CaptureImmediateEffects();
    RuntimeNpcEntity.NpcTaskSnapshot task = npc.CaptureTaskSnapshot();
    RuntimeNpcEntity.NpcHousingRelationSnapshot housing = npc.CaptureHousingRelationSnapshot();
    Vector2 finalPosition = npc.Movement.Position;
    return new
    {
      Scenario = scenario,
      InitialPosition = new { X = initialPosition.X, Y = initialPosition.Y },
      FinalPosition = new { X = finalPosition.X, Y = finalPosition.Y },
      PositionChanged = Vector2.DistanceSquared(initialPosition, finalPosition) > 0.01f,
      VelocityCleared = npc.Movement.Velocity == Vector2.Zero,
      Task = task.Kind.ToString(),
      TaskPhase = task.Phase.ToString(),
      TaskCursor = task.Cursor,
      TaskFailureReason = task.FailureReason.ToString(),
      HousingHasHome = housing.HasHome,
      HousingHomeTile = housing.HomeTile is { } home
        ? new { home.X, home.Y }
        : null,
      effects.HomeTeleportRequested,
      effects.HomeTeleportSucceeded,
      effects.HomeTeleportFailed,
      effects.HomeTeleportCandidateOffset,
      HomeTeleportFailureReason = effects.HomeTeleportFailureReason.ToString(),
      HomeTeleportPosition = new
      {
        X = effects.HomeTeleportPosition.X,
        Y = effects.HomeTeleportPosition.Y,
      },
    };
  }

  private static object CreateNpcHousingRevalidationProbeReport(
    string scenario,
    RuntimeNpcEntity npc,
    LoadedWorldSession session)
  {
    RuntimeNpcEntity.NpcImmediateEffectSnapshot effects = npc.CaptureImmediateEffects();
    RuntimeNpcEntity.NpcHousingRelationSnapshot housing = npc.CaptureHousingRelationSnapshot();
    bool registryHasRoom = TownHousingRegistrySystem.TryGetRoom(
      session.TownHousing,
      new TownHousingResidentKey(npc.Definition.TypeId),
      out TilePosition registryRoom);
    bool registryHomeless = TownHousingRegistrySystem.IsHomeless(
      session.TownHousing,
      new TownHousingResidentKey(npc.Definition.TypeId));
    RuntimeNpcEntity.NpcTaskSnapshot task = npc.CaptureTaskSnapshot();
    return new
    {
      Scenario = scenario,
      HousingHasHome = housing.HasHome,
      HousingIsHomeless = housing.IsHomeless,
      HousingHomeTile = housing.HomeTile is { } home
        ? new { home.X, home.Y }
        : null,
      RegistryHasRoom = registryHasRoom,
      RegistryRoom = registryHasRoom ? new { registryRoom.X, registryRoom.Y } : null,
      RegistryIsHomeless = registryHomeless,
      effects.HousingRevalidationFailed,
      effects.HousingRegistrySynchronized,
      effects.NetworkUpdateRequested,
      Task = task.Kind.ToString(),
      TaskPhase = task.Phase.ToString(),
      TaskFailureReason = task.FailureReason.ToString(),
    };
  }

  private static object CreateChestItemProbeReport(
    WorldContainerStore containers,
    TileCoordinate anchor,
    int itemIndex)
  {
    WorldChestSnapshot? chest = containers.CreateSnapshot()
      .FirstOrDefault(candidate => candidate.Anchor == anchor);
    if (chest is null || (uint)itemIndex >= (uint)chest.Items.Count)
    {
      throw new InvalidOperationException(
        "The chest item probe could not read its committed owner state.");
    }

    ItemState item = chest.Items[itemIndex];
    return new
    {
      Anchor = new { anchor.X, anchor.Y },
      Slot = itemIndex,
      item.Type,
      item.Prefix,
      item.Stack,
      containers.MutationRevision,
    };
  }

  private static object? CreateTileAreaSample(
    TileMapStore tileMap,
    TileCoordinate? probeCoordinate)
  {
    if (probeCoordinate is not TileCoordinate probe)
    {
      return null;
    }

    var cells = new List<object>(9);
    for (int y = probe.Y; y <= probe.Y + 2; y++)
    {
      for (int x = probe.X - 1; x <= probe.X + 1; x++)
      {
        if ((uint)x >= (uint)tileMap.Width || (uint)y >= (uint)tileMap.Height)
        {
          continue;
        }

        TileCellState cell = tileMap.GetTile(x, y);
        cells.Add(new
        {
          X = x,
          Y = y,
          cell.Type,
          cell.TileHeader,
          IsActive = (cell.TileHeader & 0x20) != 0,
          HasActuator = (cell.TileHeader & 0x800) != 0,
          IsActuated = (cell.TileHeader & 0x40) != 0,
          cell.LiquidAmount,
          cell.LiquidType,
        });
      }
    }

    return cells;
  }

  private static WorldSaveSnapshotCoordinator CreateSnapshotCoordinator(
    LoadedWorldSession session,
    RuntimeNpcStore runtimeNpcs,
    WorldStorageIoGate ioGate)
  {
    return new WorldSaveSnapshotCoordinator(
      WorldStorageCoordinatorFactory.CreateSaveCoordinator(),
      new WorldTransformTransactionComponent(),
      ioGate,
      new LoadedWorldPersistenceSnapshotSource(
        session,
        () => WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
        runtimeNpcs.CreatePersistenceSnapshot),
      WorldStorageCoordinatorFactory.CreateSaveEncoder(),
      WorldStorageCoordinatorFactory.CreateSaveValidationQuery(),
      new LegacyWorldTileEntitySavePreparation(session));
  }

  private static RuntimeNpcRollbackEvidence CaptureRuntimeNpcRollbackEvidence(
    LoadedWorldSession session,
    RuntimeNpcStore store)
  {
    RuntimeNpcEntity[] activeNpcs = store.CreateActiveSnapshot().ToArray();
    var npcEvidence = new RuntimeNpcProjectionEvidence[activeNpcs.Length];
    for (int index = 0; index < activeNpcs.Length; index++)
    {
      RuntimeNpcEntity npc = activeNpcs[index];
      if (!store.TryGetEntityReference(npc.InstanceId, out EntityReference reference) ||
          !store.TryResolveEntityReference(reference, out RuntimeEntityHandle handle) ||
          handle != npc.RuntimeHandle)
      {
        throw new InvalidOperationException(
          "The previous NPC projection did not expose a resolvable runtime root.");
      }

      npcEvidence[index] = CaptureRuntimeNpcProjectionEvidence(npc, reference, handle);
    }

    return new RuntimeNpcRollbackEvidence(
      session,
      store,
      session.EntityRuntime.EntityCount,
      npcEvidence);
  }

  private static bool HasRuntimeNpcProjectionUnchanged(
    RuntimeNpcRollbackEvidence evidence,
    LoadedWorldSession session,
    RuntimeNpcStore store)
  {
    if (!ReferenceEquals(evidence.Session, session) ||
        !ReferenceEquals(evidence.Store, store) ||
        session.EntityRuntime.EntityCount != evidence.EntityCount)
    {
      return false;
    }

    IReadOnlyList<RuntimeNpcEntity> activeNpcs = store.CreateActiveSnapshot();
    if (activeNpcs.Count != evidence.Npcs.Length)
    {
      return false;
    }

    for (int index = 0; index < evidence.Npcs.Length; index++)
    {
      RuntimeNpcProjectionEvidence expected = evidence.Npcs[index];
      RuntimeNpcEntity npc = activeNpcs[index];
      if (!ReferenceEquals(npc, expected.Entity) ||
          !store.TryGetEntityReference(npc.InstanceId, out EntityReference reference) ||
          reference != expected.Reference ||
          !store.TryResolveEntityReference(reference, out RuntimeEntityHandle handle) ||
          handle != expected.Handle ||
          CaptureRuntimeNpcProjectionEvidence(npc, reference, handle) != expected)
      {
        return false;
      }
    }

    return true;
  }

  private static RuntimeNpcProjectionEvidence CaptureRuntimeNpcProjectionEvidence(
    RuntimeNpcEntity npc,
    EntityReference reference,
    RuntimeEntityHandle handle)
  {
    WorldNpcState savedState = npc.SavedState;
    LocationComponent location = npc.Location;
    VelocityComponent velocity = npc.Velocity;
    return new RuntimeNpcProjectionEvidence(
      npc,
      npc.InstanceId,
      reference,
      handle,
      npc.Slot.Value,
      npc.SlotGeneration,
      location.X,
      location.Y,
      velocity.X,
      velocity.Y,
      new NpcSavedStateEvidence(
        savedState.NetId,
        savedState.LegacyTypeName,
        savedState.IsTownNpc,
        savedState.Name,
        savedState.X,
        savedState.Y,
        savedState.Homeless,
        savedState.Home.X,
        savedState.Home.Y,
        savedState.Variation,
        savedState.HomelessDespawn));
  }

  private sealed record RuntimeNpcRollbackEvidence(
    LoadedWorldSession Session,
    RuntimeNpcStore Store,
    int EntityCount,
    RuntimeNpcProjectionEvidence[] Npcs);

  private readonly record struct RuntimeNpcProjectionEvidence(
    RuntimeNpcEntity Entity,
    NpcInstanceId InstanceId,
    EntityReference Reference,
    RuntimeEntityHandle Handle,
    int Slot,
    uint SlotGeneration,
    float LocationX,
    float LocationY,
    float VelocityX,
    float VelocityY,
    NpcSavedStateEvidence SavedState);

  private readonly record struct NpcSavedStateEvidence(
    int? NetId,
    string? LegacyTypeName,
    bool IsTownNpc,
    string Name,
    float X,
    float Y,
    bool Homeless,
    int HomeX,
    int HomeY,
    int? Variation,
    bool HomelessDespawn);

  private static bool EnsureSourceWorldUnchanged(
    string worldPath,
    string expectedHash,
    bool replacedBySave)
  {
    if (replacedBySave)
    {
      return true;
    }

    string currentHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(worldPath)));
    return StringComparer.Ordinal.Equals(expectedHash, currentHash);
  }

  private static bool PathsReferToSameFile(string leftPath, string rightPath)
  {
    StringComparison comparison = OperatingSystem.IsWindows()
      ? StringComparison.OrdinalIgnoreCase
      : StringComparison.Ordinal;
    return string.Equals(Path.GetFullPath(leftPath), Path.GetFullPath(rightPath), comparison);
  }

  private static string? GetFileSha256IfExists(string path)
  {
    return File.Exists(path)
      ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))
      : null;
  }
}
