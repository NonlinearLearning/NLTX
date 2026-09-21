namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-783..P09-803
// crossSubsystemOwner: visible-slot calculation and renderer consumption remain integration-review
public sealed class PlayerVisibleEquipmentSelectionComponent
{
  public int Head { get; internal set; } = -1;

  public int Body { get; internal set; } = -1;

  public int Legs { get; internal set; } = -1;

  public int Coat { get; internal set; } = -1;

  public sbyte HandOn { get; internal set; } = -1;

  public sbyte HandOff { get; internal set; } = -1;

  public sbyte Back { get; internal set; } = -1;

  public sbyte Front { get; internal set; } = -1;

  public sbyte Shoe { get; internal set; } = -1;

  public sbyte Waist { get; internal set; } = -1;

  public sbyte Shield { get; internal set; } = -1;

  public sbyte Neck { get; internal set; } = -1;

  public sbyte Face { get; internal set; } = -1;

  public sbyte Balloon { get; internal set; } = -1;

  public sbyte Backpack { get; internal set; } = -1;

  public sbyte Tail { get; internal set; } = -1;

  public sbyte FaceHead { get; internal set; } = -1;

  public sbyte FaceFlower { get; internal set; } = -1;

  public sbyte FaceMask { get; internal set; } = -1;

  public sbyte BalloonFront { get; internal set; } = -1;

  public sbyte Beard { get; internal set; } = -1;
}
