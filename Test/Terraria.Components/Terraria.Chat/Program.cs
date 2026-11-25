using System;
using System.Collections.Generic;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Unshared.Chat;
using Terraria.Share.Combat;
using Terraria.Unshared.Session;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;

// 聊天系统验证：
//   1. 文本规则：归一、单行化、控制字符剔除、长度上限、命令拆分。
//   2. 文本模块帧：出站布局按参考 SerializeServerMessage 逐字节对齐，入站文本段可解。
//   3. 能力组件：只有声明 ChatSenderComponent 的实体能说话。
//   4. 广播事实：公开行进入历史，命令回复只发给说话者，操作员命令受权限位约束。
int failures = 0;

void Check(string name, bool ok, string detail)
{
  Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name} | {detail}");
  if (!ok)
  {
    failures++;
  }
}

// --- 1. 文本规则 ---
Check(
    "sanitize-collapses-whitespace",
    ChatTextRules.TrySanitize("  hello\r\nworld\t!", out string sanitized) && sanitized == "hello world !",
    $"value={Describe(sanitized)}");
Check(
    "sanitize-drops-control-characters",
    ChatTextRules.TrySanitize("a\u0001b", out string stripped) && stripped == "ab",
    $"value={Describe(stripped)}");
Check(
    "sanitize-rejects-empty",
    !ChatTextRules.TrySanitize("   ", out _) && !ChatTextRules.TrySanitize(null, out _),
    "空白与 null 都拒绝");
Check(
    "sanitize-rejects-overlong",
    !ChatTextRules.TrySanitize(new string('x', ChatTextRules.MaximumLength + 1), out _) &&
        ChatTextRules.TrySanitize(new string('x', ChatTextRules.MaximumLength), out _),
    $"上限 {ChatTextRules.MaximumLength} 字符");

Check(
    "command-parse-name-only",
    ChatTextRules.TryParseCommand("/help", out string helpName, out string helpArguments) &&
        helpName == "help" && helpArguments.Length == 0,
    $"{helpName}|{Describe(helpArguments)}");
Check(
    "command-parse-with-arguments",
    ChatTextRules.TryParseCommand("/say hi there", out string sayName, out string sayArguments) &&
        sayName == "say" && sayArguments == "hi there",
    $"{sayName}|{Describe(sayArguments)}");
Check(
    "command-parse-lowercases",
    ChatTextRules.TryParseCommand("/Roll 5", out string rollName, out string rollArguments) &&
        rollName == "roll" && rollArguments == "5",
    $"{rollName}|{Describe(rollArguments)}");
Check(
    "command-parse-ignores-plain-text",
    !ChatTextRules.TryParseCommand("hello", out _, out _) &&
        !ChatTextRules.TryParseCommand("/", out _, out _),
    "无前缀与裸斜杠都不算命令");
Check(
    "command-parse-rejects-overlong-name",
    !ChatTextRules.TryParseCommand(
        "/" + new string('a', ChatTextRules.MaximumCommandNameLength + 1),
        out _,
        out _),
    $"上限 {ChatTextRules.MaximumCommandNameLength} 字符");

// --- 2. 文本模块帧 ---
byte[] frame = ChatNetTextFrame.EncodeServerMessage(
    ChatNetTextFrame.ServerAuthorId,
    "hi",
    ChatColor.ServerMessage);
byte[] expected =
[
  (byte)ChatNetTextFrame.TextModuleId, 0,
  ChatNetTextFrame.ServerAuthorId,
  0,
  2,
  (byte)'h', (byte)'i',
  ChatColor.ServerMessage.R, ChatColor.ServerMessage.G, ChatColor.ServerMessage.B
];
Check(
    "frame-encode-matches-reference-layout",
    BytesEqual(frame, expected),
    $"bytes={Convert.ToHexString(frame)}");

using MemoryStream clientFrame = new();
using (BinaryWriter writer = new(clientFrame, System.Text.Encoding.UTF8, leaveOpen: true))
{
  writer.Write(ChatNetTextFrame.TextModuleId);
  writer.Write("Say");
  writer.Write("hello world");
}

Check(
    "frame-decode-client-command-and-text",
    ChatNetTextFrame.TryDecodeClientMessage(
        clientFrame.ToArray(),
        out string decodedCommand,
        out string decoded) &&
        decodedCommand == "Say" && decoded == "hello world",
    $"command={Describe(decodedCommand)} text={Describe(decoded)}");
Check(
    "channel-rules-map-protocol-command-names",
    ChatChannelRules.TryResolve("Say", out ChatChannel sayChannel) && sayChannel == ChatChannel.Say &&
        ChatChannelRules.TryResolve("party", out ChatChannel partyChannel) && partyChannel == ChatChannel.Party &&
        !ChatChannelRules.TryResolve("Build", out _),
    "Say/Party 可解析，其他命令名拒绝");
Check(
    "frame-decode-rejects-foreign-module",
    !ChatNetTextFrame.TryDecodeClientMessage([2, 0, 1, (byte)'x'], out _, out _),
    "模块 id 不是文本模块");
Check(
    "frame-decode-rejects-truncated",
    !ChatNetTextFrame.TryDecodeClientMessage([1, 0, 10, (byte)'a'], out _, out _),
    "长度超过载荷");

// --- 3./4. 系统行为 ---
using ArchWorld world = ArchWorld.Create();
_ = world.Create(new ChatMessageRequestQueueComponent());
_ = world.Create(new ChatBroadcastQueueComponent());
EntityRuntimeRegistry registry = new(worldScope: 1);

ArchEntity speaker = CreateSpeaker(world, registry, ChatSenderComponent.Player("Alice"), out EntityRuntimeHandle speakerHandle);
ArchEntity operatorSpeaker = CreateSpeaker(world, registry, ChatSenderComponent.Server("Admin"), out EntityRuntimeHandle operatorHandle);
ArchEntity silent = world.Create();
world.Add(silent, new EntityIdentityComponent(EntityUuid.Create()));
EntityRuntimeHandle silentHandle = registry.Allocate(EntityUuid.Create());
registry.Register(silentHandle, silent);

ChatCommandCatalog commands = new();
commands.Register(new ChatCommand(ChatCommandRules.DefaultCommandName, "say", false));
commands.Register(new ChatCommand("help", "help", false));
commands.Register(new ChatCommand("playing", "players", false));
commands.Register(new ChatCommand("roll", "roll", false));
commands.Register(new ChatCommand("admin", "operator only", true));
commands.Seal();

ChatStore store = new();
ChatSystem system = new(world, registry, new EmptySessionPlayers(), commands, store);
ChatMessageRequestQueueComponent requests = ChatMessageRequestQueue.Require(world);
ChatBroadcastQueueComponent broadcasts = ChatBroadcastQueue.Require(world);

IReadOnlyList<ChatBroadcast> Run(long tick, EntityRuntimeHandle sender, string text)
{
  requests.Enqueue(new ChatMessageRequest(sender, ChatChannel.Say, text));
  system.Update(new EcsServerTickContext(world, tick, 1f / 60f));
  return broadcasts.Drain();
}

IReadOnlyList<ChatBroadcast> say = Run(10, speakerHandle, "hello there");
Check(
    "say-broadcasts-public-line",
    say.Count == 1 && say[0].Channel == ChatChannel.Say && say[0].OnlyFor is null &&
        say[0].AuthorName == "Alice" && say[0].Text == "hello there" &&
        say[0].DisplayText == "Alice: hello there",
    $"count={say.Count} text={Describe(say.Count > 0 ? say[0].DisplayText : "-")}");
Check(
    "say-records-history",
    store.Count == 1 && store.Snapshot()[0].Text == "hello there",
    $"count={store.Count}");

IReadOnlyList<ChatBroadcast> withoutCapability = Run(11, silentHandle, "am I allowed");
Check(
    "capability-component-gates-speech",
    withoutCapability.Count == 0 && store.Count == 1,
    $"count={withoutCapability.Count}");

IReadOnlyList<ChatBroadcast> help = Run(12, speakerHandle, "/help");
Check(
    "help-replies-privately",
    help.Count == 1 && help[0].Channel == ChatChannel.Private &&
        help[0].OnlyFor == speakerHandle &&
        help[0].Text.StartsWith("Commands: ", StringComparison.Ordinal),
    $"text={Describe(help.Count > 0 ? help[0].Text : "-")}");
Check(
    "private-reply-not-recorded",
    store.Count == 1,
    $"count={store.Count}");

IReadOnlyList<ChatBroadcast> unknown = Run(13, speakerHandle, "/nope");
Check(
    "unknown-command-replies-privately",
    unknown.Count == 1 && unknown[0].OnlyFor == speakerHandle &&
        unknown[0].Text == "Unknown command: /nope",
    $"text={Describe(unknown.Count > 0 ? unknown[0].Text : "-")}");

IReadOnlyList<ChatBroadcast> denied = Run(14, speakerHandle, "/admin");
Check(
    "operator-command-rejects-non-operator",
    denied.Count == 1 && denied[0].OnlyFor == speakerHandle &&
        denied[0].Text == "/admin requires an operator",
    $"text={Describe(denied.Count > 0 ? denied[0].Text : "-")}");

IReadOnlyList<ChatBroadcast> allowed = Run(15, operatorHandle, "/admin");
Check(
    "operator-command-accepted-for-operator",
    allowed.Count == 1 && allowed[0].OnlyFor == operatorHandle &&
        allowed[0].Text.Contains("no server handler", StringComparison.Ordinal),
    $"text={Describe(allowed.Count > 0 ? allowed[0].Text : "-")}");

IReadOnlyList<ChatBroadcast> roll = Run(16, speakerHandle, "/roll");
Check(
    "roll-announces-deterministic-number",
    roll.Count == 1 && roll[0].Channel == ChatChannel.Announce &&
        roll[0].Text == $"rolled a {ChatCommandRules.Roll(16, speaker.Id)}",
    $"text={Describe(roll.Count > 0 ? roll[0].Text : "-")}");

IReadOnlyList<ChatBroadcast> playing = Run(17, speakerHandle, "/playing");
Check(
    "playing-counts-bound-sessions",
    playing.Count == 1 && playing[0].Text == "Players online: 0",
    $"text={Describe(playing.Count > 0 ? playing[0].Text : "-")}");

IReadOnlyList<ChatBroadcast> emptySay = Run(18, speakerHandle, "/say");
Check(
    "say-without-arguments-replies-usage",
    emptySay.Count == 1 && emptySay[0].OnlyFor == speakerHandle &&
        emptySay[0].Text == "Usage: /say <message>",
    $"text={Describe(emptySay.Count > 0 ? emptySay[0].Text : "-")}");

// 队伍频道未建模：请求必须被私有回绝，不能悄悄广播成公开行。
requests.Enqueue(new ChatMessageRequest(speakerHandle, ChatChannel.Party, "team up"));
system.Update(new EcsServerTickContext(world, 19, 1f / 60f));
IReadOnlyList<ChatBroadcast> party = broadcasts.Drain();
Check(
    "party-channel-refused-privately",
    party.Count == 1 && party[0].Channel == ChatChannel.Private &&
        party[0].OnlyFor == speakerHandle &&
        party[0].Text == "Party chat is not available on this server.",
    $"count={party.Count} text={Describe(party.Count > 0 ? party[0].Text : "-")}");

// 历史窗口：写入超过容量后只保留最新的 Capacity 条。
ChatStore windowed = new();
for (int index = 0; index < ChatStore.Capacity + 50; index++)
{
  windowed.Record(new ChatMessage(ChatChannel.Say, "Alice", $"line-{index}", 255, index));
}

ChatMessage[] snapshot = windowed.Snapshot();
Check(
    "history-window-is-bounded",
    snapshot.Length == ChatStore.Capacity && snapshot[^1].Text == $"line-{ChatStore.Capacity + 49}",
    $"count={snapshot.Length} last={Describe(snapshot[^1].Text)}");

Console.WriteLine();
if (failures == 0)
{
  Console.WriteLine("PASS: chat text rules, text module framing and capability-gated delivery hold.");
  return 0;
}

Console.WriteLine($"FAIL: {failures} check(s) failed.");
return 1;

static ArchEntity CreateSpeaker(
    ArchWorld world,
    EntityRuntimeRegistry registry,
    ChatSenderComponent chat,
    out EntityRuntimeHandle handle)
{
  EntityUuid id = EntityUuid.Create();
  ArchEntity entity = world.Create();
  world.Add(entity, new EntityIdentityComponent(id));
  world.Add(entity, chat);
  handle = registry.Allocate(id);
  registry.Register(handle, entity);
  return entity;
}

static string Describe(string value) => value.Replace("\n", "\\n", StringComparison.Ordinal);

static bool BytesEqual(byte[] left, byte[] right)
{
  if (left.Length != right.Length)
  {
    return false;
  }

  for (int index = 0; index < left.Length; index++)
  {
    if (left[index] != right[index])
    {
      return false;
    }
  }

  return true;
}

/// <summary>聊天验证只测命令与广播，在线人数固定为空名录。</summary>
internal sealed class EmptySessionPlayers : ISessionPlayerSource
{
  public void Collect(List<SessionPlayer> destination)
  {
    destination.Clear();
  }

  public SessionPlayer[] Snapshot()
  {
    return [];
  }
}