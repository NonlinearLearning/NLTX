using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using MinimalImplementation.Contracts;
using MinimalImplementation.Persistence;
using MinimalImplementation.Server.Host;
using MinimalImplementation.Server.World;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.Platform;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;

namespace MinimalImplementation.Verification.Integration;

internal static class Program {
  private const int MaximumWorldFileBytes = 64 * 1024 * 1024;

  private static async Task<int> Main(string[] args) {
    if (ReadOption(args, "--probe") == "player-luck-router") {
      return await PlayerLuckRouterProbe.RunAsync(ReadOption(args, "--output-root"))
          .ConfigureAwait(false);
    }
    if (ReadOption(args, "--probe") == "creative-permissions") {
      return await CreativePermissionProbe.RunAsync(ReadOption(args, "--output-root"))
          .ConfigureAwait(false);
    }
    if (ReadOption(args, "--probe") == "runtime-startup") {
      return await RuntimeStartupProbe.RunAsync(ReadOption(args, "--output-root"))
          .ConfigureAwait(false);
    }

    if (ReadOption(args, "--scenario") != "SC091") {
      Console.Error.WriteLine("Only the fixed scenario SC091 is enabled by this verifier.");
      return 2;
    }

    string outputRoot = Path.GetFullPath(ReadOption(args, "--output-root") ?? Path.Combine(
        "Build", "diagnostics", "MinimalImplementation", "SC091",
        DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff", CultureInfo.InvariantCulture)));
    Directory.CreateDirectory(outputRoot);
    using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(45));
    try {
      await RunLifecycleAsync(outputRoot, deadline.Token).ConfigureAwait(false);
      Console.WriteLine($"SC091 PASS output={outputRoot}");
      return 0;
    } catch (Exception exception) {
      Console.Error.WriteLine($"SC091 FAIL {exception.GetType().Name}: {exception.Message}");
      return 1;
    }
  }

  private static async Task RunLifecycleAsync(string outputRoot, CancellationToken cancellation) {
    string worldDirectory = Path.Combine(outputRoot, "World");
    Directory.CreateDirectory(worldDirectory);
    string primaryPath = Path.Combine(worldDirectory, "primary.wld");
    Require(!File.Exists(primaryPath), "SC091 requires a fresh output directory.");
    WorldFacts facts = WorldFacts.CreateVersion4();
    CreativeWorldPowerSnapshot powers = new(
        FreezeTime: false, ModifyTimeRate: 0.5f, FreezeRain: false, FreezeWind: true,
        DifficultySlider: 0.25f, StopBiomeSpread: true);
    var creation = new WorldCreationCoordinator(
        new WorldInitialDocumentBuilder(new CreativeWorldPowerDocumentCodec()));
    WorldPersistenceDocument fixture = creation.CreateDocument(new WorldCreationRequest(
        worldName: "P08 SC091 production lifecycle", seedText: "p08-sc091",
        uniqueId: Guid.Parse("8d99e0c2-a395-4e13-9cd9-6c5fd77c2f6f"), worldId: 91,
        maxTilesX: 2, maxTilesY: 2, spawnTileX: 1, spawnTileY: 1,
        compressedTilePayload: new byte[] { 0x40, 0x01, 0x40, 0x01 },
        frameImportant: facts.FrameImportant), powers);
    fixture = fixture.WithReplacements(
        WorldPersistenceSection.Create(
            WorldFileEventSection.SectionId,
            new WorldFileEventSection(
                combatBookWasUsed: true,
                lanternNightCooldown: 7,
                lanternNightGenuine: true,
                lanternNightManual: false,
                lanternNightNextNightIsGenuine: true)));
    var fileStore = new WorldFileStoreAdapter();
    var decoder = new WorldFileDocumentDecoder();
    var validator = new WorldFileDocumentValidator();
    var saver = new WorldDocumentSaver(
        fileStore, new WorldFileDocumentEncoder(), decoder, validator);
    WorldDocumentSaveResult fixtureSave = await saver.SaveAsync(new WorldDocumentSaveRequest(
        primaryPath, CommitVersion.Initial, fixture, MaximumWorldFileBytes,
        WorldBackupPolicy.Disabled), cancellation).ConfigureAwait(false);
    Require(fixtureSave.IsPrimaryCommitted, "Fixture creation did not commit the world document.");

    int port = ReserveLoopbackPort();
    P08BServerConfiguration configuration = P08BServerConfiguration.Parse(new[] {
      "--world", primaryPath, "--listen-port", port.ToString(CultureInfo.InvariantCulture)
    });
    var readinessBlockers = new List<string>();
    double committedTime;
    long mutationVersion;
    MinimalImplementation.Server.World.TileCoordinate persistedTileCoordinate = new(0, 0);
    WorldTile persistedTile = WorldTile.Empty;
    await using (var runtime = new P08BServerRuntime(configuration)) {
      P08BStartResult started = await runtime.StartAsync(cancellation).ConfigureAwait(false);
      ReportStart("initial", started);
      Require(started.WorldLoaded && started.WorldPublished && started.ListenerStarted &&
          started.SaveReady, "Default runtime did not load, publish, listen and initialize saving.");
      readinessBlockers.AddRange(started.Blockers);
      ServerConnectionHost transport = runtime.Composition.ConnectionHost
          ?? throw new InvalidOperationException("The real connection host is missing.");
      using var client = new TcpClient();
      await client.ConnectAsync(IPAddress.Loopback, port, cancellation).ConfigureAwait(false);
      await WaitUntilAsync(() => transport.ActiveConnectionCount == 1, cancellation)
          .ConfigureAwait(false);
      Console.WriteLine("SC091 listener accepted one real TCP connection; no Active fixture.");

      WorldSaveSnapshot before = await runtime.CaptureSaveSnapshotAsync(cancellation)
          .ConfigureAwait(false);
      if (started.TickStarted) {
        throw new InvalidOperationException(
            "SC091 must drive an admitted client before accepting autonomous game ticks.");
      }

      WorldTickResult tick = await runtime.AdvanceTickAsync(
          before.WorldEpoch, tick: 1, elapsedGameTicks: 30, dayRate: 1,
          liquidOperationBudget: 16, cancellationToken: cancellation).ConfigureAwait(false);
      Require(tick.Applied && tick.TimeChanged, tick.Rejection ?? "Real world tick did not advance.");

      WorldMutationResult? tileMutation = null;
      await runtime.Composition.CommitLoop.SubmitAsync(
          () => tileMutation = runtime.Composition.WorldOwner.ApplyTileDeltas(
              before.WorldEpoch,
              [new KeyValuePair<MinimalImplementation.Server.World.TileCoordinate, WorldTileDelta>(
                  persistedTileCoordinate,
                  new WorldTileDelta(Active: true, Type: 1))],
              producesImmediateBroadcast: false),
          cancellation).ConfigureAwait(false);
      Require(tileMutation is { Applied: true },
          tileMutation?.Rejection ?? "The real WorldOwner tile mutation was rejected.");

      WorldSaveSnapshot after = await runtime.CaptureSaveSnapshotAsync(cancellation)
          .ConfigureAwait(false);
      Require(after.Version.Value > before.Version.Value && after.Header.Time != before.Header.Time,
          "The real world owner did not publish a newer changed snapshot.");
      persistedTile = after.World.GetTile(persistedTileCoordinate);
      Require(persistedTile.Active && persistedTile.Type == 1,
          "The real WorldOwner did not retain the committed tile mutation.");
      committedTime = after.Header.Time;
      mutationVersion = after.Version.Value;
      Console.WriteLine($"SC091 real World tick applied version={mutationVersion} " +
          $"time={committedTime.ToString(CultureInfo.InvariantCulture)}; " +
          "manual commit-loop invocation does not establish nine-phase scheduler readiness.");
      P08BSaveResult saved = await runtime.SaveAsync(cancellation).ConfigureAwait(false);
      Require(saved.IsCommitted && saved.CapturedVersion?.Value >= mutationVersion,
          $"Runtime save failed or captured an older version: {saved.Detail}");
      Console.WriteLine($"SC091 runtime save committed version={saved.CapturedVersion?.Value}");

      P08BStopResult stopped = await runtime.StopAsync(
          save: true, deadline: TimeSpan.FromSeconds(10), cancellationToken: cancellation)
          .ConfigureAwait(false);
      Require(stopped.Succeeded && stopped.SaveResult?.IsCommitted == true &&
          stopped.SaveResult.CapturedVersion?.Value >= mutationVersion,
          $"Runtime stop/save failed at {stopped.Stage}: {stopped.Detail}");
      Require(!transport.IsRunning && transport.ActiveConnectionCount == 0,
          "Runtime stop retained the listener or its accepted connection.");
      Console.WriteLine($"SC091 runtime stop completed stage={stopped.Stage}");
    }

    WorldStorageReadResult bytes = await fileStore.ReadBoundedAsync(
        primaryPath, MaximumWorldFileBytes, cancellation).ConfigureAwait(false);
    Require(bytes.Succeeded && bytes.Data is not null, "Saved primary could not be read.");
    WorldPersistenceDecodeResult decoded = decoder.Decode(bytes.Data!);
    Require(decoded.Succeeded && decoded.Document is not null, "Saved primary could not be decoded.");
    WorldPersistenceDocument document = decoded.Document!;
    WorldStorageFailure validation = validator.Validate(document);
    Require(validation.Kind == WorldStorageFailureKind.None,
        $"Saved primary validation failed: {validation.Detail}");
    Require(document.TryGetSection<WorldFileEnvironmentSection>(
        WorldFileEnvironmentSection.SectionId, out var environment) && environment.IsPresent &&
        environment.Value.Time == committedTime, "Saved primary lost the committed world time.");
    Require(document.TryGetSection<WorldFileCreativePowersSection>(
        WorldFileCreativePowersSection.SectionId, out var creative) && creative.IsPresent &&
        new CreativeWorldPowerDocumentCodec().Decode(creative.Value.SerializedPayload) == powers,
        "Saved primary lost the typed Creative powers.");
    Require(document.TryGetSection<WorldFileEventSection>(
        WorldFileEventSection.SectionId, out var events) && events.IsPresent &&
        events.Value.CombatBookWasUsed && events.Value.LanternNightCooldown == 7 &&
        events.Value.LanternNightGenuine && !events.Value.LanternNightManual &&
        events.Value.LanternNightNextNightIsGenuine,
        "Saved primary lost the restored Lantern Night state.");
    Require(document.TryGetSection<WorldFileTilePayloadSection>(
        WorldFileTilePayloadSection.SectionId, out var savedTileSection) &&
        savedTileSection.IsPresent,
        "Saved primary lost the WorldFile tile payload section.");
    IReadOnlyList<WorldTile> savedTiles = new WorldFileTilePayloadDecoder().Decode(
        savedTileSection.Value, facts);
    Require(savedTiles[(persistedTileCoordinate.Y * savedTileSection.Value.MaxTilesX) +
        persistedTileCoordinate.X] == persistedTile,
        "The saved WorldFile tile payload did not encode the committed WorldOwner tile.");
    Console.WriteLine("SC091 saved WorldFile tile payload decoded the committed WorldOwner tile.");
    Require(Directory.GetFiles(worldDirectory, "*.tmp", SearchOption.AllDirectories).Length == 0,
        "Runtime left a staged world temporary file.");
    Console.WriteLine($"SC091 primary validated sha256=" +
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(primaryPath))));

    await using (var reloaded = new P08BServerRuntime(configuration)) {
      P08BStartResult restarted = await reloaded.StartAsync(cancellation).ConfigureAwait(false);
      ReportStart("reload", restarted);
      Require(restarted.WorldLoaded && restarted.WorldPublished && restarted.ListenerStarted,
          "Default runtime could not reload and listen on the released port.");
      WorldSaveSnapshot snapshot = await reloaded.CaptureSaveSnapshotAsync(cancellation)
          .ConfigureAwait(false);
      Require(snapshot.Header.Time == committedTime, "Reload did not restore the committed time.");
      Require(snapshot.World.GetTile(persistedTileCoordinate) == persistedTile,
          "Reload did not restore the committed WorldOwner tile.");
      var lanternNight = reloaded.Composition.LanternNightState.CaptureSnapshot();
      Require(lanternNight.LanternsUp && lanternNight.Cooldown == 7 &&
          lanternNight.NextNightIsLanternNight,
          "Reload did not restore the unique Lantern Night owner.");
      P08BStopResult stopped = await reloaded.StopAsync(
          save: false, cancellationToken: cancellation).ConfigureAwait(false);
      Require(stopped.Succeeded, $"Reload runtime failed to stop: {stopped.Detail}");
      readinessBlockers.AddRange(restarted.Blockers);
      Console.WriteLine("SC091 default runtime reloaded the saved tile and Lantern state and stopped.");
    }

    Require(readinessBlockers.Count == 0,
        "Production readiness remains incomplete after real lifecycle verification: " +
        string.Join(" | ", new HashSet<string>(readinessBlockers, StringComparer.Ordinal)));
  }

  private static void ReportStart(string label, P08BStartResult result) {
    Console.WriteLine($"SC091 {label} ready={result.IsReady} loaded={result.WorldLoaded} " +
        $"published={result.WorldPublished} listener={result.ListenerStarted} " +
        $"tick={result.TickStarted} save={result.SaveReady} exit={result.ExitCode}");
    foreach (string blocker in result.Blockers) {
      Console.WriteLine($"SC091 {label} blocker: {blocker}");
    }
  }

  private static async Task WaitUntilAsync(Func<bool> predicate, CancellationToken cancellation) {
    var elapsed = Stopwatch.StartNew();
    while (!predicate.Invoke()) {
      Require(elapsed.Elapsed < TimeSpan.FromSeconds(3), "Real TCP accept timed out.");
      await Task.Delay(10, cancellation).ConfigureAwait(false);
    }
  }

  private static int ReserveLoopbackPort() {
    var reservation = new TcpListener(IPAddress.Loopback, 0);
    reservation.Start();
    int port = ((IPEndPoint)reservation.LocalEndpoint).Port;
    reservation.Stop();
    return port;
  }

  private static string? ReadOption(IReadOnlyList<string> args, string option) {
    for (int index = 0; index < args.Count; index++) {
      if (args[index] == option) {
        if (index + 1 >= args.Count) {
          throw new ArgumentException($"Missing value for {option}.");
        }
        return args[index + 1];
      }
    }
    return null;
  }

  private static void Require(bool condition, string detail) {
    if (!condition) {
      throw new InvalidOperationException(detail);
    }
  }
}
