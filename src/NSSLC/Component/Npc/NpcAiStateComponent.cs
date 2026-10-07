namespace Terraria.Npc;

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
