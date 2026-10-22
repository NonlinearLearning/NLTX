using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Player identity resolved by P02 before a packet reaches a combat owner.</summary>
public readonly record struct AuthenticatedCombatSender
{
  public AuthenticatedCombatSender(SessionEpoch epoch, byte playerSlot)
  {
    if (!epoch.IsValid)
    {
      throw new ArgumentException("The sender must have an active session epoch.", nameof(epoch));
    }

    if (playerSlot == byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(playerSlot));
    }

    Epoch = epoch;
    PlayerSlot = playerSlot;
  }

  public SessionEpoch Epoch { get; }

  public byte PlayerSlot { get; }
}
