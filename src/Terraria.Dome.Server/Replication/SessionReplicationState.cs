using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;

namespace Terraria.Dome.Server.Replication;

public sealed class SessionReplicationState : IDisposable
{
  private const int DefaultJoinNpcFrameBudget = 67;
  private const int MaximumPendingFrames = 4096;
  private static readonly TimeSpan ReplicationWriteBudget = TimeSpan.FromMilliseconds(500);
  private readonly CancellationTokenSource _writeCancellation = new();
  private readonly Func<byte[], CancellationToken, Task> _writeFrameAsync;
  private readonly SemaphoreSlim _writeGate = new(1, 1);
  private readonly SemaphoreSlim _pendingFrameSignal = new(0);
  private readonly object _viewPositionGate = new();
  private readonly ConcurrentQueue<byte[]> _pendingFrames = new();
  private readonly SessionSectionVisibility _visibility = new();
  private readonly PlayerReplicationCursor _playerCursor = new();
  private readonly ItemReplicationCursor _itemCursor = new();
  private readonly CombatReplicationCursor _combatCursor = new();
  private readonly ChestReplicationCursor _chestCursor = new();
  private readonly DoorReplicationCursor _doorCursor = new();
  private readonly SignReplicationCursor _signCursor = new();
  private readonly HashSet<WorldSectionCoordinates> _visibleSections = new();
  private readonly Task _writerTask;
  private Exception? _writeFailure;
  private int _pendingFrameCount;
  private int _npcFrameCount;
  private bool _disposed;
  private bool _defaultNpcReconciliationPending = true;
  private bool _isInitialVisibilityLocked;
  private bool _worldRulesSent;
  private SimulationVector? _clientViewPosition;

  public SessionReplicationState(Func<byte[], CancellationToken, Task> writeFrameAsync)
  {
    _writeFrameAsync = writeFrameAsync ?? throw new ArgumentNullException(nameof(writeFrameAsync));
    _writerTask = WritePendingFramesAsync();
  }

  public IReadOnlySet<WorldSectionCoordinates> VisibleSections => _visibleSections;
  public bool IsActive { get; private set; }
  public bool IsInitialVisibilityLocked => _isInitialVisibilityLocked;

  public SimulationVector? ClientViewPosition
  {
    get
    {
      lock (_viewPositionGate)
      {
        return _clientViewPosition;
      }
    }
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    _writeCancellation.Cancel();
    _visibility.Clear();
    _playerCursor.Clear();
    _itemCursor.Clear();
    _combatCursor.Clear();
    _chestCursor.Clear();
    _doorCursor.Clear();
    _signCursor.Clear();
    _visibleSections.Clear();
    lock (_viewPositionGate)
    {
      _clientViewPosition = null;
    }
    try
    {
      _writerTask.Wait(TimeSpan.FromMilliseconds(50));
    }
    catch (AggregateException)
    {
    }

    _writeCancellation.Dispose();
  }

  public void MarkActive()
  {
    ThrowIfDisposed();
    IsActive = true;
  }

  public void LockInitialVisibility()
  {
    ThrowIfDisposed();
    _isInitialVisibilityLocked = true;
  }

  public void UnlockVisibility()
  {
    ThrowIfDisposed();
    _isInitialVisibilityLocked = false;
  }

  public void SetClientViewPosition(SimulationVector position)
  {
    ThrowIfDisposed();
    if (float.IsNaN(position.X) || float.IsNaN(position.Y) ||
        float.IsInfinity(position.X) || float.IsInfinity(position.Y))
    {
      return;
    }

    lock (_viewPositionGate)
    {
      _clientViewPosition = position;
    }
  }

  public IReadOnlyList<WorldSectionSnapshot> CollectChangedSections(
    IReadOnlyList<WorldSectionSnapshot> snapshots)
  {
    ThrowIfDisposed();
    for (int index = 0; index < snapshots.Count; index++)
    {
      _visibleSections.Add(snapshots[index].Coordinates);
    }

    return _visibility.CollectChangedSections(snapshots);
  }

  public IReadOnlyList<WorldSectionSnapshot> ReplaceVisibleSections(
    IReadOnlyList<WorldSectionSnapshot> snapshots)
  {
    ThrowIfDisposed();
    _visibleSections.Clear();
    for (int index = 0; index < snapshots.Count; index++)
    {
      _visibleSections.Add(snapshots[index].Coordinates);
    }

    _visibility.RemoveNotIn(_visibleSections);
    return _visibility.CollectChangedSections(snapshots);
  }

  public void MarkSectionsCurrent(
    WorldGrid world,
    IReadOnlyCollection<WorldSectionCoordinates> coordinates)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(coordinates);
    foreach (WorldSectionCoordinates coordinate in coordinates)
    {
      if (_visibleSections.Contains(coordinate))
      {
        _visibility.MarkSent(world.CreateSectionSnapshot(coordinate));
      }
    }
  }

  public IReadOnlyList<PlayerReplicationState> CollectChangedPlayers(
    IReadOnlyList<PlayerReplicationState> states)
  {
    ThrowIfDisposed();
    return _playerCursor.CollectChanged(states);
  }

  public bool ShouldSendCombatNpc(NpcReplicationSnapshot snapshot)
  {
    ThrowIfDisposed();
    return _combatCursor.ShouldSend(snapshot);
  }

  public void MarkCombatNpcSent(NpcReplicationSnapshot snapshot)
  {
    ThrowIfDisposed();
    _combatCursor.MarkNpcSent(snapshot);
    _npcFrameCount++;
  }

  public void MarkDefaultCombatNpcSent(NpcReplicationSnapshot snapshot)
  {
    ThrowIfDisposed();
    _combatCursor.MarkNpcSent(snapshot);
  }

  public bool TryTakeDefaultNpcReconciliation()
  {
    ThrowIfDisposed();
    if (!_defaultNpcReconciliationPending || _npcFrameCount >= DefaultJoinNpcFrameBudget)
    {
      return false;
    }

    _defaultNpcReconciliationPending = false;
    _npcFrameCount++;
    return true;
  }

  public bool ShouldSendCombatProjectile(ProjectileReplicationSnapshot snapshot)
  {
    ThrowIfDisposed();
    return _combatCursor.ShouldSend(snapshot);
  }

  public bool WasCombatNpcSent(int replicationId)
  {
    ThrowIfDisposed();
    return _combatCursor.WasNpcSent(replicationId);
  }

  public bool WasCombatProjectileSent(int replicationId)
  {
    ThrowIfDisposed();
    return _combatCursor.WasProjectileSent(replicationId);
  }

  public bool ShouldSendItem(ItemReplicationSnapshot snapshot)
  {
    ThrowIfDisposed();
    return _itemCursor.ShouldSend(snapshot);
  }

  public bool ShouldSendChest(ChestSnapshot snapshot)
  {
    ThrowIfDisposed();
    return _chestCursor.ShouldSend(snapshot);
  }

  public bool ShouldSendWorldRules(long tick, int intervalTicks)
  {
    ThrowIfDisposed();
    if (!_worldRulesSent || tick % intervalTicks == 0)
    {
      _worldRulesSent = true;
      return true;
    }

    return false;
  }

  public bool ShouldSendDoor(DoorSnapshot snapshot)
  {
    ThrowIfDisposed();
    return _doorCursor.ShouldSend(snapshot);
  }

  public bool ShouldSendSign(SignSnapshot snapshot)
  {
    ThrowIfDisposed();
    return _signCursor.ShouldSend(snapshot);
  }

  public Task WriteFramesAsync(
    IReadOnlyList<byte[]> frames,
    CancellationToken cancellationToken)
  {
    ThrowIfDisposed();
    ThrowIfWriteFailed();
    cancellationToken.ThrowIfCancellationRequested();
    for (int index = 0; index < frames.Count; index++)
    {
      int pendingFrames = Interlocked.Increment(ref _pendingFrameCount);
      if (pendingFrames > MaximumPendingFrames)
      {
        Interlocked.Decrement(ref _pendingFrameCount);
        throw new IOException("The session replication queue budget was exceeded.");
      }

      _pendingFrames.Enqueue(frames[index]);
      _pendingFrameSignal.Release();
    }

    return Task.CompletedTask;
  }

  private async Task WritePendingFramesAsync()
  {
    try
    {
      while (!_writeCancellation.IsCancellationRequested)
      {
        await _pendingFrameSignal.WaitAsync(_writeCancellation.Token);
        if (!_pendingFrames.TryDequeue(out byte[]? frame))
        {
          continue;
        }

        Interlocked.Decrement(ref _pendingFrameCount);
        using CancellationTokenSource frameCancellation =
          CancellationTokenSource.CreateLinkedTokenSource(_writeCancellation.Token);
        frameCancellation.CancelAfter(ReplicationWriteBudget);
        await _writeGate.WaitAsync(frameCancellation.Token);
        try
        {
          await _writeFrameAsync(frame, frameCancellation.Token);
        }
        finally
        {
          _writeGate.Release();
        }
      }
    }
    catch (OperationCanceledException) when (_writeCancellation.IsCancellationRequested)
    {
    }
    catch (SocketException exception)
    {
      _writeFailure = new IOException("The session socket was closed while writing a frame.", exception);
    }
    catch (OperationCanceledException)
    {
      _writeFailure = new IOException("The session replication write budget was exceeded.");
    }
    catch (IOException exception)
    {
      _writeFailure = exception;
    }
  }

  private void ThrowIfDisposed()
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(SessionReplicationState));
    }
  }

  private void ThrowIfWriteFailed()
  {
    if (_writeFailure is not null)
    {
      throw new IOException("The session replication writer failed.", _writeFailure);
    }
  }
}
