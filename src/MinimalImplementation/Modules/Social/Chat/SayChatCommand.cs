using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Modules.Social.Chat;

/// <summary>Produces the Version4 default public chat command after P02 supplies the chat color.</summary>
public sealed class SayChatCommand : IChatCommand
{
  private const int MaximumRecipientCount = byte.MaxValue;

  private readonly Packet82ModuleIntentProducer _producer;
  private readonly Func<ModuleDispatchContext, ChatColor> _colorProvider;
  private CommitVersion _commitVersion = CommitVersion.Initial;

  public SayChatCommand(
    Packet82ModuleIntentProducer producer,
    Func<ModuleDispatchContext, ChatColor> colorProvider)
  {
    ArgumentNullException.ThrowIfNull(producer);
    ArgumentNullException.ThrowIfNull(colorProvider);
    _producer = producer;
    _colorProvider = colorProvider;
  }

  public string CommandId => "Say";

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
        "Public chat requires an active authenticated session.");
    }

    if (authenticatedPlayerSlot != context.PlayerSlot)
    {
      return ChatCommandResult.Rejected(
        ProtocolErrorCode.Forbidden,
        "Public chat author must be bound to the authenticated player slot.");
    }

    if (text.Length == 0)
    {
      return ChatCommandResult.HandledWithoutCommit();
    }

    if (!TryBuildTargets(context, out List<SessionEpoch> targets))
    {
      return ChatCommandResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "Public chat visibility facts exceed the Version4 player recipient bound.");
    }

    ChatColor color = _colorProvider(context);
    CommitVersion nextVersion = _commitVersion.Next();
    OutboundPacketIntent intent = _producer.CreateIntent(
      (ushort)Packet82ModuleId.Text,
      new Packet82TextData(
        authenticatedPlayerSlot,
        NetworkText.Literal(text),
        color.Red,
        color.Green,
        color.Blue),
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
      if (!uniqueTargets.Add(recipient.Epoch))
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
