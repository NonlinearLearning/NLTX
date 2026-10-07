using System.Numerics;

namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 单轮移动所需的历史运动、碰撞和液体状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>
/// 主要源成员：oldPosition（第 14 行）； oldVelocity（第 16 行）； wet（第 26 行）； shimmerWet（第 28 行）； honeyWet（第 30
/// 行）。
/// </para>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：noGravity（第 6367 行）； noTileCollide（第 6369 行）； collideX（第 6371 行）； collideY（第 6373 行）。
/// </para>
/// </remarks>
public sealed class NpcMovementTickStateComponent
{
  public Vector2 OldPosition { get; private set; }

  public Vector2 OldVelocity { get; private set; }

  public bool CollideX { get; private set; }

  public bool CollideY { get; private set; }

  public bool Wet { get; private set; }

  public bool ShimmerWet { get; private set; }

  public bool HoneyWet { get; private set; }

  public bool NoGravity { get; private set; }

  public bool NoTileCollide { get; private set; }

  public void BeginTick(
    Vector2 position,
    Vector2 velocity,
    bool wet,
    bool shimmerWet,
    bool honeyWet)
  {
    OldPosition = position;
    OldVelocity = velocity;
    CollideX = false;
    CollideY = false;
    Wet = wet;
    ShimmerWet = shimmerWet;
    HoneyWet = honeyWet;
    NoGravity = false;
    NoTileCollide = false;
  }

  public void CommitPhysicsFlags(bool noGravity, bool noTileCollide)
  {
    NoGravity = noGravity;
    NoTileCollide = noTileCollide;
  }

  public void CommitCollision(bool collideX, bool collideY)
  {
    CollideX = collideX;
    CollideY = collideY;
  }
}
