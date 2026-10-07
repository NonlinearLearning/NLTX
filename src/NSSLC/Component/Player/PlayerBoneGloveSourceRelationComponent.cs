namespace Terraria.Player;

/// <summary>
/// 保存骨手套效果对应的来源物品。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：boneGloveItem（第 2036 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 1102 行。</para>
/// </remarks>
public sealed class PlayerBoneGloveSourceRelationComponent
{
  public ItemEntityRef BoneGloveItem { get; internal set; } = ItemEntityRef.None;

  internal void ResetEffects()
  {
    BoneGloveItem = ItemEntityRef.None;
  }
}
