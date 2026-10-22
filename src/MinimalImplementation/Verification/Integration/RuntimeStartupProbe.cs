using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MinimalImplementation.Contracts;
using MinimalImplementation.Persistence;
using MinimalImplementation.Server.Host;

namespace MinimalImplementation.Verification.Integration;

internal static class RuntimeStartupProbe {
  private sealed class FixtureOwner : IServerTickPhaseOwner {
    private readonly List<(ServerTickPhase Phase, long Tick)> _calls;

    public ServerTickPhase Phase { get; }
    public object TransactionIdentity { get; }

    public FixtureOwner(
        ServerTickPhase phase,
        List<(ServerTickPhase Phase, long Tick)> calls,
        object? identity = null) {
      Phase = phase;
      _calls = calls;
      TransactionIdentity = identity ?? this;
    }

    public void Advance(long gameTickIndex) {
      _calls.Add((Phase, gameTickIndex));
    }
  }

  public static async Task<int> RunAsync(string? requestedOutputRoot) {
    string outputRoot = Path.GetFullPath(requestedOutputRoot ?? Path.Combine(
        "Build", "diagnostics", "MinimalImplementation", "RuntimeStartup",
        DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff", CultureInfo.InvariantCulture)));
    if (Directory.Exists(outputRoot)) {
      throw new ArgumentException("The runtime startup probe requires a fresh output directory.");
    }
    Directory.CreateDirectory(outputRoot);
    string resultPath = Path.Combine(outputRoot, "runtime-startup-result.md");
    var evidence = new StringBuilder("# Runtime startup integration probe\n\n");
    using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(45));
    try {
      foreach (string name in new[] { "empty", "missing", "duplicate", "shared-identity" }) {
        await VerifyRejectedAsync(outputRoot, name, evidence, deadline.Token).ConfigureAwait(false);
      }
      await VerifyStartedAsync(outputRoot, false, evidence, deadline.Token).ConfigureAwait(false);
      await VerifyStartedAsync(outputRoot, true, evidence, deadline.Token).ConfigureAwait(false);
      evidence.AppendLine();
      evidence.AppendLine("Status: PASS");
      evidence.AppendLine();
      evidence.AppendLine("Boundary: targeted runtime probe, not a fixed SC scenario. Phase owners");
      evidence.AppendLine("active-client detection and inbound callbacks are explicit fixtures.");
      evidence.AppendLine("Successful startup proves phase composition and runtime lifetime only;");
      evidence.AppendLine("it does not prove domain ticks, real join, periodic module delivery,");
      evidence.AppendLine("default-runtime autonomous scheduling, same-tick retry or SC091 ready.");
      File.WriteAllText(resultPath, evidence.ToString());
      Console.WriteLine($"RuntimeStartup PASS evidence={resultPath}");
      return 0;
    } catch (Exception exception) {
      evidence.AppendLine($"\nStatus: FAIL\n\n{exception}");
      File.WriteAllText(resultPath, evidence.ToString());
      Console.Error.WriteLine($"RuntimeStartup FAIL {exception}");
      return 1;
    }
  }

  private static async Task VerifyRejectedAsync(
      string outputRoot, string name, StringBuilder evidence, CancellationToken cancellation) {
    var calls = new List<(ServerTickPhase Phase, long Tick)>();
    List<IServerTickPhaseOwner> owners = CreateOwners(calls, includePeriodic: true);
    if (name == "empty") {
      owners.Clear();
    } else if (name == "missing") {
      owners.RemoveAll(owner => owner.Phase == ServerTickPhase.Npc);
    } else if (name == "duplicate") {
      owners.Add(new FixtureOwner(ServerTickPhase.Player, calls));
    } else {
      object identity = new();
      owners[0] = new FixtureOwner(ServerTickPhase.Player, calls, identity);
      owners[1] = new FixtureOwner(ServerTickPhase.Npc, calls, identity);
    }

    // Occupying the port turns premature listener startup into an observable startup failure.
    using var reservation = new TcpListener(IPAddress.Loopback, 0);
    reservation.Start();
    int port = ((IPEndPoint)reservation.LocalEndpoint).Port;
    P08BServerConfiguration configuration = Configuration(outputRoot, name, port);
    await using var runtime = new P08BServerRuntime(configuration, Integrations(owners));
    P08BStartResult started = await runtime.StartAsync(cancellation).ConfigureAwait(false);
    Require(!started.IsReady && started.ExitCode == 3 && started.WorldLoaded &&
        started.WorldPublished && started.SaveReady && !started.ListenerStarted &&
        !started.TickStarted, $"{name}: rejection lost world state or started a listener/tick.");
    string expected = name == "empty"
        ? "No authoritative tick phase owners were supplied"
        : "The supplied tick phase owners are not a complete authoritative set";
    Require(started.Blockers.Any(blocker => blocker.StartsWith(expected, StringComparison.Ordinal)),
        $"{name}: the phase-validation blocker was missing.");
    ServerConnectionHost? host = runtime.Composition.ConnectionHost;
    Require(name == "empty" ? host is null : host is { IsRunning: false },
        $"{name}: the connection host was attached or started at the wrong stage.");
    Require(calls.Count == 0, $"{name}: rejected owners advanced.");

    P08BStopResult stopped = await runtime.StopAsync(
        save: true, deadline: TimeSpan.FromSeconds(5), cancellationToken: cancellation)
        .ConfigureAwait(false);
    Require(stopped.Succeeded && stopped.Stage == P08BStopStage.Stopped &&
        stopped.SaveResult is { IsCommitted: true },
        $"{name}: blocked-runtime save/stop failed: {stopped.Stage} {stopped.Detail}");
    Require(host is null || !host.IsRunning && host.Completion.IsCompletedSuccessfully,
        $"{name}: the unstarted connection host did not close.");
    evidence.AppendLine($"- {name}: blocked exit3; loaded/published/save-ready; no listener/tick;");
    evidence.AppendLine("  occupied port retained; no owner calls; save committed; stop complete.");
    Console.WriteLine($"RuntimeStartup {name} rejected; save/stop PASS");
  }

  private static async Task VerifyStartedAsync(
      string outputRoot, bool includePeriodic, StringBuilder evidence,
      CancellationToken cancellation) {
    string name = includePeriodic ? "explicit-periodic" : "automatic-periodic";
    var calls = new List<(ServerTickPhase Phase, long Tick)>();
    List<IServerTickPhaseOwner> owners = CreateOwners(calls, includePeriodic);
    using var reservation = new TcpListener(IPAddress.Loopback, 0);
    reservation.Start();
    int port = ((IPEndPoint)reservation.LocalEndpoint).Port;
    reservation.Stop();
    P08BServerConfiguration configuration = Configuration(outputRoot, name, port);
    await using var runtime = new P08BServerRuntime(configuration, Integrations(owners));
    P08BStartResult started = await runtime.StartAsync(cancellation).ConfigureAwait(false);
    Require(started.IsReady && started.ListenerStarted && started.TickStarted &&
        started.Blockers.Count == 0, $"{name}: valid fixture composition failed to start.");
    ServerConnectionHost host = runtime.Composition.ConnectionHost!;
    using (var client = new TcpClient()) {
      await client.ConnectAsync(IPAddress.Loopback, port, cancellation).ConfigureAwait(false);
      using var connectionDeadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
      connectionDeadline.CancelAfter(TimeSpan.FromSeconds(3));
      while (host.ActiveConnectionCount != 1) {
        await Task.Delay(10, connectionDeadline.Token).ConfigureAwait(false);
      }
      bool advanced = false;
      while (!advanced) {
        await runtime.Composition.CommitLoop.SubmitAsync(
            () => advanced = calls.Any(call => call.Tick >= 2), connectionDeadline.Token)
            .ConfigureAwait(false);
        if (!advanced) {
          await Task.Delay(10, connectionDeadline.Token).ConfigureAwait(false);
        }
      }
    }
    P08BStopResult stopped = await runtime.StopAsync(
        save: true, deadline: TimeSpan.FromSeconds(5), cancellationToken: cancellation)
        .ConfigureAwait(false);
    Require(stopped.Succeeded && stopped.SaveResult is { IsCommitted: true } &&
        !host.IsRunning && host.Completion.IsCompletedSuccessfully,
        $"{name}: scheduler/pump/listener save/stop failed: {stopped.Stage} {stopped.Detail}");
    ServerTickPhase[] expected = owners.Select(owner => owner.Phase).Order().ToArray();
    long previousTick = 0;
    foreach (var tick in calls.GroupBy(call => call.Tick)) {
      Require(tick.Key == previousTick + 1 &&
          tick.Select(call => call.Phase).SequenceEqual(expected),
          $"{name}: fixture phases were duplicated, omitted, reordered or used different ticks.");
      previousTick = tick.Key;
    }
    Require(previousTick >= 2, $"{name}: fewer than two complete fixture ticks ran.");
    using var rebound = new TcpListener(IPAddress.Loopback, port);
    rebound.Start();
    evidence.AppendLine($"- {name}: TCP accepted; {previousTick} fixture ticks in order;");
    evidence.AppendLine("  Runtime supplies Projectile; save committed; scheduler/pump/listener");
    evidence.AppendLine("  stopped; same port rebound.");
    evidence.AppendLine(includePeriodic
        ? "  Explicit PeriodicSync runs once per fixture tick without duplicate coverage."
        : "  Runtime supplies PeriodicSync; no Active sessions or module output tested.");
    Console.WriteLine($"RuntimeStartup {name} startup/accept/tick/save/stop/rebind PASS");
  }

  private static List<IServerTickPhaseOwner> CreateOwners(
      List<(ServerTickPhase Phase, long Tick)> calls, bool includePeriodic) {
    return Enum.GetValues<ServerTickPhase>()
        .Where(phase => phase != ServerTickPhase.Projectile &&
            (includePeriodic || phase != ServerTickPhase.PeriodicSync))
        .Select(phase => (IServerTickPhaseOwner)new FixtureOwner(phase, calls)).ToList();
  }

  private static P08BServerConfiguration Configuration(string outputRoot, string name, int port) {
    return P08BServerConfiguration.Parse(new[] {
      "--world", Path.Combine(outputRoot, name + ".wld"), "--create-world",
      "--listen-port", port.ToString(CultureInfo.InvariantCulture)
    });
  }

  private static P08BServerRuntimeIntegrations Integrations(
      IReadOnlyCollection<IServerTickPhaseOwner> owners) {
    return new P08BServerRuntimeIntegrations(
        allocateEpoch: () => SessionEpoch.Create(1),
        createEnvelope: _ => throw new InvalidOperationException("No inbound frames expected."),
        onPacket: (_, _) => throw new InvalidOperationException("No inbound frames in this probe."),
        onClosed: _ => ValueTask.CompletedTask,
        onConnectionFailure: exception => ValueTask.FromException(exception),
        tickPhaseOwners: owners,
        hasActiveClients: () => true);
  }

  private static void Require(bool condition, string detail) {
    if (!condition) {
      throw new InvalidOperationException(detail);
    }
  }
}
