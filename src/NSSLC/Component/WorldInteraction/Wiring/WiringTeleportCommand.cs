using System;
using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public readonly record struct WiringTeleportCommand
{
  private WiringTeleportCommand(
    Guid commandId,
    TileCoordinate source,
    TileCoordinate destination,
    byte wireColor,
    EntityReference subject,
    bool blockPlayerTeleportation)
  {
    CommandId = commandId;
    Source = source;
    Destination = destination;
    WireColor = wireColor;
    Subject = subject;
    BlockPlayerTeleportation = blockPlayerTeleportation;
  }

  public Guid CommandId { get; }

  public TileCoordinate Source { get; }

  public TileCoordinate Destination { get; }

  public byte WireColor { get; }

  public EntityReference Subject { get; }

  public bool BlockPlayerTeleportation { get; }

  public bool IsValid =>
    CommandId != Guid.Empty &&
    IsValidEndpoint(Source) &&
    IsValidEndpoint(Destination) &&
    Source != Destination &&
    IsValidWireColor(WireColor) &&
    IsSupportedSubject(Subject);

  public static bool IsSupportedSubject(EntityReference subject)
  {
    return !subject.IsEmpty &&
      subject.Scope is EntityReferenceScope.Player or EntityReferenceScope.Npc;
  }

  public static bool TryCreate(
    TileCoordinate source,
    TileCoordinate destination,
    byte wireColor,
    EntityReference subject,
    bool blockPlayerTeleportation,
    out WiringTeleportCommand command)
  {
    return TryCreate(
      Guid.NewGuid(),
      source,
      destination,
      wireColor,
      subject,
      blockPlayerTeleportation,
      out command);
  }

  public static bool TryCreate(
    Guid commandId,
    TileCoordinate source,
    TileCoordinate destination,
    byte wireColor,
    EntityReference subject,
    bool blockPlayerTeleportation,
    out WiringTeleportCommand command)
  {
    command = default;
    if (commandId == Guid.Empty ||
        !IsValidEndpoint(source) ||
        !IsValidEndpoint(destination) ||
        source == destination ||
        !IsValidWireColor(wireColor) ||
        !IsSupportedSubject(subject))
    {
      return false;
    }

    command = new WiringTeleportCommand(
      commandId,
      source,
      destination,
      wireColor,
      subject,
      blockPlayerTeleportation);
    return true;
  }

  private static bool IsValidEndpoint(TileCoordinate coordinate)
  {
    return coordinate.X >= 0 && coordinate.Y >= 0;
  }

  private static bool IsValidWireColor(byte wireColor)
  {
    return wireColor is >= 1 and <= 4;
  }
}
