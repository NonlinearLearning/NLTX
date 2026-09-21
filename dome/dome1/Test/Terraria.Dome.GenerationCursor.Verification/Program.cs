using Terraria.Dome.Simulation.WorldGeneration;

static void AssertThrows<TException>(Action action) where TException : Exception
{
  try
  {
    action();
  }
  catch (TException)
  {
    return;
  }

  throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
}

GenerationCursorComponent initial = new(WorldGenerationStage.Created, 0, 0, 7);
GenerationCursorComponent sameStage = initial.Advance(WorldGenerationStage.Created, 0, 1, 8);
if (sameStage.SectionY != 1 || sameStage.RandomState != 8)
{
  throw new InvalidOperationException("A cursor did not advance within a stage.");
}

AssertThrows<ArgumentOutOfRangeException>(() =>
  new GenerationCursorComponent(WorldGenerationStage.Created, -1, 0, 0));
AssertThrows<ArgumentOutOfRangeException>(() =>
  new GenerationCursorComponent(WorldGenerationStage.Created, 0, -1, 0));
AssertThrows<ArgumentOutOfRangeException>(() =>
  new GenerationCursorComponent((WorldGenerationStage)999, 0, 0, 0));
AssertThrows<ArgumentException>(() =>
  sameStage.Advance(WorldGenerationStage.Created, 0, 0, 9));
GenerationCursorComponent advancedX =
  sameStage.Advance(WorldGenerationStage.Created, 1, 0, 9);
AssertThrows<ArgumentException>(() =>
  advancedX.Advance(WorldGenerationStage.Created, 0, 2, 10));

GenerationCursorComponent nextStage = sameStage.Advance(WorldGenerationStage.Terrain, 0, 0, 10);
if (nextStage.Stage != WorldGenerationStage.Terrain ||
    nextStage.SectionX != 0 ||
    nextStage.SectionY != 0)
{
  throw new InvalidOperationException("A stage transition could not reset its section cursor.");
}

Console.WriteLine("PASS: generation cursor validates coordinates and monotonic section progress");
