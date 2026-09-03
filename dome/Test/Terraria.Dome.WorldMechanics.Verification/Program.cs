using System;
using System.Linq;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;

using DomeSimulation simulation = new(new WorldGrid(400, 300));
int doorId = simulation.CreateDoor(10, 10);
if (!simulation.TryToggleDoor(doorId, new SimulationVector(10.0f, 10.0f)) ||
    simulation.TryToggleDoor(doorId, new SimulationVector(100.0f, 10.0f)))
{
  throw new InvalidOperationException("Door range validation was not authoritative.");
}

DoorSnapshot door = simulation.CreateDoorSnapshots().Single();
if (!door.IsOpen || door.Revision != 2 || door.Section != new WorldSectionCoordinates(0, 0))
{
  throw new InvalidOperationException("Door revision or section projection was not deterministic.");
}

Console.WriteLine("PASS: deterministic door state has bounded authoritative range and revision");

DoorToggleIntent intent = new(DoorToggleAction.OpenDoor, 10, 10, true);
if (TerrariaPacketCodec.DecodeDoorToggle(TerrariaPacketCodec.EncodeDoorToggle(intent)) != intent)
{
  throw new InvalidOperationException("Typed V1456 ToggleDoorState did not round-trip.");
}

byte[] doorState = TerrariaPacketCodec.EncodeDoorState(new DoorStateReplicationSnapshot(
  DoorToggleAction.CloseDoor,
  10,
  10,
  false));
if (!doorState.AsSpan().SequenceEqual(Convert.FromHexString("090013010A000A0000")))
{
  throw new InvalidOperationException(
    "Server door state did not use V1456 ToggleDoorState fields.");
}

Console.WriteLine("PASS: server door projection uses source ToggleDoorState fields");

using DomeSimulation objectSimulation = new(new WorldGrid(400, 300));
int trapdoorId = objectSimulation.CreateTrapdoor(20, 20, opensDown: true);
if (!objectSimulation.TryApplyDoorTransition(
      trapdoorId,
      DoorTransition.OpenTrapdoor,
      direction: true,
      new SimulationVector(20.0f, 20.0f)))
{
  throw new InvalidOperationException("Closed V1456 trapdoor did not accept its open transition.");
}

WorldTile openedTrapdoorLeft = objectSimulation.WorldGrid.GetTile(20, 21);
WorldTile openedTrapdoorRight = objectSimulation.WorldGrid.GetTile(21, 21);
if (objectSimulation.WorldGrid.GetTile(20, 20).IsActive ||
    openedTrapdoorLeft != new WorldTile(true, 387, FrameX: 0, FrameY: 0) ||
    openedTrapdoorRight != new WorldTile(true, 387, FrameX: 18, FrameY: 0))
{
  throw new InvalidOperationException("V1456 trapdoor open transition did not retain its 2x1 footprint.");
}

if (!objectSimulation.TryApplyDoorTransition(
      trapdoorId,
      DoorTransition.CloseTrapdoor,
      direction: true,
      new SimulationVector(20.0f, 21.0f)) ||
    objectSimulation.WorldGrid.GetTile(20, 21) != new WorldTile(true, 386, FrameX: 36, FrameY: 0) ||
    objectSimulation.WorldGrid.GetTile(21, 22) != new WorldTile(true, 386, FrameX: 54, FrameY: 18))
{
  throw new InvalidOperationException("V1456 trapdoor close transition did not retain its 2x2 frames.");
}

int tallGateId = objectSimulation.CreateTallGate(40, 40);
if (!objectSimulation.TryApplyDoorTransition(
      tallGateId,
      DoorTransition.OpenTallGate,
      direction: true,
      new SimulationVector(40.0f, 40.0f)))
{
  throw new InvalidOperationException("Closed V1456 tall gate did not accept its open transition.");
}

for (int row = 0; row < 5; row++)
{
  WorldTile gateTile = objectSimulation.WorldGrid.GetTile(40, 40 + row);
  if (!gateTile.IsActive || gateTile.Type != 389 || gateTile.FrameX != 0 ||
      gateTile.FrameY != row * 18 + (row == 0 ? 0 : 2))
  {
    throw new InvalidOperationException("V1456 tall gate open transition did not retain its 1x5 frames.");
  }
}

if (objectSimulation.TryApplyDoorTransition(
      tallGateId,
      DoorTransition.OpenTrapdoor,
      direction: true,
      new SimulationVector(40.0f, 40.0f)) ||
    !objectSimulation.TryApplyDoorTransition(
      tallGateId,
      DoorTransition.CloseTallGate,
      direction: true,
      new SimulationVector(40.0f, 40.0f)))
{
  throw new InvalidOperationException("V1456 door transition kinds were not source-derived.");
}

Console.WriteLine("PASS: trapdoor and tall-gate state preserves source footprints and frames");
