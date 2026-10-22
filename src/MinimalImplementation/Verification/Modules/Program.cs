using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules;
using MinimalImplementation.Server.Modules.Protocol;
using MinimalImplementation.Server.Modules.Social.Chat;
using MinimalImplementation.Server.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

const string scenarioId = "SC081";
const string partyText = "Hello teammates";
if (!TryGetOutputRoot(args, out string outputRoot))
{
  Console.Error.WriteLine(
    "Usage: --scenario SC081 --output-root <Build/diagnostics directory>");
  return 2;
}

Directory.CreateDirectory(outputRoot);

const byte senderSlot = 7;
const byte senderTeam = 1;
SessionEpoch senderEpoch = SessionEpoch.Create(1001);
SessionEpoch teammateEpoch = SessionEpoch.Create(1002);
SessionEpoch otherTeamEpoch = SessionEpoch.Create(1003);

IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> serverCodecs =
  Version4ModuleCodecs.Create(isServer: true);
byte[] inboundFixture = CreatePartyChatFixture(partyText);
PacketWireReader inboundReader = new(inboundFixture);
(ushort moduleId, object? decodedPayload) = NetModulesPacket.Read(
  inboundReader,
  serverCodecs,
  tagEffectNpcSlotCount: 0,
  tagEffectUsesProcTimes: static _ => false);
Require(moduleId == 1, "The fixture must identify Packet 82 Text module 1.");
Require(
  decodedPayload is Version4ModulePayloads.TextCommandData { CommandName: "Party", Text: partyText },
  "The client-to-server fixture must decode as Party command id plus text.");
Require(
  ModuleRegistrationProposal.IsVersion4Complete(),
  "The P07 registry proposal must preserve the complete Version4 module order.");

IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> outboundCodecs =
  Version4ModuleCodecs.Create(isServer: true);
Packet82ModuleIntentProducer producer = new(
  outboundCodecs,
  tagEffectNpcSlotCount: 0,
  tagEffectUsesProcTimes: static _ => false);
ChatCommandProcessor commandProcessor = new(new IChatCommand[]
{
  new PartyChatCommand(producer)
});
Require(commandProcessor.IsRegistered("Party"), "Version4 chat setup must register Party.");

TextModuleHandler textHandler = new(commandProcessor);
ModuleDispatcher dispatcher = new(new IModuleHandler[] { textHandler });
ModuleDispatchContext context = new(
  senderEpoch,
  EntityUuid.From(Guid.Parse("3a6266a6-3c29-4ff9-94d4-225e4de9edc2")),
  senderSlot,
  senderTeam,
  ModuleSessionStage.Active,
  countsAsHostForGameplay: false,
  new[]
  {
    new ModuleRecipient(senderEpoch, senderSlot, senderTeam),
    new ModuleRecipient(teammateEpoch, 8, senderTeam),
    new ModuleRecipient(otherTeamEpoch, 9, 2)
  });
InboundPacketEnvelope envelope = new(
  senderEpoch,
  new PacketSequence(0),
  DateTimeOffset.UnixEpoch,
  messageId: 82,
  moduleId,
  inboundFixture,
  stage: nameof(ModuleSessionStage.Active),
  worldScope: null,
  entityGeneration: null,
  expectedRevision: null);

ModuleDispatchResult result = dispatcher.Dispatch(envelope, context, decodedPayload);
Require(result.Disposition == ModuleDispatchDisposition.Handled, "The dispatcher must handle SC081.");
Require(result.CommitVersion is CommitVersion version && version.IsValid, "Chat must return its commit version.");
Require(result.OutboundIntents.Count == 1, "Chat must create one outbound text intent.");

OutboundPacketIntent outbound = result.OutboundIntents[0];
Require(outbound.MessageId == 82, "The chat intent must remain Packet 82.");
Require(outbound.TargetEpochs.Contains(senderEpoch), "The sender must receive their Party chat.");
Require(outbound.TargetEpochs.Contains(teammateEpoch), "The visible same-team player must receive chat.");
Require(!outbound.TargetEpochs.Contains(otherTeamEpoch), "Other teams must not receive Party chat.");

PacketWireReader outboundReader = new(outbound.Payload);
(ushort outboundModuleId, object? outboundPayload) = NetModulesPacket.Read(
  outboundReader,
  Version4ModuleCodecs.Create(isServer: false),
  tagEffectNpcSlotCount: 0,
  tagEffectUsesProcTimes: static _ => false);
Require(outboundModuleId == 1, "The server response must use Packet 82 Text module 1.");
Require(outboundPayload is Packet82TextData, "The server response must decode as S2C Text data.");
Packet82TextData text = (Packet82TextData)outboundPayload!;
Require(text.Author == senderSlot, "The author must come from the authenticated player slot.");
Require(text.Text.Text == partyText, "The S2C NetworkText literal must preserve the chat text.");
Require(
  text.Red == 218 && text.Green == 59 && text.Blue == 59,
  "Team 1 chat must use the Version4 team color.");

Console.WriteLine($"{scenarioId} PASS: authenticated Party chat produced one same-team S2C intent.");
Console.WriteLine("Result records intent creation; it does not claim client delivery or processing.");
string evidencePath = Path.Combine(outputRoot, "SC081-result.md");
File.WriteAllText(evidencePath, CreateEvidenceReport(result, context, partyText));
Console.WriteLine($"Evidence: {evidencePath}");
return 0;

static byte[] CreatePartyChatFixture(string text)
{
  using MemoryStream stream = new();
  PacketWireWriter writer = new(stream, ushort.MaxValue);
  writer.WriteUInt16(1);
  writer.WriteString("Party");
  writer.WriteString(text);
  return stream.ToArray();
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static bool TryGetOutputRoot(string[] arguments, out string outputRoot)
{
  outputRoot = string.Empty;
  if (arguments.Length != 4
      || arguments[0] != "--scenario"
      || arguments[1] != "SC081"
      || arguments[2] != "--output-root"
      || string.IsNullOrWhiteSpace(arguments[3]))
  {
    return false;
  }

  outputRoot = Path.GetFullPath(arguments[3]);
  return true;
}

static string CreateEvidenceReport(
  ModuleDispatchResult result,
  ModuleDispatchContext context,
  string text)
{
  string targetSlots = string.Join(
    ", ",
    result.OutboundIntents[0].TargetEpochs.Select(epoch => epoch.Value));
  return $"""
    # SC081 result

    Status: PASS

    Input: authenticated sender slot {context.PlayerSlot}, team {context.Team}, Active stage;
    Packet 82 module 1 C2S compatibility fixture contains command-id string `Party` followed by
    text string `{text}`.

    Actual: dispatcher disposition `{result.Disposition}`; commit version
    `{result.CommitVersion?.Value}`; outbound Packet 82 module 1 Text intent has author slot
    `{context.PlayerSlot}` and target epochs `{targetSlots}`. The verifier decoded the response
    with the S2C Version4 module codec and confirmed literal NetworkText, team 1 RGB 218/59/59,
    sender inclusion, same-team visible-recipient inclusion, and other-team exclusion.

    Compatibility provenance: the complete historical snapshot
    `D:/TRbackup/无任何删减通过编译/Terraria.Chat/ChatMessage.cs` serializes command id then text as
    two .NET strings, and its `NetTextModule.DeserializeAsServer` routes the message to the command
    processor. Its `PartyChatCommand` filters by the sender's server-owned team. Root selected that
    behavior to fill the current Version4 receive gap.

    Current Version4 evidence: `D:/TRbackup/Version4/Terraria/MessageBuffer.cs` routes outer packet 82
    to `NetManager.Read`; the current `NetTextModule.Deserialize`, `ChatCommandProcessor`
    `ProcessIncomingMessage`, and `PartyChatCommand` receive method bodies are empty. The fixture
    shape and Party route are therefore a selected historical compatibility behavior, not behavior
    demonstrated by the current Version4 implementation.

    This result proves intent creation only; it does not prove current Version4 receive behavior,
    socket delivery, or client handling.
    P07 registry proposal: the 15 module ids remain in Version4 NetworkInitializer order; the
    proposal is data-only and does not replace total-control runtime registration.
    SC082-SC090 were not run.
    """;
}
