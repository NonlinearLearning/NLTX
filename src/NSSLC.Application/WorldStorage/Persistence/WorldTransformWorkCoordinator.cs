using System;
using System.Threading;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Runs world mutations under the same I/O gate used by coherent saves.
/// </summary>
public sealed class WorldTransformWorkCoordinator
{
  private readonly WorldTransformTransactionComponent _transactions;
  private readonly IWorldStorageIoGate _ioGate;
  private readonly IWorldTransformationScheduler _scheduler;

  public WorldTransformWorkCoordinator(
    WorldTransformTransactionComponent transactions,
    IWorldStorageIoGate ioGate,
    IWorldTransformationScheduler scheduler)
  {
    _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
    _ioGate = ioGate ?? throw new ArgumentNullException(nameof(ioGate));
    _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
  }

  public void Start(Action transform, Action? mainThreadFollowup = null)
  {
    Start(transform, mainThreadFollowup, transactionCompleted: null);
  }

  public void Start(
    Action transform,
    Action? mainThreadFollowup,
    Action? transactionCompleted)
  {
    ArgumentNullException.ThrowIfNull(transform);
    WorldTransformTransactionLease transaction =
      WorldTransformTransactionSystem.BeginTransaction(_transactions);
    try
    {
      _scheduler.ScheduleBackground(() => RunTransaction(
        transaction,
        transform,
        mainThreadFollowup,
        transactionCompleted));
    }
    catch
    {
      transaction.Dispose();
      throw;
    }
  }

  private void RunTransaction(
    WorldTransformTransactionLease transaction,
    Action transform,
    Action? mainThreadFollowup,
    Action? transactionCompleted)
  {
    try
    {
      using IDisposable ioLease = _ioGate.Enter(CancellationToken.None);
      transform.Invoke();
    }
    finally
    {
      try
      {
        transaction.Dispose();
      }
      finally
      {
        try
        {
          transactionCompleted?.Invoke();
        }
        finally
        {
          if (mainThreadFollowup is not null)
          {
            _scheduler.QueueMainThread(mainThreadFollowup);
          }
        }
      }
    }
  }
}
