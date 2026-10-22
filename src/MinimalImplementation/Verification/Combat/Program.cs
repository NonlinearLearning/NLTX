using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Combat.Projectiles;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Verification.Combat;

internal static class Program
{
  private const string ScenarioId = "SC056";
  private const byte PlayerSlot = 7;
  private const long WorldScope = 56;

  public static int Main(string[] args)
  {
    if (!TryGetOutputRoot(args, out string outputRoot))
    {
      Console.Error.WriteLine(
        "Usage: --scenario SC056 --output-root <Build/diagnostics directory>");
      return 2;
    }

    Directory.CreateDirectory(outputRoot);
    try
    {
      Sc056Evidence evidence = RunScenario();
      string evidencePath = Path.Combine(outputRoot, "SC056-result.md");
      File.WriteAllText(evidencePath, evidence.Report);
      Console.WriteLine(
        $"{ScenarioId} PASS owner-slice projectile lifecycle " +
        $"entity={evidence.Entity.Id.Value} generation={evidence.Entity.Generation} " +
        $"evidence={evidencePath}");
      Console.WriteLine(
        "This is P05 owner/codec/tracker evidence; full P02 admission, P03 section " +
        "visibility, and live server listener integration remain pending.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(
        $"{ScenarioId} FAIL {exception.GetType().Name}: {exception.Message}");
      return 1;
    }
  }

  private static Sc056Evidence RunScenario()
  {
    var identityAuthority = new ScenarioIdentityAuthority(WorldScope);
    ProjectileDefinitionCatalog definitions = ProjectileDefinitionCatalog.CreateVersion4Core();
    var projectiles = new ProjectileManager(identityAuthority, definitions, slotCapacity: 8);
    var trackers = new ProjectileTrackerManager(identityAuthority);
    var handler = new ProjectilePacketHandler(projectiles, definitions, trackers);
    SessionEpoch epoch = SessionEpoch.Create(56001);
    var sender = new AuthenticatedCombatSender(epoch, PlayerSlot);

    SyncProjectilePacket packet = new()
    {
      Identity = 12,
      Position = new PacketVector2(128f, 96f),
      Velocity = new PacketVector2(3f, -1f),
      Owner = PlayerSlot,
      ProjectileType = 1,
      Ai0 = 0.25f,
      Ai1 = 0f,
      Ai2 = 0f,
      BannerId = 0,
      Damage = 12,
      Knockback = 1.5f,
      OriginalDamage = 12,
      ProjectileUuid = null
    };
    InboundPacketEnvelope syncEnvelope = CreateEnvelope(
      epoch,
      sequence: 1,
      messageId: 27,
      rawFrame: new byte[] { 27, 12, 0 },
      worldScope: WorldScope,
      entityGeneration: null,
      expectedRevision: null);

    ProjectileSyncDispatchResult synced = handler.HandleSync(syncEnvelope, sender, packet);
    Require(synced.Result.Status == OwnerCommitStatus.Applied, "Packet 27 must commit.");
    Require(synced.Projectile is not null, "Packet 27 must create one projectile view.");
    Require(synced.Broadcast is not null, "Packet 27 must produce one S2C body.");
    ProjectilePacketBody syncBroadcast = synced.Broadcast!;
    Require(projectiles.ActiveCount == 1, "Packet 27 must create exactly one active projectile.");
    ProjectileView projectile = synced.Projectile!;
    Require(projectile.Owner == PlayerSlot, "The server must bind owner to the authenticated slot.");
    Require(projectile.ProtocolIdentity == packet.Identity, "The protocol identity must be retained.");
    Require(projectile.Type == packet.ProjectileType, "The content type must be retained.");
    Require(projectile.Ai0 == packet.Ai0 && projectile.Ai1 == packet.Ai1 && projectile.Ai2 == packet.Ai2,
      "All three Version4 projectile AI values must be retained.");
    Require(projectile.ProtocolUuid is null, "WoodenArrowFriendly must not carry a protocol UUID.");
    Require(projectile.Entity.IsValid, "The server must assign a valid internal entity identity.");
    Require(projectile.Entity.WorldScope == WorldScope, "The internal identity must bind to this world scope.");
    Require(projectile.Entity.Generation > 0, "The server must assign a positive generation.");
    Require(projectile.Revision > 0 && projectile.CommitVersion.IsValid,
      "The committed view must carry internal revision metadata.");

    var collisionQuery = new ScenarioCollisionQuery(WorldScope);
    ProjectileCollisionEvaluation collision = new ProjectileCollisionService().Evaluate(
      projectile,
      collisionQuery);
    Require(collision.IsAccepted && collision.TileHit,
      "Projectile collision must consume the explicit tile query result.");
    Require(collision.NpcHits.Count == 1,
      "Projectile collision must preserve one explicit NPC hit identity.");
    Require(collisionQuery.LastPosition == projectile.Position + projectile.Velocity,
      "Projectile collision must query the next authoritative position.");

    var collisionResolution = new ProjectileCollisionResolutionService(projectiles);
    ProjectileCollisionResolutionPreparation preparedDamage = collisionResolution.Prepare(collision);
    Require(preparedDamage.Result.Status == OwnerCommitStatus.Applied,
      "Accepted collision facts must prepare a bounded damage handoff.");
    Require(preparedDamage.DamageIntents.Count == 1,
      "Projectile penetration must bound the prepared NPC damage intents to one hit.");
    Require(preparedDamage.DamageIntents[0].Projectile == projectile.Entity,
      "Damage intent must remain bound to the projectile generation.");
    Require(projectile.RemainingPenetration == 1 && projectiles.ActiveCount == 1,
      "Preparing collision facts must not consume penetration or retire the projectile.");
    ProjectileCollisionResolutionResult withoutNpcConfirmation = collisionResolution.Commit(
      preparedDamage,
      Array.Empty<CombatEntityIdentity>());
    Require(withoutNpcConfirmation.IsApplied && withoutNpcConfirmation.DamageIntents.Count == 0,
      "Penetration must wait for explicit NPC damage confirmation.");
    Require(projectiles.ActiveCount == 1,
      "An unconfirmed damage handoff must leave the projectile active.");

    long beforeRejectedCommitRevision = projectile.Revision;
    CommitVersion beforeRejectedCommitVersion = projectile.CommitVersion;
    ProjectileView beforeRejectedCommit = projectile;
    identityAuthority.RejectNextRetirement = true;
    ProjectileCollisionResolutionResult rejectedRetirement = collisionResolution.Commit(
      preparedDamage,
      new[] { preparedDamage.DamageIntents[0].TargetNpc });
    Require(
      rejectedRetirement.Result.Status == OwnerCommitStatus.Rejected &&
      rejectedRetirement.Result.ErrorCode == ProtocolErrorCode.StaleEntity,
      "A rejected identity retirement must reject the collision commit.");
    Require(
      projectiles.ActiveCount == 1 &&
      projectile.RemainingPenetration == beforeRejectedCommit.RemainingPenetration &&
      projectile.Revision == beforeRejectedCommitRevision &&
      projectile.CommitVersion == beforeRejectedCommitVersion,
      "A rejected collision retirement must roll back penetration and revision state.");

    ProjectileAiService aiService = new(projectiles);
    ProjectileAiTickResult aiTick = aiService.Tick(new ProjectileAiTickRequest(projectile.Entity));
    Require(aiTick.IsApplied && aiTick.Projectile is not null,
      "The migrated Arrow profile must commit one owner-thread tick.");
    projectile = aiTick.Projectile!;
    Require(projectile.Position == new Vector2(131f, 95f),
      "The Arrow tick must advance by the authoritative velocity.");
    Require(projectile.TimeLeft == 1199,
      "The Arrow tick must decrement the authoritative lifetime once.");
    Require(projectile.RemainingPenetration == 1,
      "The owner-thread tick must not consume penetration without a confirmed NPC hit.");
    Require(projectile.IsDirty && projectiles.CaptureDirty().Count == 1,
      "A committed projectile tick must produce one dirty snapshot.");
    projectiles.AcknowledgeSnapshot(projectile);
    Require(projectiles.CaptureDirty().Count == 0,
      "Acknowledging the same revision must clear only that projectile dirty flag.");

    var decodedSync = SyncProjectilePacket.Read(
      new PacketWireReader(syncBroadcast.Payload));
    Require(decodedSync.Identity == packet.Identity, "The 27 body identity must round-trip.");
    Require(decodedSync.Owner == PlayerSlot, "The 27 body owner must use the authenticated slot.");
    Require(decodedSync.ProjectileType == packet.ProjectileType, "The 27 body type must round-trip.");
    Require(decodedSync.Position == packet.Position && decodedSync.Velocity == packet.Velocity,
      "The 27 body position and velocity must round-trip.");
    Require(decodedSync.Ai0 == packet.Ai0 && decodedSync.Ai1 == packet.Ai1 && decodedSync.Ai2 == packet.Ai2,
      "The 27 body AI values must round-trip.");
    Require(decodedSync.ProjectileUuid is null, "The 27 body must not append internal UUID data.");
    PacketWireReader syncReader = new(syncBroadcast.Payload);
    _ = SyncProjectilePacket.Read(syncReader);
    syncReader.RequireFrameEnd();

    ProjectileTrackerReference trackerReference = new(
      projectile.Entity,
      projectile.Owner,
      projectile.ProtocolIdentity,
      projectile.Type);
    OwnerCommitResult trackerApplied = trackers.Apply(
      PlayerSlot,
      epoch,
      trackerReference,
      voidLens: null);
    Require(trackerApplied.Status == OwnerCommitStatus.Applied,
      "The projectile tracker must bind to the committed generation.");
    Require(
      trackers.TryCapture(PlayerSlot, epoch, projectiles, out ProjectileTrackerSnapshotView? beforeTermination) &&
      beforeTermination?.PiggyBank?.Entity == projectile.Entity,
      "The tracker snapshot must resolve the same generation before packet 29.");

    KillProjectilePacket killPacket = new()
    {
      ProjectileIdentity = packet.Identity,
      Owner = PlayerSlot
    };
    byte[] killInput = ProjectilePacketPayloadCodec.EncodeKillBody(
      killPacket.ProjectileIdentity,
      killPacket.Owner);
    InboundPacketEnvelope killEnvelope = CreateEnvelope(
      epoch,
      sequence: 2,
      messageId: 29,
      rawFrame: killInput,
      worldScope: WorldScope,
      entityGeneration: projectile.Entity.Generation,
      expectedRevision: projectile.Revision);

    ProjectileTerminationDispatchResult terminated = handler.HandleTermination(
      killEnvelope,
      sender,
      killPacket);
    Require(terminated.Result.Status == OwnerCommitStatus.Applied, "Packet 29 must commit once.");
    Require(!terminated.IsNoOp, "The first packet 29 must not be a no-op.");
    Require(terminated.RetiredProjectile?.Entity == projectile.Entity,
      "Packet 29 must retire the same generation created by packet 27.");
    Require(terminated.Broadcast is not null && terminated.Broadcast.IgnoreEpoch == epoch,
      "Packet 29 must broadcast while excluding the requesting session.");
    Require(projectiles.ActiveCount == 0, "Packet 29 must leave no active projectile.");
    Require(!identityAuthority.IsActive(projectile.Entity),
      "Packet 29 must retire the internal identity.");
    Require(!projectiles.TryGet(projectile.Entity, out _),
      "The retired generation must not resolve through the manager.");
    Require(terminated.TrackerCleanup?.Status == OwnerCommitStatus.Applied,
      "Packet 29 must clear tracker references for the retired generation.");
    Require(
      trackers.TryCapture(PlayerSlot, epoch, projectiles, out ProjectileTrackerSnapshotView? afterTermination) &&
      afterTermination?.PiggyBank is null,
      "Tracker capture must not retain the retired projectile.");

    ProjectilePacketBody killBroadcast = terminated.Broadcast!;
    var decodedKill = KillProjectilePacket.Read(
      new PacketWireReader(killBroadcast.Payload));
    Require(decodedKill.ProjectileIdentity == packet.Identity && decodedKill.Owner == PlayerSlot,
      "The packet 29 body must preserve the owner/identity tuple.");

    ProjectileTerminationDispatchResult repeated = handler.HandleTermination(
      killEnvelope,
      sender,
      killPacket);
    Require(repeated.Result.Status == OwnerCommitStatus.Applied && repeated.IsNoOp,
      "A repeated packet 29 must be an observable idempotent no-op.");
    Require(repeated.Broadcast is null, "A repeated packet 29 must not publish a second kill.");

    return new Sc056Evidence(
      projectile.Entity,
      $"""
        # SC056 result

        Status: PASS (P05 owner slice)

        Input: one authenticated sender (slot {PlayerSlot}, epoch {epoch.Value}) sent one
        Version4 packet 27 for migrated Projectile type 1 (WoodenArrowFriendly), identity
        {packet.Identity}, position ({packet.Position.X}, {packet.Position.Y}), velocity
        ({packet.Velocity.X}, {packet.Velocity.Y}), AI ({packet.Ai0}, {packet.Ai1}, {packet.Ai2}),
        and no protocol UUID. The verifier then sent one packet 29 for the same owner/identity
        tuple with the committed world scope {WorldScope}, generation {projectile.Entity.Generation},
        and revision {projectile.Revision}.

        Observed: packet 27 committed exactly one projectile with internal EntityUuid
        `{synced.Projectile!.Entity.Id.Value}`, world scope `{synced.Projectile!.Entity.WorldScope}`, generation
        `{synced.Projectile!.Entity.Generation}`, revision `{synced.Projectile!.Revision}`, and a valid commit
        version. The emitted packet 27 body decoded back to the owner, identity, type, position,
        velocity, all three AI values, and no UUID; internal generation/revision fields were not
        present in the wire body. A forced identity-retirement rejection rolled back penetration,
        revision, commit version, and left the projectile active. The migrated Arrow owner tick
        advanced the position by velocity,
        decremented timeLeft once, emitted one dirty snapshot, and cleared that dirty flag only
        after matching acknowledgement. The explicit collision adapter returned one tile hit and
        one NPC generation at the next position. Resolution preparation produced one bounded
        generation-bound damage intent; without NPC owner confirmation it did not mutate NPC life
        or projectile penetration. A tracker was bound to the same generation, packet 29 retired it, emitted
        one kill body excluding the requesting epoch, cleared the tracker reference, and left
        zero active projectiles. A repeated packet 29 returned an applied idempotent no-op without
        a second broadcast.

        Content evidence: complete same-version source
        `D:/TRbackup/无任何删减通过编译/Terraria/Projectile.cs` sets type 1 to width 10, height 10,
        aiStyle 1, friendly true, tile collision true, penetrate 1, and timeLeft 1200 in the
        SetDefaults path; `Terraria.ID/ProjectileID.cs` names type 1 `WoodenArrowFriendly`. The
        P05 catalog stores only these explicitly migrated facts.

        Boundary: this verifier exercises the P05 decoded-packet owner, Pocket body codec,
        generation registry, Arrow tick/dirty capture, explicit collision-query boundary, tracker
        cleanup, and finite 27→29 lifecycle. It does not claim the full live-server SC056
        prerequisites: P02 `Terraria319` admission/session dispatch, P03 world-backed section and
        collision adapter, projectile damage/penetration/lifecycle integration, Host registration,
        or socket delivery remain pending for total-control integration. SC057–SC070 were not run.
        """);
  }

  private static InboundPacketEnvelope CreateEnvelope(
    SessionEpoch epoch,
    long sequence,
    byte messageId,
    ReadOnlyMemory<byte> rawFrame,
    long worldScope,
    long? entityGeneration,
    long? expectedRevision)
  {
    return new InboundPacketEnvelope(
      epoch,
      new PacketSequence(sequence),
      DateTimeOffset.UnixEpoch,
      messageId,
      moduleId: null,
      rawFrame,
      stage: "Active",
      worldScope,
      entityGeneration,
      expectedRevision);
  }

  private static bool TryGetOutputRoot(IReadOnlyList<string> arguments, out string outputRoot)
  {
    outputRoot = string.Empty;
    if (arguments.Count == 0)
    {
      outputRoot = Path.GetFullPath(Path.Combine(
        "Build",
        "diagnostics",
        "MinimalImplementation",
        "P05",
        ScenarioId));
      return true;
    }

    if (arguments.Count == 4 &&
        arguments[0] == "--scenario" && arguments[1] == ScenarioId &&
        arguments[2] == "--output-root" && !string.IsNullOrWhiteSpace(arguments[3]))
    {
      outputRoot = Path.GetFullPath(arguments[3]);
      return true;
    }

    return false;
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private sealed record Sc056Evidence(CombatEntityIdentity Entity, string Report);

  private sealed class ScenarioCollisionQuery(long worldScope) : IProjectileCollisionQuery
  {
    public Vector2 LastPosition { get; private set; }

    public bool HasSolidTileCollision(Vector2 position, int width, int height)
    {
      LastPosition = position;
      return width == 10 && height == 10;
    }

    public IReadOnlyList<ProjectileCollisionTarget> QueryNpcHits(
      ProjectileView projectile,
      Vector2 position)
    {
      LastPosition = position;
      return
      [
        new ProjectileCollisionTarget(
          new CombatEntityIdentity(EntityUuid.Create(), worldScope, Generation: 2))
      ];
    }
  }

  private sealed class ScenarioIdentityAuthority(long worldScope) : ICombatEntityIdentityAuthority
  {
    private readonly HashSet<CombatEntityIdentity> _active = [];
    private long _nextGeneration;

    public long CurrentWorldScope { get; } = worldScope;

    public bool RejectNextRetirement { get; set; }

    public CombatEntityIdentity Create()
    {
      long generation = ++_nextGeneration;
      CombatEntityIdentity identity = new(
        EntityUuid.From(new Guid(
          0x56000000,
          0x0000,
          0x0001,
          0x80,
          0x56,
          0x00,
          0x00,
          0x00,
          0x00,
          0x00,
          checked((byte)generation))),
        CurrentWorldScope,
        generation);
      _active.Add(identity);
      return identity;
    }

    public bool IsActive(CombatEntityIdentity identity)
    {
      return _active.Contains(identity);
    }

    public bool Retire(CombatEntityIdentity identity)
    {
      if (RejectNextRetirement)
      {
        RejectNextRetirement = false;
        return false;
      }

      return _active.Remove(identity);
    }
  }
}
