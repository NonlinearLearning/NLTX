namespace Terraria.Player.Presentation;

/// <summary>
/// 保存玩家足球持有和绘制状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：hasFootball（第 2008 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p11-player-presentation-derived-component-design.md。
/// </para>
/// <para>依据位置：第 89 行。</para>
/// </remarks>
public sealed class PlayerFootballPresentationStateComponent
{
  public bool HasFootball { get; private set; }

  public bool IsDrawingFootball { get; private set; }

  internal void ApplyInput(in PlayerFootballPresentationInput input)
  {
    if (input.ResetState)
    {
      Reset();
      return;
    }

    HasFootball = input.HasFootball;
    IsDrawingFootball = input.HasFootball &&
      input.CanDrawFootball &&
      !input.IsFootballItemAnimating;
  }

  internal void Reset()
  {
    HasFootball = false;
    IsDrawingFootball = false;
  }

  public PlayerFootballPresentationSnapshot ToSnapshot()
  {
    return new PlayerFootballPresentationSnapshot(HasFootball, IsDrawingFootball);
  }
}
