using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹动画帧、帧计数和帧容量。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：frameCounter（第 242 行）； frame（第 244 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
public struct ProjectileAnimationStateComponent
{
  public ProjectileAnimationStateComponent(
    int frame = 0,
    int frameCounter = 0,
    int frameCount = 1)
  {
    ValidateNonNegative(frame, nameof(frame));
    ValidateNonNegative(frameCounter, nameof(frameCounter));
    ValidateNonNegative(frameCount, nameof(frameCount));
    Frame = frame;
    FrameCounter = frameCounter;
    FrameCount = frameCount;
  }

  public int Frame;
  public int FrameCounter;

  public int FrameCount;

  public void Reset()
  {
    Frame = 0;
    FrameCounter = 0;
  }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
