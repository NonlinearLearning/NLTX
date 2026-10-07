using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text.Json;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Projectile;

namespace NSSLC.NetworkVerification;

internal static class RealClientVerification {
  private sealed class ProjectileOwner : IProjectileNetworkCommandOwner {
    public ProjectileNetworkApplyCommand? Applied { get; private set; }
    public ProjectileNetworkTerminateCommand? Terminated { get; private set; }

    public ValueTask<PacketHandlingResult> ApplyProjectileAsync(NetworkSessionContext sender,
        ProjectileNetworkApplyCommand command, CancellationToken cancellationToken) {
      Applied = command;
      return ValueTask.FromResult(HealthReply(sender, 40));
    }

    public ValueTask<PacketHandlingResult> TerminateProjectileAsync(NetworkSessionContext sender,
        ProjectileNetworkTerminateCommand command, CancellationToken cancellationToken) {
      Terminated = command;
      return ValueTask.FromResult(HealthReply(sender, 41));
    }
  }

  public static async Task<int> RunAsync(string executable, string output) {
    executable = Path.GetFullPath(executable);
    output = Path.GetFullPath(output);
    Directory.CreateDirectory(output);
    Verify.That(File.Exists(executable), "Build the real client before running this verifier.");
    int failed = 0;
    string[] scenarios = [
      "startup", "gateway", "admission", "password-correct", "password-rejected", "default-deny", "ping",
      "missing-arguments", "transport-timeout"
    ];
    foreach (string scenario in scenarios) {
      try {
        await RunCaseAsync(executable, output, scenario);
        Console.WriteLine($"PASS real-client {scenario}");
      } catch (Exception exception) {
        failed++;
        Console.Error.WriteLine($"FAIL real-client {scenario}: {exception}");
      }
    }
    Console.WriteLine($"Real client scenarios: {scenarios.Length - failed} passed, {failed} failed.");
    return failed == 0 ? 0 : 1;
  }

  private static async Task RunCaseAsync(string executable, string output, string scenario) {
    if (scenario == "startup") {
      await RunProcessAsync(executable, output, scenario, scenario, 0, null);
      return;
    }
    if (scenario == "missing-arguments") {
      await RunProcessAsync(executable, output, scenario, scenario, 0, null, expectedExitCode: 2);
      return;
    }
    if (scenario == "transport-timeout") {
      var listener = new TcpListener(IPAddress.Loopback, 0);
      listener.Start();
      try {
        Task test = RunProcessAsync(executable, output, scenario, "gateway",
            ((IPEndPoint)listener.LocalEndpoint).Port, null, expectedExitCode: 1,
            timeoutMilliseconds: 2000);
        using TcpClient connection = await listener.AcceptTcpClientAsync()
            .WaitAsync(TimeSpan.FromSeconds(5));
        await test;
        using var result = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, scenario + ".json")));
        long elapsed = result.RootElement.GetProperty("elapsedMilliseconds").GetInt64();
        Verify.That(elapsed >= 1800 && elapsed < 5000,
            "A silent TCP peer must fail near the configured total deadline.");
      } finally {
        listener.Stop();
      }
      return;
    }
    bool password = scenario.StartsWith("password-", StringComparison.Ordinal);
    var authority = new RecordingAuthority { RequiresPassword = password };
    var owner = new ProjectileOwner();
    int controls = 0;
    int health = 0;
    int names = 0;
    await using var host = new NetworkGatewayHost(IPAddress.Loopback, 0,
        GeneratedProfileVerification.CreateFacts(), authority,
        new PacketGatewayOptions { EnablePing = scenario == "ping" },
        scenario == "admission"
            ? new PacketConnectionOptions { ReceiveItems = 1024, ReceiveBytes = 512 * 1024 }
            : null);
    if (scenario != "default-deny") {
      if (scenario == "admission") {
        var admissionPackets = new PlayerAdmissionPacketHandlers();
        host.Gateway.Register<SyncPlayerPacket>(
            new PacketPolicy(4, NetworkSessionStage.AwaitPlayerData,
                MaximumPerWindow: 1, MaximumBytesPerWindow: 256), admissionPackets);
        host.Gateway.Register<SyncEquipmentPacket>(
            new PacketPolicy(5, NetworkSessionStage.AwaitPlayerData,
                MaximumPerWindow: 1000, MaximumBytesPerWindow: 16 * 1024), admissionPackets);
        host.Gateway.Register<PlayerLifeManaPacket>(
            new PacketPolicy(16, NetworkSessionStage.AwaitPlayerData,
                MaximumPerWindow: 2, MaximumBytesPerWindow: 32), admissionPackets);
        host.Gateway.Register<Unknown42Packet>(
            new PacketPolicy(42, NetworkSessionStage.AwaitPlayerData,
                MaximumPerWindow: 2, MaximumBytesPerWindow: 32), admissionPackets);
        host.Gateway.Register<PlayerBuffsPacket>(
            new PacketPolicy(50, NetworkSessionStage.AwaitPlayerData,
                MaximumPerWindow: 2, MaximumBytesPerWindow: 512), admissionPackets);
        host.Gateway.Register<Unknown68Packet>(
            new PacketPolicy(68, NetworkSessionStage.AwaitPlayerData,
                MaximumPerWindow: 1, MaximumBytesPerWindow: 132), admissionPackets);
        host.Gateway.Register<ClientSyncedInventoryPacket>(
            new PacketPolicy(138, NetworkSessionStage.AwaitPlayerData | NetworkSessionStage.Active,
                MaximumPerWindow: 4, MaximumBytesPerWindow: 1), admissionPackets);
        host.Gateway.Register<SyncLoadoutPacket>(
            new PacketPolicy(147, NetworkSessionStage.AwaitPlayerData,
                MaximumPerWindow: 4, MaximumBytesPerWindow: 8), admissionPackets);
      }
      host.Gateway.Register(new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData),
          new RecordingHandler<RequestWorldDataPacket>((context, _, _) =>
              ValueTask.FromResult(StageReply(context, 10,
                  NetworkSessionStage.AwaitSectionRequest))));
      host.Gateway.Register(new PacketPolicy(8, NetworkSessionStage.AwaitSectionRequest),
          new RecordingHandler<SpawnTileDataPacket>((context, _, _) =>
              ValueTask.FromResult(StageReply(context, 11, NetworkSessionStage.Synchronizing))));
      host.Gateway.Register(new PacketPolicy(12, NetworkSessionStage.Synchronizing),
          new RecordingHandler<PlayerSpawnPacket>((context, _, _) =>
              ValueTask.FromResult(StageReply(context, 12, NetworkSessionStage.Active))));
      host.Gateway.Register(new PacketPolicy(13, NetworkSessionStage.Active),
          new RecordingHandler<PlayerControlsPacket>((context, packet, _) => {
            Verify.That(context.Actor.PlayerSlot == 0 && packet.Player == 0
                && packet.Position == new PacketVector2(12.5f, -3.25f)
                && packet.Velocity == new PacketVector2(1.5f, -2.5f)
                && packet.SelectedItem == 7 && (packet.ControlFlags & 8) != 0,
                "The real client's controls did not decode to the expected values.");
            Interlocked.Increment(ref controls);
            packet.Player = 1;
            return ValueTask.FromResult(Reply(context, packet));
          }));
      if (scenario != "admission") {
        host.Gateway.Register(new PacketPolicy(16, NetworkSessionStage.Active),
            new RecordingHandler<PlayerLifeManaPacket>((context, packet, _) => {
              Verify.That(packet.Player == 0 && packet.Life == 75 && packet.MaximumLife == 100,
                  "The real client's health fields did not decode correctly.");
              Interlocked.Increment(ref health);
              return ValueTask.FromResult(HealthReply(context, 75));
            }));
      }
      host.Gateway.Register(new PacketPolicy(56, NetworkSessionStage.Active),
          new RecordingHandler<UniqueTownNPCInfoSyncRequestPacket>((context, packet, _) => {
            Verify.That(packet.NpcIndex == 0, "The real client sent an unexpected NPC index.");
            Interlocked.Increment(ref names);
            return ValueTask.FromResult(Reply(context, new UniqueTownNPCInfoSyncResponsePacket {
              NpcIndex = 0, GivenName = "NetworkGuide", Variation = 4
            }));
          }));
      ProjectilePacketGatewayRegistration.Register(host.Gateway, owner,
          new PacketPolicy(27, NetworkSessionStage.Active),
          new PacketPolicy(29, NetworkSessionStage.Active));
    }
    host.Start();
    string clientScenario = scenario == "password-correct" ? "gateway" : scenario;
    IReadOnlyList<PacketGatewayDiagnostic> diagnostics = [];
    try {
      await RunProcessAsync(executable, output, scenario, clientScenario,
          ((IPEndPoint)host.EndPoint).Port, password
              ? (scenario == "password-rejected" ? "wrong" : "correct") : null);
    } finally {
      await Verify.EventuallyAsync(() => host.Gateway.Sessions.Count == 0,
          "The real client's session did not close.");
      diagnostics = GatewayVerification.ReadDiagnostics(host.Gateway);
      File.WriteAllText(Path.Combine(output, scenario + ".gateway.json"),
          JsonSerializer.Serialize(diagnostics, new JsonSerializerOptions { WriteIndented = true }));
    }
    bool rejected = scenario == "password-rejected";
    await Verify.EventuallyAsync(() => host.Gateway.Sessions.Count == 0
        && authority.Releases == (rejected ? 0 : 1) && host.Budget.Used == 0,
        "The real client connection did not release its session and byte budget.");
    Verify.That(authority.Admissions == (rejected ? 0 : 1),
        "The authority admission count did not match the scenario.");
    if (rejected || scenario == "default-deny") {
      Verify.That(diagnostics.Any(item => item.Code ==
          (rejected ? "AdmissionDenied" : "AdmissionRejected")),
          "The rejection must be confirmed by the server's admission diagnostic.");
    } else {
      Verify.That(!diagnostics.Any(item => item.Code is "InvalidPing" or "HandlerFailed"
          or "AdmissionRejected" or "ProtocolError"), "The server reported a protocol failure.");
    }
    if (clientScenario == "gateway") {
      Verify.That(controls == 1 && health == 1 && names == 1,
          "The real client must reach each registered owner exactly once.");
      Verify.That(owner.Applied is { } applied && applied.OwnerSlot == 0
          && applied.Identity == 17 && applied.ProjectileType == 1
          && applied.Position == new Vector2(20, 30) && applied.Velocity == new Vector2(2, -4)
          && applied.Damage == 23 && applied.OriginalDamage == 25 && applied.Knockback == 1.5f
          && applied.Ai0 == 1.25f && applied.Ai1 == -2.5f && applied.Ai2 == 3.75f
          && applied.BannerIdToRespondTo == 7
          && owner.Terminated is { OwnerSlot: 0, Identity: 17 },
          "Projectile handlers must preserve real wire fields and replace forged owner 201.");
    }
    Verify.That(host.LastTransportError is null, "The gateway TCP listener reported a failure.");
  }

  private static PacketHandlingResult HealthReply(NetworkSessionContext sender, short life) {
    return Reply(sender, new PlayerLifeManaPacket { Player = 1, Life = life, MaximumLife = 100 });
  }

  private static PacketHandlingResult StageReply(NetworkSessionContext sender, short life,
      NetworkSessionStage stage) {
    return new PacketHandlingResult(true, new[] {
      new OutboundDispatch(new PlayerLifeManaPacket {
        Player = 1, Life = life, MaximumLife = 100
    }, PacketDispatchKind.Single, new[] { sender.Connection }, allowedStages: stage)
    }, nextStage: stage);
  }

  private static PacketHandlingResult Reply(NetworkSessionContext sender, object packet) {
    return new PacketHandlingResult(true, new[] {
      new OutboundDispatch(packet, PacketDispatchKind.Single, new[] { sender.Connection })
    });
  }

  public static async Task RunProcessAsync(string executable, string output, string name,
      string scenario, int port, string? password, int expectedExitCode = 0,
      int timeoutMilliseconds = 10000) {
    string resultPath = Path.Combine(output, name + ".json");
    // Remove only this run's stale result so it cannot make a crashed process look successful.
    if (File.Exists(resultPath)) {
      File.Delete(resultPath);
    }
    var start = new ProcessStartInfo(executable) {
      UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true,
      RedirectStandardError = true, WorkingDirectory = Path.GetDirectoryName(executable)!
    };
    if (name != "missing-arguments") {
      foreach (string argument in new[] {
        "-networktest", scenario, "-testresult", resultPath, "-testtimeoutms",
        timeoutMilliseconds.ToString(), "-join", "127.0.0.1", "-port", port.ToString()
      }) {
        start.ArgumentList.Add(argument);
      }
    }
    if (password is not null) {
      start.ArgumentList.Add("-password");
      start.ArgumentList.Add(password);
    }
    using var process = Process.Start(start)!;
    Task<string> stdout = process.StandardOutput.ReadToEndAsync();
    Task<string> stderr = process.StandardError.ReadToEndAsync();
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
    bool windowSeen = false;
    try {
      while (!process.HasExited) {
        process.Refresh();
        windowSeen |= process.MainWindowHandle != IntPtr.Zero;
        await Task.Delay(20, timeout.Token);
      }
      await process.WaitForExitAsync(timeout.Token);
    } finally {
      if (!process.HasExited) {
        process.Kill(entireProcessTree: true);
        await process.WaitForExitAsync();
      }
      File.WriteAllText(Path.Combine(output, name + ".stdout.log"), await stdout);
      File.WriteAllText(Path.Combine(output, name + ".stderr.log"), await stderr);
      File.WriteAllText(Path.Combine(output, name + ".process.json"), JsonSerializer.Serialize(new {
        Executable = executable, Scenario = scenario, process.ExitCode,
        start.CreateNoWindow, WindowSeen = windowSeen, ResultPath = resultPath
      }, new JsonSerializerOptions { WriteIndented = true }));
    }
    Verify.That(!windowSeen, "The network test process created a visible game window.");
    Verify.That(process.ExitCode == expectedExitCode,
        $"Client exited with {process.ExitCode}; see {name}.stderr.log.");
    if (name == "missing-arguments") {
      Verify.That(!File.Exists(resultPath), "Missing arguments must not produce a success result.");
      return;
    }
    Verify.That(File.Exists(resultPath), "The client did not write its result file.");
    using var result = JsonDocument.Parse(File.ReadAllText(resultPath));
    Verify.That(result.RootElement.GetProperty("success").GetBoolean() == (expectedExitCode == 0)
        && result.RootElement.GetProperty("scenario").GetString() == scenario,
        "The real client did not report a successful headless scenario.");
    if ((scenario is "world-request" or "world-section-request") && expectedExitCode == 0) {
      Verify.That(!string.IsNullOrWhiteSpace(result.RootElement.GetProperty("worldName").GetString())
          && result.RootElement.GetProperty("worldWidth").GetInt32() > 0
          && result.RootElement.GetProperty("worldHeight").GetInt32() > 0,
          "The original client parser did not apply valid world metadata from packet 7.");
    }
    if (scenario == "world-section-request" && expectedExitCode == 0) {
      Verify.That(result.RootElement.GetProperty("rejectedMessageId").GetInt32() == 8,
          "The metadata-only server must reject packet 8 until a section owner is registered.");
    }
    if (expectedExitCode == 0) {
      Verify.That(result.RootElement.GetProperty("message").GetString()!
          .Contains("game instance and graphics are null", StringComparison.Ordinal),
          "The client did not verify the absence of a game instance and graphics device.");
    }
  }
}
