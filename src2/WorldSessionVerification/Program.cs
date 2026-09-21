using Terraria.WorldSession.Components;
using Terraria.WorldSession.Definitions;
using Terraria.WorldSession.Queries;
using Terraria.WorldSession.Runtime;
using Terraria.WorldSession.Seeds;
using Terraria.WorldSession.Session;
using Terraria.WorldInteraction.Components;

static class Program
{
  private static int Main()
  {
    TestFrameActivity();
    TestBootstrapAndSeedQueries();
    TestFrameTimingAndTimeSkip();
    TestReadinessAndCatalog();
    TestHostProgressAndShutdown();
    TestDifficultyAndSessionQueries();
    TestManEaterIndex();
    TestPersistenceAndReplicationBoundary();
    Console.WriteLine("P01 verifier passed.");
    return 0;
  }

  private static void TestFrameActivity()
  {
    var state = new FrameActivityStateComponent();
    var system = new FrameActivityCommitSystem();
    system.BeginFrame(state);
    system.Record(state, new FrameActivityContribution(2, 1, true, true));
    system.Commit(state);

    FrameActivitySnapshot snapshot = FrameActivityQuery.Read(state);
    Require(snapshot.ActivePlayerCount == 2, "active player contribution was not committed");
    Require(snapshot.SleepingPlayerCount == 1, "sleeping player contribution was not committed");
    Require(snapshot.AnyActiveBoss && snapshot.HadActiveInteractableProjectile,
      "boolean activity contributions were not committed");
  }

  private static void TestBootstrapAndSeedQueries()
  {
    var state = new RuntimeBootstrapAndWorldRuleStateComponent();
    state.Commit(2, new SecretSeedFlags(
      true,
      false,
      true,
      false,
      false,
      false,
      false,
      false,
      false,
      false,
      false,
      false,
      false));

    Require(state.MapDelayTicks == 2, "map delay must be committed");
    SpecialSeedRuleInput input = new(state.SecretSeedFlags, false, false);
    Require(SpecialSeedEligibilityQuery.ShouldDropExtraGel(input),
      "special seed query must preserve extra gel rule");
    Require(input.OnlyShimmerOceanWorlds,
      "shimmer-ocean query must preserve seed precedence");
  }

  private static void TestFrameTimingAndTimeSkip()
  {
    var timing = new FrameTimingAndSchedulingStateComponent();
    new FrameTimingSystem().Update(timing, new FrameTimingInput(3661.5, true));
    Require(Math.Abs(timing.WrappedHour - 61.5f) < 0.001f,
      "wrapped hour must be derived from frame time");
    Require(timing.GlobalTimerPaused, "global timer pause must be explicit");

    var scheduler = new DeferredProcessSchedulerPort();
    scheduler.Enqueue(_ => DeferredProcessResult.Completed, DeferredProcessLifetime.Frame);
    Require(scheduler.Drain(FrameScope.Frame).Executed == 1,
      "frame-scoped deferred work must run once");
    Require(scheduler.Drain(FrameScope.Frame).Executed == 0,
      "completed deferred work must not repeat");

    var skip = new WorldTimeSkipStateComponent();
    var skipSystem = new WorldTimeSkipSystem();
    Require(skipSystem.RequestDawn(skip), "first dawn request must be accepted");
    Require(!skipSystem.RequestDawn(skip), "cooldown must reject duplicate dawn request");
    WorldTimeSkipSnapshot consumed = skipSystem.Consume(skip);
    Require(consumed.FastForwardToDawn && !skip.FastForwardToDawn,
      "time skip intent must be consumed and cleared");
  }

  private static void TestReadinessAndCatalog()
  {
    var readiness = new SessionReadinessComponent();
    var readinessSystem = new SessionReadinessSystem();
    readinessSystem.BeginLoading(readiness);
    Require(!readinessSystem.TryMarkReady(readiness, loadedEverything: true, generationActive: true),
      "readiness must reject an active generation barrier");
    Require(readinessSystem.TryMarkReady(readiness, loadedEverything: true, generationActive: false),
      "readiness must accept a completed load");
    Require(SessionReadinessQuery.IsEntityUpdateAllowed(readiness),
      "ready session should allow entity updates");

    var catalog = new WorldCatalogAdapter();
    WorldCatalogProjection projection = catalog.CreateProjection(new[]
    {
      new WorldCatalogEntry("zeta", "zeta.wld", 2),
      new WorldCatalogEntry("alpha", "alpha.wld", 1)
    });
    Require(projection.Entries[0].Name == "alpha", "world catalog must sort by stable name");
  }

  private static void TestHostProgressAndShutdown()
  {
    var progress = new RuntimeLoadProgressComponent();
    var progressSystem = new RuntimeLoadProgressSystem();
    progressSystem.Begin(progress, totalWorkUnits: 3);
    progressSystem.Advance(progress, 2);
    Require(progress.CompletedWorkUnits == 2 && !progress.IsComplete,
      "load progress must be monotonic and incomplete");
    progressSystem.Advance(progress, 5);
    Require(progress.IsComplete, "load progress must complete at its declared total");

    var shutdown = new HostShutdownSystem();
    shutdown.Submit(new ShutdownRequestCommand("test"));
    Require(shutdown.TryConsume(out ShutdownRequestCommand? request) && request!.Reason == "test",
      "shutdown command must cross the host boundary once");
    Require(!shutdown.TryConsume(out _), "shutdown command must be one-shot");

    var parameters = new LaunchParameterAdapter(new Dictionary<string, string>
    {
      ["-seed"] = "abc"
    });
    Require(parameters.TryGet("-seed", out string? seed) && seed == "abc",
      "launch parameters must expose a validated read-only view");
  }

  private static void TestDifficultyAndSessionQueries()
  {
    Require(Math.Abs(DifficultyRuleDefinition.EnemyDamageMultiplier.Sample(3f) - 3f) < 0.001f,
      "difficulty curve must preserve the master key");
    Require(DifficultyQuery.IsMasterOrAbove(3f) && DifficultyQuery.IsExpertOrAbove(2f),
      "difficulty threshold queries must be inclusive");

    var rules = new WorldSessionRuleSnapshot(
      1,
      false,
      true,
      true,
      false,
      false,
      false,
      false);
    Require(WorldSessionRuleQuery.GameMode(rules) == 1,
      "world session game mode query must read committed metadata");
    Require(WorldSessionRuleQuery.SurviveHardcoreDeath(rules),
      "hardcore death query must preserve Version4 precedence");
    Require(WorldSurfaceQuery.NoFunctionalSurface(30),
      "surface query must identify worlds below the functional threshold");
    Require(WorldGeometryQuery.UnderworldLayer(1000) == 800,
      "underworld layer must derive from world height");
  }

  private static void TestManEaterIndex()
  {
    var index = new ManEaterProtectionIndexComponent();
    var system = new ManEaterProtectionSystem();
    system.BeginFrame(index);
    system.ProtectSpot(index, 10, 20);
    system.ProtectSpot(index, 10, 20);
    Require(system.SpotProtected(index, 10, 20), "protected spot must be queryable");
    system.BeginFrame(index);
    Require(!system.SpotProtected(index, 10, 20), "spatial protection must clear at frame boundary");
  }

  private static void TestPersistenceAndReplicationBoundary()
  {
    var snapshot = new WorldSessionCommittedSnapshot
    {
      Metadata = new ActiveWorldMetadataProjection(7, 1, "alpha", "alpha.wld"),
      SecretSeedFlags = new SecretSeedFlags(
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false),
      Hardmode = new HardmodeSnapshot(true)
    };

    var persistence = new WorldPersistenceAdapter();
    WorldPersistenceResult result = persistence.Commit(snapshot);
    Require(result.Accepted && ReferenceEquals(persistence.LastCommittedSnapshot, snapshot),
      "persistence adapter must expose only the committed snapshot");

    var projection = new WorldSessionReplicationProjection();
    Require(ReferenceEquals(projection.Create(persistence.LastCommittedSnapshot), snapshot),
      "replication projection must be downstream of the committed snapshot");
    persistence.Rollback();
    Require(persistence.LastCommittedSnapshot is null,
      "persistence rollback must clear the committed snapshot");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }
}
