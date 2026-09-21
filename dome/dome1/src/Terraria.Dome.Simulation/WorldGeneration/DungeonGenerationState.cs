using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class DungeonGenerationState
{
  private readonly IReadOnlyList<DungeonGenerationContext> _contexts;

  private DungeonGenerationState(
    int currentDungeon,
    IReadOnlyList<DungeonGenerationContext> contexts)
  {
    CurrentDungeon = currentDungeon;
    _contexts = contexts;
  }

  public static DungeonGenerationState Empty { get; } = new(
    0,
    Array.Empty<DungeonGenerationContext>());

  public int CurrentDungeon { get; }

  public IReadOnlyList<DungeonGenerationContext> Contexts => _contexts;

  public DungeonGenerationState SetUp(int currentDungeon, bool clearOld = false)
  {
    if (clearOld)
    {
      return new DungeonGenerationState(
        currentDungeon,
        new ReadOnlyCollection<DungeonGenerationContext>([new DungeonGenerationContext(0)]));
    }

    DungeonGenerationContext[] nextContexts = new DungeonGenerationContext[_contexts.Count + 1];
    for (int index = 0; index < _contexts.Count; index++)
    {
      nextContexts[index] = _contexts[index];
    }

    nextContexts[^1] = new DungeonGenerationContext(_contexts.Count);
    return new DungeonGenerationState(
      currentDungeon,
      new ReadOnlyCollection<DungeonGenerationContext>(nextContexts));
  }
}
