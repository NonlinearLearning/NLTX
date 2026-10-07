using System.IO;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertEqual<T>(T expected, T actual, string message)
{
  Assert(EqualityComparer<T>.Default.Equals(expected, actual),
    $"{message} Expected {expected}, got {actual}.");
}

TownHousingRegistryComponent registry = new();
TownHousingResidentKey merchant = new(17);
TilePosition room = new(100, 200);
TownHousingRegistrySystem.AssignRoom(registry, merchant, room);
AssertEqual(TownHousingKeyMode.Type, registry.ResidentKeyMode, "NPC type key mode is selected.");
Assert(TownHousingRegistrySystem.HasRoom(registry, merchant), "Assigned room is indexed.");
Assert(TownHousingRegistrySystem.TryGetRoom(registry, merchant, out TilePosition assignedRoom),
  "Assigned room position can be queried.");
AssertEqual(room, assignedRoom, "Assigned room position is retained.");
AssertEqual(1UL, registry.Revision, "First room assignment advances revision.");

TownHousingRegistrySystem.MarkHomeless(registry, merchant);
Assert(!TownHousingRegistrySystem.HasRoom(registry, merchant), "Homeless assignment removes the room.");
Assert(TownHousingRegistrySystem.IsHomeless(registry, merchant), "Homeless resident is indexed.");
AssertEqual(2UL, registry.Revision, "Homeless transition advances revision.");
Console.WriteLine("PASS: room and homelessness remain mutually exclusive");

TownHousingRegistrySystem.AssignRoom(registry, merchant, room);
using MemoryStream stream = new();
using (BinaryWriter writer = new(stream, System.Text.Encoding.UTF8, leaveOpen: true))
{
  TownHousingRegistryPersistenceAdapter.Save(writer, registry);
}

stream.Position = 0;
TownHousingRegistryComponent restored = new();
using (BinaryReader reader = new(stream, System.Text.Encoding.UTF8, leaveOpen: true))
{
  TownHousingRegistryPersistenceAdapter.Load(reader, restored);
}

Assert(TownHousingRegistrySystem.HasRoom(restored, merchant), "Saved room restores by NPC type.");
Assert(TownHousingRegistrySystem.TryGetRoom(restored, merchant, out TilePosition restoredRoom),
  "Restored room position can be queried.");
AssertEqual(room, restoredRoom, "Saved coordinates retain Version4 order.");
AssertEqual(1, restored.AssignedRoomCount, "Restored room count.");
Console.WriteLine("PASS: Version4 room count/type/x/y persistence layout");

Assert(TownHousingRegistrySystem.RemoveResident(restored, merchant), "Resident removal reports a change.");
Assert(restored.IsEmpty, "Removed resident leaves an empty registry.");
Assert(!TownHousingRegistrySystem.RemoveResident(restored, merchant), "Repeated removal is idempotent.");
Console.WriteLine("PASS: removal and repeated removal are deterministic");
