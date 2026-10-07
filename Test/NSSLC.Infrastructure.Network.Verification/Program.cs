using NSSLC.NetworkVerification;

if (args is ["--session-behavior"]) {
  await PlayerSlotSessionAuthorityVerification.RunAsync();
  await SessionPacketBehaviorVerification.RunAsync();
  Console.WriteLine("PASS session authority, UUID, host-token and real TCP session behavior");
  return 0;
}
if (args is ["--social-behavior"]) {
  await SocialPacketVerification.RunAsync();
  Console.WriteLine("PASS social packet layouts, authenticated relays and bubble lifetime state");
  return 0;
}
if (args is ["--reserved-behavior"]) {
  await ReservedPacketVerification.RunAsync();
  Console.WriteLine("PASS reserved message directions, strict empty notices and TCP state retention");
  return 0;
}
if (args is ["--world-behavior"]) {
  await WorldPacketBehaviorVerification.RunAsync();
  Console.WriteLine("PASS world join sections, authenticated sender and tile projections");
  return 0;
}
if (args is ["--world-frame-repair-tcp"]) {
  await WorldFrameRepairTcpVerification.RunAsync();
  Console.WriteLine("PASS world frame repair publication over real TCP section subscribers");
  return 0;
}
if (args is ["--world-time-behavior"]) {
  await WorldTimePacketVerification.RunAsync();
  Console.WriteLine("PASS authoritative world time packet publication and direction");
  return 0;
}
if (args is ["--world-metrics-behavior"]) {
  await WorldTileMetricsPacketVerification.RunAsync();
  Console.WriteLine("PASS authoritative world tile metrics packet publication and direction");
  return 0;
}
if (args is ["--world-owner"]) {
  await NetworkWorldOwnerVerification.RunAsync();
  Console.WriteLine("PASS world owner thread, entity access, cancellation, capacity and shutdown");
  return 0;
}
if (args is ["--players-owner"]) {
  await PlayerNetworkOwnerVerification.RunAsync();
  await PlayerSessionLifecycleVerification.RunAsync();
  Console.WriteLine("PASS shared player entity ownership, references and slot reuse");
  return 0;
}
if (args is ["--players-extended"]) {
  await PlayerExtendedPacketVerification.RunAsync();
  Console.WriteLine("PASS extended player packet owner and relay behavior");
  return 0;
}
if (args is ["--player-lifecycle-handlers"]) {
  await PlayerLifecycleHandlerAcceptanceVerification.RunAsync();
  Console.WriteLine("PASS production player lifecycle handlers, state commits and stale sender rejection");
  return 0;
}
if (args is ["--player-lifecycle-tcp"]) {
  await GatewayLoopbackVerification.RunAsync();
  Console.WriteLine("PASS player active connect/disconnect projections over two real TCP clients");
  return 0;
}
if (args is ["--social-npc-effects"]) {
  await SocialNpcEffectVerification.RunAsync();
  Console.WriteLine("PASS social NPC reactions, Dryad projectile placement and world scope");
  return 0;
}
if (args is ["--projectile-runtime-owner"]) {
  await ProjectileNetworkCommandOwnerGatewayVerification.RunAsync();
  Console.WriteLine("PASS authenticated gateway commands through the production Projectile runtime owner");
  return 0;
}
if (args is ["--pocket-wire"]) {
  return await PocketWireVerification.RunAsync();
}
if (args is ["--steam-modules"]) {
  return await SteamModuleVerification.RunAsync();
}
if (args is ["--tile-placement"]) {
  await TilePlacementPacketVerification.RunAsync();
  Console.WriteLine("PASS tile placement, corrections, section cache and Active Steam connection");
  return 0;
}
if (args is ["--live-tile-placement", var placementPort, var placementFacts, var placementOutput]) {
  return LiveTilePlacementVerification.Run(int.Parse(placementPort), placementFacts, placementOutput);
}
if (args is ["--mining"]) {
  await MiningPacketVerification.RunAsync();
  Console.WriteLine("PASS mining partial hits, final break, repeated hits and section cache");
  return 0;
}
if (args is ["--live-mining", var miningPort, var miningFacts, var miningOutput]) {
  return LiveMiningVerification.Run(int.Parse(miningPort), miningFacts, miningOutput);
}
if (args is ["--real-client", var executable, var output]) {
  return await RealClientVerification.RunAsync(executable, output);
}
if (args.Length != 0) {
  Console.Error.WriteLine("Unrecognized verification arguments; select one supported mode.");
  return 2;
}
var cases = new (string Name, Func<Task> Run)[] {
  ("movement-driven world sections and connection transfer history",
      WorldMovementSectionVerification.RunAsync),
  ("bindings and complete-frame encoding", BindingVerification.RunAsync),
  ("generated protocol coverage and directional bodies", GeneratedProfileVerification.RunAsync),
  ("world join sections, authenticated sender and tile projections",
      WorldPacketBehaviorVerification.RunAsync),
  ("authoritative world time packet publication and direction",
      WorldTimePacketVerification.RunAsync),
  ("authoritative world tile metrics packet publication and direction",
      WorldTileMetricsPacketVerification.RunAsync),
  ("projectile codecs and authenticated gateway owners", ProjectileGatewayVerification.RunAsync),
  ("production Projectile runtime owner through authenticated gateway",
      ProjectileNetworkCommandOwnerGatewayVerification.RunAsync),
  ("production player-slot and password admission", PlayerSlotSessionAuthorityVerification.RunAsync),
  ("session UUID validation and connection identity",
      SessionPacketBehaviorVerification.RunAsync),
  ("social layouts, authenticated relays and bubble lifetime state",
      SocialPacketVerification.RunAsync),
  ("reserved message directions, strict empty notices and TCP state retention",
      ReservedPacketVerification.RunAsync),
  ("world owner thread, entity access, cancellation, capacity and shutdown",
      NetworkWorldOwnerVerification.RunAsync),
  ("player admission upload bounds and actor binding", PlayerAdmissionPacketVerification.RunAsync),
  ("tile placement and Active Steam connection", TilePlacementPacketVerification.RunAsync),
  ("mining partial hits, repeated breaks and section cache", MiningPacketVerification.RunAsync),
  ("immutable routing and interest projections", GatewayDtosVerification.RunAsync),
  ("framing boundaries and byte ownership", FramingVerification.RunAsync),
  ("receive ownership and clean EOF", ConnectionVerification.ReceiveAsync),
  ("partial-frame EOF", ConnectionVerification.PartialEofAsync),
  ("claimed large frame survives caller cancellation", LargeReadVerification.ClaimedCancellationAsync),
  ("clean EOF drains large codec waiters", LargeReadVerification.CleanEofAsync),
  ("Dispose reclaims claimed large codec waiter", LargeReadVerification.DisposeWaiterAsync),
  ("fatal close reclaims large waiter and preserves failure", LargeReadVerification.FatalCloseWaiterAsync),
  ("single reader, cancellation and stale callbacks", ConnectionVerification.ReadingCancellationAsync),
  ("pre-decode admission and receive overflow", ConnectionVerification.AdmissionAndOverflowAsync),
  ("synchronous send callbacks", ConnectionVerification.SynchronousSendAsync),
  ("partial sending and submitted cancellation", ConnectionVerification.PartialSendAndCancellationAsync),
  ("queued write cancellation", ConnectionVerification.QueuedCancellationAsync),
  ("disconnect send certainty", ConnectionVerification.DisconnectCertaintyAsync),
  ("bounded waiting producers", ConnectionVerification.CapacityAsync),
  ("submitted send timeout", ConnectionVerification.SendingTimeoutAsync),
  ("sending progress resets inactivity deadline", SendDeadlineVerification.ProgressResetsDeadlineAsync),
  ("aged queued frames remain unsubmitted", SendDeadlineVerification.QueuedExpiryAsync),
  ("provably rejected submission", ConnectionVerification.SubmissionFailureAsync),
  ("ambiguous failed submission", ConnectionVerification.AmbiguousSubmissionAsync),
  ("concurrent complete-frame writes", ConnectionVerification.ConcurrentWritesAsync),
  ("inline disconnect during submission", ConnectionVerification.InlineDisconnectAsync),
  ("invalid sent accounting", ConnectionVerification.InvalidCompletionAsync),
  ("partial-frame progress deadline", ConnectionVerification.PartialFrameTimeoutAsync),
  ("fatal decoder failure terminates queued input", ConnectionVerification.FatalDecodeAsync),
  ("gateway handshake and authoritative actor", GatewayVerification.HandshakeAndActorAsync),
  ("gateway deny before decode", GatewayVerification.DefaultDenyAsync),
  ("wire version and password admission", GatewayVerification.VersionAndPasswordAsync),
  ("client version opt-out preserves slot and password admission",
      GatewayVerification.IgnoredClientVersionAsync),
  ("startup password requirement is immutable", GatewayPolicyVerification.PasswordSnapshotAsync),
  ("protocol diagnostics preserve body offset", GatewayPolicyVerification.ProtocolDiagnosticAsync),
  ("owner-confirmed session progress", GatewayVerification.OwnerStageAndPolicyAsync),
  ("handshake phase deadline", GatewayVerification.SessionDeadlineAsync),
  ("module action admission and rate windows", GatewayVerification.PolicyRateAndModuleAsync),
  ("unsafe and duplicate policy registration", GatewayVerification.RegistrationSafetyAsync),
  ("routing targets, self echo and section generations", RoutingVerification.TargetsAndSectionsAsync),
  ("server-owned world effects and stale runtime rejection", RoutingVerification.ServerWorldEffectsAsync),
  ("slow broadcast target isolation", RoutingVerification.SlowTargetIsolationAsync),
  ("noncooperative owner deadline and late result", OwnerTimeoutVerification.NoncooperativeOwnerAsync),
  ("progressing self-send retains gateway deadline", GatewayDeadlineVerification.ProgressingSelfSendAsync),
  ("progressing recipient closes on source deadline", GatewayDeadlineVerification.ProgressingRecipientAsync),
  ("late authority admission after timeout and stop", GatewayDeadlineVerification.LateAdmissionAsync),
  ("closed session rejects late state mutation", GatewayDeadlineVerification.ClosedSessionGuardsAsync),
  ("control reserve and FIFO barriers", ControlReserveVerification.RunAsync),
  ("explicit host authorization and Ping capability", GatewayCapabilitiesVerification.HostAndPingAsync),
  ("latency Ping echo, admission, malformed input and rate limit",
      GatewayCapabilitiesVerification.LatencyPingAsync),
  ("bounded session admission", GatewayCapabilitiesVerification.SessionCapacityAsync),
  ("snapshot ownership, versions and expiration", SnapshotCacheVerification.OwnershipAndVersionsAsync),
  ("snapshot LRU and process capacity", SnapshotCacheVerification.LimitsAndEvictionAsync),
  ("cache-backed eligible section dispatch", CachedDispatchVerification.RunAsync),
  ("bounded reconnect and terminal errors", ReconnectVerification.AttemptsAndTerminalFailuresAsync),
  ("full jitter ceilings and stop", ReconnectVerification.JitterAndStopAsync),
  ("overall reconnect deadline", ReconnectVerification.TotalBudgetAsync),
  ("fresh connection epochs and stable Active reset", ReconnectVerification.FreshConnectionsAndStabilityAsync),
  ("late connect cleanup and active stop", ReconnectVerification.LateConnectionAndActiveStopAsync),
  ("generated packets over real gateway listener", GatewayLoopbackVerification.RunAsync),
  ("host registration, real listener and shared cleanup", HostCompositionVerification.RunAsync),
  ("real NetCoreServer loopback", LoopbackVerification.RunAsync)
};
int failed = 0;
foreach ((string name, Func<Task> run) in cases) {
  try {
    await run().WaitAsync(TimeSpan.FromSeconds(15));
    Console.WriteLine($"PASS {name}");
  } catch (Exception exception) {
    failed++;
    Console.Error.WriteLine($"FAIL {name}: {exception}");
  }
}
Console.WriteLine($"Verification groups: {cases.Length - failed} passed, {failed} failed.");
return failed == 0 ? 0 : 1;
