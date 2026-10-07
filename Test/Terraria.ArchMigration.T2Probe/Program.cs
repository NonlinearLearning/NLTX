using Arch.Core;

World firstWorld = World.Create();
World secondWorld = World.Create();
Entity entity = firstWorld.Create();

bool passed = firstWorld.Id != secondWorld.Id &&
  entity.WorldId == firstWorld.Id &&
  entity.Version > 0 &&
  firstWorld.IsAlive(entity) &&
  !secondWorld.IsAlive(entity);

firstWorld.Dispose();
firstWorld.Dispose();
secondWorld.Dispose();

if (!passed)
{
  Console.Error.WriteLine("Arch world/entity lifecycle probe failed.");
  return 1;
}

Console.WriteLine(
  $"PASS: entity {entity.Id}@{entity.WorldId} v{entity.Version} was alive in its owner World, absent from a distinct empty World, and repeated dispose succeeded.");
return 0;
