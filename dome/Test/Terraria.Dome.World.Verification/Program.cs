using System;
using Terraria.Dome.Simulation.WorldModel;

WorldGrid world = new(width: 400, height: 300);
WorldSectionCoordinates firstSection = world.GetSectionCoordinates(10, 10);
WorldSectionCoordinates adjacentSection = world.GetSectionCoordinates(210, 10);
if (firstSection != new WorldSectionCoordinates(0, 0) ||
    adjacentSection != new WorldSectionCoordinates(1, 0))
{
  throw new InvalidOperationException("WorldGrid did not map tiles to Terraria section units.");
}

if (!world.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 1)))
{
  throw new InvalidOperationException("WorldGrid rejected a tile inside the world boundary.");
}

WorldSectionSnapshot firstSnapshot = world.CreateSectionSnapshot(firstSection);
if (firstSnapshot.Width != 200 || firstSnapshot.Height != 150 ||
    firstSnapshot.Version != 1 ||
    firstSnapshot.GetTile(10, 10) != new WorldTile(IsActive: true, Type: 1))
{
  throw new InvalidOperationException("WorldGrid did not expose the authoritative tile section state.");
}

if (!world.TrySetTile(210, 10, new WorldTile(IsActive: true, Type: 2)))
{
  throw new InvalidOperationException("WorldGrid rejected a tile in an adjacent section.");
}

if (world.GetSectionVersion(firstSection) != 1 ||
    world.GetSectionVersion(adjacentSection) != 1)
{
  throw new InvalidOperationException("WorldGrid changed an unrelated section version.");
}

if (!world.TrySetTile(11, 10, new WorldTile(IsActive: true, Type: 3)) ||
    world.GetSectionVersion(firstSection) != 2 ||
    firstSnapshot.GetTile(10, 10) != new WorldTile(IsActive: true, Type: 1) ||
    firstSnapshot.GetTile(11, 10) != default)
{
  throw new InvalidOperationException("WorldGrid section snapshots are not immutable.");
}

if (world.TrySetTile(-1, 0, new WorldTile(IsActive: true, Type: 4)) ||
    world.TrySetTile(400, 0, new WorldTile(IsActive: true, Type: 4)))
{
  throw new InvalidOperationException("WorldGrid accepted an out-of-bounds tile mutation.");
}

Console.WriteLine("PASS: authoritative WorldGrid section and snapshot boundaries");
