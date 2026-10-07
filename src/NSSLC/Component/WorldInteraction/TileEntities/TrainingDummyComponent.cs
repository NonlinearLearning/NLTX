using Terraria.Relationships;

namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存训练假人的关联 NPC 和激活重试冷却。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Tile_Entities.TETrainingDummy。</para>
/// <para>
/// 原始文件：D:/TRbackup/Version4/Terraria.GameContent.Tile_Entities/TETrainingDummy.cs。
/// </para>
/// <para>主要源成员：npc（第 16 行）； activationRetryCooldown（第 18 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-world-interaction-and-structures-component-design.md。
/// </para>
/// <para>依据位置：第 644 行。</para>
/// </remarks>
public sealed class TrainingDummyComponent
{
  public EntityReference Npc { get; internal set; } = EntityReference.None;
  public int ActivationRetryCooldownTicks { get; internal set; }
  public bool IsActive => !Npc.IsEmpty;
}
