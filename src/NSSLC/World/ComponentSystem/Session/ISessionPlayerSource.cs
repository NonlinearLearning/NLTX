using System.Collections.Generic;

namespace Terraria.Unshared.Session;

/// <summary>Read-only view of the players currently bound to an active session.</summary>
/// <remarks>
/// 领域侧只声明"我需要当前在线玩家"，不关心这些会话从哪里来、由谁维护。实现位于
/// <c>NSSLC.Network</c>，由宿主组合后注入；领域程序集因此不反向依赖网络程序集。
/// 端口只读：调用方拿到的是值拷贝，不获得玩家实体，也不能改会话状态。
/// </remarks>
public interface ISessionPlayerSource
{
  /// <summary>Fills <paramref name="destination"/> with the current roster, replacing its contents.</summary>
  void Collect(List<SessionPlayer> destination);

  /// <summary>Returns a copy of the roster, for callers that iterate while other systems mutate.</summary>
  SessionPlayer[] Snapshot();
}
