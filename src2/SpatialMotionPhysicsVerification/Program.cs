using System.Numerics;
using Terraria.SpatialMotionPhysics;

var scenarios = new (string Name, Action Test)[]
{
  ("entity geometry", TestEntityGeometry),
  ("liquid contact", TestLiquidContact),
  ("collision payload", TestCollisionPayload),
  ("teleport queries", TestTeleportQueries),
  ("portal traversal", TestPortalTraversal),
  ("pylon snapshots", TestPylonSnapshots),
  ("shimmer unstuck", TestShimmerUnstuck),
  ("minecart definitions", TestMinecartDefinitions),
  ("minecart presentation", TestMinecartPresentation),
  ("projectile catalog", TestProjectileCatalog),
  ("tracked projectile reference", TestTrackedProjectileReference),
  ("darkness hazard", TestDarknessHazard),
  ("seat metadata", TestSeatMetadata),
  ("main runtime boundary", TestMainRuntimeBoundary)
};

foreach ((string name, Action test) in scenarios)
{
  test();
  Console.WriteLine($"PASS {name}");
}

Console.WriteLine($"P05 verifier passed: {scenarios.Length} scenarios.");
return 0;

static void TestEntityGeometry()
{
  EntityBoundsComponent bounds = new(20, 10);
  EntitySpatialState state = new(new Vector2(100, 50), bounds);
  EntityGeometrySnapshot snapshot = EntityGeometryQuery.Read(state);

  AssertEqual(new Vector2(110, 55), snapshot.Center, "Center should be derived from position and size.");
  AssertEqual(new Vector2(100, 55), snapshot.Left, "Left should use the vertical midpoint.");
  AssertEqual(new Vector2(120, 60), snapshot.BottomRight, "BottomRight should include both dimensions.");

  BoundsMutationSystem.Apply(
    state,
    new BoundsMutationCommand(BoundsAnchor.Center, new Vector2(200, 200)));
  AssertEqual(new Vector2(190, 195), state.Position, "Center mutation should preserve size.");

  BoundsMutationSystem.Apply(
    state,
    new BoundsMutationCommand(BoundsAnchor.Hitbox, new Vector2(3, 4), new Vector2(30, 40)));
  AssertEqual(new Vector2(3, 4), state.Position, "Hitbox mutation should set position atomically.");
  AssertEqual(30, state.Bounds.Width, "Hitbox mutation should set width.");
  AssertEqual(40, state.Bounds.Height, "Hitbox mutation should set height.");
}

static void TestLiquidContact()
{
  EntityLiquidContactComponent contact = new();
  LiquidContactSystem.Update(
    contact,
    new LiquidContactSample(
      Wet: false,
      ShimmerWet: true,
      HoneyWet: false,
      WetCount: 2,
      LavaWet: false));

  Assert(contact.WetCount == 2, "Liquid system should own wet count.");
  Assert(EntityLiquidContactQuery.AnyWet(contact), "AnyWet should include shimmer contact.");
  Assert(!EntityLiquidContactQuery.HasLava(contact), "Lava query should reflect the current sample.");
}

static void TestCollisionPayload()
{
  BallCollisionPayload payload = new(
    new Vector2(0, -1),
    new Vector2(12, 13),
    new TileHandle(4, 5),
    new EntityReference(9),
    0.5f);
  AssertEqual(0.5f, payload.TimeScale, "Collision payload should preserve time scale.");

  RecordingCollisionSink sink = new();
  CollisionResolutionSystem.Resolve(payload, sink);
  Assert(sink.TileHits == 1 && sink.EntityHits == 1, "Resolution should publish both contact effects.");
}

static void TestTeleportQueries()
{
  RandomTeleportationAttemptInput input = new(
    new Vector2(16, 32),
    Vector2.Zero,
    1,
    mostlySolidFloor: true,
    avoidLava: true,
    avoidAnyLiquid: true,
    avoidHurtTiles: true,
    avoidWalls: true,
    attemptsBeforeGivingUp: 2,
    maximumFallDistanceFromOrignalPoint: 8,
    strictRange: true,
    new[] { 10, 11 },
    tilesToAvoidRange: 2,
    allowSolidTopFloor: false,
    specializedConditions: coordinate => coordinate.X >= 0);

  TeleportCandidateResult result = TeleportCandidateQuery.Find(
    input,
    new[] { new Vector2(-1, 1), new Vector2(3, 4) },
    coordinate => coordinate.X == 3);
  Assert(result.Found, "Teleport query should return the first valid candidate.");
  AssertEqual(new Vector2(3, 4), result.Position, "Teleport query should preserve candidate position.");

  InterceptionResult interception = InterceptionQuery.Calculate(
    targetPosition: Vector2.Zero,
    targetVelocity: new Vector2(1, 0),
    chaserPosition: new Vector2(-10, 0),
    chaserSpeed: 5);
  Assert(interception.InterceptionHappens, "A reachable moving target should have an interception.");
}

static void TestPortalTraversal()
{
  PortalGeometryDefinitionCatalog catalog = PortalGeometryDefinitionCatalog.CreateDefault();
  PortalTraversalRuntimeCache cache = new();
  cache.SetPortalPair(0, 1);
  cache.SetPortalAvailable(true);
  PortalCooldownState cooldown = new();
  PortalTraversalSystem system = new(catalog, cache, cooldown);
  PortalTraversalCandidate? candidate = system.TryCreateCandidate(1, new Vector2(8, 9), new Vector2(1, 0));
  Assert(candidate.HasValue, "Portal system should produce a candidate for an available pair.");

  EntityMotionState motion = new(new Vector2(0, 0), new Vector2(2, 0));
  system.Commit(motion, candidate.GetValueOrDefault());
  AssertEqual(new Vector2(8, 9), motion.Position, "Teleport commit should update position.");
  Assert(cooldown.IsCoolingDown(1), "Portal traversal should install a cooldown.");
}

static void TestPylonSnapshots()
{
  PylonRegistryAdapter adapter = new();
  PylonRegistrySnapshot first = adapter.CreateSnapshot(new[]
  {
    new TeleportPylonSnapshotEntry(new Point16(2, 3), 1)
  });
  PylonRegistrySnapshot second = adapter.CreateSnapshot(new[]
  {
    new TeleportPylonSnapshotEntry(new Point16(4, 5), 2)
  });

  RecordingPylonSink sink = new();
  PylonRegistryProjection.Project(first, second, sink);
  Assert(sink.Added == 1 && sink.Removed == 1, "Pylon projection should emit a snapshot diff.");
}

static void TestShimmerUnstuck()
{
  ShimmerUnstuckStateComponent state = new();
  ShimmerUnstuckSystem.Start(state, duration: 2, indefinite: false);
  Assert(ShimmerUnstuckQuery.ShouldUnstuck(state), "Started unstuck state should be active.");
  ShimmerUnstuckSystem.Tick(state);
  ShimmerUnstuckSystem.Tick(state);
  Assert(!ShimmerUnstuckQuery.ShouldUnstuck(state), "Finite unstuck state should expire.");
}

static void TestMinecartDefinitions()
{
  MinecartCustomizationValue customization = MinecartCustomizationValue.Default;
  AssertEqual(50f, customization.MinecartTextureWidth, "Minecart customization should preserve texture width.");
  AssertEqual(new Vector2(12f, 0f), customization.WheelOffset, "Minecart customization should preserve wheel offset.");
  AssertEqual(new Vector2(25f, 26f), customization.MagnetOffset, "Minecart customization should preserve magnet offset.");

  MinecartTrackDefinitionCatalog catalog = MinecartTrackDefinitionCatalog.CreateDefault();
  MinecartTrackSample sample = MinecartTrackQuery.Read(catalog, frame: 1);
  Assert(sample.TrackType == MinecartTrackType.Pressure, "Frame one should be a pressure track in the default catalog.");

  MinecartMotionState motion = new();
  MinecartMotionSystem.Apply(motion, sample, direction: 1);
  Assert(motion.IsOnTrack && motion.Velocity > 0, "Minecart motion should apply track state and direction.");
}

static void TestMinecartPresentation()
{
  MinecartPresentationDefinitionCatalog catalog = MinecartPresentationDefinitionCatalog.CreateDefault();
  Vector2 texturePosition = MinecartPresentationQuery.GetTexturePosition(catalog, frame: 0);
  AssertEqual(Vector2.Zero, texturePosition, "Default minecart texture should start at the origin.");
  Assert(MinecartPresentationQuery.GetTileHeight(catalog, frame: 0) > 0, "Default tile height should be positive.");
}

static void TestProjectileCatalog()
{
  ProjectileDefinitionCatalog catalog = ProjectileDefinitionCatalogAdapter.Create(3);
  catalog.SetFrames(1, 4);
  catalog.SetPet(1, true);
  Assert(catalog.GetFrames(1) == 4 && catalog.IsPet(1), "Projectile catalog should preserve indexed definitions.");
}

static void TestTrackedProjectileReference()
{
  TrackedProjectileReferenceValue reference = TrackedProjectileReferenceValue.Create(
    localIndex: 2,
    ownerIndex: 3,
    identity: 4,
    type: 5);
  Assert(TrackedProjectileReferenceQuery.IsTracking(reference), "A populated reference should be trackable.");

  using MemoryStream stream = new();
  ReferenceProtocolAdapter.Write(stream, reference);
  stream.Position = 0;
  Assert(
    ReferenceProtocolAdapter.TryRead(stream, out TrackedProjectileReferenceValue decoded),
    "A serialized reference should decode.");
  AssertEqual(reference, decoded, "Decoded reference should retain all ID domains.");
}

static void TestDarknessHazard()
{
  DarknessHazardDefinition definition = new(hitTimerMaxBeforeHit: 2);
  DarknessHazardStateComponent state = new();
  DarknessHazardSystem system = new(definition);
  Assert(!system.Tick(state, tooBright: false).DamageDue, "The first dark tick should not deal damage.");
  Assert(system.Tick(state, tooBright: false).DamageDue, "The threshold dark tick should deal damage.");
  Assert(!system.Tick(state, tooBright: true).DamageDue, "Bright light should clear the hit timer.");
}

static void TestSeatMetadata()
{
  SeatMetadataValue value = SeatMetadataQuery.Create(isAToilet: true);
  Assert(value.IsAToilet, "Seat metadata should preserve toilet classification.");
  RecordingSeatSink sink = new();
  SittingProjectionAdapter.Project(value, sink);
  Assert(sink.LastValue, "Seat projection should publish the metadata.");
}

static void TestMainRuntimeBoundary()
{
  SpawnCadenceState cadence = new(initialValue: 2);
  SpawnCadenceSystem.Tick(cadence);
  Assert(cadence.CheckForSpawns == 1, "Spawn cadence should decrement once.");

  HelpTextProjectionState help = new();
  help.Set(4, 7);
  Assert(help.HelpText == 4 && help.BartenderHelpTextIndex == 7, "Help text projection should isolate UI state.");

  MainRuntimeBoundaryAdapter startup = new(autoGen: true);
  LightingDefinitionAdapter lighting = new(demonTorch: 1.5f);
  Assert(startup.AutoGen && lighting.DemonTorch == 1.5f, "Main boundary values should be adapter-owned.");
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertEqual<T>(T expected, T actual, string message)
{
  if (!EqualityComparer<T>.Default.Equals(expected, actual))
  {
    throw new InvalidOperationException(
      $"{message} Expected '{expected}', actual '{actual}'.");
  }
}

sealed class RecordingCollisionSink : ICollisionEffectSink
{
  public int TileHits { get; private set; }

  public int EntityHits { get; private set; }

  public void OnTileCollision(TileHandle tile, Vector2 impactPoint)
  {
    TileHits++;
  }

  public void OnEntityCollision(EntityReference entity, Vector2 impactPoint)
  {
    EntityHits++;
  }
}

sealed class RecordingPylonSink : PylonRegistryProjection.IPylonProjectionSink
{
  public int Added { get; private set; }

  public int Removed { get; private set; }

  public void Add(TeleportPylonSnapshotEntry entry)
  {
    Added++;
  }

  public void Remove(TeleportPylonSnapshotEntry entry)
  {
    Removed++;
  }
}

sealed class RecordingSeatSink : SittingProjectionAdapter.ISeatMetadataSink
{
  public bool LastValue { get; private set; }

  public void Publish(SeatMetadataValue value)
  {
    LastValue = value.IsAToilet;
  }
}
