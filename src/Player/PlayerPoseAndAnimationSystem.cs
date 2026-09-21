namespace Terraria.Player;

public sealed class PlayerPoseAndAnimationSystem
{
  public PlayerPoseSnapshot Update(
    PlayerPoseAndAnimationStateComponent component,
    IPlayerPoseInputSnapshot input)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(input);

    component.ApplyInput(input);

    if (input.ResetPose)
    {
      component.ResetPose();
    }
    else if (input.AdvancePose)
    {
      component.AdvancePose();
    }

    return component.ToSnapshot(input.Tick);
  }

  public PlayerPoseSnapshot ResetForSpawn(
    PlayerPoseAndAnimationStateComponent component,
    SimulationTick tick)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetPose();
    return component.ToSnapshot(tick);
  }

  public PlayerPoseSnapshot ResetForTeleport(
    PlayerPoseAndAnimationStateComponent component,
    SimulationTick tick)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetPose();
    return component.ToSnapshot(tick);
  }
}
