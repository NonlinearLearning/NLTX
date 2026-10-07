using System.Numerics;

namespace Terraria.Player;

/// <summary>
/// 保存玩家个人重生点坐标。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：SpawnX（第 1895 行）； SpawnY（第 1897 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 426 行。</para>
/// </remarks>
public readonly record struct PlayerSpawnPointComponent(Vector2 Position);
