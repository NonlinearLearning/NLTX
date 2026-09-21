using Terraria.NonAuthoritative.Diagnostics;

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void RequireEqual<T>(T expected, T actual, string message)
{
  if (!EqualityComparer<T>.Default.Equals(expected, actual))
  {
    throw new InvalidOperationException(
      $"{message} Expected '{expected}', actual '{actual}'.");
  }
}

TestRuntimeDiagnostics();
TestRuntimeTick();
TestCallTracking();
TestTimeSeriesAndEntries();
TestFormatting();
TestRandomStreams();
TestBufferAndCollections();
TestRangesAndBits();
TestCrashObservation();
TestLegacyContexts();
TestDebugProtocol();
TestDebugOptions();
TestFrameTelemetry();
TestBuildStatus();
TestTimeLoggerCoordination();
TestUtilityCaches();
TestDisplayFormats();
TestMetricProjections();
TestIssueReports();

Console.WriteLine("PASS: non-authoritative P20 diagnostics and shared boundaries");

static void TestRuntimeDiagnostics()
{
  RuntimeDiagnosticsOptionsComponent component =
    new(new RuntimeDiagnosticsOptionsSnapshot(
      ShowSplash: false,
      IgnoreErrors: false,
      DefaultIp: "127.0.0.1"));
  RuntimeDiagnosticsOptionsSystem.Apply(
    component,
    new RuntimeDiagnosticsOptionsCommand(
      ShowSplash: true,
      IgnoreErrors: true,
      DefaultIp: "localhost"));

  RuntimeDiagnosticsOptionsSnapshot snapshot =
    RuntimeDiagnosticsOptionsQuery.Snapshot(component);
  Require(snapshot.ShowSplash && snapshot.IgnoreErrors,
    "Runtime options must be committed by one command.");
  RequireEqual("localhost", snapshot.DefaultIp,
    "Runtime options must preserve endpoint text.");
}

static void TestRuntimeTick()
{
  RuntimeTickContextComponent component = new();
  component.BeginProjectile(7);
  RequireEqual(7, component.ProjectileUpdateLoopIndex,
    "Projectile cursor must be frame-local.");
  component.EndFrame();
  RequireEqual(-1, component.ProjectileUpdateLoopIndex,
    "Projectile cursor must reset at the frame boundary.");
}

static void TestCallTracking()
{
  RecordingCallTrackingOutput output = new();
  using (CallTrackingSinkAdapter sink = new(output))
  {
    Require(sink.Record("one"), "First call must be recorded.");
    Require(!sink.Record("one"), "Duplicate call must be deduplicated.");
    Require(sink.Record("two"), "A distinct call must be recorded.");
    RequireEqual(2, sink.Flush(), "Flush must publish both first occurrences.");
  }

  RequireEqual(2, output.Messages.Count,
    "Call tracking output must receive each first occurrence once.");
}

static void TestTimeSeriesAndEntries()
{
  TimeSeriesWindowState series = new(capacity: 4);
  series.Add(2);
  series.Add(6);
  series.StartNextFrame();
  TimeSeriesAggregationSnapshot snapshot =
    TimeSeriesAggregationQuery.Snapshot(series);
  RequireEqual(8, snapshot.Previous, "Previous frame total must be retained.");
  RequireEqual(8, snapshot.Maximum, "Maximum must be derived from the window.");

  TimeLogEntryState entry = new(
    "test",
    value => value.ToString(),
    budget: 10);
  entry.Add(3);
  entry.StartNextFrame();
  RequireEqual("test", entry.Name, "Entry name must be stable.");
  Require(entry.CreateSnapshot().Series.Previous == 3,
    "Entry must own its time-series sample.");
}

static void TestFormatting()
{
  DiagnosticFormatPool pool = new("{0:0.0}", maxValue: 10);
  RequireEqual("1.0", pool.Format(1),
    "Format pool must format values.");
  RequireEqual("20.0", pool.Format(20),
    "Out-of-range values must bypass the bounded cache.");
}

static void TestRandomStreams()
{
  FastRandomValue first = new(42);
  FastRandomValue second = new(42);
  RequireEqual(first.Next(100), second.Next(100),
    "Fast random streams with equal seeds must replay.");

  Lcg32RandomState lcg = new(1);
  uint before = lcg.State;
  lcg.Advance();
  Require(before != lcg.State, "LCG state must advance explicitly.");

  UnifiedRandomState unified = new(7);
  int value = unified.Next(100);
  Require(value >= 0 && value < 100,
    "Unified random values must respect the requested range.");
}

static void TestBufferAndCollections()
{
  BufferPoolAdapter pool = new();
  CachedBufferLease lease = pool.Request(32);
  lease.Writer.Write((byte)4);
  RequireEqual(32, lease.Length, "Lease length must preserve the pooled capacity.");
  pool.Recycle(lease);
  Require(!lease.IsActive, "Recycled leases must be inactive.");

  SegmentedCollectionState<int> collection = new(segmentSize: 2);
  collection.PushBack(1);
  collection.PushBack(2);
  RequireEqual(2, collection.Count, "Segmented collection count must update.");
  RequireEqual(1, collection.PopFront(), "Segmented collection order must hold.");

  EntrySortPlan<string> plan = new();
  plan.AddStep("name");
  RequireEqual(1, plan.Steps.Count, "Sort plan must retain steps.");
}

static void TestRangesAndBits()
{
  IntRangeValue range = new(2, 5);
  Require(range.Contains(2) && !range.Contains(6),
    "Integer range bounds must be deterministic.");

  Bits64Value bits = new();
  bits[3] = true;
  Require(bits[3] && !bits[2] && !bits.IsEmpty,
    "Bits64 must expose stable bit access.");

  BitSet2DScratch scratch = new();
  scratch.Reset(new GridPoint(0, 0), 1);
  Require(scratch.Add(new GridPoint(0, 0)), "Scratch must accept a new point.");
  Require(!scratch.Add(new GridPoint(0, 0)),
    "Scratch must reject a duplicate point.");
}

static void TestCrashObservation()
{
  RecordingCrashSink sink = new();
  CrashObservationAdapter adapter = new(
    new CrashObservationSettings(
      LogAllExceptions: true,
      DumpOnException: false,
      DumpOnCrash: false,
      DumpPath: "crash"),
    sink);
  adapter.Observe(new InvalidOperationException("sample"));
  RequireEqual(1, sink.Exceptions.Count,
    "Crash observation must publish an exception snapshot.");
}

static void TestLegacyContexts()
{
  LegacyAttributeMetadata metadata = new("message");
  RequireEqual("message", metadata.Message, "Legacy metadata must be immutable.");

  LegacyDelegateOperationContext context = new();
  context.SetScalar(2.5f);
  context.SetResult(true);
  Require(context.Result && context.Scalar == 2.5f,
    "Delegate context must expose explicit operation results.");
}

static void TestDebugProtocol()
{
  DebugMessage message = DebugMessage.Parse(
    author: 3,
    rawMessage: "/ping hello",
    mousePosition: new GridPoint(4, 5));
  RequireEqual("ping", message.CommandName,
    "Debug protocol must parse command names.");
  RequireEqual("hello", message.Arguments,
    "Debug protocol must preserve arguments.");

  DebugCommandCatalogAdapter catalog = new();
  catalog.Add(new DelegateDebugCommand(
    new DebugCommandMetadata("ping", "ping", "help", CommandRequirement.None),
    _ => true));
  DebugCommandDispatcher dispatcher = new(catalog);
  Require(dispatcher.Dispatch(message),
    "Registered debug command must dispatch.");
}

static void TestDebugOptions()
{
  DebugRuntimeOptionsComponent options = new();
  options.Apply(new DebugRuntimeOptionsSnapshot(
    EnableDebugCommands: true,
    ReportCommandUsage: true,
    ServerPing: 20,
    UpdateWaitInMs: 1.5,
    NoLimits: false,
    ShowNetOffsetDust: false,
    FakeNetOffset: new Vector2Value(1, 2),
    NoDamage: true,
    ProjectilesAimAtDummies: false,
    PracticeMode: true));
  Require(options.Snapshot().EnableDebugCommands &&
    options.Snapshot().NoDamage,
    "Debug options must publish a coherent snapshot.");
}

static void TestFrameTelemetry()
{
  FrameTelemetryRing ring = new(2);
  ring.BeginFrame(1);
  ring.Record(OperationCategory.Update, 10);
  ring.EndFrame(2, new GcAllocationSnapshot(TimeSpan.Zero, 1, 2));
  FrameTelemetrySnapshot snapshot = ring.CreateSnapshot();
  RequireEqual(1, snapshot.Frames.Count,
    "Telemetry ring must expose completed frames.");
  RequireEqual(2L, snapshot.Frames[0].AllocatedBytes,
    "Telemetry ring must retain allocation data.");
}

static void TestBuildStatus()
{
  BuildStatusAdapter adapter = new(() => "abc123");
  RequireEqual("abc123", adapter.GetStatus().GitSha,
    "Build status adapter must expose source metadata.");
}

static void TestTimeLoggerCoordination()
{
  InMemoryDiagnosticLogOutput output = new();
  TimeLoggerFrameCoordinatorAdapter coordinator =
    new(output, frameCount: 4);
  TimeLogEntryState entry = coordinator.RegisterEntry(
    "frame", value => value.ToString(), budget: 1);
  coordinator.StartNextFrame();
  entry.Add(5);
  coordinator.EndFrame();
  Require(output.Lines.Count >= 2,
    "Frame coordinator must publish start and end boundaries.");
}

static void TestUtilityCaches()
{
  UtilityCacheAdapter cache = new();
  RequireEqual(3, cache.GetSubstitutionRegex().Matches(
      "{first} {second} {first}").Count,
    "Utility cache must provide a reusable regex.");

  FloodFillScratchQuery scratch = new();
  scratch.Begin(new GridPoint(0, 0), 1);
  Require(scratch.TryVisit(new GridPoint(0, 0)),
    "Flood-fill scratch must accept the first point.");
  Require(!scratch.TryVisit(new GridPoint(0, 0)),
    "Flood-fill scratch must deduplicate points.");
}

static void TestDisplayFormats()
{
  TimeLoggerDisplayFormatSet formats = new();
  Require(formats.Milliseconds.Format(1.25).Length > 0,
    "Display format set must provide millisecond formatting.");
}

static void TestMetricProjections()
{
  EntityAndInterfaceMetricsProjection entityMetrics = new();
  entityMetrics.Record("NPC", 4);
  RequireEqual(4, entityMetrics.Snapshot()["NPC"],
    "Entity metric projection must expose recorded values.");

  TileAndLiquidMetricsProjection tileMetrics = new();
  tileMetrics.Record("Liquid", 5);
  RequireEqual(5, tileMetrics.Snapshot()["Liquid"],
    "Tile metric projection must expose recorded values.");

  LightingMapBackgroundMetricsProjection lightingMetrics = new();
  lightingMetrics.Record("Lighting", 6);
  RequireEqual(6, lightingMetrics.Snapshot()["Lighting"],
    "Lighting metric projection must expose recorded values.");
}

static void TestIssueReports()
{
  IssueReportCatalogAdapter catalog = new();
  catalog.Add("problem", new DateTimeOffset(2026, 9, 11, 0, 0, 0, TimeSpan.Zero));
  RequireEqual("problem", catalog.Snapshot()[0].ReportText,
    "Issue catalog must preserve report text.");
}

sealed class RecordingCallTrackingOutput : ICallTrackingOutput
{
  public List<string> Messages { get; } = new();

  public void Write(string message)
  {
    Messages.Add(message);
  }
}

sealed class RecordingCrashSink : ICrashObservationSink
{
  public List<Exception> Exceptions { get; } = new();

  public void Publish(Exception exception)
  {
    Exceptions.Add(exception);
  }
}

sealed class InMemoryDiagnosticLogOutput : IDiagnosticLogOutput
{
  public List<string> Lines { get; } = new();

  public void Write(string line)
  {
    Lines.Add(line);
  }
}
