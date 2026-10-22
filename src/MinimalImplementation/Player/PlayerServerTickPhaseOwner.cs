using System;
using MinimalImplementation.Persistence;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// Adapts the authoritative player update service to the production server tick contract.
/// </summary>
/// <remarks>
/// The context factory is evaluated on every tick so world facts and entity-lifecycle queries
/// remain owned by the composition root. The update service performs the player read/compute
/// pass and commits changed player state through <see cref="PlayerOwner"/>.
/// </remarks>
public sealed class PlayerServerTickPhaseOwner : IServerTickPhaseOwner
{
  private readonly PlayerUpdateService _updates;
  private readonly Func<PlayerUpdateContext> _contextFactory;
  private readonly Action<PlayerUpdateResult>? _captureResult;

  public PlayerServerTickPhaseOwner(
    PlayerUpdateService updates,
    Func<PlayerUpdateContext> contextFactory,
    Action<PlayerUpdateResult>? captureResult = null)
  {
    ArgumentNullException.ThrowIfNull(updates);
    ArgumentNullException.ThrowIfNull(contextFactory);
    _updates = updates;
    _contextFactory = contextFactory;
    _captureResult = captureResult;
  }

  public ServerTickPhase Phase => ServerTickPhase.Player;

  public object TransactionIdentity => this;

  public PlayerUpdateResult? LastResult { get; private set; }

  public void Advance(long gameTickIndex)
  {
    PlayerUpdateContext context = _contextFactory()
      ?? throw new InvalidOperationException(
        "The player tick owner context factory returned no context.");
    LastResult = _updates.Tick(gameTickIndex, context);
    // The root captures outputs here; transport delivery runs outside this tick callback.
    _captureResult?.Invoke(LastResult);
  }
}
