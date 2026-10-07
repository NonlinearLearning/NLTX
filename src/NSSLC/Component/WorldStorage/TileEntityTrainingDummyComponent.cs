namespace Terraria.WorldStorage;

/// <summary>Live Training Dummy TileEntity capability state.</summary>
/// <remarks>
/// <para>职责：保存存储层训练假人的关联 NPC 索引。</para>
/// <para>拆分来源：Terraria.GameContent.Tile_Entities.TETrainingDummy。</para>
/// <para>
/// 原始文件：D:/TRbackup/Version4/Terraria.GameContent.Tile_Entities/TETrainingDummy.cs。
/// </para>
/// <para>主要源成员：npc（第 16 行）。</para>
/// </remarks>
public struct TileEntityTrainingDummyComponent
{
  public short NpcIndex;

  public TileEntityTrainingDummyComponent(short npcIndex)
  {
    NpcIndex = npcIndex;
  }
}
