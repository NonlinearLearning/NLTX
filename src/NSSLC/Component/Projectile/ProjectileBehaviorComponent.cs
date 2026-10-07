namespace Terraria.Projectile;

/// <summary>
/// 保存射弹 AI 风格、权威槽位和局部行为槽位。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：ai（第 128 行）； localAI（第 130 行）； aiStyle（第 136 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-projectile-simulation-code-component-draft.md。</para>
/// <para>依据位置：第 272 行。</para>
/// </remarks>
public struct ProjectileBehaviorComponent
{
  public ProjectileBehaviorComponent(
    int style,
    float state0,
    float state1,
    float state2,
    float state3,
    float localState0,
    float localState1)
  {
    Style = style;
    State0 = state0;
    State1 = state1;
    State2 = state2;
    State3 = state3;
    LocalState0 = localState0;
    LocalState1 = localState1;
  }

  public int Style;
  public float State0;
  public float State1;
  public float State2;
  public float State3;
  public float LocalState0;
  public float LocalState1;
}
