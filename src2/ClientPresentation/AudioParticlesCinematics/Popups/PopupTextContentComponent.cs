namespace NLTX.ClientPresentation.AudioParticlesCinematics.Popups;

public sealed class PopupTextContentComponent
{
  public PopupTextContentComponent(
    string name,
    string displayText,
    long stack = 0,
    bool coinText = false,
    long coinValue = 0,
    int sonarText = -1,
    bool expert = false,
    bool master = false,
    bool sonar = false,
    int contextCode = 0,
    int npcNetId = -1,
    bool freeAdvanced = false)
  {
    ArgumentNullException.ThrowIfNull(name);
    ArgumentNullException.ThrowIfNull(displayText);

    Name = name;
    DisplayText = displayText;
    Stack = stack;
    CoinText = coinText;
    CoinValue = coinValue;
    SonarText = sonarText;
    Expert = expert;
    Master = master;
    Sonar = sonar;
    ContextCode = contextCode;
    NpcNetId = npcNetId;
    FreeAdvanced = freeAdvanced;
  }

  public string Name { get; }

  public string DisplayText { get; }

  public long Stack { get; }

  public bool CoinText { get; }

  public long CoinValue { get; }

  public int SonarText { get; }

  public bool Expert { get; }

  public bool Master { get; }

  public bool Sonar { get; }

  public int ContextCode { get; }

  public int NpcNetId { get; }

  public bool FreeAdvanced { get; }
}
