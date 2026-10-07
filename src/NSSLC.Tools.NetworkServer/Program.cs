using System.Net;
using NSSLC.Infrastructure.Network;
using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.ID;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Network;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;

return await NetworkServerProgram.RunAsync(args);

internal static class NetworkServerProgram {
  private const int PlayerSlotCount = 255;
  private const int TravelMerchantSlotCount = 40;

  public static async Task<int> RunAsync(string[] args) {
    try {
      ServerOptions options = ServerOptions.Parse(args);
      if (options.ShowHelp) {
        Console.WriteLine(ServerOptions.HelpText);
        return 0;
      }

      await using var worldOwner = new NetworkWorldOwner(
          () => NetworkWorldSessionLoader.Load(options.WorldPath));
      await worldOwner.Ready.ConfigureAwait(false);
      WorldPersistenceDocument world = await worldOwner.InvokeAsync(session =>
          session.SourceDocument ?? throw new InvalidOperationException(
              "The loaded network world has no source document."));
      var worldRuntimeId = await worldOwner.InvokeAsync(session => session.EntityRuntime.RuntimeId);
      WorldDataPacket initialPacket = WorldDataPacketProjection.Create(world);
      ProtocolFacts facts = CreateRestrictedServerFacts(options.UseSteamModuleIds);
      TileMapSnapshot tiles = new WorldFileTileCodec().Decode(Required<WorldFileTilePayloadSection>(
          world, WorldFileTilePayloadSection.SectionId));
      var synchronization = new WorldSynchronizationObservation();
      var controls = new PlayerControlsObservationStore();
      var tileBreaks = new TileBreakObservationStore();
      string? password = Environment.GetEnvironmentVariable("NLTX_SERVER_PASSWORD");
      if (string.IsNullOrEmpty(password)) {
        password = null;
      }
      string? hostToken = Environment.GetEnvironmentVariable("NLTX_HOST_TOKEN");
      if (string.IsNullOrWhiteSpace(hostToken)) {
        hostToken = null;
      }
      var authority = new PlayerSlotSessionAuthority(PlayerSlotCount, password, hostToken);
      ProtocolProfile wireProfile = options.UseSteamModuleIds
          ? SteamProtocolProfile.Create(facts) : TerrariaProtocolProfile.Create(facts);
      ProtocolProfile profile = ServerProtocolProfile.Create(wireProfile);
      TimeProvider tickClock = TimeProvider.System;
      NetworkPlayerOwner? playerOwner = null;
      NetworkWorldItemOwner worldItemOwner;
      await using var host = new NetworkGatewayHost(options.Address, options.Port, profile, authority,
          new PacketGatewayOptions {
            MaximumSessions = PlayerSlotCount,
            IgnoreClientVersion = options.IgnoreClientVersion,
            UseSteamModuleIds = options.UseSteamModuleIds,
            EnableHostAuthorization = true,
            EnablePing = true
          },
          new PacketConnectionOptions { ReceiveItems = 1024, ReceiveBytes = 512 * 1024 },
          timeProvider: tickClock, worldRuntimeIdProvider: () => worldRuntimeId,
          sessionClosing: async (context, token) => {
            NetworkPlayerOwner players = playerOwner ?? throw new InvalidOperationException(
                "The player owner must be composed before the host starts.");
            NetworkPlayerBindingStatus result = await players.DisconnectAsync(context, token)
                .ConfigureAwait(false);
            if (result is not (NetworkPlayerBindingStatus.Disconnected
                or NetworkPlayerBindingStatus.NotFound)) {
              throw new InvalidOperationException($"Player cleanup rejected: {result}.");
            }
      });
      playerOwner = new NetworkPlayerOwner(worldOwner, host.Gateway.IsCurrentSender);
      // The authoritative world simulation has not exposed a live game-tick clock yet. Keep
      // packet-21 admission tied to the real world/player owner; gameplay must replace this
      // placeholder clock and register each spawned item before clients can update it.
      worldItemOwner = new NetworkWorldItemOwner(worldOwner, playerOwner, () => 0L);
      var sectionHandlers = new WorldSynchronizationPacketHandlers(initialPacket, tiles,
          facts.Inputs!.FrameImportant, controls, tileBreaks, synchronization,
          host.Gateway.IsCurrentSender, () => worldRuntimeId,
          worldItems: options.UseSteamModuleIds ? worldItemOwner : null);
      var worldTimeProducer = new WorldTimePacketProducer(host.Gateway, worldOwner, worldRuntimeId);
      var worldTileMetricsProducer = new WorldTileMetricsPacketProducer(
          host.Gateway, worldOwner, worldRuntimeId);
      var initialWorldSynchronization = new WorldSpawnSynchronizationPacketHandler(
          sectionHandlers, worldTimeProducer, worldTileMetricsProducer);
      var admissionPackets = new PlayerAdmissionPacketHandlers();
      var lifecyclePackets = new PlayerLifecyclePacketHandlers(
          playerOwner, initialPacket.MaxTilesX, initialPacket.MaxTilesY);
      var playerStatePackets = new PlayerStatePacketHandlers(playerOwner);
      var playerExtendedPackets = new PlayerExtendedPacketHandlers(
          playerOwner, worldOwner, sectionHandlers);
      var sessionClientUuid = new SessionClientUuidPacketHandler(authority.ClientUuids);
      var socialPackets = new SocialPacketHandlers();
      SocialPacketRegistration.Register(host.Gateway, socialPackets);
      PlayerLifecyclePacketRegistration.Register(host.Gateway, lifecyclePackets);
      PlayerStatePacketRegistration.Register(host.Gateway, playerStatePackets);
      PlayerExtendedPacketRegistration.Register(host.Gateway, playerExtendedPackets);
      if (options.UseSteamModuleIds) {
        SteamItemPacketGatewayRegistration.Register(
            host.Gateway,
            worldItemOwner,
            new PacketPolicy(21, NetworkSessionStage.Active,
                MaximumPerWindow: 120, MaximumBytesPerWindow: 4096));
      }
      ReservedPacketRegistration.RegisterClientSyncedInventory(host.Gateway);
      ReservedPacketRegistration.RegisterKnownNoEffectPackets(host.Gateway);
      var spawnRates = new PlayerSpawnRatePacketHandler();
      var projectiles = new SteamProjectilePacketHandler();
      if (options.UseSteamModuleIds) {
        host.Gateway.Register<SteamProjectileSyncPacket>(
            new PacketPolicy(27, NetworkSessionStage.Active,
                MaximumPerWindow: 120, MaximumBytesPerWindow: 8 * 1024), projectiles);
        host.Gateway.Register<SteamProjectileKillPacket>(
            new PacketPolicy(29, NetworkSessionStage.Active,
                MaximumPerWindow: 120, MaximumBytesPerWindow: 2048), projectiles);
      }
      host.Gateway.Register<SyncEquipmentPacket>(
          new PacketPolicy(5, NetworkSessionStage.AwaitPlayerData | NetworkSessionStage.Active,
              MaximumPerWindow: 1000, MaximumBytesPerWindow: 16 * 1024), admissionPackets);
      host.Gateway.Register<Unknown68Packet>(
          new PacketPolicy(68, NetworkSessionStage.AwaitPlayerData,
              MaximumPerWindow: 1, MaximumBytesPerWindow: 132), sessionClientUuid);
      host.Gateway.Register(new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData,
          MaximumPerWindow: 1, MaximumBytesPerWindow: 1),
          new WorldDataPacketHandler(world, host.Gateway.IsCurrentSender));
      host.Gateway.Register<SpawnTileDataPacket>(
          new PacketPolicy(8, NetworkSessionStage.AwaitSectionRequest,
          MaximumPerWindow: 1, MaximumBytesPerWindow: 9),
          initialWorldSynchronization);
      host.Gateway.Register<TileManipulationPacket>(
          new PacketPolicy(17, NetworkSessionStage.Active,
          MaximumPerWindow: 120, MaximumBytesPerWindow: 960), sectionHandlers);
      host.Gateway.Register<RequestSectionPacket>(
          new PacketPolicy(159, NetworkSessionStage.Active,
          MaximumPerWindow: 8, MaximumBytesPerWindow: 32),
          sectionHandlers);
      host.Gateway.Register<NetModulesPacket>(
          new PacketPolicy(82, NetworkSessionStage.Active, ModuleId: 5,
              MaximumPerWindow: 4, MaximumBytesPerWindow: 64),
          spawnRates);
      if (options.TestFactsPath is not null) {
        WriteTestFacts(options.TestFactsPath, facts);
      }
      host.Start();

      Console.WriteLine($"World host listening at {host.EndPoint}.");
      int formatCount = host.Profile.Bindings.Select(binding => binding.MessageId)
          .Distinct().Count();
      int handlerCount = host.Gateway.Registrations.Select(registration =>
          registration.Policy.MessageId).Distinct().Count();
      Console.WriteLine($"Protocol format IDs: {formatCount}; explicit ingress handler IDs: " +
          $"{handlerCount}; built-in session capabilities are reported separately.");
      Console.WriteLine("Client version check: " +
          (options.IgnoreClientVersion ? "ignored" : host.Profile.HelloVersion) + ".");
      Console.WriteLine("Module layout: " +
          (options.UseSteamModuleIds ? "Steam 1.4.5.8" : "Terraria319") + ".");
      Console.WriteLine($"World: {initialPacket.WorldName} ({initialPacket.MaxTilesX}x" +
          $"{initialPacket.MaxTilesY}), id={initialPacket.WorldId}, packet 6 enabled; " +
          "initial sections, player spawn, controls, extra sections and test tile breaking enabled.");
      using var socialTickShutdown = new CancellationTokenSource();
      Task socialBubbleTicks = SocialEmoteBubbleTickScheduler.RunAsync(
          socialPackets.EmoteBubbles, tickClock, socialTickShutdown.Token);
      try {
        await WaitForShutdownAsync(options.ExitAfterFirstClient, authority, host.Gateway);
      } finally {
        socialTickShutdown.Cancel();
        await socialBubbleTicks.ConfigureAwait(false);
      }
      if (options.ReportPath is not null) {
        WriteReport(options.ReportPath, initialPacket, synchronization, controls, tileBreaks,
            spawnRates, projectiles, host.Gateway);
      }
      return 0;
    } catch (Exception exception) {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static TSection Required<TSection>(WorldPersistenceDocument document, string sectionId)
      where TSection : notnull {
    if (!document.TryGetSection(sectionId, out WorldLoadSection<TSection> section)
        || !section.IsPresent) {
      throw new InvalidDataException($"World data is missing required section '{sectionId}'.");
    }
    return section.Value;
  }

  private static void WriteTestFacts(string path, ProtocolFacts facts) {
    ProtocolInputs inputs = facts.Inputs
        ?? throw new InvalidOperationException("The protocol facts do not contain packet inputs.");
    WriteJson(path, new { frameImportant = inputs.FrameImportant });
  }

  private static void WriteReport(string path, WorldDataPacket world,
      WorldSynchronizationObservation synchronization, PlayerControlsObservationStore controls,
      TileBreakObservationStore tileBreaks, PlayerSpawnRatePacketHandler spawnRates,
      SteamProjectilePacketHandler projectiles, PacketGateway gateway) {
    WriteJson(path, new {
      WorldName = world.WorldName,
      WorldId = world.WorldId,
      WorldWidth = world.MaxTilesX,
      WorldHeight = world.MaxTilesY,
      Sections = synchronization.Sections,
      PlayerSpawns = synchronization.PlayerSpawns,
      PlayerControls = controls.Snapshot(),
      TileBreaks = tileBreaks.Snapshot(),
      SpawnRateSliders = spawnRates.Snapshot(),
      ProjectilePackets = projectiles.Snapshot(),
      IngressRegistrations = gateway.Registrations,
      SessionCapabilities = new {
        gateway.Options.EnablePing,
        gateway.Options.EnableHostAuthorization,
        gateway.Options.IgnoreClientVersion,
        gateway.Options.UseSteamModuleIds
      }
    });
  }

  private static void WriteJson(string path, object value) {
    string fullPath = Path.GetFullPath(path);
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    File.WriteAllText(fullPath, System.Text.Json.JsonSerializer.Serialize(value,
        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
  }

  private static ProtocolFacts CreateRestrictedServerFacts(bool useSteamModuleIds) {
    if (ProtocolInputs.IsInitialized) {
      throw new InvalidOperationException("Protocol inputs were already initialized in this process.");
    }

    Main.InitializeHeadlessRuntime();
    var inputs = new ProtocolInputs(
        frameImportant: Main.tileFrameImportant.ToArray(),
        allowsSaveCompressionBatching: TileID.Sets.AllowsSaveCompressionBatching.ToArray(),
        tileEntityCodecs: PacketTileEntityCodecsV4.Create(),
        isServer: true,
        // These facts belong to disabled packet handlers in this metadata-only host.
        catchableTypes: new bool[NPCID.Count],
        lifeWidthResolver: static (_, _, _) => throw new NotSupportedException(
            "NPC life-width facts are not available while packet 23 is disabled."),
        needsUuid: static _ => throw new NotSupportedException(
            "Projectile UUID facts are not available while projectile packets are disabled."),
        slotCount: TravelMerchantSlotCount,
        moduleCodecs: useSteamModuleIds
            ? Packet82KnownModuleCodecsV4.CreateSteam() : Packet82KnownModuleCodecsV4.Create(),
        tagEffectNpcSlotCount: Main.maxNPCs,
        tagEffectUsesProcTimes: static _ => throw new NotSupportedException(
            "NPC tag-effect facts are not available while tag-effect modules are disabled."));
    return new ProtocolFacts(inputs, useSteamModuleIds
        ? "Steam326-world-data-host" : "Terraria319-world-data-host");
  }

  private static async Task WaitForShutdownAsync(bool exitAfterFirstClient,
      PlayerSlotSessionAuthority authority, PacketGateway gateway) {
    using var shutdown = new CancellationTokenSource();
    ConsoleCancelEventHandler cancel = (_, eventArgs) => {
      eventArgs.Cancel = true;
      shutdown.Cancel();
    };
    Console.CancelKeyPress += cancel;
    try {
      while (!shutdown.IsCancellationRequested) {
        while (gateway.TryReadDiagnostic(out PacketGatewayDiagnostic? diagnostic)) {
          if (diagnostic!.Code != "LocallySent") {
            Console.WriteLine($"Session {diagnostic.Connection}: {diagnostic.Code}; " +
                $"packet={diagnostic.MessageId}, module={diagnostic.ModuleId}, " +
                $"action={diagnostic.Action}, bytes={diagnostic.BodyLength}, " +
                $"bodyOffset={diagnostic.BodyOffset}.");
          }
        }
        if (exitAfterFirstClient && authority.AdmissionCount > 0 && authority.ActiveBindings == 0
            && gateway.Sessions.Count == 0) {
          return;
        }
        await Task.Delay(TimeSpan.FromMilliseconds(50), shutdown.Token);
      }
    } catch (OperationCanceledException) when (shutdown.IsCancellationRequested) {
    } finally {
      Console.CancelKeyPress -= cancel;
    }
  }
}

internal sealed record ServerOptions(IPAddress Address, int Port, string WorldPath,
    bool ExitAfterFirstClient, bool IgnoreClientVersion, bool UseSteamModuleIds, string? TestFactsPath,
    string? ReportPath, bool ShowHelp) {
  public const string HelpText = "Usage: NSSLC.Tools.NetworkServer --world <world.wld> " +
      "[--listen <ip>] [--port <port>] [--exit-after-first-client] " +
      "[--ignore-client-version] [--steam-module-ids] [--test-facts <path>] [--report <path>]";

  public static ServerOptions Parse(string[] args) {
    IPAddress address = IPAddress.Loopback;
    int port = 7777;
    string? worldPath = null;
    bool exitAfterFirstClient = false;
    bool ignoreClientVersion = false;
    bool useSteamModuleIds = false;
    string? testFactsPath = null;
    string? reportPath = null;
    bool showHelp = false;
    for (int index = 0; index < args.Length; index++) {
      switch (args[index]) {
        case "--help":
        case "-h":
          showHelp = true;
          break;
        case "--world":
          worldPath = ReadValue(args, ref index);
          break;
        case "--listen":
          if (!IPAddress.TryParse(ReadValue(args, ref index), out address!)) {
            throw new ArgumentException("--listen must be an IPv4 or IPv6 address.");
          }
          break;
        case "--port":
          if (!int.TryParse(ReadValue(args, ref index), out port) || port is < 0 or > 65535) {
            throw new ArgumentException("--port must be between 0 and 65535.");
          }
          break;
        case "--exit-after-first-client":
          exitAfterFirstClient = true;
          break;
        case "--ignore-client-version":
          ignoreClientVersion = true;
          break;
        case "--steam-module-ids":
          useSteamModuleIds = true;
          break;
        case "--test-facts":
          testFactsPath = ReadValue(args, ref index);
          break;
        case "--report":
          reportPath = ReadValue(args, ref index);
          break;
        default:
          throw new ArgumentException($"Unknown server option '{args[index]}'.");
      }
    }
    if (!showHelp && string.IsNullOrWhiteSpace(worldPath)) {
      throw new ArgumentException("--world is required.");
    }
    return new ServerOptions(address, port, worldPath ?? string.Empty,
        exitAfterFirstClient, ignoreClientVersion, useSteamModuleIds, testFactsPath, reportPath,
        showHelp);
  }

  private static string ReadValue(string[] args, ref int index) {
    if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal)) {
      throw new ArgumentException($"Option '{args[index]}' requires a value.");
    }
    return args[++index];
  }
}
