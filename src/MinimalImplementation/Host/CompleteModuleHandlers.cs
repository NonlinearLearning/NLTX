using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Combat.Projectiles;
using MinimalImplementation.Combat.TagEffects;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Mechanisms;
using MinimalImplementation.Modules.Creative;
using MinimalImplementation.Server.Modules;
using MinimalImplementation.Server.Modules.Protocol;
using MinimalImplementation.Server.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Root-owned adapters for module ids whose domain owners expose a richer API than
/// <see cref="IModuleHandler"/>. These adapters perform only wire-to-owner conversion and
/// turn committed owner results back into bounded Packet 82 intents.
/// </summary>
public sealed class CreativePowerModuleHandler : IModuleHandler, IModuleEpochRetirable
{
  private readonly CreativeModuleHandler _owner;
  private readonly Packet82ModuleIntentProducer _producer;

  public CreativePowerModuleHandler(
    CreativeModuleHandler owner,
    Packet82ModuleIntentProducer producer)
  {
    _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    _producer = producer ?? throw new ArgumentNullException(nameof(producer));
  }

  public ushort ModuleId => (ushort)Packet82ModuleId.CreativePowers;

  public ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ModuleDispatchResult? validation = ModuleHandlerValidation.ValidateActiveEnvelope(
      envelope,
      context,
      ModuleId,
      "Creative power");
    if (validation is not null)
    {
      return validation;
    }

    if (decodedPayload is not Packet82CreativePowerData packet ||
        !TryConvert(packet, out CreativePowerWireMessage? message))
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Packet 82 module 5 does not contain a supported creative-power value.");
    }

    CreativeModuleHandlingResult result = _owner.HandleCreativePower(
      context.PlayerSlot,
      context.CountsAsHostForGameplay,
      message!);
    if (result.Commit.Status != OwnerCommitStatus.Applied ||
        result.Commit.Version is not CommitVersion version)
    {
      return Rejected(result.Commit, "The creative-power owner rejected the request.");
    }

    if (result.PowerEffect is not CreativePowerEffect effect) {
      return ModuleDispatchResult.Handled(version);
    }

    IReadOnlyList<CreativePowerWireMessage> broadcasts = effect.CaptureBroadcastMessages();
    if (broadcasts.Count == 0) {
      return ModuleDispatchResult.Handled(version);
    }

    SessionEpoch[] targets = context.ActiveRecipients
      .Select(static recipient => recipient.Epoch)
      .Append(context.Epoch)
      .Distinct()
      .ToArray();
    Guid batchId = Guid.NewGuid();
    var intents = new List<OutboundPacketIntent>(broadcasts.Count * targets.Length);
    foreach (CreativePowerWireMessage broadcast in broadcasts) {
      if (!TryCreatePacket(broadcast, out Packet82CreativePowerData? response)) {
        return ModuleDispatchResult.Handled(version);
      }

      foreach (SessionEpoch target in targets) {
        OutboundDeliveryToken deliveryToken = new(
            CreativePowerEffectDeliveryBatch.DeliveryOwnerKey,
            batchId,
            target,
            intents.Count);
        intents.Add(_producer.CreateIntent(
            ModuleId,
            response!,
            [target],
            version,
            deliveryToken));
      }
    }

    CreativePowerEffectDeliveryBatch batch = _owner.CreateEffectDeliveryBatch(effect, intents);
    IOutboundDeliverySettlement settlement = _owner.CreateEffectDeliverySettlement(batch);
    return ModuleDispatchResult.Handled(version, intents, settlement);
  }

  public void RetireEpoch(SessionEpoch epoch) {
    _owner.RetireEpoch(epoch);
  }

  private static bool TryCreatePacket(
      CreativePowerWireMessage message,
      out Packet82CreativePowerData? packet) {
    object? value = message.ValueKind switch {
      CreativePowerValueKind.SharedToggle =>
        new Packet82SharedTogglePowerState(message.ToggleValue),
      CreativePowerValueKind.Button => null,
      CreativePowerValueKind.PerPlayerToggleSync =>
        new Packet82PerPlayerTogglePowerState(message.CopyPlayerStates()),
      CreativePowerValueKind.PerPlayerToggle =>
        new Packet82PerPlayerToggleUpdatePowerState(
          message.PlayerId,
          message.ToggleValue),
      CreativePowerValueKind.SharedSlider =>
        new Packet82SharedSliderPowerState(message.SliderValue),
      CreativePowerValueKind.PerPlayerSlider =>
        new Packet82PerPlayerSliderPowerState(
          message.PlayerId,
          message.SliderValue),
      _ => null
    };

    packet = value is null && message.ValueKind is not CreativePowerValueKind.Button
      ? null
      : new Packet82CreativePowerData(message.PowerId, value);
    return packet is not null;
  }

  private static bool TryConvert(
    Packet82CreativePowerData packet,
    out CreativePowerWireMessage? message)
  {
    message = packet.Value switch
    {
      Packet82SharedTogglePowerState toggle =>
        CreativePowerWireMessage.SharedToggle(packet.PowerId, toggle.Enabled),
      Packet82PerPlayerTogglePowerState toggles =>
        CreativePowerWireMessage.PerPlayerToggleSync(packet.PowerId, toggles.EnabledPlayers),
      Packet82PerPlayerToggleUpdatePowerState update =>
        CreativePowerWireMessage.PerPlayerToggle(packet.PowerId, update.Player, update.Enabled),
      Packet82SharedSliderPowerState slider =>
        CreativePowerWireMessage.SharedSlider(packet.PowerId, slider.Value),
      Packet82PerPlayerSliderPowerState slider =>
        CreativePowerWireMessage.PerPlayerSlider(packet.PowerId, slider.Player, slider.Value),
      null when packet.PowerId is >= 1 and <= 4 =>
        CreativePowerWireMessage.Button(packet.PowerId),
      _ => null
    };
    return message is not null;
  }

  private static ModuleDispatchResult Rejected(OwnerCommitResult result, string fallback)
  {
    return ModuleDispatchResult.Rejected(
      result.ErrorCode ?? ProtocolErrorCode.Conflict,
      result.Detail ?? fallback);
  }
}

public sealed class CreativePermissionModuleHandler : IModuleHandler
{
  private readonly CreativeModuleHandler _owner;

  public CreativePermissionModuleHandler(CreativeModuleHandler owner)
  {
    _owner = owner ?? throw new ArgumentNullException(nameof(owner));
  }

  public ushort ModuleId => (ushort)Packet82ModuleId.CreativePowerPermissions;

  public ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ModuleDispatchResult? validation = ModuleHandlerValidation.ValidateActiveEnvelope(
      envelope,
      context,
      ModuleId,
      "Creative permission");
    if (validation is not null)
    {
      return validation;
    }

    if (decodedPayload is not Packet82PermissionData permission)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Packet 82 module 9 has an invalid permission payload.");
    }

    OwnerCommitResult result = _owner.RejectClientPermissionUpdate(permission);
    return ModuleDispatchResult.Rejected(
      result.ErrorCode ?? ProtocolErrorCode.Forbidden,
      result.Detail ?? "Creative permissions are server-authored.");
  }
}

/// <summary>
/// Performs the envelope checks shared by module adapters that are only valid for active
/// authenticated Packet 82 requests. The dispatcher performs the same checks for its normal
/// route, but keeping this guard at the handler boundary preserves the contract for direct owner
/// invocation and prevents a malformed envelope from reaching a domain owner.
/// </summary>
internal static class ModuleHandlerValidation
{
  public static ModuleDispatchResult? ValidateActiveEnvelope(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    ushort moduleId,
    string moduleName)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(context);
    ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);

    if (envelope.MessageId != 82 || envelope.ModuleId != moduleId)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.UnknownModule,
        $"The {moduleName} handler accepts only Packet 82 module {moduleId}.");
    }

    if (envelope.Epoch != context.Epoch || !context.Sender.IsValid)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.StaleEpoch,
        $"The {moduleName} packet does not match the captured authenticated session.");
    }

    if (context.Stage != ModuleSessionStage.Active ||
        !string.Equals(envelope.Stage, ModuleSessionStage.Active.ToString(), StringComparison.Ordinal))
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.WrongStage,
        $"Packet 82 module {moduleId} requires an active authenticated session.");
    }

    return null;
  }
}

public sealed class CreativeUnlockReportModuleHandler : IModuleHandler
{
  private readonly CreativeModuleHandler _owner;
  private readonly Packet82ModuleIntentProducer _producer;

  public CreativeUnlockReportModuleHandler(
    CreativeModuleHandler owner,
    Packet82ModuleIntentProducer producer)
  {
    _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    _producer = producer ?? throw new ArgumentNullException(nameof(producer));
  }

  public ushort ModuleId => 6;

  public ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ModuleDispatchResult? validation = ModuleHandlerValidation.ValidateActiveEnvelope(
      envelope,
      context,
      ModuleId,
      "Creative unlock");
    if (validation is not null)
    {
      return validation;
    }

    if (decodedPayload is not Version4ModulePayloads.CreativeSacrificeReportData report)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Packet 82 module 6 requires the five-byte Version4 sacrifice report.");
    }

    CreativeModuleHandlingResult result = _owner.HandleUnlockReport(
      context.PlayerSlot,
      new CreativeUnlockReportWireMessage(report.Player, report.ItemId, report.Amount));
    if (result.Commit.Status != OwnerCommitStatus.Applied ||
        result.UnlockReport is not CreativeUnlockReport committed)
    {
      return ModuleDispatchResult.Rejected(
        result.Commit.ErrorCode ?? ProtocolErrorCode.Conflict,
        result.Commit.Detail ?? "The sacrifice report owner rejected the request.");
    }

    SessionEpoch[] targets = context.ActiveRecipients
      .Select(static recipient => recipient.Epoch)
      .Where(epoch => epoch != context.Epoch)
      .Distinct()
      .ToArray();
    if (targets.Length == 0)
    {
      return ModuleDispatchResult.Handled(committed.Version);
    }

    var payload = new Version4ModulePayloads.CreativeSacrificeReportData(
      committed.AuthenticatedPlayerId,
      committed.ItemId,
      committed.Amount);
    return ModuleDispatchResult.Handled(
      committed.Version,
      [_producer.CreateIntent(ModuleId, payload, targets, committed.Version)]);
  }
}

public sealed class TeleportPylonModuleHandlerAdapter : IModuleHandler
{
  private readonly TeleportPylonModuleHandler _owner;
  private readonly Func<IReadOnlyCollection<SessionEpoch>> _activeSessions;

  public ushort ModuleId => 7;

  public TeleportPylonModuleHandlerAdapter(
    TeleportPylonModuleHandler owner,
    Func<IReadOnlyCollection<SessionEpoch>> activeSessions)
  {
    _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    _activeSessions = activeSessions ?? throw new ArgumentNullException(nameof(activeSessions));
  }

  public ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ModuleDispatchResult? validation = ModuleHandlerValidation.ValidateActiveEnvelope(
      envelope,
      context,
      ModuleId,
      "Teleport pylon");
    if (validation is not null)
    {
      return validation;
    }

    if (decodedPayload is not Packet82TeleportPylonData packet)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Packet 82 module 7 has an invalid teleport request payload.");
    }

    MechanismTeleportDispatchResult result = _owner.Handle(
      envelope,
      context.Sender,
      context.PlayerSlot,
      packet,
      _activeSessions());
    if (result.Status == MechanismTeleportDispatchStatus.Applied &&
        result.CommitVersion is CommitVersion version)
    {
      return ModuleDispatchResult.Handled(version);
    }

    return ModuleDispatchResult.Rejected(
      MapTeleportError(result.Status),
      result.Detail ?? "The pylon teleport request was rejected.");
  }

  private static ProtocolErrorCode MapTeleportError(MechanismTeleportDispatchStatus status)
  {
    return status switch
    {
      MechanismTeleportDispatchStatus.SessionUnavailable => ProtocolErrorCode.StaleEpoch,
      MechanismTeleportDispatchStatus.InvalidEnvelope => ProtocolErrorCode.WrongStage,
      MechanismTeleportDispatchStatus.InvalidPayload => ProtocolErrorCode.MalformedFrame,
      MechanismTeleportDispatchStatus.ServerAuthoredAction => ProtocolErrorCode.Forbidden,
      MechanismTeleportDispatchStatus.NotReady => ProtocolErrorCode.NotReady,
      MechanismTeleportDispatchStatus.Conflict => ProtocolErrorCode.Conflict,
      _ => ProtocolErrorCode.Forbidden,
    };
  }
}

public sealed class TagEffectModuleHandlerAdapter : IModuleHandler
{
  private readonly TagEffectStateOwner _owner;
  private readonly Packet82ModuleIntentProducer _producer;

  public ushort ModuleId => 12;

  public TagEffectModuleHandlerAdapter(
    TagEffectStateOwner owner,
    Packet82ModuleIntentProducer producer)
  {
    _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    _producer = producer ?? throw new ArgumentNullException(nameof(producer));
  }

  public ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ModuleDispatchResult? validation = ModuleHandlerValidation.ValidateActiveEnvelope(
      envelope,
      context,
      ModuleId,
      "TagEffect");
    if (validation is not null)
    {
      return validation;
    }

    if (decodedPayload is not Version4ModulePayloads.TagEffectMessage message)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Packet 82 module 12 has an invalid TagEffect payload.");
    }

    TagEffectCommand command = new(
      context.PlayerSlot,
      message.Action,
      message.EffectType,
      message.NpcTimes,
      message.ProcTimes,
      message.NpcIndex);
    TagEffectDispatchResult result = _owner.Handle(
      envelope,
      new AuthenticatedCombatSender(context.Epoch, context.PlayerSlot),
      command);
    if (result.Result.Status == OwnerCommitStatus.Applied &&
        result.Result.Version is CommitVersion version)
    {
      if (result.BroadcastMessage is not TagEffectCommand broadcast)
      {
        return ModuleDispatchResult.Handled(version);
      }

      Version4ModulePayloads.TagEffectMessage payload = new(
        context.PlayerSlot,
        broadcast.Action,
        broadcast.EffectType,
        broadcast.NpcTimes,
        broadcast.ProcTimes,
        broadcast.NpcIndex);
      SessionEpoch[] targets = context.ActiveRecipients
        .Select(static recipient => recipient.Epoch)
        .Append(context.Epoch)
        .Distinct()
        .ToArray();
      return targets.Length == 0
        ? ModuleDispatchResult.Handled(version)
        : ModuleDispatchResult.Handled(
          version,
          [_producer.CreateIntent(ModuleId, payload, targets, version)]);
    }

    return ModuleDispatchResult.Rejected(
      result.Result.ErrorCode ?? ProtocolErrorCode.Conflict,
      result.Result.Detail ?? "The TagEffect owner rejected the request.");
  }
}

public sealed class LeashedEntityModuleHandlerAdapter : IModuleHandler
{
  private readonly LeashedEntityModuleHandler _owner;
  private readonly LeashedEntityModuleProducer _producer;
  private readonly long _worldScope;
  private readonly Func<Version4ModulePayloads.LeashedEntityMessage, MechanismSectionCoordinate>
    _sectionResolver;
  private readonly Func<CommitVersion> _allocateCommitVersion;
  private readonly Func<IReadOnlyCollection<SessionEpoch>> _activeSessions;

  public ushort ModuleId => 13;

  public LeashedEntityModuleHandlerAdapter(
    LeashedEntityModuleHandler owner,
    LeashedEntityModuleProducer producer,
    long worldScope,
    Func<Version4ModulePayloads.LeashedEntityMessage, MechanismSectionCoordinate> sectionResolver,
    Func<CommitVersion> allocateCommitVersion,
    Func<IReadOnlyCollection<SessionEpoch>> activeSessions)
  {
    _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    _producer = producer ?? throw new ArgumentNullException(nameof(producer));
    _sectionResolver = sectionResolver ?? throw new ArgumentNullException(nameof(sectionResolver));
    _allocateCommitVersion = allocateCommitVersion ?? throw new ArgumentNullException(nameof(allocateCommitVersion));
    _activeSessions = activeSessions ?? throw new ArgumentNullException(nameof(activeSessions));
    if (worldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }

    _worldScope = worldScope;
  }

  public ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ModuleDispatchResult? validation = ModuleHandlerValidation.ValidateActiveEnvelope(
      envelope,
      context,
      ModuleId,
      "Leashed entity");
    if (validation is not null)
    {
      return validation;
    }

    if (decodedPayload is not Version4ModulePayloads.LeashedEntityMessage message)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Packet 82 module 13 has an invalid leashed-entity payload.");
    }

    CommitVersion commitVersion;
    try
    {
      commitVersion = _allocateCommitVersion();
    }
    catch (InvalidOperationException exception)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.NotReady,
        $"The leashed-entity commit version is unavailable: {exception.Message}");
    }

    IReadOnlyCollection<SessionEpoch> activeSessions = _activeSessions();
    if (activeSessions is null)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.NotReady,
        "The active session epoch snapshot is unavailable.");
    }

    MechanismSectionCoordinate section;
    try
    {
      section = _sectionResolver(message);
    }
    catch (ArgumentException exception)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        $"The leashed-entity section could not be resolved: {exception.Message}");
    }

    LeashedEntityModuleDispatchResult result = _owner.Handle(
      envelope,
      context.PlayerSlot,
      message,
      section,
      activeSessions,
      commitVersion);
    if (result.Status is not (LeashedEntityModuleDispatchStatus.Applied or
        LeashedEntityModuleDispatchStatus.NoOp))
    {
      return ModuleDispatchResult.Rejected(
        MapLeashedError(result.Status),
        result.Detail ?? "The leashed-entity owner rejected the request.");
    }

    if (result.Status == LeashedEntityModuleDispatchStatus.Applied &&
        result.Change is LeashedEntityChangeResult change)
    {
      OutboundPacketIntent? intent = _producer.CreateChangeIntent(
        change,
        _worldScope,
        result.Recipients,
        commitVersion);
      return intent is null
        ? ModuleDispatchResult.Handled(commitVersion)
        : ModuleDispatchResult.Handled(commitVersion, [intent]);
    }

    return ModuleDispatchResult.Handled(commitVersion);
  }

  private static ProtocolErrorCode MapLeashedError(LeashedEntityModuleDispatchStatus status)
  {
    return status switch
    {
      LeashedEntityModuleDispatchStatus.SessionUnavailable => ProtocolErrorCode.StaleEpoch,
      LeashedEntityModuleDispatchStatus.InvalidEnvelope => ProtocolErrorCode.StaleEntity,
      LeashedEntityModuleDispatchStatus.InvalidPayload => ProtocolErrorCode.MalformedFrame,
      LeashedEntityModuleDispatchStatus.StaleRevision => ProtocolErrorCode.Conflict,
      LeashedEntityModuleDispatchStatus.Conflict => ProtocolErrorCode.Conflict,
      _ => ProtocolErrorCode.Forbidden,
    };
  }
}
