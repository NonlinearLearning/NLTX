using System.Numerics;

namespace Terraria.Physics;

/// <summary>Vector guarantees that simulation code relies on.</summary>
/// <remarks>
/// 这两个操作此前在多个域里各自实现了一份逐字节相同的私有副本：
/// <c>IsFinite(Vector2)</c> 见于 <c>Communication/GameCommunicationAdapter</c>、
/// <c>Projectile/ProjectileSimulationSystem</c>、<c>Share/Physics/Liquid/LiquidSimulation</c>、
/// <c>World/Solver/WorldTileMotionSolver</c> 四处；<c>MoveTowards</c> 见于
/// <c>Projectile/ProjectileHomingAiBehavior</c> 与 <c>Share/SpatialSimulationSystem</c> 两处。
///
/// 它们的语义是"所有参与模拟的向量都必须满足的最小性质"，属于物理域的公共前提，
/// 因此集中在这里，而不是让每个使用者复制一份。公式保持与原来逐字符一致，
/// 以避免任何浮点结果差异。
/// </remarks>
internal static class SimulationVector
{
  /// <summary>True when both components are finite; used to reject poisoned simulation input.</summary>
  internal static bool IsFinite(Vector2 value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }

  /// <summary>Moves <paramref name="current"/> toward <paramref name="target"/> by at most the given distance.</summary>
  internal static Vector2 MoveTowards(Vector2 current, Vector2 target, float distance)
  {
    Vector2 delta = target - current;
    float length = delta.Length();
    return length <= distance || length == 0f ? target : current + delta / length * distance;
  }
}
