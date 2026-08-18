namespace Terraria.Dome.Protocol.V1456.Packets;

public enum DoorToggleAction : byte
{
  OpenDoor = 0,
  CloseDoor = 1,
  ShiftTrapdoor = 2,
  ShiftTrapdoorReverse = 3,
  OpenTallGate = 4,
  CloseTallGate = 5
}
