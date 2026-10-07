namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 纵向朝向和精灵朝向。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：directionY（第 6305 行）； spriteDirection（第 6377 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/baseline/。</para>
/// <para>拆分依据文件：空间移动碰撞与液体组件字段设计.md。</para>
/// <para>依据位置：第 348 行。</para>
/// </remarks>
public struct NpcDirectionComponent
{
  public NpcDirectionComponent(int vertical, int sprite)
  {
    Vertical = vertical;
    Sprite = sprite;
  }

  public int Vertical;
  public int Sprite;
}
