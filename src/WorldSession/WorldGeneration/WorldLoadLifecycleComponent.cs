using System;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldLoadLifecycleComponent
{
  public bool IsGeneratingOrLoadingWorld { get; private set; }

  public bool LoadFailed { get; private set; }

  public bool WorldCleared { get; private set; }

  public bool WorldBackup { get; private set; }

  public void SetLoadingOrGenerating(bool isGeneratingOrLoadingWorld)
  {
    IsGeneratingOrLoadingWorld = isGeneratingOrLoadingWorld;
  }

  public void SetLoadResult(bool loadFailed, bool worldBackup)
  {
    LoadFailed = loadFailed;
    WorldBackup = worldBackup;
  }

  public void MarkWorldCleared()
  {
    WorldCleared = true;
  }

  public void ResetWorldCleared()
  {
    WorldCleared = false;
  }
}
