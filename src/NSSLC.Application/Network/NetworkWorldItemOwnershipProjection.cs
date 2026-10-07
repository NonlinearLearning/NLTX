namespace Terraria.Network;

/// <summary>Detached reservation and pickup-delay state for Steam packet 22.</summary>
public readonly record struct NetworkWorldItemOwnershipProjection(
  short ItemIndex,
  byte ReservedForPlayer,
  int TimeToKeepReservation,
  byte GrabDelayPlayer,
  int GrabDelayTime,
  float PositionX,
  float PositionY);
