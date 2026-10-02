using System.Numerics;

namespace NLTX.PlayerInputGameplay.Presentation;

public readonly record struct CharacterPreviewSelectionSettings(
  int StartFrame,
  int FrameCount,
  int DelayPerFrame,
  bool BounceLoop);

public sealed class CharacterPreviewSettingsComponent
{
  public Vector2 Offset { get; private set; }

  public CharacterPreviewSelectionSettings Selected { get; private set; }

  public CharacterPreviewSelectionSettings Unselected { get; private set; }

  public int Direction { get; private set; } = 1;

  public void Set(
    Vector2 offset,
    CharacterPreviewSelectionSettings selected,
    CharacterPreviewSelectionSettings unselected,
    int direction)
  {
    Validate(selected);
    Validate(unselected);
    if (direction is not (-1 or 1))
    {
      throw new ArgumentOutOfRangeException(nameof(direction));
    }

    Offset = offset;
    Selected = selected;
    Unselected = unselected;
    Direction = direction;
  }

  private static void Validate(CharacterPreviewSelectionSettings settings)
  {
    if (settings.StartFrame < 0 || settings.FrameCount <= 0 || settings.DelayPerFrame < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(settings));
    }
  }
}
