namespace Terraria.Player.Presentation;

public sealed class PlayerCompositeArmPresentationSystem
{
  public PlayerCompositeArmsSnapshot SetFrontArm(
    PlayerCompositeArmStateComponent component,
    bool isEnabled,
    PlayerArmStretchAmount stretch,
    float rotation,
    float gravityDirection)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.SetFrontArm(CreateArm(isEnabled, stretch, rotation, gravityDirection));
    return component.ToSnapshot();
  }

  public PlayerCompositeArmsSnapshot SetBackArm(
    PlayerCompositeArmStateComponent component,
    bool isEnabled,
    PlayerArmStretchAmount stretch,
    float rotation,
    float gravityDirection)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.SetBackArm(CreateArm(isEnabled, stretch, rotation, gravityDirection));
    return component.ToSnapshot();
  }

  public PlayerCompositeArmsSnapshot ResetForLifecycle(
    PlayerCompositeArmStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Reset();
    return component.ToSnapshot();
  }

  private static PlayerCompositeArmSnapshot CreateArm(
    bool isEnabled,
    PlayerArmStretchAmount stretch,
    float rotation,
    float gravityDirection)
  {
    if (gravityDirection == -1f)
    {
      rotation = 0f - rotation;
    }

    return new PlayerCompositeArmSnapshot(isEnabled, stretch, rotation);
  }
}
