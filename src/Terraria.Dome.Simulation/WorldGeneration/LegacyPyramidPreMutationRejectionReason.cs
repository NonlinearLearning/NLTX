namespace Terraria.Dome.Simulation.WorldGeneration;

public enum LegacyPyramidPreMutationRejectionReason
{
  None,
  ExistingPyramidTileOrWall,
  PotentialDungeonBounds,
  NearbyPyramidTile,
  NearbySandstonePyramidTile,
  NearbyEvilTile,
  NearbyDungeonBrickTile
}
