using System;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;
using Terraria.Unshared.World;

namespace NSSLC.Server.Debugging;

/// <summary>
/// 服务器调试工具 API：以代码调用的方式注入物理验证场景。
/// </summary>
/// <remarks>
/// 这些能力只服务于“复现物理缺陷”的调试场景，默认不启用，也没有任何命令行开关——
/// 需要时在宿主代码里显式调用。生产路径不应调用本类型的任何方法。
/// <para>
/// 副作用登记：<see cref="PlaceStepBlock" /> 会写入世界方块并推进世界版本；
/// <see cref="CreatePlayerWalkRegistration" /> 只构造系统注册，不修改世界状态。
/// </para>
/// </remarks>
public static class ServerDebugTools
{
  /// <summary>
  /// 在指定 tile 坐标放一格实心块，制造“高一格的平面”。
  /// </summary>
  /// <param name="world">被写入的世界运行时；调用方持有其生命周期。</param>
  /// <param name="tileX">方块的 tile X。</param>
  /// <param name="tileY">方块的 tile Y；通常取目标地表的上一格。</param>
  /// <returns>写入结果；失败时携带原因，调用方可直接记录。</returns>
  public static WorldInteractionResult PlaceStepBlock(
      WorldRuntime world,
      int tileX,
      int tileY)
  {
    ArgumentNullException.ThrowIfNull(world);
    return world.TryPlaceTile(new WorldTileCoordinate(tileX, tileY), tileType: 1, style: 0);
  }

  /// <summary>
  /// 构造一个“让玩家持续朝 +X 移动”的系统注册，使 NPC 获得可追击目标。
  /// </summary>
  /// <param name="world">ECS 世界。</param>
  /// <param name="entities">实体运行时句柄解析器。</param>
  /// <param name="horizontalSpeed">水平速度，单位与移动意图一致；0 会抛异常。</param>
  /// <param name="registrationKey">系统注册键，默认 <c>debug.player.walk</c>。</param>
  public static EcsServerSystemRegistration CreatePlayerWalkRegistration(
      ArchWorld world,
      IEntityRuntimeHandleResolver entities,
      float horizontalSpeed = 120f,
      string registrationKey = "debug.player.walk")
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(entities);
    ArgumentException.ThrowIfNullOrWhiteSpace(registrationKey);
    return new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        registrationKey,
        new DebugPlayerWalkSystem(world, entities, horizontalSpeed));
  }
}
