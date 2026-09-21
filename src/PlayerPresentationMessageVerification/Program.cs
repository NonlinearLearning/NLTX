using System.Numerics;
using Terraria.Player;
using Terraria.Player.Presentation;

List<string> snippets = new() { "hello", "world" };
PlayerOverheadMessageInput input = new(
  FrontArm: new PlayerCompositeArmSnapshot(
    IsEnabled: true,
    Stretch: PlayerArmStretchAmount.ThreeQuarters,
    Rotation: 0.25f),
  BackArm: new PlayerCompositeArmSnapshot(
    IsEnabled: false,
    Stretch: PlayerArmStretchAmount.None,
    Rotation: -0.5f),
  ChatText: "hello world",
  Snippets: snippets,
  MessageSize: new Vector2(120.0f, 24.0f),
  TimeLeft: 10,
  Color: new PlayerAppearanceColor(10, 20, 30));

PlayerOverheadMessageSnapshot snapshot =
  new PlayerOverheadMessageProjection().Project(input);

Require(snapshot.FrontArm == input.FrontArm, "The front arm snapshot must be projected by value.");
Require(snapshot.BackArm == input.BackArm, "The back arm snapshot must be projected by value.");
Require(snapshot.ChatText == input.ChatText, "Chat text must be preserved.");
Require(snapshot.MessageSize == input.MessageSize, "Message size must be preserved.");
Require(snapshot.TimeLeft == input.TimeLeft, "Message expiry ticks must be preserved.");
Require(snapshot.Color == input.Color, "Message color must be preserved.");
Require(snapshot.IsVisible, "A message with remaining time must be visible.");
Require(snapshot.Snippets.SequenceEqual(snippets), "Snippet text must be preserved in order.");

snippets[0] = "mutated after projection";
Require(
  snapshot.Snippets[0] == "hello",
  "The projected snippet list must not alias the input list.");

snippets[0] = "hello";
PlayerOverheadMessageStateComponent state = new();
PlayerOverheadMessageStateSystem stateSystem = new();
stateSystem.ReplaceMessage(state, input);
Require(state.ChatText == input.ChatText, "The state must retain the replacement text.");
Require(state.Snippets.SequenceEqual(new[] { "hello", "world" }),
  "The state must retain snippet order.");
Require(state.MessageSize == input.MessageSize, "The state must retain message size.");
Require(state.TimeLeft == input.TimeLeft, "The state must retain expiry ticks.");
Require(state.Color == input.Color, "The state must retain message color.");

snippets[0] = "mutated after state replacement";
Require(
  state.Snippets[0] == "hello",
  "The state must not alias the replacement input list.");

PlayerOverheadMessageSnapshot stateSnapshot =
  new PlayerOverheadMessageProjection().Project(state, input.FrontArm, input.BackArm);
Require(stateSnapshot.IsVisible, "A live state message must project as visible.");
Require(stateSystem.AdvancePresentationTick(state), "The first presentation tick must advance time.");
Require(state.TimeLeft == 9, "A presentation tick must decrement timeLeft exactly once.");

for (int tick = 0; tick < 9; tick++)
{
  stateSystem.AdvancePresentationTick(state);
}

Require(state.TimeLeft == 0, "The message must expire at zero remaining ticks.");
Require(
  !stateSystem.AdvancePresentationTick(state),
  "An expired message must not continue decrementing.");
Require(
  !new PlayerOverheadMessageProjection().Project(state, input.FrontArm, input.BackArm).IsVisible,
  "An expired state message must project as invisible.");

PlayerOverheadMessageSnapshot expired =
  new PlayerOverheadMessageProjection().Project(input with { TimeLeft = 0 });
Require(!expired.IsVisible, "An expired message must not be visible.");

PlayerOverheadMessageSnapshot replaced =
  new PlayerOverheadMessageProjection().Project(
    input with
    {
      ChatText = "replacement",
      Snippets = new[] { "replacement" },
      TimeLeft = 5,
    });
Require(replaced.ChatText == "replacement", "A new projection must replace the previous message.");
Require(replaced.TimeLeft == 5, "A replacement message must use its own expiry.");

PlayerOverheadMessageInput replacementInput = input with
{
  ChatText = "replacement",
  Snippets = new[] { "replacement" },
  TimeLeft = 5,
};
stateSystem.ReplaceMessage(state, replacementInput);
Require(state.ChatText == "replacement", "A state replacement must replace the old text.");
Require(state.TimeLeft == 5, "A state replacement must use its own expiry.");
stateSystem.ResetForSpawn(state);
Require(state.TimeLeft == 0, "Spawn reset must clear the message timer.");
Require(state.Snippets.Count == 0, "Spawn reset must clear snippets.");
Require(state.ChatText == string.Empty, "Spawn reset must clear chat text.");

stateSystem.ReplaceMessage(state, replacementInput);
stateSystem.ResetForRemoval(state);
Require(state.TimeLeft == 0, "Removal reset must clear the message timer.");
Require(state.Snippets.Count == 0, "Removal reset must clear snippets.");

Console.WriteLine("PASS: player overhead message state and arm projection");

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
