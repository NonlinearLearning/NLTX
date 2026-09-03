using System;
using Terraria.Dome.Simulation.Npc.Systems;

if (NpcDifficultyOverridePolicy.Resolve(1.5f, null) != 1.5f ||
    NpcDifficultyOverridePolicy.Resolve(1.5f, 2.0f) != 2.0f)
{
  throw new InvalidOperationException("NPC difficulty override resolution diverged.");
}

AssertThrows(() => NpcDifficultyOverridePolicy.Resolve(0.0f, null));
AssertThrows(() => NpcDifficultyOverridePolicy.Resolve(1.0f, float.NaN));
Console.WriteLine("PASS: NPC difficulty override nullable resolution");
Console.WriteLine("DEFERRED: legacy override lifecycle and runtime consumer remain outside this slice");

static void AssertThrows(Func<float> action)
{
  try
  {
    _ = action();
  }
  catch (ArgumentOutOfRangeException)
  {
    return;
  }

  throw new InvalidOperationException("Expected invalid difficulty multiplier to be rejected.");
}
