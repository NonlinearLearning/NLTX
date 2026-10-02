namespace Terraria.WorldGeneration.Host;

public sealed class WorldGenerationHostRuntime
{
  private const float MaxProgress = 1f;

  public WorldGenerationColor FavoriteColor { get; private set; } =
    new(255, 231, 69);

  public bool MapEnabled { get; private set; } = true;

  public bool IsEnginePreloaded { get; private set; }

  public uint GameUpdateCount { get; private set; }

  public bool SkipAssemblyLoad { get; private set; }

  public int RenderCount { get; private set; } = 99;

  public float Progress { get; private set; }

  public string? ProgressMessage { get; private set; }

  public float ShimmerAlpha { get; private set; }

  public float ShimmerDarken { get; private set; }

  public bool AfterPartyOfDoom { get; private set; }

  public WorldGenerationHostPhase Phase { get; private set; }

  public void MarkEnginePreloaded()
  {
    IsEnginePreloaded = true;
  }

  public void SetFavoriteColor(WorldGenerationColor color)
  {
    FavoriteColor = color;
  }

  public void SetMapEnabled(bool enabled)
  {
    MapEnabled = enabled;
  }

  public void SetSkipAssemblyLoad(bool skip)
  {
    SkipAssemblyLoad = skip;
  }

  public void SetRenderCount(int renderCount)
  {
    if (renderCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(renderCount));
    }

    RenderCount = renderCount;
  }

  public void SetShimmer(float alpha, float darken)
  {
    ValidateUnitInterval(alpha, nameof(alpha));
    ValidateUnitInterval(darken, nameof(darken));
    ShimmerAlpha = alpha;
    ShimmerDarken = darken;
  }

  public void SetAfterPartyOfDoom(bool enabled)
  {
    AfterPartyOfDoom = enabled;
  }

  public void BeginGeneration()
  {
    if (Phase == WorldGenerationHostPhase.Generating)
    {
      throw new InvalidOperationException("World generation is already active.");
    }

    Progress = 0f;
    ProgressMessage = null;
    Phase = WorldGenerationHostPhase.Generating;
  }

  public void SetProgress(float progress, string? message = null)
  {
    if (Phase != WorldGenerationHostPhase.Generating)
    {
      throw new InvalidOperationException(
        "Progress can only be changed during world generation.");
    }

    ValidateUnitInterval(progress, nameof(progress));
    if (progress < Progress)
    {
      throw new InvalidOperationException(
        "World generation progress cannot move backwards.");
    }

    Progress = progress;
    ProgressMessage = message;
  }

  public void CompleteGeneration()
  {
    if (Phase != WorldGenerationHostPhase.Generating)
    {
      throw new InvalidOperationException(
        "Only an active generation can be completed.");
    }

    Progress = MaxProgress;
    Phase = WorldGenerationHostPhase.Completed;
  }

  public void FailGeneration(string? message = null)
  {
    if (Phase != WorldGenerationHostPhase.Generating)
    {
      throw new InvalidOperationException(
        "Only an active generation can fail.");
    }

    ProgressMessage = message;
    Phase = WorldGenerationHostPhase.Failed;
  }

  internal void AdvanceGameUpdateCount()
  {
    checked
    {
      GameUpdateCount++;
    }
  }

  private static void ValidateUnitInterval(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0f || value > MaxProgress)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
