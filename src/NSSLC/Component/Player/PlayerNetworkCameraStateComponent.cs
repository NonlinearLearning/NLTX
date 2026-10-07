using System.Numerics;

namespace Terraria.Player;

/// <summary>
/// 保存玩家网络显示偏移和镜头目标同步状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：netOffset（第 1128 行）； netCameraTarget（第 1130 行）； lastSyncedNetCameraTarget（第 1132 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p11-player-presentation-derived-component-design.md。
/// </para>
/// <para>依据位置：第 86 行。</para>
/// </remarks>
public sealed class PlayerNetworkCameraStateComponent
{
  public Vector2 NetOffset { get; private set; }

  public Vector2? NetCameraTarget { get; private set; }

  public Vector2? LastSyncedNetCameraTarget { get; private set; }

  internal void SetNetOffset(Vector2 netOffset)
  {
    NetOffset = netOffset;
  }

  internal void SetCameraTarget(Vector2? cameraTarget)
  {
    NetCameraTarget = cameraTarget;
  }

  public void ApplyNetworkCameraTarget(Vector2? cameraTarget)
  {
    SetCameraTarget(cameraTarget);
  }

  internal void SetLastSyncedCameraTarget(Vector2? cameraTarget)
  {
    LastSyncedNetCameraTarget = cameraTarget;
  }

  internal void Reset()
  {
    NetOffset = Vector2.Zero;
    NetCameraTarget = null;
    LastSyncedNetCameraTarget = null;
  }

  public PlayerCameraSnapshot ToSnapshot(SimulationTick tick)
  {
    return new PlayerCameraSnapshot(
      tick,
      NetOffset,
      NetCameraTarget,
      LastSyncedNetCameraTarget);
  }
}
