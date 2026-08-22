using System;
using System.Linq;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Tick;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.WorldObjects.Chest.Commands;
using Terraria.Dome.Simulation.WorldObjects.Chest.Systems;

SimulationCommandQueue queue = new();
queue.Enqueue(new LiquidChangeCommand(2, 10, 4, 80, 1));
queue.Enqueue(new LiquidChangeCommand(1, 9, 4, 90, 1));
queue.Enqueue(new LiquidChangeCommand(1, 8, 4, 90, 1));
queue.Enqueue(new MechanismActivationCommand(
  3,
  7,
  MechanismActivationKind.Open,
  4));

if (!queue.LiquidChangeCommands.SequenceEqual(
      new[]
      {
        new LiquidChangeCommand(1, 8, 4, 90, 1),
        new LiquidChangeCommand(1, 9, 4, 90, 1),
        new LiquidChangeCommand(2, 10, 4, 80, 1)
      }))
{
  throw new InvalidOperationException("Liquid commands were not exposed in deterministic order.");
}

if (queue.MechanismActivationCommands.Single().MechanismId != 7)
{
  throw new InvalidOperationException("Mechanism activation command was not retained.");
}

bool rejectedNegativeSequence = false;
try
{
  queue.Enqueue(new LiquidChangeCommand(-1, 0, 0, 1, 0));
}
catch (ArgumentOutOfRangeException)
{
  rejectedNegativeSequence = true;
}

if (!rejectedNegativeSequence)
{
  throw new InvalidOperationException("Negative command sequence was accepted.");
}

WorldGrid grid = new(400, 300);
long before = grid.GetSectionVersion(new WorldSectionCoordinates(0, 0));
grid.CommitLiquidChanges(queue.LiquidChangeCommands);
long after = grid.GetSectionVersion(new WorldSectionCoordinates(0, 0));
if (after != before + 3 || grid.GetTile(8, 4).LiquidAmount != 90 ||
    grid.GetTile(10, 4).LiquidType != 1)
{
  throw new InvalidOperationException("Liquid commit did not update tiles and section revision.");
}

ChestComponent chest = new(11, 200, 150);
PlayerHandle opener = new(3);
PlayerHandle contender = new(4);
if (chest.Identity.Section != new WorldSectionCoordinates(1, 1) ||
    chest.Definition.Kind != ChestKind.World ||
    chest.Inventory.GetSlot(ChestInventoryComponent.SlotCount - 1) != ItemStack.Empty ||
    !chest.TryOpen(opener) || chest.TryOpen(contender) || chest.Opener != opener)
{
  throw new InvalidOperationException("Chest component split did not preserve identity or ownership.");
}

long chestRevision = chest.Revision;
chest.SetSlot(0, new ItemStack(1, 5));
chest.IncrementRevision();
chest.Close(opener);
if (chest.Revision != chestRevision + 1 || chest.Opener is not null ||
    chest.GetSlot(0) != new ItemStack(1, 5))
{
  throw new InvalidOperationException("Chest component split did not preserve revision and slots.");
}

ChestOpenSystem openSystem = new();
ChestCloseSystem closeSystem = new();
ChestOpenCommand openCommand = new(4, chest.ChestId, opener, new SimulationVector(200, 150));
if (!openSystem.TryApply(chest, openCommand) ||
    !closeSystem.TryApply(chest, new ChestCloseCommand(5, chest.ChestId, opener)) ||
    closeSystem.TryApply(chest, new ChestCloseCommand(6, chest.ChestId, contender)))
{
  throw new InvalidOperationException("Chest access systems did not enforce command ownership.");
}

chest.SetLocked(true);
if (openSystem.TryApply(chest, new ChestOpenCommand(7, chest.ChestId, opener, new SimulationVector(200, 150))) ||
    chest.Opener is not null)
{
  throw new InvalidOperationException("A locked Chest was opened without an authorized key.");
}

InventoryComponent keyInventory = new();
keyInventory.SetSlot(0, new ItemStack(327, 2));
if (!openSystem.TryApply(
      chest,
      new ChestOpenCommand(8, chest.ChestId, opener, new SimulationVector(200, 150)),
      keyInventory) ||
    chest.IsLocked || chest.Opener != opener || keyInventory.GetSlot(0) != new ItemStack(327, 1))
{
  throw new InvalidOperationException("A valid Gold Key did not atomically unlock and open a Chest.");
}

ChestComponent wrongKeyChest = new(12, 200, 150);
wrongKeyChest.SetLocked(true);
InventoryComponent wrongKeyInventory = new();
wrongKeyInventory.SetSlot(0, new ItemStack(328, 1));
if (openSystem.TryApply(
      wrongKeyChest,
      new ChestOpenCommand(9, wrongKeyChest.ChestId, opener, new SimulationVector(200, 150)),
      wrongKeyInventory) || wrongKeyChest.Opener is not null || !wrongKeyChest.IsLocked ||
    wrongKeyInventory.GetSlot(0) != new ItemStack(328, 1))
{
  throw new InvalidOperationException("A wrong key changed locked Chest or inventory state.");
}

ChestComponent nonConsumingChest = new(13, 200, 150);
nonConsumingChest.SetLocked(true, ChestLockDefinition.ShadowKey);
InventoryComponent shadowKeyInventory = new();
shadowKeyInventory.SetSlot(0, new ItemStack(329, 1));
if (!openSystem.TryApply(
      nonConsumingChest,
      new ChestOpenCommand(10, nonConsumingChest.ChestId, opener, new SimulationVector(200, 150)),
      shadowKeyInventory) || nonConsumingChest.IsLocked ||
    shadowKeyInventory.GetSlot(0) != new ItemStack(329, 1))
{
  throw new InvalidOperationException("A non-consuming key did not preserve its inventory stack.");
}

Console.WriteLine("PASS: shared liquid/mechanism command contracts are deterministic and commit-safe");
