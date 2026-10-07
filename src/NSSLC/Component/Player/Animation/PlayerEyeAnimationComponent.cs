namespace Terraria.Player.Animation;

/// <summary>
/// 保存玩家眼部状态、状态停留时间和眼帧。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.PlayerEyeHelper。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/PlayerEyeHelper.cs。</para>
/// <para>主要源成员：_state（第 24 行）； _timeInState（第 26 行）； EyeFrameToShow（第 30 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p11-player-presentation-derived-component-design.md。
/// </para>
/// <para>依据位置：第 101 行。</para>
/// </remarks>
public sealed class PlayerEyeAnimationComponent
{
  public PlayerEyeAnimationComponent()
  {
    State = PlayerEyeAnimationState.NormalBlinking;
  }

  public PlayerEyeAnimationState State { get; private set; }

  public int TimeInState { get; private set; }

  public int EyeFrameToShow { get; private set; }

  internal void AdvanceTime()
  {
    TimeInState++;
  }

  internal void SetEyeFrame(int eyeFrameToShow)
  {
    EyeFrameToShow = eyeFrameToShow;
  }

  internal void SetState(PlayerEyeAnimationState state, bool resetStateTimerEvenIfAlreadyInState = false)
  {
    if (State == state && !resetStateTimerEvenIfAlreadyInState)
    {
      return;
    }

    State = state;
    TimeInState = 0;
  }

  internal void SetTimeInState(int timeInState)
  {
    TimeInState = timeInState;
  }

  internal void Reset()
  {
    State = PlayerEyeAnimationState.NormalBlinking;
    TimeInState = 0;
    EyeFrameToShow = 0;
  }
}
