using Terraria.ClientPresentation.Ui.Text;

namespace Terraria.ClientPresentation.Ui.Screens;

public sealed class UiWorldLoadProjection
{
  public bool TryProject(
    float overallProgress,
    float currentProgress,
    string message,
    out Projection projection)
  {
    if (!float.IsFinite(overallProgress)
      || !float.IsFinite(currentProgress)
      || overallProgress is < 0f or > 1f
      || currentProgress is < 0f or > 1f
      || string.IsNullOrWhiteSpace(message))
    {
      projection = default;
      return false;
    }

    projection = new Projection(
      overallProgress,
      currentProgress,
      UiTextPanelComponent.TextSource.FromLiteral(message));
    return true;
  }

  public readonly record struct Projection(
    float OverallProgress,
    float CurrentProgress,
    UiTextPanelComponent.TextSource Message);
}
