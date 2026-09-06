using Terraria.Relationships;
using Terraria.WorldInteraction.PressurePlates;
using Terraria.WorldInteraction.TileEntities;
using Terraria.WorldInteraction.Tiles;
using Terraria.WorldInteraction.Wiring;

StoredItemState storedItem = new(Type: 17, Prefix: 2, Stack: 1);
if (storedItem.IsEmpty)
{
  throw new InvalidOperationException("A stored item with a positive stack must not be empty.");
}

StoredItemState emptyItem = new(Type: 17, Prefix: 2, Stack: 0);
if (!emptyItem.IsEmpty)
{
  throw new InvalidOperationException("A stored item without a stack must be empty.");
}

TileCoordinate coordinate = new(42, 64);
MechanismCooldownEntry cooldown = new(coordinate, RemainingTicks: 12);
if (cooldown.Position != coordinate || cooldown.RemainingTicks != 12)
{
  throw new InvalidOperationException("Mechanism cooldown entries must retain their coordinate and duration.");
}

MechanismCooldownComponent mechanisms = new();
if (mechanisms.Entries.Count != 0)
{
  throw new InvalidOperationException("Mechanism cooldowns must start without active entries.");
}

PressurePlateOccupancyComponent pressurePlates = new();
if (pressurePlates.OccupantsByPlate.Count != 0 || pressurePlates.NeedsFirstUpdate)
{
  throw new InvalidOperationException("Pressure plate occupancy must start empty and ready for normal updates.");
}

DisplayDollComponent displayDoll = new();
if (displayDoll.Equipment.Count != 9 || displayDoll.Dyes.Count != 9 ||
    displayDoll.Miscellaneous.Count != 1 || displayDoll.ContainsItems)
{
  throw new InvalidOperationException("Display doll slot capacities or initial item state are incorrect.");
}

HatRackComponent hatRack = new();
if (hatRack.Items.Count != 2 || hatRack.Dyes.Count != 2 || hatRack.ContainsItems)
{
  throw new InvalidOperationException("Hat rack slot capacities or initial item state are incorrect.");
}

TrainingDummyComponent trainingDummy = new();
if (trainingDummy.Npc != EntityReference.None || trainingDummy.IsActive)
{
  throw new InvalidOperationException("A training dummy must start without an NPC reference.");
}

TileEntityRuntimeId runtimeId = new(5);
TileEntityKindId kindId = new(3);
if (runtimeId.Value != 5 || kindId.Value != 3 ||
    LogicCheckType.Liquid != (LogicCheckType)7)
{
  throw new InvalidOperationException("Tile entity identity or logic-sensor values are incorrect.");
}

TileCellComponent tileCell = new();
TileFrameComponent tileFrame = new();
TileSignalTopologyComponent signalTopology = new();
if (tileCell.IsActive || tileCell.LiquidKind != LiquidKind.Water ||
    tileFrame.TileFrameX != 0 || signalTopology.HasWire1 || signalTopology.HasActuator)
{
  throw new InvalidOperationException("Tile components must expose stable default structural state.");
}

Console.WriteLine("PASS: world interaction component field composition");
