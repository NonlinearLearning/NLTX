using Terraria.Entity;
using Terraria.Relationships;
using Terraria.Session;

namespace Terraria.Network;

/// <summary>Authoritative communication state attached to one ECS session entity.</summary>
public sealed class CommunicationSessionComponent
{
  private readonly HashSet<(int X, int Y)> _sentWorldSections = [];
  private readonly HashSet<(byte Slot, byte Generation)> _sentNpcIdentities = [];

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

  /// <summary>Returns whether this session has already been given a world section.</summary>
  public bool HasSentWorldSection(int sectionX, int sectionY)
  {
    return _sentWorldSections.Contains((sectionX, sectionY));
  }

  /// <summary>Records a section planned for delivery to this session.</summary>
  public void MarkWorldSectionSent(int sectionX, int sectionY)
  {
    _sentWorldSections.Add((sectionX, sectionY));
  }

  /// <summary>Returns whether the initial snapshot for a client NPC slot was sent.</summary>
  public bool HasSentNpcSlot(byte slot, byte generation)
  {
    return _sentNpcIdentities.Contains((slot, generation));
  }

  /// <summary>Records the initial snapshot for a client NPC slot.</summary>
  public void MarkNpcSlotSent(byte slot, byte generation)
  {
    _sentNpcIdentities.Add((slot, generation));
  }

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

  public bool UnbindPlayer(EntityRuntimeHandle player)
  {
    if (Player != player)
    {
      return false;
    }

    Player = null;
    return true;
  }
}
