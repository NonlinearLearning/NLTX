using Terraria.WorldSession.Definitions;

namespace Terraria.WorldSession.Queries;

public static class DifficultyQuery
{
  public static float EffectiveDifficulty(
    bool hasActiveWorld,
    int gameMode,
    bool goodWorld,
    float? difficultyOverride)
  {
    if (!hasActiveWorld)
    {
      return DifficultyLevelDefinition.Classic;
    }

    float difficulty = DifficultyLevelDefinition.Classic;
    if (difficultyOverride.HasValue)
    {
      difficulty = difficultyOverride.Value;
    }
    else if (gameMode == 1)
    {
      difficulty = DifficultyLevelDefinition.Expert;
    }
    else if (gameMode == 2)
    {
      difficulty = DifficultyLevelDefinition.Master;
    }

    if (goodWorld)
    {
      difficulty += 1f;
    }

    return difficulty;
  }

  public static bool IsExpertOrAbove(float difficulty)
  {
    return difficulty >= DifficultyLevelDefinition.Expert;
  }

  public static bool IsMasterOrAbove(float difficulty)
  {
    return difficulty >= DifficultyLevelDefinition.Master;
  }
}
