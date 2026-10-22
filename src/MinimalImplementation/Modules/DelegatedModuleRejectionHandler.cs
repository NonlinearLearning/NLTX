using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules;

/// <summary>
/// Reports a missing owner route for a delegated Packet 82 module. This is a composition fallback,
/// not a permanent Version4 behavior: P08 must replace it with the corresponding P03/P04/P05/P06
/// handler before the server is considered ready.
/// </summary>
public sealed class DelegatedModuleRejectionHandler : IModuleHandler
{
  private static readonly IReadOnlyDictionary<ushort, (ProtocolErrorCode Code, string Detail)>
    RejectionPolicies = new Dictionary<ushort, (ProtocolErrorCode, string)>
    {
      [0] = (ProtocolErrorCode.NotReady,
        "P03 must register the Version4 liquid owner route before module 0 is ready."),
      [7] = (ProtocolErrorCode.NotReady,
        "P06 must register the Version4 teleport-pylon owner route before module 7 is ready."),
      [11] = (ProtocolErrorCode.NotReady,
        "P04 must register the Version4 crafting owner route before module 11 is ready."),
      [12] = (ProtocolErrorCode.NotReady,
        "P05 must register the Version4 TagEffect owner route before module 12 is ready."),
      [13] = (ProtocolErrorCode.NotReady,
        "P06 must register the Version4 leashed-entity owner route before module 13 is ready."),
      [14] = (ProtocolErrorCode.NotReady,
        "P03 must register the Version4 unbreakable-wall producer route before module 14 is ready.")
    };

  public const ushort LiquidModuleId = 0;
  public const ushort TeleportPylonModuleId = 7;
  public const ushort CraftingRequestsModuleId = 11;
  public const ushort TagEffectModuleId = 12;
  public const ushort LeashedEntityModuleId = 13;
  public const ushort UnbreakableWallModuleId = 14;

  public static IReadOnlyCollection<ushort> DelegatedModuleIds =>
    RejectionPolicies.Keys.OrderBy(static moduleId => moduleId).ToArray();

  public static bool IsDelegatedModule(ushort moduleId)
  {
    return RejectionPolicies.ContainsKey(moduleId);
  }

  public DelegatedModuleRejectionHandler(ushort moduleId)
  {
    if (!RejectionPolicies.ContainsKey(moduleId))
    {
      throw new ArgumentOutOfRangeException(
        nameof(moduleId),
        "Only delegated modules with a frozen non-mutating policy can use this handler.");
    }

    ModuleId = moduleId;
  }

  public ushort ModuleId { get; }

  /// <summary>Creates route handlers for delegated ids not supplied by the P03 composition.</summary>
  public static IReadOnlyList<IModuleHandler> CreatePendingRouteHandlers()
  {
    IModuleHandler[] handlers =
    [
      new DelegatedModuleRejectionHandler(TeleportPylonModuleId),
      new DelegatedModuleRejectionHandler(CraftingRequestsModuleId),
      new DelegatedModuleRejectionHandler(TagEffectModuleId),
      new DelegatedModuleRejectionHandler(LeashedEntityModuleId)
    ];
    return Array.AsReadOnly(handlers);
  }

  public ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(context);
    _ = decodedPayload;

    ModuleDispatchResult? validation = Packet82ModuleHandlerValidation.ValidateActiveEnvelope(
      envelope,
      context,
      ModuleId,
      "delegated route");
    if (validation is not null)
    {
      return validation;
    }

    (ProtocolErrorCode errorCode, string detail) = RejectionPolicies[ModuleId];
    return ModuleDispatchResult.Rejected(errorCode, detail);
  }
}
