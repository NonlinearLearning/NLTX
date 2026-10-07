namespace Terraria.Player;

/// <summary>
/// 保存玩家持久身份、名字、队伍、难度和连接身份。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：active（第 478 行）； name（第 505 行）； team（第 978 行）； difficulty（第 1154 行）。</para>
/// <para>重组说明：持久玩家 UUID、连接身份和主机标记是 ECS 玩家身份模型新增的表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 111 行。</para>
/// </remarks>
public sealed class PlayerIdentityComponent
{
  // Runtime EntityUuid remains owned by the shared entity identity component.
  public PersistentPlayerId? PersistentPlayerId { get; set; }

  public string CharacterName { get; set; } = string.Empty;

  // Compatibility alias for the Version4 public identity name.
  public string DisplayName
  {
    get => CharacterName;
    set => CharacterName = value;
  }

  public int TeamId { get; set; }

  public PlayerDifficulty Difficulty { get; set; }

  public PlayerConnectionState ConnectionState { get; internal set; }

  // Derived compatibility view; ConnectionState is authoritative.
  public bool IsActive => ConnectionState == PlayerConnectionState.Active;

  public bool IsHost { get; set; }

  // Compatibility projection only; it is not a persistence key.
  public LegacyPlayerSlot? LegacyPlayerSlot { get; set; }
}
