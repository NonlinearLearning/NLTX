using Terraria.ClientPresentation.Ui.World;

namespace Terraria.ClientPresentation.Ui.Systems;

public sealed class UiWiresInteractionSystem
{
  private readonly WorldInteractionVisualsComponent _visuals;

  public UiWiresInteractionSystem(WorldInteractionVisualsComponent visuals)
  {
    _visuals = visuals ?? throw new ArgumentNullException(nameof(visuals));
  }

  public bool OpenRadial()
  {
    _visuals.SetRadial(true);
    return true;
  }

  public bool CloseRadial()
  {
    _visuals.SetRadial(false);
    return true;
  }

  public bool SelectTool(WorldInteractionVisualsComponent.ToolMode tool)
  {
    return _visuals.SetTool(tool);
  }

  public bool TryCreateIntent(out WiringIntent intent)
  {
    if (!_visuals.RadialOpen
      || _visuals.SelectedTool
        == WorldInteractionVisualsComponent.ToolMode.None)
    {
      intent = default;
      return false;
    }

    intent = new WiringIntent(_visuals.SelectedTool);
    return true;
  }

  public readonly record struct WiringIntent(
    WorldInteractionVisualsComponent.ToolMode Tool);
}
