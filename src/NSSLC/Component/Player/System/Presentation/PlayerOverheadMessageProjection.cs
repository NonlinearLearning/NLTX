namespace Terraria.Player.Presentation;

public sealed class PlayerOverheadMessageProjection
{
  public PlayerOverheadMessageSnapshot Project(
    PlayerOverheadMessageStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return Project(state.ToInput());
  }

  public PlayerOverheadMessageSnapshot Project(
    in PlayerOverheadMessageInput input)
  {
    return new PlayerOverheadMessageSnapshot(
      input.ChatText,
      input.Snippets,
      input.MessageSize,
      input.TimeLeft,
      input.Color);
  }
}
