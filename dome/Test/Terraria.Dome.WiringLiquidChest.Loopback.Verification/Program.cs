using System;
using System.Linq;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.WorldObjects.Chest.Commands;
using Terraria.Dome.Simulation.WorldObjects.Chest.Systems;
using Terraria.Dome.Simulation.Wiring.Commands;
using Terraria.Dome.Simulation.Wiring.Components;
using PipelineLiquidSourceComponent = Terraria.Dome.Simulation.Liquid.Components.LiquidSourceComponent;

using DomeSimulation source = new(new WorldGrid(400, 300));
PlayerHandle player = source.CreatePlayer(new SimulationVector(20, 20));
int chestId = source.CreateChest(20, 20);
ChestComponent sourceChest = source.GetChest(chestId);
if (sourceChest.Name != string.Empty ||
    !sourceChest.TryRename("Storage") ||
    sourceChest.Name != "Storage" ||
    sourceChest.Revision != 2 ||
    !sourceChest.TryRename("Storage") ||
    sourceChest.Revision != 2 ||
    sourceChest.TryRename(new string('x', 21)))
{
  throw new InvalidOperationException(
    "Chest rename did not enforce bounded names and revision semantics.");
}

source.SetChestItem(chestId, 0, new Terraria.Dome.Simulation.Items.ItemStack(1, 4));

if (!source.TryOpenChest(chestId, player, new SimulationVector(20, 20)))
{
  throw new InvalidOperationException("Loopback chest fixture could not open the source chest.");
}

DomeSimulationSnapshot persisted = source.CreatePersistenceSnapshot(
  new WorldMetadata("loopback", new WorldSeed(5), 400, 300));
using DomeSimulation restored = new(persisted);
ChestSnapshot restoredChest = restored.CreateChestSnapshots().Single();
if (restoredChest.Opener is not null || restoredChest.Slots[0].Quantity != 4 ||
    restoredChest.Revision != source.CreateChestSnapshots().Single().Revision ||
    restored.CreateChestPersistentSnapshots().Single().Name != "Storage")
{
  throw new InvalidOperationException(
    "Chest persistence did not release opener or preserve name, revision and slots.");
}

if (!new ChestOpenSystem().TryApply(
      restored.GetChest(chestId),
      new ChestOpenCommand(10, chestId, player, new SimulationVector(20, 20))) ||
    !new ChestCloseSystem().TryApply(
      restored.GetChest(chestId),
      new ChestCloseCommand(11, chestId, player)))
{
  throw new InvalidOperationException("Chest reconnect open/close did not restore exclusive access.");
}

using DomeSimulation first = new(new WorldGrid(400, 300));
using DomeSimulation second = new(new WorldGrid(400, 300));
PipelineLiquidSourceComponent liquidSource = new(30, 30, 128, LiquidType.Water, 1);
if (!first.TryQueueLiquidSource(liquidSource) || !second.TryQueueLiquidSource(liquidSource))
{
  throw new InvalidOperationException("Liquid replay fixtures could not be seeded.");
}

first.Tick(new SimulationInputBatch([]));
second.Tick(new SimulationInputBatch([]));
if (!first.CreateLiquidReplicationSnapshots().SequenceEqual(second.CreateLiquidReplicationSnapshots()) ||
    first.WorldGrid.GetTile(30, 29).LiquidAmount != second.WorldGrid.GetTile(30, 29).LiquidAmount)
{
  throw new InvalidOperationException("Liquid deterministic replay diverged between identical inputs.");
}

Console.WriteLine("PASS: chest reconnect persistence and liquid deterministic loopback are backed");

using DomeSimulation wiringSimulation = new(new WorldGrid(400, 300));
PlayerHandle wiringPlayer = wiringSimulation.CreatePlayer(new SimulationVector(10, 10));
wiringSimulation.SetWireMask(10, 10, 1);
wiringSimulation.SetWireMask(11, 10, 1);
_ = wiringSimulation.WorldGrid.TrySetLiquid(10, 10, 128, (byte)LiquidType.Water);
if (!wiringSimulation.TryRegisterMechanism(
      new MechanismComponent(7, MechanismType.Pump),
      pump: new PumpComponent(7, 10, 10, 12, 10, 16, cooldownTicks: 0)) ||
    !wiringSimulation.TryRegisterWiringTrigger(11, 10, new TriggerComponent(8, 7, false)) ||
    !wiringSimulation.TryQueueWiringInput(
      new WiringInputCommand(1, wiringPlayer, 10, 10, WireColor.Red)))
{
  throw new InvalidOperationException("Wiring loopback could not queue its authoritative input.");
}

wiringSimulation.Tick(new SimulationInputBatch([]));
if (!wiringSimulation.CreateMechanismSnapshots().Single().IsActive ||
    wiringSimulation.WorldGrid.GetTile(10, 10).LiquidAmount >= 128 ||
    wiringSimulation.CreateLiquidReplicationSnapshots().Count == 0)
{
  throw new InvalidOperationException(
    "Wiring pump did not cross the authoritative liquid boundary.");
}

Console.WriteLine("PASS: wiring trigger and pump cross-domain loopback is authoritative");

using DomeSimulation pressureSimulation = new(new WorldGrid(400, 300));
PlayerHandle pressurePlayer = pressureSimulation.CreatePlayer(new SimulationVector(30, 30));
int pressureDoor = pressureSimulation.CreateDoor(30, 30);
if (!pressureSimulation.TryRegisterMechanism(new MechanismComponent(9, MechanismType.Door)) ||
    !pressureSimulation.TryRegisterDoorMechanism(9, pressureDoor) ||
    !pressureSimulation.TryRegisterMechanism(
      new MechanismComponent(12, MechanismType.Lamp),
      lamp: new LampComponent(12, new WiringTileCoordinate(31, 30), 10, 11)) ||
    !pressureSimulation.TryRegisterPressurePlate(new PressurePlateComponent(9, 30, 30, 2, true)) ||
    !pressureSimulation.TryRegisterPressurePlate(new PressurePlateComponent(12, 30, 30, 2, true)))
{
  throw new InvalidOperationException("Pressure-plate door fixture could not be registered.");
}

pressureSimulation.Tick(new SimulationInputBatch([]));
if (!pressureSimulation.CreateDoorSnapshots().Single().IsOpen ||
    pressureSimulation.WorldGrid.GetTile(31, 30).Type != 10 ||
    !pressureSimulation.WorldGrid.GetTile(31, 30).IsActive ||
    pressureSimulation.CreateMechanismSnapshots().Count != 2 ||
    pressureSimulation.CreateMechanismSnapshots().All(snapshot => !snapshot.IsActive))
{
  throw new InvalidOperationException("Pressure plate did not produce an authoritative door action.");
}

Console.WriteLine("PASS: pressure plate to door loopback is authoritative");

WorldTile actuatorTile = new(
  IsActive: true,
  Type: 1,
  FrameX: 18,
  FrameY: 36,
  WallType: 4,
  IsActuated: true);
_ = pressureSimulation.WorldGrid.TrySetTile(35, 30, actuatorTile);
if (!pressureSimulation.TryRegisterMechanism(
      new MechanismComponent(10, MechanismType.Actuator),
      actuator: new ActuatorComponent(
        10,
        [new WiringTileCoordinate(35, 30)],
        tileType: 1)) ||
    !pressureSimulation.TryRegisterLogicGate(new LogicGateComponent(11, [9], true, 10)))
{
  throw new InvalidOperationException("Logic-gate fixture could not be registered.");
}

pressureSimulation.Tick(new SimulationInputBatch([]));
WorldTile inactiveActuatorTile = pressureSimulation.WorldGrid.GetTile(35, 30);
if (!inactiveActuatorTile.IsInactive || inactiveActuatorTile.Type != actuatorTile.Type ||
    inactiveActuatorTile.FrameX != actuatorTile.FrameX ||
    inactiveActuatorTile.FrameY != actuatorTile.FrameY ||
    inactiveActuatorTile.WallType != actuatorTile.WallType ||
    !inactiveActuatorTile.IsActuated)
{
  throw new InvalidOperationException(
    "Logic gate did not commit a tile-preserving actuator inactive-state transition.");
}

Console.WriteLine(
  "PASS: logic-gate output commits actuator inactive state through the world boundary");
