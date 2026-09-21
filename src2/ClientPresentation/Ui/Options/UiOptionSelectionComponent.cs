using Terraria.ClientPresentation.Ui.Layout;
using Terraria.ClientPresentation.Ui.Text;
using Terraria.ClientPresentation.Ui.Tree;
using TextSource = Terraria.ClientPresentation.Ui.Text.UiTextPanelComponent.TextSource;

namespace Terraria.ClientPresentation.Ui.Options;

public sealed class UiOptionSelectionComponent
{
  public UiOptionSelectionComponent(
    UiElementId hostId,
    OptionToken optionValue,
    OptionToken initialSelection,
    uint color = 0xffffffff,
    uint borderColor = 0xffffffff,
    bool fadeFromBlack = false,
    bool innerHighlightRim = false,
    bool showHighlightWhenSelected = true,
    uint? overrideUnpickedColor = null,
    uint? overridePickedColor = null,
    TextSource description = default,
    UiElementId? titleId = null,
    IconProjection icon = default)
  {
    if (!hostId.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(hostId));
    }

    ValidateToken(optionValue, nameof(optionValue));
    ValidateToken(initialSelection, nameof(initialSelection));
    if (optionValue.GroupId != initialSelection.GroupId)
    {
      throw new ArgumentException(
        "The initial selection must belong to the option group.",
        nameof(initialSelection));
    }

    HostId = hostId;
    OptionValue = optionValue;
    CurrentSelection = initialSelection;
    Color = color;
    BorderColor = borderColor;
    FadeFromBlack = fadeFromBlack;
    InnerHighlightRim = innerHighlightRim;
    ShowHighlightWhenSelected = showHighlightWhenSelected;
    OverrideUnpickedColor = overrideUnpickedColor;
    OverridePickedColor = overridePickedColor;
    Description = description;
    TitleId = titleId;
    Icon = icon;
  }

  public UiElementId HostId { get; }

  public OptionToken OptionValue { get; }

  public OptionToken CurrentSelection { get; private set; }

  public bool IsSelected => OptionValue == CurrentSelection;

  public uint Color { get; }

  public uint BorderColor { get; }

  public bool FadeFromBlack { get; }

  public bool InnerHighlightRim { get; }

  public bool ShowHighlightWhenSelected { get; }

  public uint? OverrideUnpickedColor { get; }

  public uint? OverridePickedColor { get; }

  public TextSource Description { get; }

  public UiElementId? TitleId { get; }

  public IconProjection Icon { get; }

  internal void SetCurrentSelection(OptionToken selection)
  {
    CurrentSelection = selection;
  }

  private static void ValidateToken(OptionToken token, string parameterName)
  {
    if (!token.IsValid)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }

  public readonly record struct OptionToken(int GroupId, int Value)
  {
    public static OptionToken Invalid => new(0, 0);

    public bool IsValid => GroupId > 0;
  }

  public readonly record struct IconProjection(
    bool HasIcon,
    int Frame,
    float Scale,
    UiVector2 Offset,
    uint Color)
  {
    public static IconProjection None => new(
      false,
      0,
      1f,
      UiVector2.Zero,
      0xffffffff);
  }
}
