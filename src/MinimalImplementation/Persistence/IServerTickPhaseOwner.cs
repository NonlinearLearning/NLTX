using System.Collections.Generic;

namespace MinimalImplementation.Persistence;

/// <summary>One domain owner's synchronous contribution to the fixed server tick order.</summary>
public interface IServerTickPhaseOwner
{
  ServerTickPhase Phase { get; }

  /// <summary>
  /// Stable reference identity of the transaction advanced by this registration. A scheduler
  /// registration must not expose the same identity more than once.
  /// </summary>
  object TransactionIdentity { get; }

  void Advance(long gameTickIndex);
}

/// <summary>
/// An owner that atomically covers more than one scheduled phase with one Advance call.
/// </summary>
/// <remarks>
/// <see cref="IServerTickPhaseOwner.Phase"/> is the ordering anchor and must be the first phase in
/// <see cref="Participants"/>. Participants declare coverage and shared transaction identity; they
/// are not additional execution callbacks. The composite's single <see cref="Advance"/> call is
/// the only commit boundary.
/// </remarks>
public interface IServerTickPhaseCompositeOwner : IServerTickPhaseOwner
{
  IReadOnlyList<IServerTickPhaseParticipant> Participants { get; }
}

/// <summary>Declares one phase covered by an atomic server tick transaction.</summary>
public interface IServerTickPhaseParticipant
{
  ServerTickPhase Phase { get; }

  /// <summary>
  /// The same stable reference as the composite owner and its sibling participants.
  /// </summary>
  object TransactionIdentity { get; }
}
