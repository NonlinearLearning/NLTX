using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MinimalImplementation.Server.Host;

/// <summary>Serializes synchronous domain commits on one logical server loop.</summary>
public sealed class ServerLoop : IAsyncDisposable
{
  private readonly Channel<WorkItem> _queue;
  private Task? _worker;
  private int _state;

  [ThreadStatic]
  private static ServerLoop? s_currentLoop;

  public ServerLoop(int capacity)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
    _queue = Channel.CreateBounded<WorkItem>(new BoundedChannelOptions(capacity)
    {
      FullMode = BoundedChannelFullMode.Wait,
      SingleReader = true,
      SingleWriter = false,
      AllowSynchronousContinuations = false
    });
  }

  public bool IsRunning => Volatile.Read(ref _state) == 1;

  public bool IsCurrent => ReferenceEquals(s_currentLoop, this);

  public Task StartAsync()
  {
    if (Interlocked.CompareExchange(ref _state, 1, 0) != 0)
    {
      throw new InvalidOperationException("The server loop can only be started once.");
    }

    _worker = Task.Run(RunAsync);
    return Task.CompletedTask;
  }

  public async Task SubmitAsync(Action commit, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(commit);
    if (ReferenceEquals(s_currentLoop, this))
    {
      throw new InvalidOperationException(
        "A commit cannot enqueue work onto the server loop and wait for itself.");
    }

    if (!IsRunning)
    {
      throw new InvalidOperationException("The server loop is not accepting work.");
    }

    var work = new WorkItem(commit);
    await _queue.Writer.WriteAsync(work, cancellationToken).ConfigureAwait(false);
    await work.Completion.Task.ConfigureAwait(false);
  }

  public async ValueTask StopAsync()
  {
    int priorState = Interlocked.Exchange(ref _state, 2);
    if (priorState == 3)
    {
      return;
    }

    _queue.Writer.TryComplete();
    if (_worker is not null)
    {
      await _worker.ConfigureAwait(false);
    }

    Volatile.Write(ref _state, 3);
  }

  public async ValueTask DisposeAsync()
  {
    await StopAsync().ConfigureAwait(false);
  }

  private async Task RunAsync()
  {
    await foreach (WorkItem work in _queue.Reader.ReadAllAsync().ConfigureAwait(false))
    {
      s_currentLoop = this;
      try
      {
        work.Commit();
        work.Completion.TrySetResult();
      }
      catch (Exception exception)
      {
        work.Completion.TrySetException(exception);
      }
      finally
      {
        s_currentLoop = null;
      }
    }
  }

  private sealed class WorkItem(Action commit)
  {
    public Action Commit { get; } = commit;

    public TaskCompletionSource Completion { get; } =
      new(TaskCreationOptions.RunContinuationsAsynchronously);
  }
}
