using NSSLC.NetworkVerification;

var cases = new (string Name, Func<Task> Run)[] {
  ("bindings and complete-frame encoding", BindingVerification.RunAsync),
  ("generated protocol coverage and directional bodies", GeneratedProfileVerification.RunAsync),
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
  ("startup password requirement is immutable", GatewayPolicyVerification.PasswordSnapshotAsync),
  ("protocol diagnostics preserve body offset", GatewayPolicyVerification.ProtocolDiagnosticAsync),
  ("owner-confirmed session progress", GatewayVerification.OwnerStageAndPolicyAsync),
  ("handshake phase deadline", GatewayVerification.SessionDeadlineAsync),
  ("module action admission and rate windows", GatewayVerification.PolicyRateAndModuleAsync),
  ("unsafe and duplicate policy registration", GatewayVerification.RegistrationSafetyAsync),
  ("routing targets, self echo and section generations", RoutingVerification.TargetsAndSectionsAsync),
  ("slow broadcast target isolation", RoutingVerification.SlowTargetIsolationAsync),
  ("noncooperative owner deadline and late result", OwnerTimeoutVerification.NoncooperativeOwnerAsync),
  ("progressing self-send retains gateway deadline", GatewayDeadlineVerification.ProgressingSelfSendAsync),
  ("progressing recipient closes on source deadline", GatewayDeadlineVerification.ProgressingRecipientAsync),
  ("late authority admission after timeout and stop", GatewayDeadlineVerification.LateAdmissionAsync),
  ("closed session rejects late state mutation", GatewayDeadlineVerification.ClosedSessionGuardsAsync),
  ("control reserve and FIFO barriers", ControlReserveVerification.RunAsync),
  ("explicit host authorization and Ping capability", GatewayCapabilitiesVerification.HostAndPingAsync),
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
