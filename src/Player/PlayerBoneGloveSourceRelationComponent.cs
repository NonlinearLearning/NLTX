namespace Terraria.Player;

public sealed class PlayerBoneGloveSourceRelationComponent
{
  public ItemEntityRef BoneGloveItem { get; internal set; } = ItemEntityRef.None;

  internal void ResetEffects()
  {
    BoneGloveItem = ItemEntityRef.None;
  }
}
