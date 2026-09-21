namespace Terraria.UiItemLocalization.Achievements;

public sealed class AchievementEncounterContextComponent
{
  public bool MiningActive { get; private set; }

  public bool MechaMayhemEligible { get; private set; }

  public bool MechaMayhemFirstDown { get; private set; }

  public bool MechaMayhemSecondDown { get; private set; }

  public bool MechaMayhemThirdDown { get; private set; }

  public void SetMining(bool active)
  {
    MiningActive = active;
  }

  public void BeginMechaMayhem()
  {
    MechaMayhemEligible = true;
    MechaMayhemFirstDown = false;
    MechaMayhemSecondDown = false;
    MechaMayhemThirdDown = false;
  }

  public void MarkMechaMayhemDown(int index)
  {
    if (!MechaMayhemEligible)
    {
      return;
    }

    switch (index)
    {
      case 1:
        MechaMayhemFirstDown = true;
        break;
      case 2:
        MechaMayhemSecondDown = true;
        break;
      case 3:
        MechaMayhemThirdDown = true;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(index));
    }
  }

  public void Clear()
  {
    MiningActive = false;
    MechaMayhemEligible = false;
    MechaMayhemFirstDown = false;
    MechaMayhemSecondDown = false;
    MechaMayhemThirdDown = false;
  }
}
