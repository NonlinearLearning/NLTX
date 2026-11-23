namespace Terraria.World;

/// <summary>
/// Tile entity kinds, in the registration order Version4 <c>TileEntitiesManager.RegisterAll</c>
/// assigns, which is also the <c>byte</c> type written into the world file.
/// </summary>
/// <remarks>
/// The server only decodes the kinds whose record layout it can parse. The complex clothing kinds
/// (display doll, hat rack) and the leashed-entity anchors carry variable bitsets that the runtime
/// does not consume yet, so they are reported as unsupported instead of being guessed at.
/// </remarks>
public enum WorldTileEntityKind : byte
{
  TrainingDummy = 0,
  ItemFrame = 1,
  LogicSensor = 2,
  DisplayDoll = 3,
  WeaponsRack = 4,
  HatRack = 5,
  FoodPlate = 6,
  TeleportationPylon = 7,
  DeadCellsDisplayJar = 8,
  KiteAnchor = 9,
  CritterAnchor = 10
}
