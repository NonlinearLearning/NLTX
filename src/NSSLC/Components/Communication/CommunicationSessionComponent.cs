using Terraria.Relationships;
using Terraria.Entity;

namespace Terraria.Communication;

/// <summary>Authoritative communication state attached to one ECS session entity.</summary>
public sealed class CommunicationSessionComponent
{
  public CommunicationSessionComponent(SessionEpoch epoch)
  {
    if (!epoch.IsValid)
    {
      throw new ArgumentException("A communication session requires a valid epoch.", nameof(epoch));
    }

    Epoch = epoch;
    Stage = CommunicationSessionStage.Connected;
  }

  public SessionEpoch Epoch { get; }

  public CommunicationSessionStage Stage { get; private set; }

  public PacketSequence LastInboundSequence { get; private set; } = new(0);

  public bool IsConnected => Stage is not CommunicationSessionStage.Closed;

  public EntityRuntimeHandle? Player { get; private set; }

  public bool TryAdvance(CommunicationSessionStage stage)
  {
    if (stage < Stage && stage is not CommunicationSessionStage.Closed)
    {
      return false;
    }

    Stage = stage;
    return true;
  }

  public bool AcceptInbound(PacketSequence sequence)
  {
    if (!IsConnected || !sequence.IsValid || sequence.Value <= LastInboundSequence.Value)
    {
      return false;
    }

    LastInboundSequence = sequence;
    return true;
  }

  public bool BindPlayer(EntityRuntimeHandle player)
  {
    if (!player.IsValid || !IsConnected || Player is not null)
    {
      return false;
    }

    Player = player;
    return true;
  }
}
