using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Protocol;
using MinimalImplementation.Server.Modules;
using MinimalImplementation.Server.Modules.Protocol;
using MinimalImplementation.Server.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Inventory.Crafting;

public sealed class CraftingModule11Handler : IModuleHandler
{
  private readonly CraftingTransactionCoordinator _coordinator;
  private readonly Packet82ModuleIntentProducer _producer;
  private readonly object _syncRoot = new();
  private CommitVersion _responseVersion = CommitVersion.Initial;

  public CraftingModule11Handler(
    CraftingTransactionCoordinator coordinator,
    Packet82ModuleIntentProducer producer)
  {
    _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
    _producer = producer ?? throw new ArgumentNullException(nameof(producer));
  }

  public ushort ModuleId => (ushort)Packet82ModuleId.CraftingRequests;

  public ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(context);
    lock (_syncRoot)
    {
      if (envelope.ModuleId != ModuleId)
      {
        return ModuleDispatchResult.Rejected(
          ProtocolErrorCode.Conflict,
          "The crafting handler received a different Packet 82 module id.");
      }

      if (context.Stage != ModuleSessionStage.Active)
      {
        return ModuleDispatchResult.Rejected(
          ProtocolErrorCode.WrongStage,
          "Packet 82 CraftingRequests requires an active authenticated session.");
      }

      if (!TryCreateRequest(decodedPayload, out CraftingModule11Request? request) ||
          request is null)
      {
        return ModuleDispatchResult.Rejected(
          ProtocolErrorCode.MalformedFrame,
          "Packet 82 CraftingRequests must contain a material and chest request.");
      }

      if (envelope.WorldScope is not long worldScope || worldScope <= 0)
      {
        return ModuleDispatchResult.Rejected(
          ProtocolErrorCode.NotReady,
          "The crafting request is not bound to an active world.");
      }

      CommitVersion commitVersion;
      try
      {
        commitVersion = _responseVersion.Next();
      }
      catch (InvalidOperationException)
      {
        return ModuleDispatchResult.Rejected(
          ProtocolErrorCode.NotReady,
          "The crafting response version space is exhausted.");
      }

      SessionEpoch[] targets = [context.Epoch];
      CraftingTransactionResult result = _coordinator.Execute(
        request,
        context.Sender,
        context.Epoch,
        new WorldScopeId(worldScope),
        envelope.ReceivedAt);
      _responseVersion = commitVersion;
      Version4ModulePayloads.CraftingApprovalData approval =
        new(result.Approved);
      return ModuleDispatchResult.Handled(
        commitVersion,
        [_producer.CreateIntent(ModuleId, approval, targets, commitVersion)]);
    }
  }

  private static bool TryCreateRequest(
    object? decodedPayload,
    out CraftingModule11Request? request)
  {
    if (decodedPayload is CraftingModule11Request direct)
    {
      request = direct;
      return true;
    }

    if (decodedPayload is not Version4ModulePayloads.CraftingRequestData data ||
        data.Materials is null ||
        data.ChestIndexes is null ||
        data.Materials.Any(static material => material is null))
    {
      request = null;
      return false;
    }

    CraftingMaterialRequirement[] materials = data.Materials
      .Select(static material => new CraftingMaterialRequirement(
        material.ItemIdOrRecipeGroup,
        material.Stack))
      .ToArray();
    request = new CraftingModule11Request(materials, data.ChestIndexes);
    return true;
  }
}
