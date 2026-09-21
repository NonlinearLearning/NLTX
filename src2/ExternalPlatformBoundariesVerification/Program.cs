using System.IO.Compression;
using System.Text;
using Terraria.ExternalPlatformBoundaries.RuntimeComposition.Platform;
using Terraria.ExternalPlatformBoundaries.Cryptography;
using Terraria.ExternalPlatformBoundaries.Nat;
using Terraria.ExternalPlatformBoundaries.ResourcePacks;
using Terraria.ExternalPlatformBoundaries.Social.Provider;
using Terraria.ExternalPlatformBoundaries.Social.WeGame;
using Terraria.ExternalPlatformBoundaries.Workshop;

static class Program
{
  private static int Main()
  {
    try
    {
      TestAcquireAndReleaseHaveExplicitResults();
      TestUnsupportedPlatformDoesNotCallPort();
      TestNativeFailureDoesNotClaimPlatformLease();
      TestSocialRegistryInitializesAndShutsDownInOrder();
      TestSocialRegistryFailureCleansUpInitializedModules();
      TestIpcPreservesPartialFramesAndFifoDrain();
      TestIpcSendUsesUtf8AndCloseStopsNewWork();
      TestWorkshopSnapshotsDefensivelyCopyExternalData();
      TestJoinRequestInboxRejectsStaleGenerationAndExpires();
      TestRichPresenceQueryIsPure();
      TestResourcePackDiscoveryReadsDirectoryAndZipMetadata();
      TestSecretDerivationKeepsOrderedTwoPassTransform();
      TestNatMappingRecognizesExistingAndOwnsAddedMapping();
      Console.WriteLine("P15 external-platform boundary verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestAcquireAndReleaseHaveExplicitResults()
  {
    var port = new RecordingExecutionStatePort(0x1234u);
    var adapter = new MainPlatformExecutionBoundaryAdapter(port);

    PlatformExecutionResult acquired = adapter.Acquire(isWindows: true);
    Require(acquired == PlatformExecutionResult.Acquired, "Windows acquire must succeed");
    Require(port.RequestedFlags == (MainPlatformExecutionBoundaryAdapter.EsContinuous |
      MainPlatformExecutionBoundaryAdapter.EsSystemRequired),
      "acquire must use the Version4 execution flags");

    PlatformExecutionResult released = adapter.Release(isWindows: true);
    Require(released == PlatformExecutionResult.Released, "release must report success");
    Require(port.RequestedFlags == 0x1234u, "release must restore the previous execution state");

    Require(adapter.Release(isWindows: true) == PlatformExecutionResult.NotHeld,
      "duplicate release must be explicit and harmless");
  }

  private static void TestUnsupportedPlatformDoesNotCallPort()
  {
    var port = new RecordingExecutionStatePort(0x5678u);
    var adapter = new MainPlatformExecutionBoundaryAdapter(port);

    Require(adapter.Acquire(isWindows: false) == PlatformExecutionResult.Unsupported,
      "non-Windows acquire must be unsupported");
    Require(port.CallCount == 0, "unsupported platform must not call the native port");
  }

  private static void TestNativeFailureDoesNotClaimPlatformLease()
  {
    var port = new RecordingExecutionStatePort(0u);
    var adapter = new MainPlatformExecutionBoundaryAdapter(port);

    Require(adapter.Acquire(isWindows: true) == PlatformExecutionResult.NativeFailure,
      "zero native return must be classified as a native failure");
    Require(!adapter.Snapshot.IsHeld, "native failure must not claim a lease");
    Require(adapter.Acquire(isWindows: true) == PlatformExecutionResult.NativeFailure,
      "native failure must not become an already-held result");
  }

  private static void TestSocialRegistryInitializesAndShutsDownInOrder()
  {
    var events = new List<string>();
    var first = new RecordingSocialModule("first", SocialProviderCapabilities.Cloud, events);
    var second = new RecordingSocialModule("second", SocialProviderCapabilities.Network, events);
    var registry = new SocialProviderRegistryAdapter(new[] { first, second });
    var lifecycle = new SocialRegistryLifecycleSystem(registry);

    Require(lifecycle.Start(SocialProviderMode.WeGame) == SocialProviderLifecycleResult.Initialized,
      "registry must initialize once");
    Require(registry.Snapshot.Mode == SocialProviderMode.WeGame,
      "registry snapshot must expose the selected mode");
    Require(registry.Snapshot.CloudAvailable && registry.Snapshot.NetworkAvailable,
      "registry snapshot must aggregate module capabilities");
    Require(lifecycle.Start(SocialProviderMode.WeGame) == SocialProviderLifecycleResult.AlreadyInitialized,
      "duplicate start must be explicit");
    Require(lifecycle.Stop() == SocialProviderLifecycleResult.ShutdownCompleted,
      "registry must shut down successfully");
    Require(string.Join(",", events) == "first:init,second:init,second:shutdown,first:shutdown",
      "modules must initialize in registration order and shut down in reverse order");
    Require(lifecycle.Stop() == SocialProviderLifecycleResult.AlreadyStopped,
      "duplicate stop must be idempotent");
  }

  private static void TestSocialRegistryFailureCleansUpInitializedModules()
  {
    var first = new RecordingSocialModule("first", SocialProviderCapabilities.None);
    var failing = new RecordingSocialModule(
      "failing",
      SocialProviderCapabilities.None,
      shouldFailInitialization: true);
    var registry = new SocialProviderRegistryAdapter(new[] { first, failing });

    Require(registry.Initialize(SocialProviderMode.Steam) ==
      SocialProviderLifecycleResult.InitializationFailed,
      "module initialization failure must be explicit");
    Require(first.Events.SequenceEqual(new[] { "first:init", "first:shutdown" }),
      "partially initialized modules must be cleaned up");
    Require(!registry.Snapshot.IsInitialized, "failed registry must not report initialized");
  }

  private static void TestIpcPreservesPartialFramesAndFifoDrain()
  {
    var adapter = new WeGameIpcTransportAdapter(
      new WeGameIpcTransportOptions(bufferSize: 4, maxFrameBytes: 32),
      new RecordingIpcPipe());

    Require(adapter.Open() == IpcTransportResult.Opened, "IPC must open once");
    Require(adapter.AcceptReadChunk(Encoding.UTF8.GetBytes("hel"), isMessageComplete: false) ==
      IpcTransportResult.Buffered, "partial IPC frame must be retained");
    Require(adapter.AcceptReadChunk(Encoding.UTF8.GetBytes("lo"), isMessageComplete: true) ==
      IpcTransportResult.Buffered, "completed IPC frame must be queued");
    Require(adapter.AcceptReadChunk(Encoding.UTF8.GetBytes("world"), isMessageComplete: true) ==
      IpcTransportResult.Buffered, "second IPC frame must be queued");

    IReadOnlyList<WeGameIpcFrame> frames = adapter.DrainCompleteFrames();
    Require(frames.Count == 2, "IPC drain must return both complete frames");
    Require(Encoding.UTF8.GetString(frames[0].Payload.Span) == "hello" &&
      Encoding.UTF8.GetString(frames[1].Payload.Span) == "world",
      "IPC drain must preserve FIFO payload order");
  }

  private static void TestIpcSendUsesUtf8AndCloseStopsNewWork()
  {
    var pipe = new RecordingIpcPipe();
    var adapter = new WeGameIpcTransportAdapter(new WeGameIpcTransportOptions(), pipe);
    Require(adapter.Open() == IpcTransportResult.Opened, "IPC must open before sending");
    Require(adapter.SendAsync("中文").GetAwaiter().GetResult() == IpcTransportResult.Submitted,
      "IPC send must report local submission");
    Require(Encoding.UTF8.GetString(pipe.LastWrite) == "中文", "IPC send must use UTF-8");
    Require(adapter.Close() == IpcTransportResult.Closed, "IPC close must be explicit");
    Require(adapter.SendAsync("late").GetAwaiter().GetResult() == IpcTransportResult.NotOpen,
      "closed IPC must reject new sends");
  }

  private static void TestWorkshopSnapshotsDefensivelyCopyExternalData()
  {
    var provider = new RecordingWorkshopProvider();
    var adapter = new WorkshopBoundaryAdapter(provider);
    WorkshopOperationResult<WorkshopEntrySnapshot> result = adapter.Lookup(
      new WorkshopLookupRequest(42uL));
    Require(result.Status == WorkshopOperationStatus.Succeeded, "workshop lookup must succeed");
    Require(result.Value!.Tags[0] == "world", "workshop snapshots must copy provider arrays");

    WorkshopPublishRequest request = new(
      new[] { new WorkshopTagValue("tag.key", "tag-api") },
      WorkshopPublicity.Public,
      "preview.png");
    Require(adapter.Publish(request).Status == WorkshopOperationStatus.Succeeded,
      "workshop publish must pass a validated request to the provider");
    Require(provider.LastPublish!.Tags[0].InternalNameForApis == "tag-api",
      "workshop publish must preserve localization/API tag identity separately");
  }

  private static void TestJoinRequestInboxRejectsStaleGenerationAndExpires()
  {
    var inbox = new JoinRequestInboxAdapter();
    DateTimeOffset now = new(2026, 9, 12, 0, 0, 0, TimeSpan.Zero);
    inbox.AddOrReplace(new JoinRequestSnapshot("Player", "user", now.AddMinutes(1), 2));
    Require(!inbox.Remove("user", generation: 1), "stale generation must not remove a request");
    Require(inbox.PruneExpired(now.AddMinutes(2)) == 1, "expired requests must be removed");
    Require(inbox.Snapshot().Count == 0, "expired request must not remain visible");
  }

  private static void TestRichPresenceQueryIsPure()
  {
    RichPresenceGameSnapshot snapshot = new(
      IsMenu: false,
      IsServer: false,
      IsMultiplayer: true);
    Require(RichPresenceQuery.Compute(snapshot) == RichPresenceGameMode.Multiplayer,
      "rich presence mode must be derived from explicit input");
    Require(RichPresenceQuery.Compute(new RichPresenceGameSnapshot(
      IsMenu: true,
      IsServer: true,
      IsMultiplayer: true)) == RichPresenceGameMode.Menu,
      "menu must have deterministic precedence");
  }

  private static void TestResourcePackDiscoveryReadsDirectoryAndZipMetadata()
  {
    string root = Path.Combine(Path.GetTempPath(), "p15-resource-pack-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(root);
    string directoryPack = Path.Combine(root, "DirectoryPack");
    Directory.CreateDirectory(directoryPack);
    File.WriteAllText(
      Path.Combine(directoryPack, "pack.json"),
      "{\"name\":\"Directory Pack\",\"author\":\"Test\",\"version\":1}");
    string zipPath = Path.Combine(root, "ZipPack.zip");
    using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
    {
      ZipArchiveEntry entry = archive.CreateEntry("pack.json");
      using StreamWriter writer = new(entry.Open());
      writer.Write("{\"name\":\"Zip Pack\",\"author\":\"Test\",\"version\":2}");
    }

    try
    {
      using var collection = new ResourcePackCollectionAdapter();
      ResourcePackDiscoveryResult result = collection.Discover(new[]
      {
        new ResourcePackCandidate(directoryPack, ResourcePackBranding.Local),
        new ResourcePackCandidate(zipPath, ResourcePackBranding.Workshop)
      });
      Require(result.Packs.Count == 2, "resource discovery must load directory and zip packs");
      Require(result.Packs[0].FileName == "DirectoryPack", "resource packs must sort by filename");
      Require(result.Packs[1].IsCompressed, "zip resource pack must expose compression metadata");
    }
    finally
    {
      Directory.Delete(root, recursive: true);
    }
  }

  private static void TestSecretDerivationKeepsOrderedTwoPassTransform()
  {
    var primitive = new RecordingSecretPrimitive();
    var adapter = new SecretDerivationAdapter(primitive);
    string secret = adapter.ToSecret("seed");

    Require(primitive.Calls.Count == 2, "secret derivation must call the primitive twice");
    Require(primitive.Calls.All(call => call.WorkFactor == 4 && call.Salt.Length == 16),
      "secret derivation must preserve the fixed salt and work factor");
    Require(secret == Convert.ToBase64String(primitive.SecondResult),
      "secret derivation must return the second transform as standard base64");
  }

  private static void TestNatMappingRecognizesExistingAndOwnsAddedMapping()
  {
    var collection = new RecordingNatCollection();
    var adapter = new NatPortMappingAdapter(collection);
    NatPortMappingKey key = new(7777, 7777, "TCP", "192.168.1.10", "Terraria Server");
    collection.Mappings.Add(new RecordingNatMapping(7777, "tcp", "192.168.1.10"));
    Require(adapter.Ensure(key).Status == NatPortMappingStatus.AlreadyPresent,
      "matching TCP mapping must be recognized");
    Require(collection.AddCount == 0, "existing mapping must not be duplicated");

    NatPortMappingKey newKey = new(7778, 7778, "TCP", "192.168.1.10", "Terraria Server");
    Require(adapter.Ensure(newKey).Status == NatPortMappingStatus.Added,
      "missing mapping must be added");
    Require(adapter.Release(newKey).Status == NatPortMappingStatus.Removed,
      "only an adapter-owned mapping may be removed");
    Require(adapter.Release(key).Status == NatPortMappingStatus.NotOwned,
      "pre-existing mapping must not be removed by this adapter");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private sealed class RecordingExecutionStatePort : IPlatformExecutionStatePort
  {
    public RecordingExecutionStatePort(uint previousState)
    {
      PreviousState = previousState;
    }

    public int CallCount { get; private set; }

    public uint PreviousState { get; }

    public uint RequestedFlags { get; private set; }

    public uint SetExecutionState(uint flags)
    {
      CallCount++;
      RequestedFlags = flags;
      return PreviousState;
    }
  }

  private sealed class RecordingSocialModule : ISocialProviderModule
  {
    private readonly bool _shouldFailInitialization;

    public RecordingSocialModule(
      string name,
      SocialProviderCapabilities capabilities,
      List<string>? events = null,
      bool shouldFailInitialization = false)
    {
      Name = name;
      Capabilities = capabilities;
      _events = events;
      _shouldFailInitialization = shouldFailInitialization;
    }

    private readonly List<string>? _events;

    public SocialProviderCapabilities Capabilities { get; }

    public List<string> Events { get; } = new();

    public string Name { get; }

    public SocialProviderLifecycleResult Initialize()
    {
      Events.Add(Name + ":init");
      _events?.Add(Name + ":init");
      return _shouldFailInitialization
        ? SocialProviderLifecycleResult.InitializationFailed
        : SocialProviderLifecycleResult.Initialized;
    }

    public SocialProviderLifecycleResult Shutdown()
    {
      Events.Add(Name + ":shutdown");
      _events?.Add(Name + ":shutdown");
      return SocialProviderLifecycleResult.ShutdownCompleted;
    }
  }

  private sealed class RecordingIpcPipe : IWeGameIpcPipe
  {
    public byte[] LastWrite { get; private set; } = Array.Empty<byte>();

    public ValueTask DisposeAsync()
    {
      return ValueTask.CompletedTask;
    }

    public ValueTask WriteAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
      LastWrite = payload.ToArray();
      return ValueTask.CompletedTask;
    }
  }

  private sealed class RecordingWorkshopProvider : IWorkshopProviderPort
  {
    public WorkshopPublishRequest? LastPublish { get; private set; }

    public WorkshopOperationResult<WorkshopEntrySnapshot> Lookup(WorkshopLookupRequest request)
    {
      string[] tags = { "world", "vanilla" };
      var snapshot = new WorkshopEntrySnapshot(
        request.ExternalWorkshopId,
        WorkshopPublicity.Public,
        tags,
        "preview.png",
        1);
      tags[0] = "mutated";
      return WorkshopOperationResult<WorkshopEntrySnapshot>.Succeeded(
        snapshot);
    }

    public WorkshopOperationResult<WorkshopEntrySnapshot> Publish(WorkshopPublishRequest request)
    {
      LastPublish = request;
      return WorkshopOperationResult<WorkshopEntrySnapshot>.Succeeded(
        new WorkshopEntrySnapshot(42uL, request.Publicity, request.Tags.Select(tag => tag.InternalNameForApis), request.PreviewImagePath, 1));
    }
  }

  private sealed class RecordingSecretPrimitive : ISecretDerivationPrimitive
  {
    public List<SecretPrimitiveCall> Calls { get; } = new();

    public byte[] SecondResult { get; private set; } = Array.Empty<byte>();

    public byte[] CryptRaw(byte[] input, byte[] salt, int workFactor)
    {
      byte[] result = new byte[24];
      for (int index = 0; index < result.Length; index++)
      {
        result[index] = (byte)(input.Length + salt[index % salt.Length] + workFactor + index);
      }

      Calls.Add(new SecretPrimitiveCall(input.ToArray(), salt.ToArray(), workFactor));
      if (Calls.Count == 2)
      {
        SecondResult = result;
      }

      return result;
    }
  }

  private sealed class RecordingNatCollection : INatPortMappingCollectionPort
  {
    public List<RecordingNatMapping> Mappings { get; } = new();

    public int AddCount { get; private set; }

    public IReadOnlyList<IStaticPortMappingSnapshot> Enumerate()
    {
      return Mappings.Cast<IStaticPortMappingSnapshot>().ToArray();
    }

    public void Add(NatPortMappingKey key)
    {
      AddCount++;
      Mappings.Add(new RecordingNatMapping(key.InternalPort, key.Protocol, key.InternalClient));
    }

    public void Remove(NatPortMappingKey key)
    {
      Mappings.RemoveAll(mapping => mapping.InternalPort == key.InternalPort &&
        string.Equals(mapping.Protocol, key.Protocol, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(mapping.InternalClient, key.InternalClient, StringComparison.Ordinal));
    }
  }

  private sealed record RecordingNatMapping(
    int InternalPort,
    string Protocol,
    string InternalClient) : IStaticPortMappingSnapshot;

  private sealed record SecretPrimitiveCall(byte[] Input, byte[] Salt, int WorkFactor);
}
