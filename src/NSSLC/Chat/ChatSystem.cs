using System;
using System.Collections.Generic;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Session;
using Terraria.Entity;
using Terraria.Relationships;

namespace Terraria.Chat;

/// <summary>
/// Turns chat lines typed by entities into delivered chat facts, ported from Version4's chat
/// pipeline (<c>ChatMessage</c>, <c>ChatCommandProcessor</c> and <c>ChatHelper</c>).
/// </summary>
/// <remarks>
/// Admission is the capability component alone: an entity may speak exactly when it declares
/// <see cref="ChatSenderComponent"/>. A line without the command prefix is broadcast on
/// <see cref="ChatChannel.Say"/>; a line with the prefix is looked up in the command catalog, gated
/// by the operator bit, and answered privately when it is a request for information.
/// </remarks>
public sealed class ChatSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private const string HelpCommandName = "help";
  private const string PlayingCommandName = "playing";
  private const string RollCommandName = "roll";

  private readonly IEntityRuntimeResolver _resolver;
  private readonly ISessionPlayerSource _players;
  private readonly ChatCommandCatalog _commands;
  private readonly ChatStore _store;
  private ChatBroadcastQueueComponent? _broadcasts;

  public ChatSystem(
      ArchWorld ecsWorld,
      IEntityRuntimeResolver resolver,
      ISessionPlayerSource players,
      ChatCommandCatalog commands,
      ChatStore store)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld)))
  {
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    _players = players ?? throw new ArgumentNullException(nameof(players));
    _commands = commands ?? throw new ArgumentNullException(nameof(commands));
    _store = store ?? throw new ArgumentNullException(nameof(store));
  }

  // 在查询之外解析队列实体：Require 内部本身要跑一次查询。
  private ChatBroadcastQueueComponent Broadcasts
    => _broadcasts ??= ChatBroadcastQueue.Require(World);

  public override void Update(in EcsServerTickContext context)
  {
    IReadOnlyList<ChatMessageRequest> requests = ChatMessageRequestQueue.Require(World).Drain();
    if (requests.Count == 0)
    {
      return;
    }

    ChatBroadcastQueueComponent broadcasts = Broadcasts;
    for (int index = 0; index < requests.Count; index++)
    {
      Apply(requests[index], context.Tick, broadcasts);
    }
  }

  private void Apply(
      in ChatMessageRequest request,
      long tick,
      ChatBroadcastQueueComponent broadcasts)
  {
    if (!_resolver.TryResolve(request.Sender, out ArchEntity sender) ||
        sender == ArchEntity.Null || !World.IsAlive(sender) ||
        !World.Has<ChatSenderComponent>(sender))
    {
      return;
    }

    ChatSenderComponent speaker = World.Get<ChatSenderComponent>(sender);
    if (!speaker.IsValid || !ChatTextRules.TrySanitize(request.Text, out string text))
    {
      return;
    }

    // 队伍频道需要队伍状态，尚未建模：明确回绝，而不是把队伍私聊广播成公开行。
    if (request.Channel == ChatChannel.Party)
    {
      Reply(speaker, request.Sender, "Party chat is not available on this server.", tick, broadcasts);
      return;
    }

    if (ChatTextRules.TryParseCommand(text, out string name, out string arguments))
    {
      ApplyCommand(speaker, request.Sender, sender.Id, name, arguments, tick, broadcasts);
      return;
    }

    Publish(
        speaker,
        ChatChannel.Say,
        ChatColor.PlayerText,
        text,
        tick,
        broadcasts,
        onlyFor: null);
  }

  private void ApplyCommand(
      in ChatSenderComponent speaker,
      EntityRuntimeHandle senderHandle,
      int senderId,
      string name,
      string arguments,
      long tick,
      ChatBroadcastQueueComponent broadcasts)
  {
    if (!_commands.TryGet(name, out ChatCommandDefinition definition))
    {
      Reply(speaker, senderHandle, $"Unknown command: /{name}", tick, broadcasts);
      return;
    }

    if (definition.RequiresOperator && !speaker.IsOperator)
    {
      Reply(speaker, senderHandle, $"/{name} requires an operator", tick, broadcasts);
      return;
    }

    switch (name)
    {
      case ChatCommandRules.DefaultCommandName:
        if (arguments.Length == 0)
        {
          Reply(speaker, senderHandle, $"Usage: /{name} <message>", tick, broadcasts);
          return;
        }

        Publish(
            speaker,
            ChatChannel.Say,
            ChatColor.PlayerText,
            arguments,
            tick,
            broadcasts,
            onlyFor: null);
        return;
      case HelpCommandName:
        Reply(speaker, senderHandle, ChatCommandRules.FormatHelp(_commands), tick, broadcasts);
        return;
      case PlayingCommandName:
        int online = _players.Snapshot().Length;
        Reply(speaker, senderHandle, ChatCommandRules.FormatPlayerCount(online), tick, broadcasts);
        return;
      case RollCommandName:
        Publish(
            speaker,
            ChatChannel.Announce,
            ChatColor.World,
            $"rolled a {ChatCommandRules.Roll(tick, senderId)}",
            tick,
            broadcasts,
            onlyFor: null);
        return;      default:
        Reply(
            speaker,
            senderHandle,
            $"Command /{name} is registered but has no server handler.",
            tick,
            broadcasts);
        return;
    }
  }

  private void Reply(
      in ChatSenderComponent speaker,
      EntityRuntimeHandle senderHandle,
      string text,
      long tick,
      ChatBroadcastQueueComponent broadcasts)
    => Publish(
        speaker,
        ChatChannel.Private,
        ChatColor.ServerMessage,
        text,
        tick,
        broadcasts,
        onlyFor: senderHandle);

  private void Publish(
      in ChatSenderComponent speaker,
      ChatChannel channel,
      ChatColor color,
      string text,
      long tick,
      ChatBroadcastQueueComponent broadcasts,
      EntityRuntimeHandle? onlyFor)
  {
    broadcasts.Enqueue(new ChatBroadcast(
        channel,
        speaker.DisplayName,
        text,
        ChatNetTextFrame.ServerAuthorId,
        color,
        onlyFor,
        tick));
    // 私有回复不进入历史：历史是给后来者读的公开行。
    if (onlyFor is null)
    {
      _store.Record(new ChatMessage(
          channel,
          speaker.DisplayName,
          text,
          ChatNetTextFrame.ServerAuthorId,
          tick));
    }
  }
}
