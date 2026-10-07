namespace Terraria.Npc;

/// <summary>
/// 保存 NPC AI 风格、权威槽位、局部槽位和计时状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：ai（第 6309 行）； localAI（第 6311 行）； aiAction（第 6313 行）； aiStyle（第 6315 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-code-draft.md。</para>
/// <para>依据位置：第 75 行。</para>
/// </remarks>
public struct NpcAiStateComponent
{
  public NpcAiStateComponent(
    int style,
    float state0,
    float state1,
    float state2,
    float state3,
    int timer,
    float localAi0 = 0f,
    float localAi1 = 0f,
    float localAi2 = 0f,
    float localAi3 = 0f)
  {
    Style = style;
    State0 = state0;
    State1 = state1;
    State2 = state2;
    State3 = state3;
    Timer = timer;
    LocalAi0 = localAi0;
    LocalAi1 = localAi1;
    LocalAi2 = localAi2;
    LocalAi3 = localAi3;
  }

  public int Style;
  public float State0;
  public float State1;
  public float State2;
  public float State3;
  public int Timer;
  public float LocalAi0;
  public float LocalAi1;
  public float LocalAi2;
  public float LocalAi3;
}
