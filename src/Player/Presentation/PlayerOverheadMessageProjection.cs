namespace Terraria.Player.Presentation;

public sealed class PlayerOverheadMessageProjection
{
  public PlayerOverheadMessageSnapshot Project(
    PlayerOverheadMessageStateComponent state,
    PlayerCompositeArmSnapshot frontArm,
    PlayerCompositeArmSnapshot backArm)
  {
    ArgumentNullException.ThrowIfNull(state);
    return Project(state.ToInput(frontArm, backArm));
  }

  public PlayerOverheadMessageSnapshot Project(
    in PlayerOverheadMessageInput input)
  {
    return new PlayerOverheadMessageSnapshot(
      input.FrontArm,
      input.BackArm,
      input.ChatText,
      input.Snippets,
      input.MessageSize,
      input.TimeLeft,
      input.Color);
  }
}
