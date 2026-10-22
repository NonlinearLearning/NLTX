using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// Coordinates Version4 sign request/update behavior and creates formal packet DTOs.
/// Session validation and actual packet routing remain the host/dispatcher's responsibility.
/// </summary>
public sealed class SignInteractionService
{
  public const byte OpenSignRequestMessageId = 46;
  public const byte OpenSignResponseMessageId = 47;

  private readonly SignRegistry _registry;

  public SignInteractionService(SignRegistry registry)
  {
    ArgumentNullException.ThrowIfNull(registry);
    _registry = registry;
  }

  public long WorldScope => _registry.WorldScope;

  /// <summary>
  /// Handles packet 46. Version4 replies only when Sign.ReadSign finds or creates a valid entry.
  /// </summary>
  public SignOpenDispatchResult HandleOpen(
    WorldSnapshot world,
    OpenSignRequestPacket request,
    byte authenticatedPlayer)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(request);

    SignLookupResult lookup = _registry.ReadSign(
      world,
      new TileCoordinate(request.X, request.Y));
    if (lookup.Sign is null)
    {
      return new SignOpenDispatchResult(lookup, null);
    }

    SignSnapshot sign = lookup.Sign;
    var packet = new OpenSignResponsePacket
    {
      SignIndex = ToWireValue(sign.Id, nameof(sign.Id)),
      X = ToWireValue(sign.Anchor.X, nameof(sign.Anchor.X)),
      Y = ToWireValue(sign.Anchor.Y, nameof(sign.Anchor.Y)),
      Text = sign.Text,
      Player = authenticatedPlayer,
      Flags = 0
    };
    return new SignOpenDispatchResult(lookup, packet);
  }

  /// <summary>
  /// Handles packet 47. The claimed player and incoming flags are not trusted; Version4 uses the
  /// authenticated sender for the broadcast and emits zero flags. Unchanged text has no reply.
  /// </summary>
  public SignUpdateDispatchResult HandleUpdate(
    WorldSnapshot world,
    OpenSignResponsePacket request,
    byte authenticatedPlayer)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(request);

    SignUpdateResult update = _registry.ApplyText(
      world,
      request.SignIndex,
      new TileCoordinate(request.X, request.Y),
      request.Text);
    if (update.Status != SignUpdateStatus.Applied || update.Sign is null)
    {
      return new SignUpdateDispatchResult(update, null, false);
    }

    SignSnapshot sign = update.Sign;
    var broadcast = new OpenSignResponsePacket
    {
      SignIndex = ToWireValue(sign.Id, nameof(sign.Id)),
      X = ToWireValue(sign.Anchor.X, nameof(sign.Anchor.X)),
      Y = ToWireValue(sign.Anchor.Y, nameof(sign.Anchor.Y)),
      Text = sign.Text,
      Player = authenticatedPlayer,
      Flags = 0
    };
    return new SignUpdateDispatchResult(update, broadcast, true);
  }

  private static short ToWireValue(int value, string parameterName)
  {
    if (value < 0 || value > short.MaxValue)
    {
      throw new InvalidOperationException(
        $"Sign {parameterName} cannot be represented by the Version4 Int16 field.");
    }
    return (short)value;
  }
}

public sealed record SignOpenDispatchResult(
  SignLookupResult Lookup,
  OpenSignResponsePacket? Response);

public sealed record SignUpdateDispatchResult(
  SignUpdateResult Update,
  OpenSignResponsePacket? Broadcast,
  bool ExcludeAuthenticatedSender);
