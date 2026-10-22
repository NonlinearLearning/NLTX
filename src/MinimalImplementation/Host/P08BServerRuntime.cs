using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinimalImplementation.Contracts;
using MinimalImplementation.Persistence;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Modules.Events;
using MinimalImplementation.Server.Persistence;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NonAuthoritative.Platform;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;
using TileCoordinate = MinimalImplementation.Server.World.TileCoordinate;

namespace MinimalImplementation.Server.Host;

/// <summary>Explicit command-line configuration for the P08-B runtime boundary.</summary>
public sealed class P08BServerConfiguration
{
  private const int DefaultCommitQueueCapacity = 1024;
  private const int DefaultMaximumWorldFileBytes = 64 * 1024 * 1024;
  private const int DefaultListenPort = 7777;
  private const int DefaultStopDeadlineMilliseconds = 15000;

  private P08BServerConfiguration(
    string listenAddress,
    int listenPort,
    string worldFilePath,
    int maximumWorldFileBytes,
    int commitQueueCapacity,
    TimeSpan stopDeadline,
    bool createWorld,
    bool saveOnStop,
    bool diagnosticNpcs)
  {
    ListenAddress = listenAddress;
    ListenPort = listenPort;
    WorldFilePath = worldFilePath;
    MaximumWorldFileBytes = maximumWorldFileBytes;
    CommitQueueCapacity = commitQueueCapacity;
    StopDeadline = stopDeadline;
    CreateWorld = createWorld;
    SaveOnStop = saveOnStop;
    DiagnosticNpcs = diagnosticNpcs;
  }

  public string ListenAddress { get; }

  public int ListenPort { get; }

  public string WorldFilePath { get; }

  public int MaximumWorldFileBytes { get; }

  public int CommitQueueCapacity { get; }

  public TimeSpan StopDeadline { get; }

  public bool CreateWorld { get; }

  public bool SaveOnStop { get; }

  public bool DiagnosticNpcs { get; }

  public static bool IsHelpRequested(IReadOnlyList<string> args)
  {
    ArgumentNullException.ThrowIfNull(args);
    return args.Count == 1 && args[0] is "--help" or "-h";
  }

  public static P08BServerConfiguration Parse(IReadOnlyList<string> args)
  {
    ArgumentNullException.ThrowIfNull(args);

    string listenAddress = "127.0.0.1";
    int listenPort = DefaultListenPort;
    string? worldFilePath = null;
    int maximumWorldFileBytes = DefaultMaximumWorldFileBytes;
    int commitQueueCapacity = DefaultCommitQueueCapacity;
    int stopDeadlineMilliseconds = DefaultStopDeadlineMilliseconds;
    bool createWorld = false;
    bool saveOnStop = true;
    bool diagnosticNpcs = false;
    var seenOptions = new HashSet<string>(StringComparer.Ordinal);

    for (int index = 0; index < args.Count; index++)
    {
      string option = args[index];
      switch (option)
      {
        case "--listen-address":
          listenAddress = ReadStringValue(args, ref index, option, seenOptions);
          break;
        case "--listen-port":
          listenPort = ReadIntValue(args, ref index, option, seenOptions);
          break;
        case "--world":
          worldFilePath = ReadStringValue(args, ref index, option, seenOptions);
          break;
        case "--max-world-bytes":
          maximumWorldFileBytes = ReadIntValue(args, ref index, option, seenOptions);
          break;
        case "--commit-queue-capacity":
          commitQueueCapacity = ReadIntValue(args, ref index, option, seenOptions);
          break;
        case "--stop-deadline-ms":
          stopDeadlineMilliseconds = ReadIntValue(args, ref index, option, seenOptions);
          break;
        case "--create-world":
          AddFlag(option, seenOptions);
          createWorld = true;
          break;
        case "--no-save-on-stop":
          AddFlag(option, seenOptions);
          saveOnStop = false;
          break;
        case "--diagnostic-npcs":
          AddFlag(option, seenOptions);
          diagnosticNpcs = true;
          break;
        case "--help":
        case "-h":
          throw new ArgumentException("Help cannot be combined with other arguments.");
        default:
          throw new ArgumentException($"Unknown option '{option}'.", nameof(args));
      }
    }

    if (string.IsNullOrWhiteSpace(worldFilePath))
    {
      throw new ArgumentException("--world is required.", nameof(args));
    }

    if (listenPort is < 0 or > ushort.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(listenPort));
    }

    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumWorldFileBytes);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(commitQueueCapacity);
    if (stopDeadlineMilliseconds <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(stopDeadlineMilliseconds));
    }

    return new P08BServerConfiguration(
      listenAddress,
      listenPort,
      Path.GetFullPath(worldFilePath),
      maximumWorldFileBytes,
      commitQueueCapacity,
      TimeSpan.FromMilliseconds(stopDeadlineMilliseconds),
      createWorld,
      saveOnStop,
      diagnosticNpcs);
  }

  public ServerOptions ToServerOptions()
  {
    return new ServerOptions(
      ListenAddress,
      ListenPort,
      WorldFilePath,
      MaximumWorldFileBytes,
      CommitQueueCapacity);
  }

  public static void WriteUsage(TextWriter writer)
  {
    ArgumentNullException.ThrowIfNull(writer);
    writer.WriteLine(
      "Usage: MinimalImplementation.Server --world <path> " +
      "[--listen-address <ip>] [--listen-port <0..65535>] " +
      "[--max-world-bytes <bytes>] [--commit-queue-capacity <count>] " +
      "[--stop-deadline-ms <milliseconds>] [--create-world] [--no-save-on-stop] " +
      "[--diagnostic-npcs]");
  }

  private static string ReadStringValue(
    IReadOnlyList<string> args,
    ref int index,
    string option,
    ISet<string> seenOptions)
  {
    AddFlag(option, seenOptions);
    if (++index >= args.Count || string.IsNullOrWhiteSpace(args[index]))
    {
      throw new ArgumentException($"Option '{option}' requires a value.", nameof(args));
    }

    return args[index];
  }

  private static int ReadIntValue(
    IReadOnlyList<string> args,
    ref int index,
    string option,
    ISet<string> seenOptions)
  {
    string value = ReadStringValue(args, ref index, option, seenOptions);
    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
    {
      throw new ArgumentException(
        $"Option '{option}' requires an invariant decimal integer.",
        nameof(args));
    }

    return result;
  }

  private static void AddFlag(string option, ISet<string> seenOptions)
  {
    if (!seenOptions.Add(option))
    {
      throw new ArgumentException($"Option '{option}' was provided more than once.");
    }
  }
}

/// <summary>
/// Runs the part of P08-B that is available in the current Server project.
/// The default constructor supplies the built-in Version4 inbound router; external integrations
/// may still replace that router through the existing callback surface.
/// </summary>
public sealed class P08BServerRuntime : IAsyncDisposable
{

  private static readonly IReadOnlyList<string> G2Blockers = Array.AsReadOnly(
    new[]
    {
      "The built-in Version4 inbound dispatcher/admission callback set is incomplete.",
      "The authoritative tick scheduler was not started because the nine-phase owner catalog " +
      "is incomplete.",
    });

  private readonly object _gate = new();
  private readonly SemaphoreSlim _lifecycleGate = new(1, 1);
  private readonly P08BServerConfiguration _configuration;
  private readonly P08BHostStopBoundary _stopBoundary;
  private readonly P08BServerRuntimeIntegrations? _integrations;
  private readonly P08BWorldSnapshotProjector _snapshotProjector = new();
  private readonly WorldAttachmentPersistenceProjector _attachmentProjector = new();
  private readonly CreativeWorldPowerDocumentCodec _creativeWorldPowerCodec = new();
  private readonly WorldStopCoordinator _stopCoordinator;
  private ServerComposition? _composition;
  private WorldTickCoordinator? _worldTick;
  private WorldTickScheduler? _tickScheduler;
  private Task? _tickRunTask;
  private ServerOutboundDeliveryPump? _outboundPump;
  private WorldFileStoreAdapter? _fileStore;
  private WorldDocumentSaver? _documentSaver;
  private WorldDocumentSaveCoordinator? _saveCoordinator;
  private WorldPersistenceDocument? _loadedDocument;
  private WorldSaveSnapshot? _baselineSnapshot;
  private P08BRuntimeState _state = P08BRuntimeState.Created;
  private bool _commitLoopStarted;
  private bool _worldPublished;
  private bool _tickAdmissionStopped;
  private bool _listenerStarted;
  private bool _inboundDispatcherComplete;
  private bool _tickStarted;
  private bool _saveReady;
  private bool _disposed;

  public P08BServerRuntime(P08BServerConfiguration configuration)
    : this(configuration, integrations: null)
  {
  }

  public P08BServerRuntime(
    P08BServerConfiguration configuration,
    P08BServerRuntimeIntegrations? integrations)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    _configuration = configuration;
    _integrations = integrations;
    _stopBoundary = new P08BHostStopBoundary(this);
    _stopCoordinator = new WorldStopCoordinator(_stopBoundary, SaveDocumentAsync);
  }

  public ServerComposition Composition => _composition ?? throw new InvalidOperationException(
    "The server composition is not available before runtime startup.");

  public async Task<P08BStartResult> StartAsync(CancellationToken cancellationToken = default)
  {
    await _lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return await StartCoreAsync(cancellationToken).ConfigureAwait(false);
    }
    finally
    {
      _lifecycleGate.Release();
    }
  }

  private async Task<P08BStartResult> StartCoreAsync(CancellationToken cancellationToken)
  {
    lock (_gate)
    {
      ObjectDisposedException.ThrowIf(_disposed, this);
      if (_state != P08BRuntimeState.Created)
      {
        throw new InvalidOperationException("The P08-B runtime can only be started once.");
      }

      _state = P08BRuntimeState.Starting;
    }

    try
    {
      ServerLog.Info(
        "server.runtime.starting",
        $"world={ServerLog.Quote(_configuration.WorldFilePath)} " +
        $"createWorld={_configuration.CreateWorld}");
      ServerComposition composition = ServerComposition.Create(_configuration.ToServerOptions());
      lock (_gate)
      {
        _composition = composition;
      }

      await composition.CommitLoop.StartAsync().ConfigureAwait(false);
      _commitLoopStarted = true;
      _worldTick = new WorldTickCoordinator(composition.WorldOwner);
      InitializePersistence(composition);

      if (_configuration.CreateWorld)
      {
        ServerLog.Info(
          "server.world.create.started",
          $"path={ServerLog.Quote(_configuration.WorldFilePath)}");
        string? creationFailure = await TryCreateWorldDocumentAsync(cancellationToken)
          .ConfigureAwait(false);
        if (creationFailure is not null)
        {
          ServerLog.Warning(
            "server.world.create.failed",
            ServerLog.Quote(creationFailure));
          P08BStartResult blocked = CreateBlockedStartResult(
            worldLoaded: false,
            worldPublished: false,
            creationFailure);
          SetState(P08BRuntimeState.Blocked);
          return blocked;
        }

        ServerLog.Info(
          "server.world.create.completed",
          $"path={ServerLog.Quote(_configuration.WorldFilePath)}");
      }

      ServerLog.Info(
        "server.world.load.started",
        $"path={ServerLog.Quote(_configuration.WorldFilePath)}");
      WorldDocumentLoadResult load = await composition
        .LoadConfiguredWorldDocumentAsync(cancellationToken)
        .ConfigureAwait(false);
      if (!load.IsValidated || load.Document is null)
      {
        string detail = load.Failure.Detail is null
          ? $"World document load failed with {load.Failure.Kind}."
          : $"World document load failed with {load.Failure.Kind}: {load.Failure.Detail}";
        ServerLog.Warning("server.world.load.failed", ServerLog.Quote(detail));
        P08BStartResult blocked = CreateBlockedStartResult(false, false, detail);
        SetState(P08BRuntimeState.Blocked);
        return blocked;
      }

      _loadedDocument = load.Document;
      ServerLog.Info("server.world.load.validated", "The world document passed validation.");

      WorldPreparedWorld candidate;
      try
      {
        candidate = composition.PrepareWorldCandidate(load.Document);
      }
      catch (InvalidDataException exception)
      {
        string detail = DescribeCandidatePreparationFailure(load.Document, exception.Message);
        ServerLog.Warning("server.world.prepare.failed", ServerLog.Quote(detail));
        P08BStartResult blocked = CreateBlockedStartResult(
          worldLoaded: true,
          worldPublished: false,
          $"World candidate preparation failed: {detail}");
        SetState(P08BRuntimeState.Blocked);
        return blocked;
      }

      ServerLog.Info(
        "server.world.prepared",
        $"size={candidate.Header.Geometry.Width}x{candidate.Header.Geometry.Height} " +
        $"spawn={candidate.Header.Spawn} activeTiles={candidate.Tiles.Count(tile => tile.Active)}");

      WorldPublicationResult? publication = null;
      string? attachmentFailure = null;
      await composition.CommitLoop.SubmitAsync(
        () =>
        {
          publication = composition.WorldOwner.CommitPreparedWorld(candidate);
          if (publication.Published)
          {
            try
            {
              composition.RestoreWorldAttachments(
                load.Document,
                composition.WorldOwner.CaptureSnapshot());
              composition.RestoreWorldModules(
                load.Document, composition.WorldOwner.CaptureWorldHeader().Version);
              _baselineSnapshot = composition.WorldOwner.CaptureSaveSnapshot();
              Volatile.Write(ref _worldPublished, true);
            }
            catch (Exception exception) when (IsAttachmentHydrationFailure(exception))
            {
              attachmentFailure = exception.Message;
              WorldPublicationResult retirement = composition.WorldOwner.UnloadWorld();
              if (!retirement.Published)
              {
                throw new InvalidOperationException(
                  "The published world could not be retired after attachment hydration failed.",
                  exception);
              }
            }
          }
        },
        cancellationToken).ConfigureAwait(false);
      if (attachmentFailure is not null)
      {
        ServerLog.Warning(
          "server.world.attachments.failed",
          ServerLog.Quote(attachmentFailure));
        P08BStartResult blocked = CreateBlockedStartResult(
          worldLoaded: true,
          worldPublished: false,
          $"World attachment hydration failed: {attachmentFailure}");
        SetState(P08BRuntimeState.Blocked);
        return blocked;
      }

      if (publication is null || !publication.Published)
      {
        string detail = publication?.Rejection ?? "The prepared world was not published.";
        ServerLog.Warning("server.world.publish.failed", ServerLog.Quote(detail));
        P08BStartResult blocked = CreateBlockedStartResult(
          worldLoaded: true,
          worldPublished: false,
          detail);
        SetState(P08BRuntimeState.Blocked);
        return blocked;
      }

      ServerLog.Info("server.world.published", "The prepared world is active.");

      if (_configuration.DiagnosticNpcs) {
        await composition.CommitLoop.SubmitAsync(
            () => NpcDiagnosticSpawn.Create(composition), cancellationToken).ConfigureAwait(false);
      }

      ServerLog.Info("server.integrations.starting", "Preparing connection and tick services.");
      string? integrationFailure = await StartOptionalIntegrationsAsync(composition)
        .ConfigureAwait(false);
      if (integrationFailure is not null)
      {
        ServerLog.Warning("server.integrations.blocked", ServerLog.Quote(integrationFailure));
      }

      P08BStartResult result = CreateStartResult(
        worldLoaded: true,
        worldPublished: true,
        integrationFailure);
      SetState(result.IsReady ? P08BRuntimeState.Running : P08BRuntimeState.Blocked);
      if (result.IsReady)
      {
        ServerLog.Info("server.runtime.started", "World, listener, tick scheduler, and save service are ready.");
      }

      return result;
    }
    catch
    {
      SetState(P08BRuntimeState.Failed);
      throw;
    }
  }

  public async Task WaitForShutdownAsync(CancellationToken cancellationToken = default)
  {
    ServerConnectionHost? connectionHost = _composition?.ConnectionHost;
    if (connectionHost is null)
    {
      throw new InvalidOperationException(
        "A real connection host is not attached; the runtime cannot wait for listener shutdown.");
    }

    Task listenerCompletion = connectionHost.Completion;
    var workers = new List<Task> { listenerCompletion };
    if (_tickRunTask is not null)
    {
      workers.Add(_tickRunTask);
    }

    if (_outboundPump is not null)
    {
      workers.Add(_outboundPump.Completion);
    }

    Task completed = await Task.WhenAny(workers).WaitAsync(cancellationToken)
      .ConfigureAwait(false);
    try
    {
      await completed.ConfigureAwait(false);
    }
    catch (OperationCanceledException) when (
      ReferenceEquals(completed, _tickRunTask) && Volatile.Read(ref _tickAdmissionStopped))
    {
      // The stop boundary cancels the scheduler before draining and closing transport.
    }

    if (!ReferenceEquals(completed, listenerCompletion) &&
        !Volatile.Read(ref _tickAdmissionStopped))
    {
      string worker = ReferenceEquals(completed, _tickRunTask)
        ? "The authoritative tick scheduler"
        : "The outbound delivery pump";
      throw new InvalidOperationException($"{worker} stopped before runtime shutdown.");
    }

    await listenerCompletion.WaitAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>Advances one real P03 world tick on the existing commit loop.</summary>
  public async Task<WorldTickResult> AdvanceTickAsync(
    ulong worldEpoch,
    long tick,
    double elapsedGameTicks,
    int dayRate,
    int liquidOperationBudget,
    CancellationToken cancellationToken = default)
  {
    WorldTickCoordinator worldTick = _worldTick ?? throw new InvalidOperationException(
      "The world tick coordinator is not available before runtime startup.");
    if (!Volatile.Read(ref _worldPublished))
    {
      throw new InvalidOperationException("A world must be published before it can tick.");
    }

    if (Volatile.Read(ref _tickAdmissionStopped))
    {
      throw new InvalidOperationException("World tick admission has stopped.");
    }

    if (Volatile.Read(ref _tickStarted))
    {
      throw new InvalidOperationException("The autonomous scheduler owns world tick indices.");
    }

    WorldTickResult? result = null;
    await Composition.CommitLoop.SubmitAsync(
      () =>
      {
        if (Volatile.Read(ref _tickAdmissionStopped))
        {
          throw new InvalidOperationException("World tick admission has stopped.");
        }

        if (Volatile.Read(ref _tickStarted))
        {
          throw new InvalidOperationException("The autonomous scheduler owns world tick indices.");
        }

        WorldTickResult tickResult = worldTick.Advance(
          worldEpoch,
          tick,
          elapsedGameTicks,
          dayRate,
          liquidOperationBudget);
        result = tickResult;
        if (!tickResult.Applied)
        {
          string detail = tickResult.Rejection ??
            "The world tick coordinator rejected the transaction.";
          throw new InvalidOperationException($"The direct world tick was rejected: {detail}");
        }

        Composition.WorldTickInvasionBridge.Consume(tickResult);
      },
      cancellationToken).ConfigureAwait(false);
    return result ?? throw new InvalidOperationException("The world tick did not produce a result.");
  }

  /// <summary>Captures and queues one authoritative packet 77 world broadcast.</summary>
  public async Task<bool> BroadcastTemporaryAnimationAsync(
      SessionEpoch sender,
      short animationType,
      ushort tileType,
      TileCoordinate coordinate,
      CancellationToken cancellationToken = default)
  {
    if (!sender.IsValid)
    {
      throw new ArgumentException(
        "A temporary-animation broadcast requires a valid sender epoch.",
        nameof(sender));
    }

    if (!Volatile.Read(ref _worldPublished))
    {
      throw new InvalidOperationException("A world must be published before packet 77 can be sent.");
    }

    if (!Volatile.Read(ref _commitLoopStarted))
    {
      throw new InvalidOperationException(
        "The server commit loop must be started before packet 77 can be sent.");
    }

    ServerOutboundDeliveryPump outboundPump = _outboundPump ?? throw new InvalidOperationException(
      "The outbound delivery pump must be started before packet 77 can be sent.");
    bool enqueued = false;
    await Composition.CommitLoop.SubmitAsync(
      () =>
      {
        if (!Composition.SessionAdmission.TryGetStage(sender, out SessionStage stage) ||
            stage != SessionStage.Active)
        {
          return;
        }

        WorldHeaderCapture header = Composition.WorldOwner.CaptureWorldHeader();
        if (!Composition.WorldOwner.TryCreateTemporaryAnimationIntent(
          header.WorldEpoch,
          animationType,
          tileType,
          coordinate,
          out WorldTemporaryAnimationIntent intent))
        {
          return;
        }

        IReadOnlyList<SessionEpoch> recipients =
          Composition.SessionAdmission.CaptureActiveSessionEpochs();
        if (!recipients.Contains(sender))
        {
          return;
        }

        OutboundPacketIntent outbound = Composition.WorldTemporaryAnimationPacketProducer
          .CreateIntent(intent, recipients, intent.Version);
        var settlement = new WorldTemporaryAnimationDeliverySettlement(
          outbound,
          intent.WorldEpoch);
        enqueued = outboundPump.TryEnqueue(
          settlement,
          () => Volatile.Read(ref _worldPublished) &&
            Composition.WorldOwner.IsCurrentWorldEpoch(settlement.WorldEpoch));
      },
      cancellationToken).ConfigureAwait(false);
    return enqueued;
  }

  /// <summary>Captures the authoritative world snapshot on the commit boundary.</summary>
  public async Task<WorldSaveSnapshot> CaptureSaveSnapshotAsync(
    CancellationToken cancellationToken = default)
  {
    if (!Volatile.Read(ref _worldPublished))
    {
      throw new InvalidOperationException("A world must be published before it can be captured.");
    }

    WorldSaveSnapshot? snapshot = null;
    await Composition.CommitLoop.SubmitAsync(
      () => snapshot = Composition.WorldOwner.CaptureSaveSnapshot(),
      cancellationToken).ConfigureAwait(false);
    return snapshot ?? throw new InvalidOperationException("The save snapshot was not captured.");
  }

  /// <summary>Captures on the commit loop, projects all 27 sections and commits the document.</summary>
  public async Task<P08BSaveResult> SaveAsync(CancellationToken cancellationToken = default)
  {
    WorldSaveSnapshot snapshot = await CaptureSaveSnapshotAsync(cancellationToken)
      .ConfigureAwait(false);
    WorldDocumentSaveCoordinator saveCoordinator = _saveCoordinator ?? throw new InvalidOperationException(
      "The world document save coordinator is not available before runtime startup.");
    WorldDocumentSaveResult result = await saveCoordinator.SaveAsync(
      ToWorldScope(snapshot.WorldEpoch),
      snapshot.Version,
      _configuration.WorldFilePath,
      _configuration.MaximumWorldFileBytes,
      WorldBackupPolicy.KeepPreviousPrimary,
      cancellationToken).ConfigureAwait(false);
    return P08BSaveResult.From(result);
  }

  public async Task<P08BStopResult> StopAsync(
    bool save,
    TimeSpan? deadline = null,
    CancellationToken cancellationToken = default)
  {
    await _lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return await StopCoreAsync(save, deadline, cancellationToken).ConfigureAwait(false);
    }
    finally
    {
      _lifecycleGate.Release();
    }
  }

  private async Task<P08BStopResult> StopCoreAsync(
    bool save,
    TimeSpan? deadline,
    CancellationToken cancellationToken)
  {
    TimeSpan effectiveDeadline = deadline ?? _configuration.StopDeadline;
    if (effectiveDeadline <= TimeSpan.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(deadline));
    }

    WorldStopResult result = await _stopCoordinator.StopAsync(
      save,
      effectiveDeadline,
      cancellationToken).ConfigureAwait(false);
    SetState(result.IsStopped ? P08BRuntimeState.Stopped : P08BRuntimeState.Failed);
    if (result.IsStopped)
    {
      ServerLog.Info("server.runtime.stopped", $"saveRequested={save}");
    }
    else
    {
      ServerLog.Warning(
        "server.runtime.stop.failed",
        $"stage={result.FailedStage} detail={ServerLog.Quote(result.FailureDetail ?? string.Empty)}");
    }

    return P08BStopResult.From(result);
  }

  public async ValueTask DisposeAsync()
  {
    lock (_gate)
    {
      if (_disposed)
      {
        return;
      }

      _disposed = true;
    }

    await StopAsync(save: false, _configuration.StopDeadline).ConfigureAwait(false);
    if (_tickScheduler is not null)
    {
      await _tickScheduler.DisposeAsync().ConfigureAwait(false);
      _tickScheduler = null;
    }

    if (_composition is not null)
    {
      await _composition.DisposeAsync().ConfigureAwait(false);
    }
  }

  internal async Task StopAdmissionsAsync(CancellationToken cancellationToken)
  {
    ServerConnectionHost? connectionHost = _composition?.ConnectionHost;
    if (connectionHost is not null)
    {
      await connectionHost.StopAcceptingAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
      connectionHost.StopPacketAdmission();
    }
  }

  internal async Task StopTickAdmissionAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    Volatile.Write(ref _tickAdmissionStopped, true);
    if (_tickScheduler is not null)
    {
      await _tickScheduler.StopAsync().AsTask().WaitAsync(cancellationToken)
        .ConfigureAwait(false);
    }
  }

  internal async Task DrainAcceptedCommandsAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    ServerConnectionHost? connectionHost = _composition?.ConnectionHost;
    if (connectionHost is not null)
    {
      await connectionHost.DrainAcceptedPacketsAsync(cancellationToken).ConfigureAwait(false);
    }

    if (_outboundPump is not null)
    {
      await _outboundPump.CompleteAsync(cancellationToken).ConfigureAwait(false);
    }

    if (_commitLoopStarted)
    {
      await Composition.CommitLoop.SubmitAsync(
        static () => { },
        cancellationToken).ConfigureAwait(false);
    }
  }

  internal async Task CloseTransportAsync(CancellationToken cancellationToken)
  {
    ServerConnectionHost? connectionHost = _composition?.ConnectionHost;
    if (connectionHost is not null)
    {
      await connectionHost.DisposeAsync().AsTask().WaitAsync(cancellationToken)
        .ConfigureAwait(false);
    }
  }

  internal async Task RetireWorldAndResourcesAsync(CancellationToken cancellationToken)
  {
    if (!Volatile.Read(ref _worldPublished) || !Volatile.Read(ref _commitLoopStarted))
    {
      return;
    }

    WorldPublicationResult? retirement = null;
    await Composition.CommitLoop.SubmitAsync(
      () => {
        OwnerCommitResult inventoryRetirement = Composition.WorldItemLifecycle.RetireWorldScope(
          new WorldScopeId(Composition.IdentityRegistry.CurrentWorldScope));
        if (inventoryRetirement.Status != OwnerCommitStatus.Applied) {
          throw new InvalidDataException(
            inventoryRetirement.Detail ?? "World item state could not be retired.");
        }

        retirement = Composition.WorldOwner.UnloadWorld();
      },
      cancellationToken).ConfigureAwait(false);
    if (retirement is null || !retirement.Published)
    {
      throw new InvalidOperationException(
        retirement?.Rejection ?? "The published world could not be retired.");
    }

    Volatile.Write(ref _worldPublished, false);
  }

  internal async Task StopCommitLoopAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (Volatile.Read(ref _commitLoopStarted))
    {
      await Composition.CommitLoop.StopAsync().AsTask().WaitAsync(cancellationToken)
        .ConfigureAwait(false);
      Volatile.Write(ref _commitLoopStarted, false);
    }
  }

  private void InitializePersistence(ServerComposition composition)
  {
    _fileStore = new WorldFileStoreAdapter();
    _documentSaver = new WorldDocumentSaver(
      _fileStore,
      new WorldFileDocumentEncoder(),
      new WorldFileDocumentDecoder(),
      new WorldFileDocumentValidator());
    _saveCoordinator = new WorldDocumentSaveCoordinator(
      new ServerLoopCommitQueue(composition.CommitLoop),
      _documentSaver,
      CaptureDocumentForSave);
    _saveReady = true;
  }

  private async Task<string?> TryCreateWorldDocumentAsync(CancellationToken cancellationToken)
  {
    WorldFileStoreAdapter fileStore = _fileStore ?? throw new InvalidOperationException(
      "The world file store is not available before persistence initialization.");
    WorldStorageReadResult existing = await fileStore.ReadBoundedAsync(
      _configuration.WorldFilePath,
      _configuration.MaximumWorldFileBytes,
      cancellationToken).ConfigureAwait(false);
    if (existing.Succeeded)
    {
      return "Create-world refused to overwrite an existing world document.";
    }

    if (existing.Failure.Kind != WorldStorageFailureKind.Missing)
    {
      return $"Create-world could not inspect the target path: " +
        DescribeStorageFailure(existing.Failure);
    }

    string stage = "generation";
    try {
      WorldFacts facts = Composition.Version4WorldFacts;
      Guid uniqueId = Guid.NewGuid();
      DateTime creationTime = DateTime.UtcNow;
      WorldPersistenceDocument document = await Task.Run(() => {
        ServerLog.Info("server.world.generate.started", "seed=12345 size=Small difficulty=0");
        NSSLC.WorldGeneration.GeneratedWorld generated = NSSLC.WorldGeneration.WorldCreation.Create(
          new NSSLC.WorldGeneration.WorldCreationRequest("12345"),
          pass => ServerLog.Info("server.world.generate.pass", $"name={ServerLog.Quote(pass)}"),
          cancellationToken);
        ServerLog.Info(
          "server.world.generate.completed",
          $"size={generated.Width}x{generated.Height} " +
          $"spawn=({generated.SpawnX},{generated.SpawnY}) surface={generated.Surface} " +
          $"rockLayer={generated.RockLayer} chests={generated.Chests.Count} " +
          $"npcs={generated.Npcs.Count} passes={generated.Passes.Count}");
        stage = "projection";
        ServerLog.Info("server.world.project.started", "version=326");
        WorldPersistenceDocument projected = GeneratedWorldDocumentFactory.Create(
          generated, facts, "Generated world", uniqueId, creationTime, cancellationToken);
        ServerLog.Info("server.world.project.completed", "version=326");
        return projected;
      }, cancellationToken).ConfigureAwait(false);

      stage = "save";
      ServerLog.Info("server.world.save.started", "reason=create version=326");
      var documentSaver = new WorldDocumentSaver(
        new WorldFileStoreAdapter(overwriteExistingFiles: false),
        new WorldFileDocumentEncoder(),
        new WorldFileDocumentDecoder(),
        new WorldFileDocumentValidator());
      WorldDocumentSaveResult result = await documentSaver.SaveAsync(
        new WorldDocumentSaveRequest(
          _configuration.WorldFilePath,
          CommitVersion.Initial,
          document,
          _configuration.MaximumWorldFileBytes,
          WorldBackupPolicy.Disabled),
        cancellationToken).ConfigureAwait(false);
      if (!result.IsPrimaryCommitted) {
        return $"Create-world could not commit the generated document: {DescribeSaveFailure(result)}";
      }
      ServerLog.Info("server.world.save.completed", $"reason=create bytes={result.PayloadLength}");
      return null;
    } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
      ServerLog.Warning("server.world.create.canceled", $"stage={stage}");
      throw;
    } catch (Exception exception) {
      ServerLog.Error(
        "server.world.create.stage.failed", $"stage={stage} {exception.GetType().Name}: {exception.Message}");
      throw;
    }
  }

  private async Task<string?> StartOptionalIntegrationsAsync(ServerComposition composition)
  {
    if (_integrations is null)
    {
      WorldPersistenceDocument loadedDocument = _loadedDocument ?? throw new InvalidOperationException(
        "The loaded world document is unavailable while attaching the default router.");
      var worldDataSupplementProvider = new WorldDataPacketSupplementProvider(loadedDocument);
      var sectionAttachmentProvider = new WorldSectionAttachmentProvider(
        loadedDocument,
        composition.ChestStore,
        composition.SignRegistry,
        composition.TileEntityRegistry,
        composition.TileEntityProjection,
        new WorldScopeId(composition.MechanismWorldScope));
      var router = new Version4InboundRouter(
        composition,
        worldDataSupplementProvider.Capture,
        sectionAttachmentProvider.Capture);
      ServerConnectionHost defaultConnectionHost = composition.AttachConnectionHost(
        composition.Version4WireProfile,
        router.AllocateEpoch,
        router.CreateEnvelope,
        router.OnPacket,
        router.OnClosed,
        router.OnConnectionFailure,
        onConnected: router.OnConnected);
      router.AttachConnectionHost(defaultConnectionHost);
      _outboundPump = new ServerOutboundDeliveryPump(
        composition.CommitLoop, defaultConnectionHost, _configuration.CommitQueueCapacity);

      _inboundDispatcherComplete = router.IsComplete;
      if (!_inboundDispatcherComplete)
      {
        await _outboundPump.CompleteAsync().ConfigureAwait(false);
        _outboundPump = null;
        return "The built-in Version4 dispatcher/admission callback set is incomplete.";
      }

      IReadOnlyCollection<IServerTickPhaseOwner> tickPhaseOwners =
        DefaultServerTickPhaseOwners.Create(composition, _worldTick!, _outboundPump);
      ServerTickPhaseOwnerCatalog ownerCatalog =
        ServerTickPhaseOwnerCatalog.Capture(tickPhaseOwners);
      if (!ownerCatalog.IsComplete)
      {
        await _outboundPump.CompleteAsync().ConfigureAwait(false);
        _outboundPump = null;
        return ownerCatalog.DescribeBlocker();
      }

      try
      {
        _tickScheduler = new WorldTickScheduler(
          new ServerLoopCommitQueue(composition.CommitLoop),
          ownerCatalog.Owners,
          () => composition.SessionAdmission.CaptureActiveSessionEpochs().Count > 0);
      }
      catch (ArgumentException exception)
      {
        await _outboundPump.CompleteAsync().ConfigureAwait(false);
        _outboundPump = null;
        return "The built-in tick phase owners are not a complete authoritative set: " +
          exception.Message;
      }

      await defaultConnectionHost.StartAsync().ConfigureAwait(false);
      _listenerStarted = defaultConnectionHost.IsRunning;
      ServerLog.Info(
        "server.listener.started",
        $"running={_listenerStarted} address={_configuration.ListenAddress}:{_configuration.ListenPort}");
      await composition.CommitLoop.SubmitAsync(
        () => Volatile.Write(ref _tickStarted, true)).ConfigureAwait(false);
      _tickRunTask = _tickScheduler.StartAsync();
      ServerLog.Info("server.tick.started", "The authoritative tick scheduler is running.");
      return null;
    }

    if (_integrations.TickPhaseOwners.Count == 0)
    {
      return "No authoritative tick phase owners were supplied; listener startup was withheld.";
    }

    ServerConnectionHost connectionHost = composition.AttachConnectionHost(
      composition.Version4WireProfile,
      _integrations.AllocateEpoch,
      _integrations.CreateEnvelope,
      _integrations.OnPacket,
      _integrations.OnClosed,
      _integrations.OnConnectionFailure,
      onConnected: _integrations.OnConnected);
    ServerOutboundDeliveryPump outboundPump = new(
      composition.CommitLoop, connectionHost, _configuration.CommitQueueCapacity);
    _outboundPump = outboundPump;
    var moduleSync = new ModulePeriodicSyncOwner(composition, outboundPump);

    try
    {
      Func<bool> hasActiveClients = _integrations.HasActiveClients ??
        (() => composition.SessionAdmission.CaptureActiveSessionEpochs().Count > 0);
      IReadOnlyCollection<IServerTickPhaseOwner> tickPhaseOwners =
        ComposeTickPhaseOwners(composition, _integrations.TickPhaseOwners, moduleSync);
      ServerTickPhaseOwnerCatalog ownerCatalog =
        ServerTickPhaseOwnerCatalog.Capture(tickPhaseOwners);
      if (!ownerCatalog.IsComplete)
      {
        await outboundPump.CompleteAsync().ConfigureAwait(false);
        _outboundPump = null;
        return ownerCatalog.DescribeBlocker();
      }

      _tickScheduler = new WorldTickScheduler(
        new ServerLoopCommitQueue(composition.CommitLoop),
        ownerCatalog.Owners,
        hasActiveClients);
    }
    catch (ArgumentException exception)
    {
      await outboundPump.CompleteAsync().ConfigureAwait(false);
      _outboundPump = null;
      return $"The supplied tick phase owners are not a complete authoritative set: " +
        exception.Message;
    }

    await connectionHost.StartAsync().ConfigureAwait(false);
    _listenerStarted = connectionHost.IsRunning;
    ServerLog.Info(
      "server.listener.started",
      $"running={_listenerStarted} address={_configuration.ListenAddress}:{_configuration.ListenPort}");
    _inboundDispatcherComplete = true;
    await composition.CommitLoop.SubmitAsync(
      () => Volatile.Write(ref _tickStarted, true)).ConfigureAwait(false);
    _tickRunTask = _tickScheduler.StartAsync();
    ServerLog.Info("server.tick.started", "The authoritative tick scheduler is running.");
    return null;
  }

  private static IReadOnlyCollection<IServerTickPhaseOwner> ComposeTickPhaseOwners(
    ServerComposition composition,
    IReadOnlyCollection<IServerTickPhaseOwner> suppliedOwners,
    ModulePeriodicSyncOwner moduleSync) {
    ArgumentNullException.ThrowIfNull(composition);
    ArgumentNullException.ThrowIfNull(suppliedOwners);
    var owners = new List<IServerTickPhaseOwner>(suppliedOwners.Count + 2);
    owners.AddRange(suppliedOwners);
    if (!CoversPhase(suppliedOwners, ServerTickPhase.Projectile)) {
      owners.Add(composition.ProjectileTickPhaseOwner);
    }
    if (!CoversPhase(suppliedOwners, ServerTickPhase.PeriodicSync)) {
      owners.Add(moduleSync);
    }
    return owners.AsReadOnly();
  }

  private static bool CoversPhase(
    IReadOnlyCollection<IServerTickPhaseOwner> owners,
    ServerTickPhase phase)
  {
    foreach (IServerTickPhaseOwner owner in owners)
    {
      if (owner.Phase == phase)
      {
        return true;
      }

      if (owner is not IServerTickPhaseCompositeOwner composite)
      {
        continue;
      }

      foreach (IServerTickPhaseParticipant participant in composite.Participants)
      {
        if (participant.Phase == phase)
        {
          return true;
        }
      }
    }

    return false;
  }

  private WorldDocumentSaveSnapshot CaptureDocumentForSave(long worldScope)
  {
    if (!Volatile.Read(ref _worldPublished) || _loadedDocument is null ||
        _baselineSnapshot is null)
    {
      throw new InvalidOperationException("No published loaded world is available for saving.");
    }

    WorldSaveSnapshot snapshot = Composition.WorldOwner.CaptureSaveSnapshot();
    if (ToWorldScope(snapshot.WorldEpoch) != worldScope)
    {
      throw new InvalidOperationException(
        "The requested save scope does not match the currently published world.");
    }

    WorldPersistenceDocument document = _snapshotProjector.Project(
      _loadedDocument,
      _baselineSnapshot,
      snapshot);
    document = _attachmentProjector.Project(
      document,
      new WorldScopeId(worldScope),
      Composition.ChestStore,
      Composition.SignRegistry,
      Composition.TileEntityRegistry);
    InvasionSnapshot invasion = Composition.InvasionState.CaptureSnapshot();
    LanternNightSnapshot lanternNight = Composition.LanternNightState.CaptureSnapshot();
    WorldFileProgressionSection progression = ReadWorldSection<WorldFileProgressionSection>(
      document,
      WorldFileProgressionSection.SectionId);
    WorldFileQuestSection quests = ReadWorldSection<WorldFileQuestSection>(
      document,
      WorldFileQuestSection.SectionId);
    WorldFileEventSection events = ReadWorldSection<WorldFileEventSection>(
      document,
      WorldFileEventSection.SectionId);
    document = document.WithReplacements(
      WorldPersistenceSection.Create(
        WorldFileProgressionSection.SectionId,
        invasion.ToPersistenceSection(progression)),
      WorldPersistenceSection.Create(
        WorldFileQuestSection.SectionId,
        invasion.ToPersistenceSection(quests)),
      WorldPersistenceSection.Create(WorldFileBestiarySection.SectionId,
        Composition.BestiaryState.CaptureSnapshot().ToPersistenceSection()),
      WorldPersistenceSection.Create(WorldFileBannerSection.SectionId,
        Composition.BannerState.CaptureSnapshot().ToPersistenceSection()),
      WorldPersistenceSection.Create(
        WorldFileEventSection.SectionId,
        lanternNight.ToPersistenceSection(events)));
    document = document.WithReplacements(
      WorldPersistenceSection.Create(
        WorldFileCreativePowersSection.SectionId,
        new WorldFileCreativePowersSection(
          _creativeWorldPowerCodec.Encode(Composition.CaptureCreativeWorldPowers()))));
    return new WorldDocumentSaveSnapshot(worldScope, snapshot.Version, document);
  }

  private static TSection ReadWorldSection<TSection>(
    WorldPersistenceDocument document,
    string sectionId)
    where TSection : notnull
  {
    if (!document.TryGetSection(sectionId, out WorldLoadSection<TSection> section) ||
        !section.IsPresent)
    {
      throw new InvalidDataException($"World save section '{sectionId}' is missing.");
    }

    return section.Value;
  }

  private static bool IsAttachmentHydrationFailure(Exception exception)
  {
    return exception is ArgumentException or InvalidDataException or InvalidOperationException or
      IOException;
  }

  private async Task<WorldDocumentSaveResult> SaveDocumentAsync(
    CancellationToken cancellationToken)
  {
    WorldSaveSnapshot snapshot = await CaptureSaveSnapshotAsync(cancellationToken)
      .ConfigureAwait(false);
    WorldDocumentSaveCoordinator saveCoordinator = _saveCoordinator ?? throw new InvalidOperationException(
      "The world document save coordinator is not available before runtime startup.");
    return await saveCoordinator.SaveAsync(
      ToWorldScope(snapshot.WorldEpoch),
      snapshot.Version,
      _configuration.WorldFilePath,
      _configuration.MaximumWorldFileBytes,
      WorldBackupPolicy.KeepPreviousPrimary,
      cancellationToken).ConfigureAwait(false);
  }

  private P08BStartResult CreateBlockedStartResult(
    bool worldLoaded,
    bool worldPublished,
    string? additionalBlocker = null)
  {
    return CreateStartResult(worldLoaded, worldPublished, additionalBlocker);
  }

  private P08BStartResult CreateStartResult(
    bool worldLoaded,
    bool worldPublished,
    string? additionalBlocker = null)
  {
    var blockers = new List<string>();
    if (additionalBlocker is not null)
    {
      blockers.Add(additionalBlocker);
    }

    if (!_listenerStarted)
    {
      blockers.Add("No production Version4 connection host is running.");
    }

    if (!_inboundDispatcherComplete)
    {
      blockers.Add(G2Blockers[0]);
    }

    if (!_tickStarted)
    {
      blockers.Add(G2Blockers[1]);
    }

    if (worldPublished && !_saveReady)
    {
      blockers.Add("The real world document save coordinator was not initialized.");
    }

    bool saveReady = worldPublished && _saveReady;
    bool isReady = worldLoaded && worldPublished && _listenerStarted && _tickStarted &&
      saveReady && blockers.Count == 0;
    return new P08BStartResult(
      isReady,
      worldLoaded,
      worldPublished,
      ListenerStarted: _listenerStarted,
      TickStarted: _tickStarted,
      SaveReady: saveReady,
      blockers);
  }

  private static long ToWorldScope(ulong worldEpoch)
  {
    if (worldEpoch == 0 || worldEpoch > long.MaxValue)
    {
      throw new InvalidOperationException("The published world epoch cannot be represented by P08.");
    }

    return (long)worldEpoch;
  }

  private static string DescribeSaveFailure(WorldDocumentSaveResult result)
  {
    return result.CommitResult.Failure.Detail ??
      $"World document commit ended with {result.CommitResult.Status}.";
  }

  private static string DescribeStorageFailure(WorldStorageFailure failure)
  {
    return failure.Detail ?? failure.Kind.ToString();
  }

  private sealed class ServerLoopCommitQueue : IWorldCommitQueue
  {
    private readonly ServerLoop _serverLoop;

    public ServerLoopCommitQueue(ServerLoop serverLoop)
    {
      _serverLoop = serverLoop ?? throw new ArgumentNullException(nameof(serverLoop));
    }

    public Task ExecuteAsync(Action commit, CancellationToken cancellationToken = default)
    {
      return _serverLoop.SubmitAsync(commit, cancellationToken);
    }
  }

  private static string DescribeCandidatePreparationFailure(
    WorldPersistenceDocument document,
    string fallback)
  {
    if (document.TryGetSection<WorldFileHeaderSection>(
          WorldFileHeaderSection.SectionId,
          out WorldLoadSection<WorldFileHeaderSection> headerSection) &&
        document.TryGetSection<WorldFileTilePayloadSection>(
          WorldFileTilePayloadSection.SectionId,
          out WorldLoadSection<WorldFileTilePayloadSection> tileSection))
    {
      WorldFileHeaderSection header = headerSection.Value;
      WorldFileTilePayloadSection tilePayload = tileSection.Value;
      if (header.MaxTilesX != tilePayload.MaxTilesX ||
          header.MaxTilesY != tilePayload.MaxTilesY)
      {
        return $"{fallback} Header dimensions {header.MaxTilesX}x{header.MaxTilesY} " +
          $"do not match tile payload dimensions {tilePayload.MaxTilesX}x{tilePayload.MaxTilesY}.";
      }

      if (tilePayload.FrameImportant.Count != WorldFacts.TileCount)
      {
        return $"{fallback} Tile frame-facts count is {tilePayload.FrameImportant.Count}; " +
          $"Version4 requires {WorldFacts.TileCount}.";
      }

      if (tilePayload.CompressedPayload.IsEmpty)
      {
        return $"{fallback} Tile compressed payload is empty.";
      }
    }

    return fallback;
  }

  private void SetState(P08BRuntimeState state)
  {
    lock (_gate)
    {
      _state = state;
    }
  }

  private enum P08BRuntimeState
  {
    Created,
    Starting,
    Running,
    Blocked,
    Failed,
    Stopped
  }
}

/// <summary>Owns the explicit stop stages used by the P08-B runtime.</summary>
internal sealed class P08BHostStopBoundary : IWorldHostStopBoundary
{
  private readonly P08BServerRuntime _runtime;

  public P08BHostStopBoundary(P08BServerRuntime runtime)
  {
    _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
  }

  public Task StopAdmissionsAsync(CancellationToken cancellationToken)
  {
    return _runtime.StopAdmissionsAsync(cancellationToken);
  }

  public Task StopTickAdmissionAsync(CancellationToken cancellationToken)
  {
    return _runtime.StopTickAdmissionAsync(cancellationToken);
  }

  public Task DrainAcceptedCommandsAsync(CancellationToken cancellationToken)
  {
    return _runtime.DrainAcceptedCommandsAsync(cancellationToken);
  }

  public Task CloseTransportAsync(CancellationToken cancellationToken)
  {
    return _runtime.CloseTransportAsync(cancellationToken);
  }

  public Task RetireWorldAndResourcesAsync(CancellationToken cancellationToken)
  {
    return _runtime.RetireWorldAndResourcesAsync(cancellationToken);
  }

  public Task StopCommitLoopAsync(CancellationToken cancellationToken)
  {
    return _runtime.StopCommitLoopAsync(cancellationToken);
  }
}

public sealed record P08BStartResult(
  bool IsReady,
  bool WorldLoaded,
  bool WorldPublished,
  bool ListenerStarted,
  bool TickStarted,
  bool SaveReady,
  IReadOnlyList<string> Blockers)
{
  public int ExitCode => IsReady ? 0 : 3;
}

public sealed record P08BSaveResult(
  bool IsCommitted,
  CommitVersion? CapturedVersion,
  string Detail)
{
  public static P08BSaveResult From(WorldDocumentSaveResult result)
  {
    ArgumentNullException.ThrowIfNull(result);
    return new(
      result.IsPrimaryCommitted,
      result.CapturedVersion,
      result.IsPrimaryCommitted
        ? $"Committed {result.PayloadLength} bytes to {result.PrimaryPath}."
        : result.CommitResult.Failure.Detail ??
          $"World document commit ended with {result.CommitResult.Status}.");
  }

  public static P08BSaveResult Blocked(CommitVersion capturedVersion, string detail)
  {
    return new P08BSaveResult(false, capturedVersion, detail);
  }
}

public enum P08BStopStage
{
  None,
  StopAdmissions,
  StopTickAdmission,
  DrainAcceptedCommands,
  Save,
  CloseTransport,
  RetireWorldAndResources,
  StopCommitLoop,
  Stopped
}

public sealed record P08BStopResult(
  bool Succeeded,
  bool SaveRequested,
  P08BSaveResult? SaveResult,
  P08BStopStage Stage,
  string? Detail)
{
  public static P08BStopResult From(WorldStopResult result)
  {
    ArgumentNullException.ThrowIfNull(result);
    return new(
      result.IsStopped,
      result.SaveRequested,
      result.SaveResult is null ? null : P08BSaveResult.From(result.SaveResult),
      result.IsStopped ? P08BStopStage.Stopped : MapStage(result.FailedStage),
      result.FailureDetail);
  }

  private static P08BStopStage MapStage(WorldStopStage stage)
  {
    return stage switch
    {
      WorldStopStage.StopAdmissions => P08BStopStage.StopAdmissions,
      WorldStopStage.StopTickAdmission => P08BStopStage.StopTickAdmission,
      WorldStopStage.DrainAcceptedCommands => P08BStopStage.DrainAcceptedCommands,
      WorldStopStage.Save => P08BStopStage.Save,
      WorldStopStage.CloseTransport => P08BStopStage.CloseTransport,
      WorldStopStage.RetireWorldAndResources => P08BStopStage.RetireWorldAndResources,
      WorldStopStage.StopCommitLoop => P08BStopStage.StopCommitLoop,
      WorldStopStage.Stopped => P08BStopStage.Stopped,
      _ => P08BStopStage.None
    };
  }
}
