using Terraria.Player;

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

PlayerBuilderOverlayInput input = new(
  RulerGridEnabled: true,
  RulerLineEnabled: false);

PlayerBuilderOverlayProjection projection = BuilderOverlayQuery.Calculate(input);
Require(projection.RulerGrid, "The overlay must preserve the ruler-grid fact.");
Require(!projection.RulerLine, "The overlay must preserve the ruler-line fact.");

PlayerBuilderOverlayProjection repeatedProjection = BuilderOverlayQuery.Calculate(input);
Require(
  projection == repeatedProjection,
  "The overlay query must be deterministic for the same facts.");

PlayerBuilderOverlayProjection emptyProjection = BuilderOverlayQuery.Calculate(
  new PlayerBuilderOverlayInput(
    RulerGridEnabled: false,
    RulerLineEnabled: false));
Require(!emptyProjection.RulerGrid, "An absent ruler-grid fact must remain disabled.");
Require(!emptyProjection.RulerLine, "An absent ruler-line fact must remain disabled.");

Console.WriteLine("PASS: builder overlay projection is deterministic and one-way");
