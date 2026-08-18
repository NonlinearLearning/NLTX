namespace Terraria.Dome.Simulation.Player.Components;

public enum PlayerInteractionMode
{
  None,
  PlaceTile,
  RemoveTile,
  OpenChest,
  ToggleDoor,
  UpdateSign,
  PickupItem
}

public struct PlayerInteractionComponent
{
  public bool HasTarget;
  public PlayerInteractionMode Mode;
  public int TargetId;
  public SimulationVector TargetPosition;

  public void Clear()
  {
    HasTarget = false;
    Mode = PlayerInteractionMode.None;
    TargetId = 0;
    TargetPosition = default;
  }

  public void SetTarget(int targetId, PlayerInteractionMode mode)
  {
    HasTarget = true;
    Mode = mode;
    TargetId = targetId;
  }
}
