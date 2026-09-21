using Terraria.UiItemLocalization.Achievements;

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

AchievementDefinitionCatalog definitions = new();
AchievementDefinition definition = new(
  new LocalAchievementId("achievement.test"),
  "achievement.test",
  "achievement.test.name",
  "achievement.test.description",
  7,
  [new AchievementConditionDefinition("progress", 10f, "float")]);
Require(definitions.Register(definition), "The achievement definition was not registered.");

AchievementProgressSystem progressSystem = new();
AchievementProgressState progress = new(10f, "progress", "float");
AchievementProgressCommand command = new(
  "event-1",
  new PlayerEntityId(3),
  definition.Id,
  "progress",
  12f);
AchievementProgressUpdateResult update = progressSystem.Apply(progress, definition, command);
Require(update.Accepted && update.Completed && progress.CurrentValue == 10f,
  "Achievement progress did not clamp and complete.");
Require(!progressSystem.Apply(progress, definition, command).Accepted,
  "Duplicate achievement events were not rejected.");

Console.WriteLine("C01 verifier passed.");
