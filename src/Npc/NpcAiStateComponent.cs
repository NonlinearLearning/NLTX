namespace Terraria.Npc;

public struct NpcAiStateComponent
{
  public NpcAiStateComponent(
    int style,
    float state0,
    float state1,
    float state2,
    float state3,
    int timer)
  {
    Style = style;
    State0 = state0;
    State1 = state1;
    State2 = state2;
    State3 = state3;
    Timer = timer;
  }

  public int Style;
  public float State0;
  public float State1;
  public float State2;
  public float State3;
  public int Timer;
}
