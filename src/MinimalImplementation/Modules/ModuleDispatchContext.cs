using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules;

/// <summary>
/// Immutable P02 facts supplied to a module handler for one authenticated request.
/// ActiveRecipients must already be filtered to sessions visible to the sender.
/// </summary>
public sealed class ModuleDispatchContext
{
  private readonly ModuleRecipient[] _recipients;

  public ModuleDispatchContext(
    SessionEpoch epoch,
    EntityUuid sender,
    byte playerSlot,
    byte team,
    ModuleSessionStage stage,
    bool countsAsHostForGameplay,
    IReadOnlyCollection<ModuleRecipient> activeRecipients)
  {
    if (!epoch.IsValid)
    {
      throw new ArgumentException("Module dispatch requires a valid session epoch.", nameof(epoch));
    }

    if (!sender.IsValid)
    {
      throw new ArgumentException("Module dispatch requires an authenticated entity identity.", nameof(sender));
    }

    ArgumentNullException.ThrowIfNull(activeRecipients);
    if (playerSlot >= byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(playerSlot));
    }

    if (team > 5)
    {
      throw new ArgumentOutOfRangeException(nameof(team));
    }

    if (!Enum.IsDefined(stage))
    {
      throw new ArgumentOutOfRangeException(nameof(stage));
    }

    foreach (ModuleRecipient recipient in activeRecipients)
    {
      if (!recipient.Epoch.IsValid || recipient.PlayerSlot >= byte.MaxValue || recipient.Team > 5)
      {
        throw new ArgumentException(
          "Module recipients must carry valid epochs, Version4 player slots, and team values.",
          nameof(activeRecipients));
      }
    }

    Epoch = epoch;
    Sender = sender;
    PlayerSlot = playerSlot;
    Team = team;
    Stage = stage;
    CountsAsHostForGameplay = countsAsHostForGameplay;
    _recipients = activeRecipients.ToArray();
    ActiveRecipients = Array.AsReadOnly(_recipients);
  }

  public SessionEpoch Epoch { get; }

  public EntityUuid Sender { get; }

  public byte PlayerSlot { get; }

  public byte Team { get; }

  public ModuleSessionStage Stage { get; }

  public bool CountsAsHostForGameplay { get; }

  public IReadOnlyList<ModuleRecipient> ActiveRecipients { get; }
}
