namespace NLTX.PlayerInputGameplay.Input;

public sealed class RuntimeAllocationSettingsAdapter
{
  public bool NoPooling { get; private set; }

  public bool CollectGenerationZeroEveryFrame { get; private set; }

  public void Set(bool noPooling, bool collectGenerationZeroEveryFrame)
  {
    NoPooling = noPooling;
    CollectGenerationZeroEveryFrame = collectGenerationZeroEveryFrame;
  }
}
