using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Modules.Events;
using MinimalImplementation.Persistence;
using MinimalImplementation.Server.Host;
using MinimalImplementation.Server.Mechanisms;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Verification.ServerTick;

internal static class Program
{
  private static async Task<int> Main(string[] args)
  {
    string? scenario = ReadOption(args, "--scenario");
    if (!string.Equals(scenario, "SC093", StringComparison.Ordinal) &&
        !string.Equals(scenario, "SC094", StringComparison.Ordinal))
    {
      Console.Error.WriteLine("Only the fixed scenarios SC093 and SC094 are enabled by this verifier.");
      return 2;
    }

    string outputRoot = ReadOption(args, "--output-root") ?? Path.Combine(
      "Build",
      "diagnostics",
      "MinimalImplementation",
      scenario!,
      DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
    Directory.CreateDirectory(outputRoot);

    try
    {
      string evidenceFileName;
      string evidence;
      if (string.Equals(scenario, "SC093", StringComparison.Ordinal))
      {
        await RunRealCompositeAsync().ConfigureAwait(false);
        evidenceFileName = "SC093-server-tick-composite-result.md";
        evidence =
          "SC093 PASS: WorldTickPhaseCompositeOwner advanced WorldTickCoordinator once, " +
          "rejected a duplicate tick, retained NotReady for missing input, and rejected duplicate " +
          "coordinator registration.";
      }
      else
      {
        await RunLeashedEntityOwnerAsync().ConfigureAwait(false);
        evidenceFileName = "SC094-leashed-entity-owner-result.md";
        evidence =
          "SC094 PASS: LeashedEntityServerTickPhaseOwner advanced the real " +
          "LeashedEntitySectionService through explicit motion and effect ports, committed one " +
          "partial state change, and published the resulting effect.";
      }

      string evidencePath = Path.Combine(outputRoot, evidenceFileName);
      File.WriteAllText(
        evidencePath,
        evidence);
      Console.WriteLine($"{scenario} PASS evidence={Path.GetFullPath(evidencePath)}");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine($"SC093 FAIL {exception.GetType().Name}: {exception.Message}");
      return 1;
    }
  }

  private static Task RunRealCompositeAsync()
  {
    WorldFacts facts = WorldFacts.CreateVersion4();
    WorldGeometry geometry = new(600, 450);
    WorldHeaderSnapshot header = new(
      "SC093 composite world",
      geometry,
      new TileCoordinate(300, 225),
      Guid.Parse("D8A4F7B1-4F4B-4F1A-9B8A-2E6E9B7A3093"),
      dayTime: true,
      time: 1234,
      moonPhase: 2,
      worldId: 26093,
      CreateHeaderFacts());
    var owner = new WorldOwner(facts);
    WorldPreparedWorld prepared = owner.PrepareWorld(
      new WorldPreparationRequest(
        header,
        Enumerable.Repeat(WorldTile.Empty, geometry.TileCount).ToArray(),
        facts.Fingerprint));
    WorldValidationResult validation = owner.ValidatePreparedWorld(prepared);
    Require(validation.IsValid, string.Join("; ", validation.Errors));
    WorldPublicationResult publication = owner.CommitPreparedWorld(prepared);
    Require(publication.Published, publication.Rejection ?? "The world was not published.");

    ulong worldEpoch = publication.WorldEpoch;
    var coordinator = new WorldTickCoordinator(owner);
    var compositeInvasion = new InvasionState(worldScope: 1);
    var compositeBridge = new WorldTickInvasionBridge(compositeInvasion, stableWorldScope: 1);
    var composite = new WorldTickPhaseCompositeOwner(
      coordinator,
      _ => new WorldTickPhaseInput(
        worldEpoch,
        ElapsedGameTicks: 1,
        DayRate: 1,
        LiquidOperationBudget: 8),
      compositeBridge);
    Require(composite.Participants.Count == 2, "The composite did not declare Time and World.");
    Require(
      composite.Participants.All(participant =>
        ReferenceEquals(participant.TransactionIdentity, composite.TransactionIdentity)),
      "Composite participants do not share the coordinator identity.");

    WorldSnapshot before = owner.CaptureSnapshot();
    composite.Advance(0);
    WorldSnapshot after = owner.CaptureSnapshot();
    Require(
      after.Version.Value == before.Version.Value + 1,
      "The Time/World composite did not publish exactly one world commit.");
    Require(
      compositeInvasion.CaptureSnapshot().Version == CommitVersion.Initial,
      "A successful tick without a morning transition advanced invasion state.");
    RequireThrows<InvalidOperationException>(
      () => composite.Advance(0),
      "A repeated game tick was accepted by the shared coordinator.");

    WorldTickPhaseCompositeOwner notReady = new(
      new WorldTickCoordinator(owner),
      static _ => null);
    RequireThrows<InvalidOperationException>(
      () => notReady.Advance(1),
      "The composite invented a successful tick without authoritative input.");

    IServerTickPhaseOwner[] ordinaryOwners = Enum.GetValues<ServerTickPhase>()
      .Where(phase => phase != ServerTickPhase.Time && phase != ServerTickPhase.World)
      .Select(phase => (IServerTickPhaseOwner)new RecordingOwner(phase))
      .ToArray();
    IServerTickPhaseOwner[] owners = ordinaryOwners.Append(composite).ToArray();
    _ = new WorldTickScheduler(
      new InlineCommitQueue(),
      owners,
      hasActiveClients: static () => true);

    var duplicateComposite = new WorldTickPhaseCompositeOwner(
      coordinator,
      _ => new WorldTickPhaseInput(worldEpoch, 1, 1, 8));
    RequireThrows<ArgumentException>(
      () => _ = new WorldTickScheduler(
        new InlineCommitQueue(),
        owners.Append(duplicateComposite).ToArray(),
        hasActiveClients: static () => true),
      "The scheduler accepted the same WorldTickCoordinator twice.");
    RunInvasionBridgeChecks();
    return Task.CompletedTask;
  }

  private static void RunInvasionBridgeChecks()
  {
    (WorldOwner owner, ulong worldEpoch) = CreatePublishedTickWorld(
      dayTime: false,
      time: WorldTimeState.NightLength,
      moonPhase: 7);
    var coordinator = new WorldTickCoordinator(owner);
    var invasion = new InvasionState(worldScope: 1);
    var bridge = new WorldTickInvasionBridge(invasion, stableWorldScope: 1);

    InvasionSnapshot beforeRejected = invasion.CaptureSnapshot();
    WorldTickResult rejected = coordinator.Advance(
      worldEpoch,
      tick: 0,
      elapsedGameTicks: 1,
      dayRate: 1,
      liquidOperationBudget: 0);
    Require(!rejected.Applied, "The bridge fixture did not produce a rejected tick.");
    bridge.Consume(rejected);
    Require(invasion.CaptureSnapshot() == beforeRejected,
      "A rejected world tick changed the invasion snapshot.");

    WorldTickResult morning = coordinator.Advance(
      worldEpoch,
      tick: 0,
      elapsedGameTicks: 1,
      dayRate: 1,
      liquidOperationBudget: 8);
    Require(morning.Applied && morning.MorningTransitionCount == 1 &&
        morning.Version != morning.SourceVersion,
      "The bridge fixture did not produce one morning with distinct source and commit versions.");
    bridge.Consume(morning);
    InvasionSnapshot afterMorning = invasion.CaptureSnapshot();
    Require(invasion.AdvanceMorning(1, morning.Version).Status ==
        WorldEventOutcomeStatus.Duplicate,
      "The bridge did not use the committed world Version as its invasion source cursor.");
    bridge.Consume(morning);
    Require(invasion.CaptureSnapshot() == afterMorning,
      "Re-consuming one committed morning changed invasion state twice.");

    (WorldOwner dayOwner, ulong dayWorldEpoch) = CreatePublishedTickWorld(
      dayTime: true,
      time: 0,
      moonPhase: 2);
    var dayInvasion = new InvasionState(worldScope: 1);
    var dayBridge = new WorldTickInvasionBridge(dayInvasion, stableWorldScope: 1);
    WorldTickResult noMorning = new WorldTickCoordinator(dayOwner).Advance(
      dayWorldEpoch,
      tick: 0,
      elapsedGameTicks: 1,
      dayRate: 1,
      liquidOperationBudget: 8);
    Require(noMorning.Applied && !noMorning.MorningTransition,
      "The bridge fixture did not produce an applied no-morning tick.");
    InvasionSnapshot beforeNoMorning = dayInvasion.CaptureSnapshot();
    dayBridge.Consume(noMorning);
    Require(dayInvasion.CaptureSnapshot() == beforeNoMorning,
      "An applied tick without a morning changed the invasion snapshot.");

    var foreignInvasion = new InvasionState(worldScope: 2);
    var foreignBridge = new WorldTickInvasionBridge(foreignInvasion, stableWorldScope: 1);
    RequireThrows<InvalidOperationException>(
      () => foreignBridge.Consume(morning),
      "A morning result from another identity world scope was silently accepted.");
  }

  private static (WorldOwner Owner, ulong WorldEpoch) CreatePublishedTickWorld(
    bool dayTime,
    double time,
    byte moonPhase)
  {
    WorldFacts facts = WorldFacts.CreateVersion4();
    WorldGeometry geometry = new(3, 3);
    WorldHeaderSnapshot header = new(
      "SC093 bridge world",
      geometry,
      new TileCoordinate(1, 1),
      Guid.NewGuid(),
      dayTime,
      time,
      moonPhase,
      worldId: 26094,
      CreateHeaderFacts());
    var owner = new WorldOwner(facts);
    WorldPreparedWorld prepared = owner.PrepareWorld(
      new WorldPreparationRequest(
        header,
        Enumerable.Repeat(WorldTile.Empty, geometry.TileCount).ToArray(),
        facts.Fingerprint));
    WorldPublicationResult publication = owner.CommitPreparedWorld(prepared);
    Require(publication.Published, publication.Rejection ?? "The bridge world was not published.");
    return (owner, publication.WorldEpoch);
  }

  private static Task RunLeashedEntityOwnerAsync()
  {
    var identityRegistry = new EntityIdentityRegistry();
    var identityAuthority = new MechanismIdentityAuthority(identityRegistry);
    MechanismFactsCatalog facts = MechanismFactsCatalog.CreateVersion4();
    var registry = new LeashedEntityRegistry(
      facts,
      identityAuthority,
      identityAuthority.Create,
      identityRegistry.CurrentWorldScope);
    var section = new MechanismSectionCoordinate(0, 0);
    LeashedEntityChangeResult created = registry.ApplyFull(
      new Version4ModulePayloads.LeashedEntityMessage(
        Action: 1,
        Slot: 1,
        EntityType: 1,
        AnchorX: 10,
        AnchorY: 10,
        State: new Version4ModulePayloads.KiteLeashedEntityState(
          ProjectileType: 1,
          PositionX: 1,
          PositionY: 2,
          PackedVelocity: 0,
          Rotation: 0,
          WindTarget: 0,
          CloudAlpha: 0,
          TimeCounter: 0)),
      section);
    Require(created.Kind == LeashedEntityChangeKind.Created,
      "The leashed fixture did not create its authoritative registry entry.");

    var sectionService = new LeashedEntitySectionService(registry);
    LeashedEntitySectionService.SectionActivationResult activation =
      sectionService.ActivateSection(section, tick: 0, static (entity, tick) => {
        var fullState = (Version4ModulePayloads.KiteLeashedEntityState)entity.State;
        return new LeashedEntitySpawnState(
          fullState,
          new LeashedEntityMotionState(
            new Vector2(fullState.PositionX, fullState.PositionY),
            Vector2.Zero,
            SpriteDirection: 0,
            LastTick: tick));
      });
    Require(activation.WasActivated && activation.FullEntities.Count == 1,
      "The leashed fixture did not activate its authoritative section.");

    var effects = new RecordingLeashedEntityEffects();
    var owner = new LeashedEntityServerTickPhaseOwner(
      sectionService,
      new AdvancingLeashedEntityMotion(),
      effects);
    owner.Advance(1);

    LeashedEntityRuntimeSnapshot updated = registry.Capture().Single();
    Require(updated.State is Version4ModulePayloads.KiteLeashedEntityState kite &&
      kite.PositionX == 2 && kite.PositionY == 3 && updated.Motion?.LastTick == 1,
      "The leashed owner did not commit the motion-port partial state.");
    Require(effects.Last is not null && effects.Last.StateChanges.Count == 1,
      "The leashed owner did not publish the committed section effect.");
    return Task.CompletedTask;
  }

  private static WorldHeaderFacts CreateHeaderFacts()
  {
    return new WorldHeaderFacts(
      seedText: "SC093-composite",
      worldGeneratorVersion: 1,
      leftWorld: 0,
      rightWorld: 9600,
      topWorld: 0,
      bottomWorld: 7200,
      gameMode: 0,
      drunkWorld: false,
      getGoodWorld: false,
      tenthAnniversaryWorld: false,
      dontStarveWorld: false,
      notTheBeesWorld: false,
      remixWorld: false,
      noTrapsWorld: false,
      zenithWorld: false,
      skyblockWorld: false,
      creationTime: null,
      lastPlayed: null,
      moonType: 0,
      treeX: [100, 300, 500],
      treeStyle: [0, 1, 2, 3],
      caveBackX: [120, 300, 480],
      caveBackStyle: [0, 1, 2, 3],
      iceBackStyle: 0,
      jungleBackStyle: 0,
      hellBackStyle: 0,
      worldSurface: 180,
      rockLayer: 300,
      bloodMoon: false,
      eclipse: false,
      dungeonX: 500,
      dungeonY: 250,
      crimson: false);
  }

  private static string? ReadOption(IReadOnlyList<string> args, string option)
  {
    for (int index = 0; index + 1 < args.Count; index++)
    {
      if (string.Equals(args[index], option, StringComparison.Ordinal))
      {
        return args[index + 1];
      }
    }

    return null;
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void RequireThrows<TException>(Action action, string message)
    where TException : Exception
  {
    try
    {
      action();
    }
    catch (TException)
    {
      return;
    }

    throw new InvalidOperationException(message);
  }

  private sealed class InlineCommitQueue : IWorldCommitQueue
  {
    public Task ExecuteAsync(Action commit, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      commit();
      return Task.CompletedTask;
    }
  }

  private sealed class RecordingOwner(ServerTickPhase phase) : IServerTickPhaseOwner
  {
    public ServerTickPhase Phase { get; } = phase;

    public object TransactionIdentity => this;

    public void Advance(long gameTickIndex)
    {
      ArgumentOutOfRangeException.ThrowIfNegative(gameTickIndex);
    }
  }

  private sealed class AdvancingLeashedEntityMotion : ILeashedEntityMotionPort
  {
    public bool TryAdvance(
      LeashedEntityRuntimeSnapshot entity,
      long tick,
      out LeashedEntityMotionStep? step)
    {
      Require(tick == 1, "The leashed motion port received the wrong game tick.");
      Require(entity.State is Version4ModulePayloads.KiteLeashedEntityState,
        "The leashed motion port received an unexpected prototype state.");
      var position = new Vector2(2, 3);
      var partialState = new Version4ModulePayloads.KiteLeashedEntityState(
        ProjectileType: null,
        PositionX: position.X,
        PositionY: position.Y,
        PackedVelocity: 0,
        Rotation: 0,
        WindTarget: 0,
        CloudAlpha: 0,
        TimeCounter: 1);
      step = new LeashedEntityMotionStep(
        partialState,
        new LeashedEntityMotionState(position, Vector2.Zero, SpriteDirection: 0, LastTick: tick));
      return true;
    }
  }

  private sealed class RecordingLeashedEntityEffects : ILeashedEntityTickEffectPort
  {
    public LeashedEntitySectionService.SectionTickResult? Last { get; private set; }

    public void Publish(LeashedEntitySectionService.SectionTickResult result)
    {
      Last = result;
    }
  }
}
