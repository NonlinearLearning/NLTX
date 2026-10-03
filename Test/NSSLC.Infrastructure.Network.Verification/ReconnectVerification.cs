using NSSLC.Infrastructure.Network;
using Terraria.Network;

namespace NSSLC.NetworkVerification;

internal static class ReconnectVerification {
  public static async Task AttemptsAndTerminalFailuresAsync() {
    int attempted = 0;
    await using (var coordinator = new ReconnectCoordinator((timeout, _) => {
      attempted++;
      Verify.That(timeout <= TimeSpan.FromSeconds(5), "Each connect timeout must respect its five-second cap.");
      return ValueTask.FromException<IPacketConnection>(new IOException("Temporary reset."));
    }, random: () => 0)) {
      ReconnectRunResult exhausted = await coordinator.RunAsync((_, _) =>
          Task.FromResult(ReconnectSessionOutcome.Stop));
      Verify.That(exhausted.Reason == "AttemptsExhausted" && exhausted.Attempts == 8
          && attempted == 8 && coordinator.State == ReconnectState.Stopped,
          "Repeated transient failures must stop after exactly the default eight attempts.");
      Verify.Throws<InvalidOperationException>(() => coordinator.RunAsync((_, _) =>
          Task.FromResult(ReconnectSessionOutcome.Stop)));
    }
    foreach (Exception terminal in new Exception[] {
      new PacketProtocolException("VersionRejected"), new InvalidOperationException("Invalid config."),
      new OperationCanceledException("Unrelated operation canceled.")
    }) {
      await using var coordinator = new ReconnectCoordinator((_, _) =>
          ValueTask.FromException<IPacketConnection>(terminal), random: () => 0);
      ReconnectRunResult result = await coordinator.RunAsync((_, _) =>
          Task.FromResult(ReconnectSessionOutcome.Stop));
      Verify.That(result.Attempts == 1 && result.Reason == "TerminalFailure"
          && ReferenceEquals(result.Error, terminal),
          "Protocol, configuration and unrelated cancellation failures must terminate without retry.");
    }
  }

  public static async Task JitterAndStopAsync() {
    var time = new ManualTimeProvider();
    await using var coordinator = new ReconnectCoordinator((_, _) =>
        ValueTask.FromException<IPacketConnection>(new IOException("Transient.")),
        timeProvider: time, random: () => 0.5);
    Task<ReconnectRunResult> run = coordinator.RunAsync((_, _) =>
        Task.FromResult(ReconnectSessionOutcome.Stop));
    double[] expectedMilliseconds = { 100, 200, 400, 800, 1600, 3200, 5000 };
    for (int index = 0; index < expectedMilliseconds.Length; index++) {
      int expectedAttempts = index + 1;
      await Verify.EventuallyAsync(() => coordinator.Attempts == expectedAttempts
          && coordinator.State == ReconnectState.Backoff && time.NextTimerDelay is not null,
          "A retry did not enter a timed backoff.");
      TimeSpan expected = TimeSpan.FromMilliseconds(expectedMilliseconds[index]);
      Verify.That(time.NextTimerDelay == expected,
          "Full jitter must honor exponential ceilings and the ten-second maximum delay.");
      time.Advance(expected - TimeSpan.FromTicks(1));
      Verify.That(coordinator.Attempts == expectedAttempts,
          "A reconnect attempt must not begin before its selected backoff expires.");
      time.Advance(TimeSpan.FromTicks(1));
    }
    ReconnectRunResult exhausted = await run.WaitAsync(TimeSpan.FromSeconds(5));
    Verify.That(exhausted.Attempts == 8 && exhausted.Reason == "AttemptsExhausted",
        "Jittered retries must retain the same finite attempt budget.");

    var stoppingTime = new ManualTimeProvider();
    await using var stopped = new ReconnectCoordinator((_, _) =>
        ValueTask.FromException<IPacketConnection>(new IOException("Transient.")),
        timeProvider: stoppingTime, random: () => 0.5);
    Task<ReconnectRunResult> stoppingRun = stopped.RunAsync((_, _) =>
        Task.FromResult(ReconnectSessionOutcome.Stop));
    stopped.Stop();
    Verify.That((await stoppingRun).Reason == "Stopped" && stopped.Attempts == 1,
        "Explicit Stop must cancel an outstanding backoff.");
    stoppingTime.Advance(TimeSpan.FromMinutes(1));
    Verify.That(stopped.Attempts == 1 && stopped.State == ReconnectState.Stopped,
        "A canceled backoff must not revive a stopped coordinator.");
  }

  public static async Task TotalBudgetAsync() {
    var time = new ManualTimeProvider();
    var connectTimeouts = new List<TimeSpan>();
    await using var coordinator = new ReconnectCoordinator((timeout, _) => {
      connectTimeouts.Add(timeout);
      time.Advance(TimeSpan.FromSeconds(30));
      return ValueTask.FromException<IPacketConnection>(new IOException("Transient."));
    }, timeProvider: time, random: () => 0);
    ReconnectRunResult result = await coordinator.RunAsync((_, _) =>
        Task.FromResult(ReconnectSessionOutcome.Stop));
    Verify.That(result.Attempts == 2 && result.Reason == "BudgetExhausted"
        && connectTimeouts.All(item => item <= TimeSpan.FromSeconds(5)),
        "The overall 60-second budget must terminate retries before the attempt count can grow.");
  }

  public static async Task FreshConnectionsAndStabilityAsync() {
    var time = new ManualTimeProvider();
    var connections = new List<StubPacketConnection>();
    Guid sessionKey = Guid.NewGuid();
    await using var coordinator = new ReconnectCoordinator((_, _) => {
      var connection = new StubPacketConnection(new(sessionKey, connections.Count + 1));
      connections.Add(connection);
      return ValueTask.FromResult<IPacketConnection>(connection);
    }, new ReconnectOptions { MaximumAttempts = 2 }, time, () => 0);
    ReconnectSession? retained = null;
    ReconnectRunResult result = await coordinator.RunAsync((session, _) => {
      retained = session;
      int attempt = connections.Count;
      if (attempt is 1 or 2) {
        session.MarkActive();
        time.Advance(TimeSpan.FromSeconds(attempt == 1 ? 1 : 31));
      }
      return Task.FromResult(attempt == 4 ? ReconnectSessionOutcome.Stop
          : ReconnectSessionOutcome.RetryAfterDisconnect);
    });
    Verify.That(result.Attempts == 4 && result.Reason == "SessionStopped"
        && connections.All(item => item.Disposals == 1 && item.Writes == 0)
        && connections.Select(item => item.Identity.Epoch).SequenceEqual(new long[] { 1, 2, 3, 4 }),
        "Only 30-second stable Active may reset failure history; each retry must use and dispose a fresh connection.");
    Verify.Throws<InvalidOperationException>(() => retained!.MarkActive());

    var shortTime = new ManualTimeProvider();
    int shortConnections = 0;
    await using var shortLived = new ReconnectCoordinator((_, _) =>
        ValueTask.FromResult<IPacketConnection>(new StubPacketConnection(new(Guid.NewGuid(),
            ++shortConnections))), new ReconnectOptions { MaximumAttempts = 2 }, shortTime, () => 0);
    ReconnectRunResult shortResult = await shortLived.RunAsync((session, _) => {
      session.MarkActive();
      shortTime.Advance(TimeSpan.FromSeconds(1));
      return Task.FromResult(ReconnectSessionOutcome.RetryAfterDisconnect);
    });
    Verify.That(shortResult.Attempts == 2 && shortResult.Reason == "AttemptsExhausted",
        "Brief successful TCP sessions must not reset retry history into an infinite reconnect loop.");
  }

  public static async Task LateConnectionAndActiveStopAsync() {
    var pending = new TaskCompletionSource<IPacketConnection>(TaskCreationOptions.RunContinuationsAsynchronously);
    int callbacks = 0;
    await using (var coordinator = new ReconnectCoordinator((_, _) => new(pending.Task))) {
      Task<ReconnectRunResult> run = coordinator.RunAsync((_, _) => {
        callbacks++;
        return Task.FromResult(ReconnectSessionOutcome.Stop);
      });
      coordinator.Stop();
      Verify.That((await run).Reason == "Stopped", "Stop must cancel a noncooperative connect wait.");
      var late = new StubPacketConnection(new(Guid.NewGuid(), 1));
      pending.TrySetResult(late);
      await Verify.EventuallyAsync(() => late.Disposals == 1,
          "A connect completing after Stop must dispose its isolated late connection.");
      Verify.That(callbacks == 0 && late.Writes == 0 && coordinator.State == ReconnectState.Stopped,
          "Late connect completion must not start rebuilding or replay any old packet.");
    }

    var activeConnection = new StubPacketConnection(new(Guid.NewGuid(), 1));
    var noncooperative = new TaskCompletionSource<ReconnectSessionOutcome>(
        TaskCreationOptions.RunContinuationsAsynchronously);
    await using var active = new ReconnectCoordinator((_, _) =>
        ValueTask.FromResult<IPacketConnection>(activeConnection));
    ReconnectSession? retained = null;
    Task<ReconnectRunResult> activeRun = active.RunAsync((session, _) => {
      retained = session;
      session.MarkActive();
      return noncooperative.Task;
    });
    active.Stop();
    Verify.That((await activeRun).Reason == "Stopped" && activeConnection.Disposals == 1,
        "Stop must terminate an Active callback ignoring cancellation and dispose its connection.");
    Verify.Throws<InvalidOperationException>(() => retained!.MarkActive());
    noncooperative.TrySetResult(ReconnectSessionOutcome.RetryAfterDisconnect);
    Verify.That(active.Attempts == 1 && active.State == ReconnectState.Stopped,
        "A late active callback must not reconnect after lifecycle stop.");
  }
}
