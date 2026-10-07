using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSSLC.WorldGeneration.ID;
using NSSLC.WorldGeneration.WorldBuilding;
using Terraria.WorldSession.Components;
using Terraria.WorldGeneration.Systems;

namespace NSSLC.WorldGeneration;

public static partial class Main {
  private sealed record PendingMainThreadAction(
    Action Action,
    TaskCompletionSource Completion);

  private sealed class MainThreadActionQueueBinding : IDisposable {
    private int _ownerThreadId;

    public MainThreadActionQueueBinding(int ownerThreadId) {
      _ownerThreadId = ownerThreadId;
    }

    public void Dispose() {
      int ownerThreadId = Volatile.Read(ref _ownerThreadId);
      if (ownerThreadId == 0) {
        return;
      }
      if (Environment.CurrentManagedThreadId != ownerThreadId) {
        throw new InvalidOperationException(
          "The main-thread action queue must be released by its owner thread.");
      }

      if (Interlocked.Exchange(ref _ownerThreadId, 0) != 0) {
        ReleaseMainThreadActionQueue(ownerThreadId);
      }
    }
  }

  private static readonly object MainThreadActionGate = new();
  private static readonly Queue<PendingMainThreadAction> PendingMainThreadActions = new();
  private static int _mainThreadActionOwnerThreadId;

  private sealed record WorldLoadHandlerRegistration(
    Action LoadWorld,
    Action ResetWorldStorage);

  private static WorldLoadHandlerRegistration _worldLoadHandlerRegistration;
  private static bool _objectDataInitialized;
  private static WorldSessionRestoreState _activeWorldSession = CreateWorldSession();
  public static bool onlyShimmerOceanWorldsGeneration;
  public static bool onlyShimmerOceanWorlds;
  public static bool Setting_UseReducedMaxLiquids;
  public static int starsHit;
  public static int mapTimeMax = 30;
  public static List<string> anglerWhoFinishedToday = new List<string>();
  public static bool anglerQuestFinished;
  public static int anglerQuest;
  public static string worldName = "Generated World";
  public static NSSLC.WorldGeneration.Geometry.Vector2 screenPosition;
  public static WorldSessionRestoreState ActiveWorldSession =>
    Volatile.Read(ref _activeWorldSession);
  public static bool ShouldShowInvisibleBlocksAndWalls() => true;

  public static void SetActiveWorldSession(WorldSessionRestoreState session) {
    ArgumentNullException.ThrowIfNull(session);
    WorldGen.RunWorldLifecycleMutation(() => {
      WorldTileMetricsSystem.EnsureTileTypeCount(session.TileMetrics, TileID.Count);
      Volatile.Write(ref _activeWorldSession, session);
    });
  }

  private static WorldSessionRestoreState CreateWorldSession() {
    var session = new WorldSessionRestoreState();
    WorldTileMetricsSystem.EnsureTileTypeCount(session.TileMetrics, TileID.Count);
    return session;
  }

  internal static void ResetWorldStorage() {
    Volatile.Read(ref _worldLoadHandlerRegistration)?.ResetWorldStorage.Invoke();
    SetActiveWorldSession(CreateWorldSession());
    tile = new Tile[maxTilesX, maxTilesY];
    for (int x = 0; x < maxTilesX; x++) {
      for (int y = 0; y < maxTilesY; y++) {
        tile[x, y] = new Tile();
      }
    }
    chest = new Chest[8000];
    sign = new Sign[32000];
    npc = Enumerable.Range(0, 200).Select(index => new NPC { whoAmI = index }).ToArray();
    player = Enumerable.Range(0, 256).Select(index => new Player()).ToArray();
    projectile = Enumerable.Range(0, 1000)
      .Select(index => new Projectile { whoAmI = index }).ToArray();
    item = Enumerable.Range(0, 400)
      .Select(index => new WorldItem { whoAmI = index }).ToArray();
    Array.Clear(countsAsHostForGameplay, 0, countsAsHostForGameplay.Length);
    Array.Clear(timeItemSlotCannotBeReusedFor, 0, item.Length);
    Array.Clear(NPC.spawnSlotProtected, 0, NPC.spawnSlotProtected.Length);
    dust = Enumerable.Range(0, 6001)
      .Select(index => new Dust { dustIndex = index }).ToArray();
    gore = Enumerable.Range(0, 601).Select(index => new Gore()).ToArray();
    maxNPCs = npc.Length;
    liquid = Enumerable.Range(0, Liquid.maxLiquid).Select(index => new Liquid()).ToArray();
    liquidBuffer = Enumerable.Range(0, Liquid.maxLiquidBuffer)
      .Select(index => new LiquidBuffer()).ToArray();
    leftWorld = 0;
    topWorld = 0;
    rightWorld = maxTilesX * 16;
    bottomWorld = maxTilesY * 16;
    maxSectionsX = (maxTilesX + 199) / 200;
    maxSectionsY = (maxTilesY + 149) / 150;
    spawnTileX = maxTilesX / 2;
    spawnTileY = maxTilesY / 3;
    gameMenu = true;
    dedServ = true;
    netMode = 0;
    dayTime = true;
    time = 13500;
    hardMode = false;
    UnderworldLayer = maxTilesY - 200;
    InitializeTileRules();
    ID.TileID.Sets.PostSetupContent();
    Framing.Initialize();
    Minecart.Initialize();
    ItemCatalog.InitializePlacementDetails();
    if (!_objectDataInitialized) {
      ObjectData.TileObjectData.Initialize();
      _objectDataInitialized = true;
    }
    TileEntity.Clear();
    Chest.Clear();
    WorldGen.ResetWorldTileMetrics();
  }

  /// <summary>Initializes the legacy runtime tables before a headless host publishes its first world.</summary>
  public static void InitializeHeadlessRuntime() {
    WorldGen.RunWorldLifecycleMutation(ResetWorldStorage);
  }

  public static void checkXMas() { }
  public static void checkHalloween() { }
  public static void UpdateTimeRate() { }
  public static void RestoreSlimeRainAfterWorldLoad() {
    if (slimeRainTime <= 0.0 || remixWorld || !isThereAWorldSurface || slimeRain || raining) {
      return;
    }

    slimeRain = true;
    slimeRainKillCount = 0;
  }
	public static void ClearWorldSeedFlags()
{
	
		getGoodWorld = false;
		drunkWorld = false;
		tenthAnniversaryWorld = false;
		dontStarveWorld = false;
		notTheBeesWorld = false;
		remixWorld = false;
		noTrapsWorld = false;
		zenithWorld = false;
		skyblockWorld = false;
		vampireSeed = false;
		infectedSeed = false;
		teamBasedSpawnsSeed = false;
		dualDungeonsSeed = false;
	
	}
  public static void ResetWindCounter(bool resetExtreme = false) {
    windSpeedCurrent = 0;
    windSpeedTarget = 0;
  }
  public static void ChangeRain(bool instant = false, float? strengthOverride = null) {
    raining = true;
    maxRaining = strengthOverride ?? 0.5f;
  }
  public static void AnglerQuestSwap() { }
  public static void QueueMainThreadAction(Action action) {
    ArgumentNullException.ThrowIfNull(action);
    if (!TryQueueMainThreadAction(action, completion: null)) {
      action.Invoke();
    }
  }

  /// <summary>Installs a tick-drained action queue for a single simulation owner thread.</summary>
  public static IDisposable BindMainThreadActionQueue() {
    int ownerThreadId = Environment.CurrentManagedThreadId;
    lock (MainThreadActionGate) {
      if (_mainThreadActionOwnerThreadId != 0) {
        throw new InvalidOperationException("A main-thread action queue is already bound.");
      }

      _mainThreadActionOwnerThreadId = ownerThreadId;
    }

    return new MainThreadActionQueueBinding(ownerThreadId);
  }

  /// <summary>Runs queued legacy actions on the simulation owner thread.</summary>
  public static int DrainQueuedMainThreadActions() {
    return DrainQueuedMainThreadActions(static () => true);
  }

  /// <summary>Runs queued legacy actions until the queue empties or its owner guard stops.</summary>
  /// <param name="shouldContinue">Checked before each queued action.</param>
  public static int DrainQueuedMainThreadActions(Func<bool> shouldContinue) {
    ArgumentNullException.ThrowIfNull(shouldContinue);
    int ownerThreadId = Volatile.Read(ref _mainThreadActionOwnerThreadId);
    if (ownerThreadId == 0 || Environment.CurrentManagedThreadId != ownerThreadId) {
      throw new InvalidOperationException(
        "Queued main-thread actions must be drained by the simulation owner thread.");
    }

    int processed = 0;
    while (true) {
      if (!shouldContinue.Invoke()) {
        return processed;
      }

      PendingMainThreadAction pending;
      lock (MainThreadActionGate) {
        if (_mainThreadActionOwnerThreadId != ownerThreadId ||
            !PendingMainThreadActions.TryDequeue(out pending)) {
          return processed;
        }
      }

      try {
        pending.Action.Invoke();
        pending.Completion?.TrySetResult();
      }
      catch (Exception exception) {
        if (pending.Completion is null) {
          throw;
        }

        pending.Completion.TrySetException(exception);
      }

      processed++;
    }
  }
  public static void RegisterWorldLoadHandler(Action handler) {
    RegisterWorldLoadHandler(handler, static () => { });
  }

  /// <summary>
  /// Registers world loading and a callback for runtime world-storage resets.
  /// </summary>
  public static void RegisterWorldLoadHandler(Action handler, Action resetWorldStorage) {
    ArgumentNullException.ThrowIfNull(handler);
    ArgumentNullException.ThrowIfNull(resetWorldStorage);
    var registration = new WorldLoadHandlerRegistration(handler, resetWorldStorage);
    if (Interlocked.CompareExchange(
          ref _worldLoadHandlerRegistration,
          registration,
          null) is not null) {
      throw new InvalidOperationException("A world-load handler is already registered.");
    }
  }
  public static void LoadWorld() {
    WorldLoadHandlerRegistration registration = Volatile.Read(ref _worldLoadHandlerRegistration);
    if (registration is null) {
      throw new NotSupportedException("Loading worlds requires a registered world storage adapter.");
    }
    registration.LoadWorld.Invoke();
  }
  public static Task RunOnMainThread(Action action) {
    ArgumentNullException.ThrowIfNull(action);
    int ownerThreadId = Volatile.Read(ref _mainThreadActionOwnerThreadId);
    if (ownerThreadId == 0 || Environment.CurrentManagedThreadId == ownerThreadId) {
      action.Invoke();
      return Task.CompletedTask;
    }

    var completion = new TaskCompletionSource(
      TaskCreationOptions.RunContinuationsAsynchronously);
    if (TryQueueMainThreadAction(action, completion)) {
      return completion.Task;
    }

    action.Invoke();
    return Task.CompletedTask;
  }

  private static bool TryQueueMainThreadAction(
    Action action,
    TaskCompletionSource completion) {
    lock (MainThreadActionGate) {
      if (_mainThreadActionOwnerThreadId == 0) {
        return false;
      }

      PendingMainThreadActions.Enqueue(new PendingMainThreadAction(action, completion));
      return true;
    }
  }

  private static void ReleaseMainThreadActionQueue(int ownerThreadId) {
    List<TaskCompletionSource> canceledCompletions = new();
    lock (MainThreadActionGate) {
      if (_mainThreadActionOwnerThreadId != ownerThreadId) {
        throw new InvalidOperationException("The main-thread action queue binding is stale.");
      }

      _mainThreadActionOwnerThreadId = 0;
      while (PendingMainThreadActions.TryDequeue(out PendingMainThreadAction pending)) {
        if (pending.Completion is not null) {
          canceledCompletions.Add(pending.Completion);
        }
      }
    }

    foreach (TaskCompletionSource completion in canceledCompletions) {
      completion.TrySetCanceled();
    }
  }
  public static void FixUIScale() { }
  	public static double starGameMath(double value = 1.0)
{
	
		if (!starGame)
		{
			return 1.0;
		}
		double num = (double)starsHit / 200.0;
		if (num > 1.0)
		{
			num = 1.0;
		}
		return 1.0 + num * value;
	
	}
}
