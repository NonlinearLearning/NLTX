using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Modules.Social.Chat;

/// <summary>Produces authenticated, same-team Party chat without writing to a transport.</summary>
public sealed class PartyChatCommand : IChatCommand
{
  private const string NoTeamLocalizationKey = "LegacyMultiplayer.10";
  private const int MaximumRecipientCount = byte.MaxValue;
  private static readonly (byte Red, byte Green, byte Blue) ServerMessageColor = (255, 240, 20);

  private static readonly (byte Red, byte Green, byte Blue)[] TeamColors =
  [
    (0, 0, 0),
    (218, 59, 59),
    (59, 218, 85),
    (59, 149, 218),
    (242, 221, 100),
    (224, 100, 242)
  ];

  private readonly Packet82ModuleIntentProducer _producer;
  private CommitVersion _commitVersion = CommitVersion.Initial;

  public PartyChatCommand(Packet82ModuleIntentProducer producer)
  {
    ArgumentNullException.ThrowIfNull(producer);
    _producer = producer;
  }

  public string CommandId => "Party";

  public ChatCommandResult ProcessIncomingMessage(
    string text,
    byte authenticatedPlayerSlot,
    ModuleDispatchContext context)
  {
    ArgumentNullException.ThrowIfNull(text);
    ArgumentNullException.ThrowIfNull(context);
    if (context.Stage != ModuleSessionStage.Active)
    {
      return ChatCommandResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "Party chat requires an active authenticated session.");
    }

    if (authenticatedPlayerSlot != context.PlayerSlot)
    {
      return ChatCommandResult.Rejected(
        ProtocolErrorCode.Forbidden,
        "Party chat author must be bound to the authenticated player slot.");
    }

    if (context.Team == 0)
    {
      CommitVersion errorVersion = _commitVersion.Next();
      OutboundPacketIntent errorIntent = _producer.CreateIntent(
        (ushort)Packet82ModuleId.Text,
        new Packet82TextData(
          byte.MaxValue,
          NetworkText.Key(NoTeamLocalizationKey),
          ServerMessageColor.Red,
          ServerMessageColor.Green,
          ServerMessageColor.Blue),
        new[] { context.Epoch },
        errorVersion);
      _commitVersion = errorVersion;
      return ChatCommandResult.Handled(errorVersion, new[] { errorIntent });
    }

    if (text.Length == 0)
    {
      return ChatCommandResult.HandledWithoutCommit();
    }

    if (!TryBuildTargets(context, out List<SessionEpoch> targets))
    {
      return ChatCommandResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "Party chat visibility facts exceed the Version4 player recipient bound.");
    }

    (byte red, byte green, byte blue) = TeamColors[context.Team];
    CommitVersion nextVersion = _commitVersion.Next();
    OutboundPacketIntent intent = _producer.CreateIntent(
      (ushort)Packet82ModuleId.Text,
      new Packet82TextData(
        authenticatedPlayerSlot,
        NetworkText.Literal(text),
        red,
        green,
        blue),
      targets,
      nextVersion);

    _commitVersion = nextVersion;
    return ChatCommandResult.Handled(nextVersion, new[] { intent });
  }

  private static bool TryBuildTargets(
    ModuleDispatchContext context,
    out List<SessionEpoch> targets)
  {
    targets = new(MaximumRecipientCount);
    HashSet<SessionEpoch> uniqueTargets = new() { context.Epoch };
    targets.Add(context.Epoch);
    foreach (ModuleRecipient recipient in context.ActiveRecipients)
    {
      if (recipient.Team != context.Team || !uniqueTargets.Add(recipient.Epoch))
      {
        continue;
      }

      if (targets.Count == MaximumRecipientCount)
      {
        return false;
      }

      targets.Add(recipient.Epoch);
    }

    return true;
  }
}
