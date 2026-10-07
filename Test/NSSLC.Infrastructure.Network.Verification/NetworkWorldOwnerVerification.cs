using EntityEcs;
using EntityEcs.Components;
using Terraria.Network;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Relationships;

namespace NSSLC.NetworkVerification;

internal static class NetworkWorldOwnerVerification {
  public static async Task RunAsync() {
    await VerifyOwnedEntityAccessAsync();
    await VerifyCancellationAsync();
    await VerifyShutdownAndCapacityAsync();
    await VerifyFactoryFailureAsync();
  }

  private static async Task VerifyOwnedEntityAccessAsync() {
    int factoryThread = 0;
    await using var owner = new NetworkWorldOwner(() => {
      factoryThread = Environment.CurrentManagedThreadId;
      return new LoadedWorldSession();
    });
    await owner.Ready.WaitAsync(TimeSpan.FromSeconds(5));
    RuntimeEntityHandle handle = await owner.InvokeAsync(CreateLocatedEntity);
    (float X, float Y, int Thread) snapshot = await Task.Run(async () =>
        await owner.InvokeAsync(session => {
          LocationComponent location = ReadLocation(session, handle);
          return (location.X, location.Y, Environment.CurrentManagedThreadId);
        }));
    Verify.That(snapshot.X == 1 && snapshot.Y == 2 && snapshot.Thread == factoryThread,
        "Network commands must inspect the actual published entity on its creation thread.");
    await Verify.ThrowsAsync<InvalidDataException>(() => owner.InvokeAsync<bool>(
        _ => throw new InvalidDataException("Rejected operation")).AsTask());
    Verify.That(await owner.InvokeAsync(session => ReadLocation(session, handle).X) == 1,
        "A failed command must leave the owning runtime usable by the next command.");
  }

  private static async Task VerifyCancellationAsync() {
    await using var owner = new NetworkWorldOwner(() => new LoadedWorldSession());
    await owner.Ready.WaitAsync(TimeSpan.FromSeconds(5));
    RuntimeEntityHandle handle = await owner.InvokeAsync(CreateLocatedEntity);
    var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    using var release = new ManualResetEventSlim();
    using var runningCancellation = new CancellationTokenSource();
    using var queuedCancellation = new CancellationTokenSource();
    Task<float> running = owner.InvokeAsync(session => {
      started.SetResult();
      WaitForRelease(release);
      SetLocationX(session, handle, 3);
      return ReadLocation(session, handle).X;
    }, runningCancellation.Token).AsTask();
    try {
      await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
      Task<bool> canceled = owner.InvokeAsync(session => {
        SetLocationX(session, handle, 99);
        return true;
      }, queuedCancellation.Token).AsTask();
      queuedCancellation.Cancel();
      runningCancellation.Cancel();
      await Verify.ThrowsAsync<OperationCanceledException>(() => canceled);
      release.Set();
      Verify.That(await running == 3,
          "Cancellation after execution starts must return the actual committed result.");
      Verify.That(await owner.InvokeAsync(session => ReadLocation(session, handle).X) == 3,
          "Cancellation while queued must prevent the subsequent component write.");
    } finally {
      release.Set();
    }
  }

  private static async Task VerifyShutdownAndCapacityAsync() {
    LoadedWorldSession? createdSession = null;
    await using var owner = new NetworkWorldOwner(() => {
      createdSession = new LoadedWorldSession();
      return createdSession;
    }, maximumPendingCommands: 1);
    await owner.Ready.WaitAsync(TimeSpan.FromSeconds(5));
    RuntimeEntityHandle handle = await owner.InvokeAsync(CreateLocatedEntity);
    var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    using var release = new ManualResetEventSlim();
    Task<float> running = owner.InvokeAsync(session => {
      started.SetResult();
      WaitForRelease(release);
      SetLocationX(session, handle, 4);
      return ReadLocation(session, handle).X;
    }).AsTask();
    try {
      await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
      Task<bool> pending = owner.InvokeAsync(session => {
        SetLocationX(session, handle, 99);
        return true;
      }).AsTask();
      await Verify.ThrowsAsync<InvalidOperationException>(() => owner.InvokeAsync(session => {
        SetLocationX(session, handle, 100);
        return true;
      }).AsTask());
      Task shutdown = owner.DisposeAsync().AsTask();
      Verify.That(!shutdown.IsCompleted,
          "World shutdown must await the running command before disposing its entity runtime.");
      Verify.Throws<ObjectDisposedException>(() => owner.InvokeAsync(_ => true));
      release.Set();
      Verify.That(await running == 4, "The already running world command must finish during stop.");
      await Verify.ThrowsAsync<ObjectDisposedException>(() => pending);
      await shutdown.WaitAsync(TimeSpan.FromSeconds(5));
      Verify.That(createdSession?.IsDisposed == true,
          "Shutdown must dispose the real loaded session and its entity storage.");
      Verify.Throws<ObjectDisposedException>(() => owner.InvokeAsync(_ => true));
    } finally {
      release.Set();
    }
  }

  private static async Task VerifyFactoryFailureAsync() {
    using var release = new ManualResetEventSlim();
    var owner = new NetworkWorldOwner(() => {
      WaitForRelease(release);
      throw new InvalidDataException("World load rejected");
    });
    Task<bool> pending = owner.InvokeAsync(_ => true).AsTask();
    release.Set();
    await Verify.ThrowsAsync<InvalidDataException>(() => owner.Ready);
    await Verify.ThrowsAsync<InvalidDataException>(() => pending);
    await Verify.ThrowsAsync<InvalidDataException>(() => owner.DisposeAsync().AsTask());
  }

  private static RuntimeEntityHandle CreateLocatedEntity(LoadedWorldSession session) {
    RuntimeEntityHandle handle = session.EntityRuntime.CreateEntity();
    Verify.That(session.EntityRuntime.TryAttach(handle, new LocationComponent(1, 2))
        && session.EntityRuntime.TryPublishEntity(handle),
        "The fixture must attach and publish an actual ECS location component.");
    return handle;
  }

  private static LocationComponent ReadLocation(LoadedWorldSession session,
      RuntimeEntityHandle handle) {
    LocationComponent location = default;
    Verify.That(session.EntityRuntime.TryInspect<LocationComponent>(handle,
        (in LocationComponent current) => location = current),
        "The entity location must be inspected through the formal component store.");
    return location;
  }

  private static void SetLocationX(LoadedWorldSession session, RuntimeEntityHandle handle,
      float value) {
    Verify.That(session.EntityRuntime.TryEdit<LocationComponent>(handle,
        (ref LocationComponent location) => location.X = value),
        "The world command must commit its component update on the owner thread.");
  }

  private static void WaitForRelease(ManualResetEventSlim release) {
    if (!release.Wait(TimeSpan.FromSeconds(5))) {
      throw new TimeoutException("The world owner fixture was not released.");
    }
  }
}
