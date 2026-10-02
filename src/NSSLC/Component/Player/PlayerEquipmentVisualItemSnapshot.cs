namespace Terraria.Player;

public readonly record struct PlayerEquipmentVisualItemSnapshot(
  ItemEntityRef Entity,
  int TypeId,
  int DyeId,
  int HeadSlot = -1,
  int BodySlot = -1,
  int LegSlot = -1,
  int HandOnSlot = -1,
  int HandOffSlot = -1,
  int BackSlot = -1,
  int FrontSlot = -1,
  int ShoeSlot = -1,
  int WaistSlot = -1,
  int ShieldSlot = -1,
  int NeckSlot = -1,
  int FaceSlot = -1,
  int BalloonSlot = -1,
  int BeardSlot = -1,
  int WingSlot = -1,
  bool DrawBackInBackpackLayer = false,
  bool DrawBackInTailLayer = false,
  bool DrawFaceInHeadLayer = false,
  bool DrawFaceInMaskLayer = false,
  bool DrawFaceInFlowerLayer = false,
  bool DrawBalloonInFrontOfBackArmLayer = false,
  bool IsWingLike = false)
{
  public bool IsAir => Entity.IsEmpty;

  public static PlayerEquipmentVisualItemSnapshot Air =>
    new(ItemEntityRef.None, 0, 0);
}
