using System.Numerics;

namespace NLTX.PlayerInputGameplay.PressurePlates;

public sealed class PressurePlateSystem
{
  public PressurePlateTransition? UpdatePlayer(
    PressurePlateOccupancyComponent occupancy,
    PressurePlateCoordinate plate,
    int playerSlot,
    Vector2 currentPosition,
    bool inside)
  {
    ArgumentNullException.ThrowIfNull(occupancy);
    _ = occupancy.GetPreviousPosition(playerSlot);
    occupancy.SetPreviousPosition(playerSlot, currentPosition);
    return occupancy.SetPressed(plate, playerSlot, inside)
      ? new PressurePlateTransition(plate, playerSlot, inside)
      : null;
  }
}
