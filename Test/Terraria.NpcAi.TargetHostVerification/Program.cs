using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using EntityEcs;
using EntityEcs.Components;
using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.IO;
using NSSLC.WorldGeneration.Utilities;
using Terraria.Content;
using Terraria.Npc;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.SimulationHost;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.Projectile;
using Terraria.SpatialSimulation.Components;
using Terraria.WorldStorage;
using RuntimeMain = NSSLC.WorldGeneration.Main;

internal static class Program
{
  private static int Main(string[] args)
  {
    if (args.Length != 2)
    {
      Console.Error.WriteLine(
        "Usage: <world.wld> <report.json>");
      return 2;
    }

    string worldPath = Path.GetFullPath(args[0]);
    string reportPath = Path.GetFullPath(args[1]);
    if (!File.Exists(worldPath))
    {
      Console.Error.WriteLine($"World file does not exist: {worldPath}");
      return 2;
    }

    string sourceWorldHash = HashFile(worldPath);
    var evidence = new List<string>();
    Exception? failure = null;
    bool succeeded = false;
    RuntimeNpcStore? npcs = null;
    RuntimePlayerStore? players = null;
    LoadedWorldSession? session = null;

    try
    {
      RunDomainVerification(evidence);
      RunProductionHostVerification(
        worldPath,
        sourceWorldHash,
        evidence,
        out npcs,
        out players,
        out session);
      succeeded = true;
    }
    catch (Exception exception)
    {
      failure = exception;
      Console.Error.WriteLine(exception);
    }
    finally
    {
      try
      {
        players?.Dispose();
        npcs?.Dispose();
        (session ?? WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession)?.Dispose();
      }
      catch (Exception cleanupException)
      {
        succeeded = false;
        failure ??= cleanupException;
        Console.Error.WriteLine(cleanupException);
      }

      string finalWorldHash = HashFile(worldPath);
      bool sourceWorldUnchanged = sourceWorldHash == finalWorldHash;
      if (!sourceWorldUnchanged)
      {
        succeeded = false;
        failure ??= new InvalidOperationException(
          "The target host verifier changed its source world file.");
      }

      WriteReport(
        reportPath,
        worldPath,
        sourceWorldHash,
        finalWorldHash,
        sourceWorldUnchanged,
        succeeded,
        failure,
        evidence);
    }

    if (succeeded)
    {
      Console.WriteLine($"PASS: NPC target selection and production host adapter; report {reportPath}");
      return 0;
    }

    Console.Error.WriteLine($"FAIL: NPC target selection host verification; report {reportPath}");
    return 1;
  }

  private static void RunDomainVerification(List<string> evidence)
  {
    NpcTargetGeometrySnapshot source = Geometry(100f, 100f, 24, 18);
    NpcPlayerTargetSnapshot playerA = Player(0, Geometry(80f, 100f, 20, 40));
    NpcPlayerTargetSnapshot playerB = Player(1, Geometry(120f, 100f, 20, 40));
    NpcTargetSelectionResult sourceScoring = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [playerA, playerB]);
    Require(sourceScoring.LegacyTargetIndex == 0,
      "Normal uses the literal source score instead of geometric center distance.");
    Require(sourceScoring.Score == 31f,
      $"Source score is 31, actual {sourceScoring.Score}.");
    evidence.Add("normal-source-score-counterexample: target=0 score=31; geometric-center ordering would choose slot 1");

    NpcTargetSelectionResult tied = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [
        Player(3, Geometry(70f, 71f, 20, 40)),
        Player(9, Geometry(86f, 71f, 20, 40)),
      ]);
    Require(tied.LegacyTargetIndex == 3, "Strict less-than preserves the first equal-score player.");
    evidence.Add("strict-tie-order: first slot retained");

    NpcTargetSelectionResult aggro = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [
        Player(0, Geometry(100f, 100f, 20, 40)),
        Player(1, Geometry(130f, 100f, 20, 40), aggro: 100),
      ]);
    Require(aggro.LegacyTargetIndex == 1, "Player aggro changes the normal selection score.");
    NpcTargetSelectionResult noAggroFacing = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [
        Player(0, Geometry(100f, 100f, 20, 40), noAggro: true),
        Player(1, Geometry(120f, 100f, 20, 40)),
      ]);
    Require(noAggroFacing.LegacyTargetIndex == 1,
      "NPC type-specific no-aggro adds the source 1000-point penalty when direction is nonzero.");
    NpcTargetSelectionResult noAggroIdle = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [
        Player(0, Geometry(100f, 100f, 20, 40), noAggro: true),
        Player(1, Geometry(120f, 100f, 20, 40)),
      ],
      direction: 0);
    Require(noAggroIdle.LegacyTargetIndex == 0,
      "No-aggro penalty is absent when source direction is zero.");

    NpcTargetSelectionResult normalGross = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [
        Player(0, Geometry(100f, 100f, 20, 40), gross: false),
        Player(1, Geometry(120f, 100f, 20, 40), gross: true),
      ]);
    NpcTargetSelectionResult wallOfFleshGross = Select(
      NpcTargetSelectionStrategy.WallOfFlesh,
      source,
      [
        Player(0, Geometry(100f, 100f, 20, 40), gross: false),
        Player(1, Geometry(120f, 100f, 20, 40), gross: true),
      ]);
    Require(normalGross.LegacyTargetIndex == 0 && wallOfFleshGross.LegacyTargetIndex == 1,
      "Wall of Flesh selection filters on Gross while Normal selection does not.");
    NpcTargetSelectionResult inactiveFilters = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [
        Player(0, Geometry(100f, 100f, 20, 40), ghost: true),
        Player(1, Geometry(110f, 100f, 20, 40), dead: true),
        Player(2, Geometry(130f, 100f, 20, 40)),
      ]);
    Require(inactiveFilters.LegacyTargetIndex == 2,
      "Inactive, dead, and ghost Player owners are excluded from candidate selection.");
    evidence.Add("player-owner-filters: Aggro, NPC-type NoAggro, Gross, dead, and ghost cases pass");

    NpcPlayerTargetSnapshot facingPlayer = Player(
      0,
      Geometry(0f, 100f, 20, 40),
      aggro: -100,
      itemAnimation: 0);
    NpcTargetSelectionResult preservesDirection = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [facingPlayer],
      oldTarget: 2,
      direction: 1,
      directionY: -1,
      oldDirection: 1,
      oldDirectionY: -1);
    Require(preservesDirection.Direction == 1 && preservesDirection.DirectionY == -1,
      "A negative-aggro Player with no item animation retains historical direction for non-boss NPCs.");
    NpcTargetSelectionResult bossFaces = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [facingPlayer],
      oldTarget: 2,
      direction: 1,
      directionY: -1,
      oldDirection: 1,
      oldDirectionY: -1,
      boss: true);
    Require(bossFaces.Direction == -1 && bossFaces.DirectionY == 1,
      "Boss status disables the historical direction hold.");
    NpcTargetSelectionResult animationFaces = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [facingPlayer with { ItemAnimation = 1 }],
      oldTarget: 2,
      direction: 1,
      directionY: -1,
      oldDirection: 1,
      oldDirectionY: -1);
    Require(animationFaces.Direction == -1 && animationFaces.DirectionY == 1,
      "Active item animation disables the historical direction hold.");

    NpcTargetSelectionResult confused = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [Player(0, Geometry(0f, 0f, 20, 40))],
      confused: true);
    NpcTargetSelectionResult notConfused = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [Player(0, Geometry(0f, 0f, 20, 40))]);
    Require(confused.Direction == -notConfused.Direction &&
            confused.DirectionY == notConfused.DirectionY,
      "Confusion reverses horizontal direction without changing vertical direction.");

    NpcTargetSelectionResult netUpdate = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [Player(0, Geometry(80f, 100f, 20, 40))],
      oldTarget: 1,
      oldDirection: 1,
      oldDirectionY: 1,
      faceTarget: false);
    Require(netUpdate.NetUpdateRequested,
      "Normal selection requests a network update when target changes without collision.");
    Require(!Select(
        NpcTargetSelectionStrategy.Normal,
        source,
        [Player(0, Geometry(80f, 100f, 20, 40))],
        oldTarget: 1,
        oldDirection: 1,
        oldDirectionY: 1,
        collideX: true,
        faceTarget: false).NetUpdateRequested,
      "Horizontal collision suppresses the network update request.");
    Require(!Select(
        NpcTargetSelectionStrategy.Normal,
        source,
        [Player(0, Geometry(80f, 100f, 20, 40))],
        oldTarget: 1,
        oldDirection: 1,
        oldDirectionY: 1,
        collideY: true,
        faceTarget: false).NetUpdateRequested,
      "Vertical collision suppresses the network update request.");
    evidence.Add("history-and-commit-gates: old direction, Boss, confusion, item animation, and collision checks pass");

    var tankPet = new NpcTankPetTargetSnapshot(
      12,
      Geometry(100.7f, 100.8f, 10, 10),
      CanHit: true)
    {
      OwnerSlot = 17,
    };
    NpcTargetSelectionResult normalPet = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [Player(0, Geometry(150f, 120f, 20, 40), tankPet: tankPet)]);
    Require(normalPet.TargetKind == NpcTargetKind.PlayerTankPet &&
            normalPet.LegacyTargetIndex == 0 &&
            normalPet.SecondaryLegacySlot == 12,
      "Normal pet selection keeps the selected Player target and projectile slot separately.");
    Require(normalPet.TargetGeometry.Position == new Vector2(100f, 100f),
      "Normal target rectangle coordinates are truncated to integers.");
    NpcTargetSelectionResult blockedPet = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [Player(0, Geometry(150f, 120f, 20, 40), tankPet: tankPet with { CanHit = false })]);
    NpcTargetSelectionResult unknownPet = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [Player(0, Geometry(150f, 120f, 20, 40), tankPet: tankPet with { CanHit = null })]);
    Require(blockedPet.TargetKind == NpcTargetKind.Player &&
            unknownPet.TargetKind == NpcTargetKind.Player,
      "False and unavailable CanHit facts do not select a tank pet.");
    NpcTargetSelectionResult wallOfFleshPet = Select(
      NpcTargetSelectionStrategy.WallOfFlesh,
      source,
      [
        Player(0, Geometry(150f, 120f, 20, 40), gross: false, tankPet: tankPet),
        Player(1, Geometry(130f, 100f, 20, 40), gross: true),
      ]);
    Require(wallOfFleshPet.TargetKind == NpcTargetKind.Player &&
            wallOfFleshPet.LegacyTargetIndex == 1,
      "Wall of Flesh ignores a pet owned by a non-Gross player.");
    NpcTargetSelectionResult noAggroPet = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [Player(0, Geometry(150f, 120f, 20, 40), noAggro: true, tankPet: tankPet)],
      direction: 0);
    Require(noAggroPet.TargetKind == NpcTargetKind.Player,
      "NoAggro suppresses a tank-pet candidate even when NPC direction is zero.");
    evidence.Add("normal-and-wof-pet-rules: CanHit, owner target preservation, geometry, Gross, and NoAggro cases pass");

    NpcTargetSelectionResult currentTargetFallback = Select(
      NpcTargetSelectionStrategy.Normal,
      source,
      [
        Player(0, Geometry(0f, 0f, 20, 40), active: false),
        Player(1, Geometry(10.9f, 20.8f, 20, 40), dead: true),
      ],
      oldTarget: 1,
      currentTarget: 1,
      oldDirection: 1,
      oldDirectionY: 1);
    Require(currentTargetFallback.ShouldCommit && currentTargetFallback.HasTarget &&
            currentTargetFallback.LegacyTargetIndex == 1 &&
            currentTargetFallback.TargetGeometry.Position == new Vector2(10f, 20f),
      "Normal no-candidate selection commits the usable current target and its truncated geometry.");
    NpcTargetSelectionResult invalidTargetFallback = Select(
      NpcTargetSelectionStrategy.WallOfFlesh,
      source,
      [Player(0, Geometry(30f, 40f, 20, 40), active: false, gross: false)],
      oldTarget: 255,
      currentTarget: 255);
    Require(invalidTargetFallback.ShouldCommit && invalidTargetFallback.LegacyTargetIndex == 0,
      "Wall of Flesh no-candidate selection resets an out-of-range target to slot zero.");
    evidence.Add("normal-and-wof-fallback: current-target retention and invalid-target reset pass");

    NpcTargetSelectionResult upgradedNpc = Select(
      NpcTargetSelectionStrategy.Upgraded,
      source,
      [],
      [new NpcNpcTargetSnapshot(7, 548, Geometry(180.8f, 100.5f, 20, 30), IsActive: true)]);
    Require(upgradedNpc.TargetKind == NpcTargetKind.Npc &&
            upgradedNpc.LegacyTargetIndex == 307 &&
            upgradedNpc.TargetGeometry.Position == new Vector2(180f, 100f),
      "Upgraded NPC candidates use TypeId 548, targeting index slot+300, and hitbox geometry.");
    NpcTargetSelectionResult wrongNpcType = Select(
      NpcTargetSelectionStrategy.Upgraded,
      source,
      [],
      [new NpcNpcTargetSnapshot(7, 549, Geometry(180f, 100f, 20, 30), IsActive: true)]);
    Require(!wrongNpcType.ShouldCommit,
      "Upgraded ignores active NPCs whose TypeId is not 548.");
    var upgradedPet = new NpcTankPetTargetSnapshot(
      12,
      Geometry(100f, 100f, 10, 10),
      CanHit: true)
    {
      OwnerSlot = 17,
    };
    NpcTargetSelectionResult upgradedPetSelection = Select(
      NpcTargetSelectionStrategy.Upgraded,
      source,
      [Player(0, Geometry(500f, 500f, 20, 40), tankPet: upgradedPet)]);
    Require(upgradedPetSelection.TargetKind == NpcTargetKind.PlayerTankPet &&
            upgradedPetSelection.LegacyTargetIndex == 17,
      "Upgraded pet target comes from projectile OwnerSlot rather than its Player list entry.");
    NpcTargetSelectionResult upgradedPetOwnerFallback = Select(
      NpcTargetSelectionStrategy.Upgraded,
      source,
      [Player(4, Geometry(500f, 500f, 20, 40), tankPet: upgradedPet with { OwnerSlot = -1 })]);
    Require(upgradedPetOwnerFallback.TargetKind == NpcTargetKind.PlayerTankPet &&
            upgradedPetOwnerFallback.LegacyTargetIndex == 4 &&
            upgradedPetOwnerFallback.SecondaryLegacySlot == upgradedPet.ProjectileSlot,
      "Upgraded falls back to the containing Player slot when a target snapshot omits projectile OwnerSlot.");
    NpcTargetSelectionResult upgradedCheckPosition = Select(
      NpcTargetSelectionStrategy.Upgraded,
      source,
      [Player(0, Geometry(990f, 990f, 20, 40))],
      checkPosition: new Vector2(1000f, 1000f));
    Require(upgradedCheckPosition.LegacyTargetIndex == 0 &&
            upgradedCheckPosition.Direction == 1,
      "Upgraded checkPosition changes distance origin while direction uses the NPC center.");
    var upgradedState = new NpcTargetSelectionStateComponent();
    NpcTargetSelectionResult upgradedNoCandidate = NpcTargetSelectionSystem.SelectAndCommit(
      Inputs(NpcTargetSelectionStrategy.Upgraded, source, [], []),
      upgradedState);
    Require(!upgradedNoCandidate.ShouldCommit && upgradedState.LegacyTargetIndex == -1,
      "Upgraded no-candidate early return leaves target selection state unchanged.");
    evidence.Add("upgraded-rules: NPC 548, +300 encoding, checkPosition, owner slot, and no-candidate early return pass");
  }

  private static void RunProductionHostVerification(
    string worldPath,
    string sourceWorldHash,
    List<string> evidence,
    out RuntimeNpcStore? npcs,
    out RuntimePlayerStore? players,
    out LoadedWorldSession? loadedSession)
  {
    ContentCatalog catalog = SimulationContentBootstrap.Build();
    var identityRegistry = new EntityIdentityRegistry();
    var immunity = new ProjectileStaticNpcImmunityRegistryComponent(
      checked(catalog.Snapshot.Projectiles.MaximumTypeId + 1),
      RuntimeNpcStore.MaximumNpcCapacity);
    RuntimeNpcStore npcStore = new(immunity, identityRegistry);
    npcs = npcStore;
    players = null;
    loadedSession = null;
    var ioGate = new WorldStorageIoGate();
    WorldLoadRecoveryResult? loadResult = null;
    Exception? loadException = null;

    RuntimeMain.InitializeHeadlessRuntime();
    using IDisposable mainThreadQueue = RuntimeMain.BindMainThreadActionQueue();
    RuntimeMain.dedServ = true;
    RuntimeMain.netMode = 0;
    RuntimeMain.gameMenu = false;
    RuntimeMain.worldPathName = worldPath;
    RuntimeMain.rand = new UnifiedRandom(0x4E505442);
    new WorldFileData(Path.GetFileNameWithoutExtension(worldPath)).SetAsActive();

    WorldStorageCoordinatorFactory.RegisterGeneratedWorldLoadHandlerWithRuntimeTileMap(
      ioGate,
      (session, _) =>
      {
        npcStore.Hydrate(session, catalog);
        return WorldStorageOperationResult.Success;
      },
      static (_, _) => WorldStorageOperationResult.Success,
      static (_, _) => WorldStorageOperationResult.Success,
      resultObserver: result => loadResult = result,
      exceptionObserver: exception => loadException = exception,
      resetHostWorldState: _ =>
      {
        npcStore.Reset();
        return WorldStorageOperationResult.Success;
      },
      sessionFactory: () => new LoadedWorldSession(identityRegistry));

    WorldGen.serverLoadWorldCallBack();
    LoadedWorldSession? publishedSession =
      WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession as LoadedWorldSession;
    if (loadException is not null || loadResult?.Succeeded != true || publishedSession is null)
    {
      string loadDetails = loadResult is null
        ? "No recovery result was observed."
        : $"Action={loadResult.TerminalAction}; attempts={loadResult.LoadAttemptCount}; " +
          $"failure={loadResult.Failure.Kind}: {loadResult.Failure.Detail}; " +
          $"cleanup={loadResult.CleanupFailure.Kind}: {loadResult.CleanupFailure.Detail}; " +
          $"requiresReset={loadResult.RequiresWorldReset}; worldCleared={loadResult.WorldCleared}; " +
          FormatLastLoadOutcome(loadResult.LastLoadOutcome);
      evidence.Add($"production-world-load-failure: {loadDetails}");
      throw new InvalidOperationException(
        $"The production world loader did not publish the verification world. {loadDetails}",
        loadException);
    }
    LoadedWorldSession session = publishedSession;
    loadedSession = session;
    Require(HashFile(worldPath) == sourceWorldHash,
      "Production world load leaves the input world file unchanged.");
    evidence.Add("production-world-load: real world loaded; source hash unchanged");

    var runtimeItems = new RuntimeItemRegistry(catalog, session.EntityRuntime);
    var runtimePlayers = new RuntimePlayerStore(session.EntityRuntime, identityRegistry);
    runtimePlayers.Initialize(session, 2, catalog, runtimeItems);
    players = runtimePlayers;

    Vector2 npcPosition = new(
      session.World.Descriptor.SpawnTileX * 16f + 100f,
      session.World.Descriptor.SpawnTileY * 16f - 40f);
    Vector2 player0Position = npcPosition;
    Vector2 player1Position = npcPosition + new Vector2(100f, 0f);
    runtimePlayers.Players[0].Movement = new MovementStateComponent(player0Position);
    runtimePlayers.Players[1].Movement = new MovementStateComponent(player1Position);

    RuntimeNpcEntity SpawnHostNpc()
    {
      if (!npcStore.TrySpawn(
            netId: 1,
            position: npcPosition,
            catalog,
            session.World.Descriptor.WorldId,
            out RuntimeNpcEntity? spawnedNpc) ||
          spawnedNpc is null)
      {
        throw new InvalidOperationException("The host verifier could not spawn a Blue Slime NPC owner.");
      }

      return spawnedNpc;
    }

    RuntimeNpcTargetSelectionAdapter CreateAdapter(RuntimeNpcEntity npc)
    {
      RuntimeNpcEntity.NpcMovementTickSnapshot movementTick = npc.CaptureMovementTickSnapshot();
      return new RuntimeNpcTargetSelectionAdapter(
        npc,
        session,
        runtimePlayers,
        static () => Array.Empty<NpcNpcTargetSnapshot>(),
        npc.CaptureDirection(),
        npc.CaptureTargetSlot(),
        movementTick.CollideX,
        movementTick.CollideY);
    }

    RuntimeNpcEntity geometryNpc = SpawnHostNpc();
    int width = Math.Max(1, (int)geometryNpc.Collider.Width);
    int height = Math.Max(1, (int)geometryNpc.Collider.Height);

    runtimePlayers.Players[0].CommitGhostState(false);
    runtimePlayers.Players[1].CommitGhostState(false);
    runtimePlayers.Players[0].CommitNpcTargetingState(0, [1]);
    runtimePlayers.Players[1].CommitNpcTargetingState(0, []);
    RuntimeNpcEntity noAggroNpc = SpawnHostNpc();
    RuntimeNpcTargetSelectionAdapter noAggroAdapter = CreateAdapter(noAggroNpc);
    NpcTargetSelectionResult noAggroResult = RuntimeNpcStore.SelectFinitePlayerTarget(
      noAggroAdapter,
      noAggroNpc.Movement.Position,
      width,
      height);
    Require(noAggroResult.LegacyTargetIndex == 1,
      "Real Player no-aggro state is read for the NPC's TypeId.");
    noAggroNpc.CommitTargetSelection(in noAggroResult, requestNetworkUpdate: true);
    Require(noAggroNpc.CaptureTargetSlot() == 1,
      "The common target entry commits the selected real Player slot.");
    evidence.Add("production-owner-input: real Player Aggro and type-specific NoAggro are mapped and committed");

    runtimePlayers.Players[0].CommitNpcTargetingState(500, []);
    RuntimeNpcEntity aggroNpc = SpawnHostNpc();
    RuntimeNpcTargetSelectionAdapter aggroAdapter = CreateAdapter(aggroNpc);
    NpcTargetSelectionResult aggroResult = RuntimeNpcStore.SelectFinitePlayerTarget(
      aggroAdapter,
      aggroNpc.Movement.Position,
      width,
      height);
    Require(aggroResult.LegacyTargetIndex == 0,
      "Real Player aggro moves the closest Player ahead of the farther Player.");

    runtimePlayers.Players[0].CommitGhostState(true);
    RuntimeNpcEntity ghostNpc = SpawnHostNpc();
    RuntimeNpcTargetSelectionAdapter ghostAdapter = CreateAdapter(ghostNpc);
    NpcTargetSelectionResult ghostResult = RuntimeNpcStore.SelectFinitePlayerTarget(
      ghostAdapter,
      ghostNpc.Movement.Position,
      width,
      height);
    Require(ghostResult.LegacyTargetIndex == 1,
      "The real Player ghost owner state excludes that Player from selection.");

    runtimePlayers.Players[0].CommitGhostState(false);
    runtimePlayers.Players[0].CommitNpcTargetingState(0, []);
    runtimePlayers.Players[0].CommitGrossStatus(false);
    runtimePlayers.Players[1].CommitGrossStatus(true);
    RuntimeNpcEntity wallOfFleshNpc = SpawnHostNpc();
    RuntimeNpcTargetSelectionAdapter wallOfFleshAdapter = CreateAdapter(wallOfFleshNpc);
    NpcTargetSelectionResult wallOfFleshResult = RuntimeNpcStore.SelectFinitePlayerTarget(
      wallOfFleshAdapter,
      wallOfFleshNpc.Movement.Position,
      width,
      height,
      NpcTargetSelectionStrategy.WallOfFlesh);
    Require(wallOfFleshResult.LegacyTargetIndex == 1,
      "The WOF strategy reads Gross from the real Player alias owner.");

    var projectileLifecycle = new ProjectileLifecycleSystem(session.Storage);
    var projectileHydration = new ProjectileDefinitionHydrationContext(
      checked((int)catalog.CatalogRevision),
      RuntimeNpcStore.MaximumNpcCapacity,
      playerCapacity: byte.MaxValue);
    var projectileCommand = new ProjectileSpawnCommand(
      projectileType: 1,
      new ProjectileOwnerReference(
        runtimePlayers.Players[1].Reference,
        runtimePlayers.Players[1].Slot),
      center: npcPosition + new Vector2(width * 0.5f, height * 0.5f),
      velocity: Vector2.Zero,
      damage: 0,
      originalDamage: 0,
      knockback: 0f);
    if (!projectileLifecycle.TrySpawn(
          projectileCommand,
          catalog.Projectiles,
          projectileHydration,
          out ProjectileHandle projectileHandle))
    {
      throw new InvalidOperationException("The host verifier could not create an active Projectile owner.");
    }
    runtimePlayers.Players[0].CommitTankPetProjectileSlot(projectileHandle.Slot.Value);

    RuntimeNpcEntity normalPetNpc = SpawnHostNpc();
    RuntimeNpcTargetSelectionAdapter normalPetAdapter = CreateAdapter(normalPetNpc);
    NpcTargetSelectionResult normalPetResult = RuntimeNpcStore.SelectFinitePlayerTarget(
      normalPetAdapter,
      normalPetNpc.Movement.Position,
      width,
      height,
      NpcTargetSelectionStrategy.Normal);
    Require(normalPetResult.TargetKind == NpcTargetKind.PlayerTankPet &&
            normalPetResult.LegacyTargetIndex == 0 &&
            normalPetResult.SecondaryLegacySlot == projectileHandle.Slot.Value,
      "The real active Projectile geometry passes the Normal CanHit gate while target remains the selected Player.");
    normalPetNpc.CommitTargetSelection(in normalPetResult, requestNetworkUpdate: true);
    Require(normalPetNpc.CaptureTargetSlot() == 0 &&
            normalPetNpc.CaptureImmediateEffects().NetworkUpdateRequested,
      "The normal pet result immediately commits its Player target and network intent.");

    RuntimeNpcEntity upgradedPetNpc = SpawnHostNpc();
    RuntimeNpcTargetSelectionAdapter upgradedPetAdapter = CreateAdapter(upgradedPetNpc);
    NpcTargetSelectionResult upgradedPetResult = RuntimeNpcStore.SelectFinitePlayerTarget(
      upgradedPetAdapter,
      upgradedPetNpc.Movement.Position,
      width,
      height,
      NpcTargetSelectionStrategy.Upgraded);
    Require(upgradedPetResult.TargetKind == NpcTargetKind.PlayerTankPet &&
            runtimePlayers.Players[0].Slot != runtimePlayers.Players[1].Slot &&
            upgradedPetResult.LegacyTargetIndex == runtimePlayers.Players[1].Slot,
      "Upgraded reads the real Projectile identity OwnerSlot, distinct from the containing Player slot.");
    upgradedPetNpc.CommitTargetSelection(in upgradedPetResult, requestNetworkUpdate: true);
    Require(upgradedPetNpc.CaptureTargetSlot() == runtimePlayers.Players[1].Slot,
      "The Upgraded pet owner slot is committed to the NPC target component.");

    runtimePlayers.Players[0].CommitGhostState(true);
    runtimePlayers.Players[1].CommitGhostState(true);
    RuntimeNpcEntity fallbackNpc = SpawnHostNpc();
    fallbackNpc.CommitTargetSelection(1);
    RuntimeNpcTargetSelectionAdapter fallbackAdapter = CreateAdapter(fallbackNpc);
    NpcTargetSelectionResult fallbackResult = RuntimeNpcStore.SelectFinitePlayerTarget(
      fallbackAdapter,
      fallbackNpc.Movement.Position,
      width,
      height);
    Require(fallbackResult.ShouldCommit && fallbackResult.HasTarget &&
            fallbackResult.LegacyTargetIndex == 1 &&
            fallbackResult.TargetGeometry.Position == new Vector2(
              (int)runtimePlayers.Players[1].Collider.OffsetX + (int)runtimePlayers.Players[1].Location.X,
              (int)runtimePlayers.Players[1].Collider.OffsetY + (int)runtimePlayers.Players[1].Location.Y),
      "Normal no-candidate fallback uses the current target's real Player hitbox.");
    fallbackNpc.CommitTargetSelection(in fallbackResult, requestNetworkUpdate: true);
    Require(fallbackNpc.CaptureTargetSlot() == 1,
      "The no-candidate fallback target is committed immediately.");

    NpcTargetSelectionResult upgradedNoCandidate = RuntimeNpcStore.SelectFinitePlayerTarget(
      fallbackAdapter,
      fallbackNpc.Movement.Position,
      width,
      height,
      NpcTargetSelectionStrategy.Upgraded);
    Require(!upgradedNoCandidate.ShouldCommit,
      "The production Upgraded adapter reports its no-candidate early return.");
    fallbackNpc.CommitTargetSelection(in upgradedNoCandidate, requestNetworkUpdate: true);
    Require(fallbackNpc.CaptureTargetSlot() == 1,
      "Upgraded no-candidate commit leaves the existing NPC target untouched.");
    evidence.Add("production-pet-and-fallback: active Projectile, CanHit, owner identity, immediate commit, and strategy-specific fallback pass");

    runtimePlayers.Players[0].CommitGhostState(false);
    runtimePlayers.Players[1].CommitGhostState(true);
    runtimePlayers.Players[0].CommitNpcTargetingState(0, []);
    runtimePlayers.Players[0].Movement =
      new MovementStateComponent(npcPosition + new Vector2(0f, 100f));
    RuntimeNpcEntity clientEye = SpawnHostNpcByNetId(
      npcStore,
      4,
      npcPosition,
      catalog,
      session.World.Descriptor.WorldId);
    clientEye.CommitTargetSelection(0);
    NpcAiStateComponent clientEyeState = clientEye.CaptureAiState();
    clientEye.CommitAiState(
      new NpcAiStateComponent(
        clientEyeState.Style,
        0f,
        0f,
        0f,
        109f,
        clientEyeState.Timer,
        clientEyeState.LocalAi0,
        clientEyeState.LocalAi1,
        clientEyeState.LocalAi2,
        clientEyeState.LocalAi3),
      clientEye.BehaviorAction);
    int clientActiveCount = npcStore.ActiveCount;
    RuntimeNpcEntity.NpcMovementTickSnapshot clientMovementBefore =
      clientEye.CaptureMovementTickSnapshot();
    long clientLastBehaviorUpdatedTick = clientEye.LastBehaviorUpdatedTick;
    int clientBehaviorAction = clientEye.BehaviorAction;
    RuntimeMain.netMode = 1;
    npcStore.Update(1, session, runtimePlayers);
    RuntimeMain.netMode = 0;
    Require(npcStore.ActiveCount == clientActiveCount,
      "A client Eye update must not allocate a Servant NPC.");
    NpcAiStateComponent clientEyeStateAfter = clientEye.CaptureAiState();
    Require(clientEyeStateAfter.Style == clientEyeState.Style &&
      clientEyeStateAfter.State0 == clientEyeState.State0 &&
      clientEyeStateAfter.State1 == clientEyeState.State1 &&
      clientEyeStateAfter.State2 == clientEyeState.State2 &&
      clientEyeStateAfter.State3 == clientEyeState.State3 &&
      clientEyeStateAfter.Timer == clientEyeState.Timer &&
      clientEyeStateAfter.LocalAi0 == clientEyeState.LocalAi0 &&
      clientEyeStateAfter.LocalAi1 == clientEyeState.LocalAi1 &&
      clientEyeStateAfter.LocalAi2 == clientEyeState.LocalAi2 &&
      clientEyeStateAfter.LocalAi3 == clientEyeState.LocalAi3,
      "A client NPC update must not advance authoritative AI state.");
    Require(clientEye.CaptureMovementTickSnapshot() == clientMovementBefore,
      "A client NPC update must not advance authoritative movement or collision state.");
    Require(clientEye.LastBehaviorUpdatedTick == clientLastBehaviorUpdatedTick &&
      clientEye.BehaviorAction == clientBehaviorAction,
      "A client NPC update must not commit an authoritative behavior action.");
    Require(clientEye.CaptureEyeOfCthulhuEffectTrace()
      .Contains("ServantSpawnSkipped:ClientAuthority"),
      $"The client Eye effect trace records the server-only Servant gate; " +
      $"trace={string.Join("|", clientEye.CaptureEyeOfCthulhuEffectTrace())}");

    RuntimeNpcEntity serverEye = SpawnHostNpcByNetId(
      npcStore,
      4,
      npcPosition + new Vector2(160f, 0f),
      catalog,
      session.World.Descriptor.WorldId);
    serverEye.CommitTargetSelection(0);
    NpcAiStateComponent serverEyeState = serverEye.CaptureAiState();
    serverEye.CommitAiState(
      new NpcAiStateComponent(
        serverEyeState.Style,
        0f,
        0f,
        0f,
        109f,
        serverEyeState.Timer,
        serverEyeState.LocalAi0,
        serverEyeState.LocalAi1,
        serverEyeState.LocalAi2,
        serverEyeState.LocalAi3),
      serverEye.BehaviorAction);
    int serverActiveCount = npcStore.ActiveCount;
    RuntimeMain.netMode = 2;
    npcStore.Update(2, session, runtimePlayers);
    RuntimeMain.netMode = 0;
    Require(npcStore.ActiveCount > serverActiveCount,
      "An authoritative server Eye update must be able to allocate its Servant NPC.");
    Require(serverEye.CaptureEyeOfCthulhuEffectTrace()
      .Any(static effect => effect.StartsWith("ServantSpawned(slot=", StringComparison.Ordinal)),
      "The server Eye effect trace records the authoritative Servant spawn.");
    evidence.Add("authority-gate: client Eye does not spawn Servant; server Eye spawns and records the child");
  }

  private static RuntimeNpcEntity SpawnHostNpcByNetId(
    RuntimeNpcStore npcStore,
    int netId,
    Vector2 position,
    ContentCatalog catalog,
    int worldId)
  {
    if (!npcStore.TrySpawn(
          netId,
          position,
          catalog,
          worldId,
          out RuntimeNpcEntity? spawnedNpc) ||
        spawnedNpc is null)
    {
      throw new InvalidOperationException(
        $"The host verifier could not spawn NPC netId={netId}.");
    }

    return spawnedNpc;
  }

  private static NpcTargetSelectionResult Select(
    NpcTargetSelectionStrategy strategy,
    NpcTargetGeometrySnapshot source,
    IReadOnlyList<NpcPlayerTargetSnapshot> players,
    IReadOnlyList<NpcNpcTargetSnapshot>? npcs = null,
    int direction = 1,
    int directionY = 1,
    int oldDirection = 1,
    int oldDirectionY = 1,
    int oldTarget = -1,
    int currentTarget = int.MinValue,
    bool collideX = false,
    bool collideY = false,
    bool confused = false,
    bool boss = false,
    bool faceTarget = true,
    Vector2? checkPosition = null)
  {
    return NpcTargetSelectionSystem.Select(
      Inputs(
        strategy,
        source,
        players,
        npcs ?? Array.Empty<NpcNpcTargetSnapshot>(),
        direction,
        directionY,
        oldDirection,
        oldDirectionY,
        oldTarget,
        currentTarget,
        collideX,
        collideY,
        confused,
        boss,
        faceTarget,
        checkPosition));
  }

  private static NpcTargetSelectionInputs Inputs(
    NpcTargetSelectionStrategy strategy,
    NpcTargetGeometrySnapshot source,
    IReadOnlyList<NpcPlayerTargetSnapshot> players,
    IReadOnlyList<NpcNpcTargetSnapshot> npcs,
    int direction = 1,
    int directionY = 1,
    int oldDirection = 1,
    int oldDirectionY = 1,
    int oldTarget = -1,
    int currentTarget = int.MinValue,
    bool collideX = false,
    bool collideY = false,
    bool confused = false,
    bool boss = false,
    bool faceTarget = true,
    Vector2? checkPosition = null)
  {
    return new NpcTargetSelectionInputs(
      strategy,
      source,
      direction,
      directionY,
      oldDirection,
      oldDirectionY,
      oldTarget,
      collideX,
      collideY,
      confused,
      boss,
      faceTarget,
      players,
      npcs)
    {
      CurrentTarget = currentTarget,
      CheckPosition = checkPosition,
    };
  }

  private static NpcPlayerTargetSnapshot Player(
    int slot,
    NpcTargetGeometrySnapshot geometry,
    bool active = true,
    bool dead = false,
    bool ghost = false,
    int aggro = 0,
    bool noAggro = false,
    bool gross = true,
    int itemAnimation = 0,
    NpcTankPetTargetSnapshot? tankPet = null)
  {
    return new NpcPlayerTargetSnapshot(
      slot,
      geometry,
      active,
      dead,
      ghost,
      aggro,
      noAggro,
      gross,
      itemAnimation,
      tankPet);
  }

  private static NpcTargetGeometrySnapshot Geometry(
    float x,
    float y,
    int width,
    int height)
  {
    return new NpcTargetGeometrySnapshot(new Vector2(x, y), width, height);
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void WriteReport(
    string reportPath,
    string worldPath,
    string sourceWorldHash,
    string finalWorldHash,
    bool sourceWorldUnchanged,
    bool passed,
    Exception? failure,
    IReadOnlyList<string> evidence)
  {
    string root = FindRepositoryRoot();
    string[] sourceFiles =
    [
      "src/NSSLC.Tools.Simulation/RuntimeNpcTargetSelectionAdapter.cs",
      "src/NSSLC.Tools.Simulation/RuntimeNpcStore.cs",
      "src/NSSLC.Tools.Simulation/RuntimeNpcEntity.cs",
      "src/NSSLC.Tools.Simulation/RuntimePlayerEntity.cs",
      "src/NSSLC/Component/Npc/System/NpcTargetSelectionInputs.cs",
      "src/NSSLC/Component/Npc/System/NpcTargetSelectionSystem.cs",
      "src/NSSLC/Component/Npc/System/NpcNpcTargetSnapshot.cs",
      "src/NSSLC/Component/Npc/System/NpcTankPetTargetSnapshot.cs",
      "src/NSSLC/Component/Player/PlayerNpcTargetingStateComponent.cs",
      "Test/Terraria.NpcAi.TargetHostVerification/Program.cs",
      "Test/Terraria.NpcAi.TargetHostVerification/Terraria.NpcAi.TargetHostVerification.csproj",
    ];
    Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
    File.WriteAllText(
      reportPath,
      JsonSerializer.Serialize(new
      {
        Passed = passed,
        WorldPath = worldPath,
        SourceWorldSha256 = sourceWorldHash,
        FinalWorldSha256 = finalWorldHash,
        SourceWorldUnchanged = sourceWorldUnchanged,
        SourceFilesAtRuntime = sourceFiles.ToDictionary(
          path => path,
          path => HashFile(Path.Combine(root, path))),
        ExecutedAssemblies = new[]
        {
          typeof(RuntimeNpcStore).Assembly,
          typeof(NpcTargetSelectionSystem).Assembly,
          typeof(EntityRuntime).Assembly,
        }.ToDictionary(
          assembly => assembly.GetName().Name!,
          assembly => HashFile(assembly.Location)),
        Evidence = evidence,
        Failure = failure?.ToString(),
      },
      new JsonSerializerOptions
      {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
      }));
  }

  private static string FindRepositoryRoot()
  {
    for (DirectoryInfo? directory = new(Environment.CurrentDirectory);
         directory is not null;
         directory = directory.Parent)
    {
      if (File.Exists(Path.Combine(directory.FullName, "global.json")))
      {
        return directory.FullName;
      }
    }

    throw new InvalidOperationException("Run the target verifier from the repository tree.");
  }

  private static string HashFile(string path)
  {
    return Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
  }

  private static string FormatLastLoadOutcome(WorldRecoveryOutcome? outcome)
  {
    if (outcome is null)
    {
      return "No load outcome was captured.";
    }

    return $"LastLoad={outcome.Status}; readAttempts={outcome.Attempts}; " +
      $"loadFailure={outcome.Failure.Kind}: {outcome.Failure.Detail}; " +
      $"apiStage={outcome.ApiExecution?.Stage}.";
  }
}
