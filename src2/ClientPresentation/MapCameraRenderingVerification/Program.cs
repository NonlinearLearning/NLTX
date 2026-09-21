using System.Numerics;
using NLTX.ClientPresentation.MapCameraRendering;

namespace NLTX.ClientPresentation.MapCameraRenderingVerification;

internal static class Program
{
  private static int Main()
  {
    TestCameraRejectsInvalidScale();
    TestCameraProjectsCoordinates();
    TestMapTilePackingRoundTrips();
    TestMapEncodingRoundTrip();
    TestLightingKeepsWorkingFramePrivateUntilSwap();
    TestLightMapScanUsesBoundsAndInjectedRandomness();
    TestAnimationAdvancesAndRegistryCommitsDeferredChanges();
    TestDrawCommandsRespectFrameLeaseAndSubmitOnce();
    TestWorldDrawingProjectionKeepsShadowAndVertexBounds();
    TestShaderRegistryRejectsDuplicatesAndAppliesStagedParameters();
    TestMapInvalidationQueueCoalescesAndOverlayUsesInjectedClock();
    TestMapPersistenceRoundTripsAndReportsFailure();
    Console.WriteLine("P18 verifier passed.");
    return 0;
  }

  private static void TestCameraRejectsInvalidScale()
  {
    CameraUiTransformStateComponent state = new();
    CameraUiTransformSystem system = new();

    AssertThrows<ArgumentOutOfRangeException>(
      () => system.Apply(
        state,
        new CameraUiTransformInput(
          UiScaleWanted: 0,
          UiScaleUsed: 1,
          CameraPosition: Vector2.Zero,
          CameraSize: new Vector2(800, 600),
          Zoom: Vector2.One,
          Translation: Vector2.Zero,
          Pan: Vector2.Zero,
          Lerp: 0,
          LerpTimer: 0,
          LerpTimeToggle: 0)),
      "Camera UI scale must reject zero.");
  }

  private static void TestCameraProjectsCoordinates()
  {
    CameraUiTransformStateComponent state = new();
    CameraUiTransformSystem system = new();
    system.Apply(
      state,
      new CameraUiTransformInput(
        UiScaleWanted: 2,
        UiScaleUsed: 1.5f,
        CameraPosition: new Vector2(10, 20),
        CameraSize: new Vector2(800, 600),
        Zoom: new Vector2(2, 2),
        Translation: new Vector2(3, 4),
        Pan: new Vector2(5, 6),
        Lerp: 0.25f,
        LerpTimer: 2,
        LerpTimeToggle: 1));

    CameraTransformSnapshot snapshot = CameraTransformQuery.Read(state);
    AssertEqual(new Vector2(10, 20), snapshot.UnscaledPosition,
      "Camera position should remain unscaled.");
    AssertEqual(new Vector2(1600, 1200), snapshot.ScaledSize,
      "Scaled size should use zoom.");
    Assert(!snapshot.TransformationMatrix.Equals(Matrix4x4.Identity),
      "A changed transform should rebuild the matrix.");
  }

  private static void TestMapTilePackingRoundTrips()
  {
    MapTileSnapshotComponent tile = new(type: 42, light: 200, color: 7);
    tile.IsChanged = true;
    tile.UpdateQueued = true;

    AssertEqual((ushort)42, tile.Type, "Map tile type should round-trip.");
    AssertEqual((byte)200, tile.Light, "Map tile light should round-trip.");
    AssertEqual((byte)7, tile.Color, "Map tile color should round-trip.");
    Assert(tile.IsChanged && tile.UpdateQueued,
      "Packed map tile flags should round-trip.");
  }

  private static void TestMapEncodingRoundTrip()
  {
    MapEncodingCatalogComponent catalog = MapEncodingCatalogComponent.CreateDefault();
    MapCodecAdapter codec = new(catalog);
    MapTileSnapshotComponent tile = new(type: 15, light: 99, color: 3)
    {
      IsChanged = true,
      UpdateQueued = false
    };

    byte[] encoded = codec.Encode(tile);
    MapTileSnapshotComponent decoded = codec.Decode(encoded);
    AssertEqual(tile, decoded, "Map encoding should preserve tile state.");
  }

  private static void TestLightingKeepsWorkingFramePrivateUntilSwap()
  {
    LightingCoordinatorComponent coordinator =
      new(width: 2, height: 1);
    LightingModeSystem modeSystem = new();
    LightingFrameSwapSystem swapSystem = new();

    modeSystem.Apply(
      coordinator,
      LightingMode.NewEngine,
      globalBrightness: 0.75f,
      offScreenTileBudget: 128);
    swapSystem.Commit(
      coordinator,
      new LightingFrameSnapshot(
        width: 2,
        height: 1,
        sourceRevision: 1,
        colors: new[] { new RgbaColor(10, 20, 30, 255), RgbaColor.White },
        masks: new byte[] { 100, 200 }));

    Assert(
      LightingQuery.TryRead(coordinator, 0, 0, out LightingSample first),
      "An active lighting frame should be readable.");
    AssertEqual(
      new RgbaColor(10, 20, 30, 255),
      first.Color,
      "The first lighting frame should be active.");

    LightingFrameSnapshot pending = new(
      width: 2,
      height: 1,
      sourceRevision: 2,
      colors: new[] { new RgbaColor(200, 10, 10, 255), RgbaColor.White },
      masks: new byte[] { 50, 200 });
    swapSystem.Prepare(coordinator, pending);

    Assert(
      LightingQuery.TryRead(coordinator, 0, 0, out LightingSample beforeSwap),
      "The previous active frame should remain readable while preparing.");
    AssertEqual(
      new RgbaColor(10, 20, 30, 255),
      beforeSwap.Color,
      "Preparing a frame must not expose working data.");

    swapSystem.CommitPrepared(coordinator);
    Assert(
      LightingQuery.TryRead(coordinator, 0, 0, out LightingSample afterSwap),
      "The prepared frame should become active after commit.");
    AssertEqual(
      new RgbaColor(200, 10, 10, 255),
      afterSwap.Color,
      "The committed working frame should become active.");
    AssertEqual(2u, afterSwap.FrameRevision, "The active frame revision should advance.");
  }

  private static void TestLightMapScanUsesBoundsAndInjectedRandomness()
  {
    LightMapCacheComponent cache = new(width: 2, height: 1, lightDecay: 0.5f);
    LightMapScanSystem scanSystem = new();
    ConstantRandomSource random = new(value: 0);
    scanSystem.Scan(
      cache,
      new[]
      {
        new LightMapCellInput(0, 0, RgbaColor.White, 200),
        new LightMapCellInput(2, 0, new RgbaColor(1, 2, 3, 255), 255)
      },
      random,
      scanStride: 2);

    Assert(
      LightMapQuery.TryRead(cache, 0, 0, out LightMapSample sample),
      "A scanned light-map cell should be readable.");
    AssertEqual((byte)100, sample.Mask, "Light decay should be applied to the mask.");
    Assert(
      !LightMapQuery.TryRead(cache, 2, 0, out _),
      "Out-of-bounds light-map cells must be rejected.");

    scanSystem.Clear(cache);
    Assert(
      !LightMapQuery.TryRead(cache, 0, 0, out _),
      "Clearing a light-map should remove the active sample.");
  }

  private static void TestAnimationAdvancesAndRegistryCommitsDeferredChanges()
  {
    SpriteAnimationComponent animation = new(
      frameCount: 3,
      ticksPerFrame: 2,
      pingPong: true,
      notActuallyAnimating: false,
      paddingX: 1,
      paddingY: 2,
      columnCount: 2,
      rowCount: 2);
    AnimationAdvanceSystem advanceSystem = new();
    advanceSystem.Advance(animation, elapsedTicks: 2);
    AssertEqual((byte)1, animation.CurrentColumn, "Animation should advance one frame.");
    advanceSystem.Advance(animation, elapsedTicks: 4);
    AssertEqual((byte)1, animation.CurrentColumn, "Ping-pong animation should reverse at its end.");

    TileAnimationRegistryComponent registry = new(capacity: 2);
    AnimationRegistrySystem registrySystem = new();
    TileAnimationKey key = new(3, 4);
    registrySystem.QueueAdd(
      registry,
      new TileAnimationDefinition(key, FrameCount: 4, TicksPerFrame: 1));
    AssertEqual(0, registry.Count, "Queued animation changes must not publish early.");
    registrySystem.Commit(registry);
    AssertEqual(1, registry.Count, "Animation additions should publish at commit.");
    Assert(
      registry.TryGet(key, out TileAnimationDefinition definition),
      "Committed animation definitions should be queryable.");
    AssertEqual(4, definition.FrameCount, "Animation definition should preserve frame count.");

    registrySystem.QueueRemove(registry, key);
    registrySystem.Commit(registry);
    AssertEqual(0, registry.Count, "Deferred animation removals should commit atomically.");
  }

  private static void TestDrawCommandsRespectFrameLeaseAndSubmitOnce()
  {
    DrawCommandWorksetComponent workset = new(capacity: 2);
    DrawCommandBuildSystem buildSystem = new();
    uint lease = buildSystem.BeginFrame(workset);
    buildSystem.Build(
      workset,
      lease,
      new DrawCommandInput(
        ResourceToken: "tile",
        Position: Vector2.Zero,
        DestinationRectangle: null,
        SourceRectangle: null,
        Color: RgbaColor.White,
        Rotation: 0,
        Origin: Vector2.Zero,
        Scale: Vector2.One,
        ShaderHandle: -1,
        IgnorePlayerRotation: false,
        UseDestinationRectangle: false,
        NullRectangle: null));

    RecordingSpriteBatchSink sink = new();
    SpriteBatchSubmitAdapter adapter = new(sink);
    SpriteBatchStateComponent batchState = new();
    new SpriteBatchBeginSystem().Begin(lease, batchState, new SpriteBatchStateInput(
      SortMode: "deferred",
      BlendState: "alpha",
      SamplerState: "point",
      DepthStencilState: "none",
      RasterizerState: "cull-none",
      EffectToken: null,
      TransformToken: "camera"));
    SpriteBatchSubmissionResult result = adapter.Submit(workset, batchState, lease);

    Assert(result.Submitted, "A valid draw workset should submit.");
    AssertEqual(1, sink.Count, "Each draw command should submit exactly once.");
    AssertEqual(0, workset.Count, "A submitted frame should be cleared.");
    AssertThrows<InvalidOperationException>(
      () => buildSystem.Build(
        workset,
        lease,
        new DrawCommandInput(
          "late",
          Vector2.Zero,
          null,
          null,
          RgbaColor.White,
          0,
          Vector2.Zero,
          Vector2.One,
          -1,
          false,
          false,
          null)),
      "A frame lease cannot be reused after submission.");
  }

  private static void TestWorldDrawingProjectionKeepsShadowAndVertexBounds()
  {
    WorldDrawingProjectionComponent projection = new();
    WorldDrawingProjectionSystem projectionSystem = new();
    projectionSystem.Apply(
      projection,
      new WorldDrawingInput(
        HorizonColor: new RgbaColor(5, 6, 7, 255),
        HorizonBlend: 0.5f,
        ParticleToken: "dust"));
    Guid entityId = Guid.NewGuid();
    EntityShadowSnapshot shadow = projectionSystem.ProjectShadow(
      projection,
      entityId,
      new Vector2(4, 5),
      rotation: 0.25f,
      opacity: 0.8f,
      color: RgbaColor.White,
      sourceRevision: 9);
    AssertEqual(entityId, shadow.EntityId, "Shadow snapshots should retain entity identity.");
    AssertEqual(1, projection.Shadows.Count, "Projection should expose one shadow snapshot.");

    VertexStripWorksetComponent vertices = new(maxVertices: 2);
    VertexStripBuildSystem vertexSystem = new();
    vertexSystem.Build(
      vertices,
      new[]
      {
        new VertexStripPoint(Vector2.Zero, RgbaColor.White, Vector3.Zero),
        new VertexStripPoint(Vector2.One, RgbaColor.White, Vector3.One)
      });
    AssertEqual(2, vertices.VertexCount, "Vertex workset should retain bounded vertices.");
    AssertThrows<ArgumentException>(
      () => vertexSystem.Build(
        vertices,
        new[]
        {
          new VertexStripPoint(Vector2.Zero, RgbaColor.White, Vector3.Zero),
          new VertexStripPoint(Vector2.One, RgbaColor.White, Vector3.One),
          new VertexStripPoint(Vector2.One, RgbaColor.White, Vector3.One)
        }),
      "Vertex workset should reject data above its capacity.");
  }

  private static void TestShaderRegistryRejectsDuplicatesAndAppliesStagedParameters()
  {
    ShaderRegistryComponent registry = new();
    ShaderRegistryBuildSystem registrySystem = new();
    ShaderHandle handle = registrySystem.Register(
      registry,
      new ShaderDefinition("armor-basic", "armor"));
    Assert(
      ShaderLookupQuery.TryFind(registry, "armor-basic", out ShaderHandle found),
      "Registered shader definitions should be queryable.");
    AssertEqual(handle, found, "Shader lookup should return the registered handle.");
    Assert(
      !ShaderLookupQuery.TryFind(registry, "missing", out _),
      "Missing shader keys should return a miss.");
    AssertThrows<InvalidOperationException>(
      () => registrySystem.Register(registry, new ShaderDefinition("armor-basic", "armor")),
      "Duplicate shader keys must be rejected.");

    ShaderParameterComponent parameters = new();
    ShaderFamilyParameterComponent family = new("armor");
    ShaderParameterSystem parameterSystem = new();
    parameterSystem.Stage(
      parameters,
      "Opacity",
      ShaderParameterValue.FromScalar(0.5f));
    parameterSystem.StageFamily(
      family,
      "Dye",
      ShaderParameterValue.FromAsset("dye-red"));
    RecordingShaderSink sink = new();
    ShaderAssetAdapter assets = new(new KnownShaderAssetSource("dye-red"));
    ShaderApplyAdapter adapter = new(sink, assets);
    ShaderApplyResult result = adapter.Apply(handle, parameters, family);

    Assert(result.Applied, "Staged shader parameters should apply through the adapter.");
    AssertEqual(1, sink.ApplyCount, "The shader sink should receive one apply operation.");
    AssertEqual(0, parameters.StagedCount, "Applied parameters should leave the staged set.");

    parameterSystem.Stage(
      parameters,
      "MissingTexture",
      ShaderParameterValue.FromAsset("missing"));
    ShaderApplyResult missingResult = adapter.Apply(handle, parameters, family);
    Assert(!missingResult.Applied, "Missing shader assets should fail explicitly.");
    AssertEqual(1, sink.ApplyCount, "A missing asset must not call the shader sink.");
  }

  private static void TestMapInvalidationQueueCoalescesAndOverlayUsesInjectedClock()
  {
    MapInvalidationQueueComponent queue = new(capacity: 1);
    MapInvalidationQueueSystem queueSystem = new();
    Assert(
      queueSystem.Enqueue(
        queue,
        new MapInvalidationCommand(new SceneScanRectangle(0, 0, 2, 2), 1)),
      "The first map invalidation should be accepted.");
    Assert(
      queueSystem.Enqueue(
        queue,
        new MapInvalidationCommand(new SceneScanRectangle(2, 0, 2, 2), 2)),
      "Adjacent map invalidations should coalesce within capacity.");
    AssertEqual(1, queue.Count, "Coalesced invalidations should use one queue entry.");
    Assert(
      !queueSystem.Enqueue(
        queue,
        new MapInvalidationCommand(new SceneScanRectangle(10, 10, 1, 1), 3)),
      "A non-coalescible full queue must reject explicitly.");
    Assert(
      queueSystem.TryDequeue(queue, out MapInvalidationCommand merged),
      "A coalesced invalidation should be dequeueable.");
    AssertEqual(4, merged.Area.Width, "Coalesced invalidations should cover both areas.");

    MapOverlayComponent overlay = new();
    MapOverlaySystem overlaySystem = new();
    overlaySystem.ApplyTransform(
      overlay,
      translation: new Vector2(3, 4),
      clipArea: new SceneScanRectangle(0, 0, 20, 10),
      opacity: 0.5f);
    Guid pingId = Guid.NewGuid();
    overlaySystem.AddPing(
      overlay,
      new MapPingProjection(
        pingId,
        new Vector2(5, 6),
        CreatedMilliseconds: 0,
        ExpiresMilliseconds: 100,
        Color: RgbaColor.White));
    FixedMapClock clock = new(100);
    overlaySystem.ExpirePings(overlay, MapClockQuery.Read(clock));
    AssertEqual(0, overlay.PingCount, "Expired pings should be removed by the overlay system.");
    AssertEqual(0.5f, overlay.Opacity, "Overlay transform should preserve opacity.");
  }

  private static void TestMapPersistenceRoundTripsAndReportsFailure()
  {
    WorldMapSnapshotComponent map = new(maxWidth: 2, maxHeight: 1, blackEdgeWidth: 3);
    MapTileSnapshotSystem mapSystem = new();
    mapSystem.Capture(
      map,
      0,
      0,
      new MapTileSnapshotValue(17, 88, 4, IsChanged: true, UpdateQueued: false));
    MapPersistenceSnapshot snapshot = MapSaveProjection.Capture(map);
    string path = Path.Combine(
      Path.GetTempPath(),
      $"nltx-p18-{Guid.NewGuid():N}.map");

    try
    {
      using MapPersistenceAdapter adapter = new(
        new MapCodecAdapter(MapEncodingCatalogComponent.CreateDefault()),
        maxRetries: 1);
      MapPersistenceResult saved = adapter.Save(path, snapshot);
      Assert(saved.Success, "A valid map snapshot should be persisted.");
      MapLoadResult loaded = adapter.Load(path);
      Assert(loaded.Success, "A persisted map snapshot should load.");
      AssertEqual(2, loaded.Snapshot!.MaxWidth, "Loaded map width should round-trip.");
      AssertEqual(
        snapshot.Tiles.Span[0],
        loaded.Snapshot.Tiles.Span[0],
        "Loaded map tiles should round-trip.");

      string missingDirectory = Path.Combine(
        Path.GetTempPath(),
        $"nltx-p18-missing-{Guid.NewGuid():N}",
        "map.bin");
      MapPersistenceResult failed = adapter.Save(missingDirectory, snapshot);
      Assert(!failed.Success, "A missing persistence directory should report failure.");
      AssertEqual(
        MapPersistenceFailureKind.DirectoryMissing,
        failed.FailureKind,
        "Persistence should expose a typed directory failure.");
    }
    finally
    {
      if (File.Exists(path))
      {
        File.Delete(path);
      }
    }
  }

  private sealed class RecordingShaderSink : IShaderParameterSink
  {
    public int ApplyCount { get; private set; }

    public void Apply(
      ShaderHandle handle,
      IReadOnlyDictionary<string, ShaderParameterValue> parameters,
      IReadOnlyDictionary<string, ShaderParameterValue> familyParameters)
    {
      ApplyCount++;
      AssertEqual("armor-basic", handle.Key, "The sink should receive the shader handle.");
      AssertEqual(1, parameters.Count, "The sink should receive scalar parameters.");
      AssertEqual(1, familyParameters.Count, "The sink should receive family parameters.");
    }
  }

  private sealed class KnownShaderAssetSource : IShaderAssetSource
  {
    private readonly string _knownToken;

    public KnownShaderAssetSource(string knownToken)
    {
      _knownToken = knownToken;
    }

    public bool TryResolve(string assetToken, out string resolvedToken)
    {
      if (assetToken == _knownToken)
      {
        resolvedToken = assetToken;
        return true;
      }

      resolvedToken = string.Empty;
      return false;
    }
  }

  private sealed class FixedMapClock : IMapClockSource
  {
    public FixedMapClock(long nowMilliseconds)
    {
      NowMilliseconds = nowMilliseconds;
    }

    public long NowMilliseconds { get; }
  }

  private sealed class RecordingSpriteBatchSink : ISpriteBatchSink
  {
    public int Count { get; private set; }

    public void Submit(DrawCommand command)
    {
      Count++;
      AssertEqual("tile", command.ResourceToken, "The sink should receive the source resource token.");
    }
  }

  private sealed class ConstantRandomSource : IRandomSampleSource
  {
    private readonly int _value;

    public ConstantRandomSource(int value)
    {
      _value = value;
    }

    public int Next(int maxExclusive)
    {
      if (maxExclusive <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(maxExclusive));
      }

      return Math.Min(_value, maxExclusive - 1);
    }
  }

  private static void AssertThrows<TException>(Action action, string message)
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

  private static void Assert(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void AssertEqual<T>(T expected, T actual, string message)
  {
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
      throw new InvalidOperationException(
        $"{message} Expected '{expected}', actual '{actual}'.");
    }
  }
}
