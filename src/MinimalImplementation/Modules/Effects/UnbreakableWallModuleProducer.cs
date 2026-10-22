using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules.Protocol;

namespace MinimalImplementation.Server.Modules.Effects;

/// <summary>Preserves the rejected/pending Version4 policy for module 14.</summary>
public sealed class UnbreakableWallModuleProducer
{
  public UnbreakableWallModuleProducer(Packet82ModuleIntentProducer producer)
  {
    ArgumentNullException.ThrowIfNull(producer);
  }

  /// <summary>
  /// Module 14 has no selected Version4 scan notification. The compatibility-shaped API remains
  /// available for read-only composition, but it cannot create a local-only outbound intent.
  /// </summary>
  public OutboundPacketIntent? CreateCommittedChangeIntent(
    byte playerSlot,
    bool previousInside,
    bool inside,
    IReadOnlyCollection<SessionEpoch> recipients,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(recipients);
    if (previousInside == inside || recipients.Count == 0)
    {
      return null;
    }

    if (playerSlot >= byte.MaxValue || !commitVersion.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(playerSlot));
    }

    return null;
  }
}
