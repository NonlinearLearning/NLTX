using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Projections;

namespace Terraria.WorldGeneration.Host;

public sealed class WorldGenerationHostRuntimeSystem
{
  private readonly IGraphicsGenerationPort _graphics;
  private readonly IGenerationClockPort _clock;
  private readonly IProgressSink _progressSink;
  private readonly IAmbientWindGenerationPort _ambientWind;
  private readonly IChumBucketProjectilePort _chumBucketProjectile;

  public WorldGenerationHostRuntimeSystem(
    IGraphicsGenerationPort graphics,
    IGenerationClockPort clock,
    IProgressSink progressSink,
    IAmbientWindGenerationPort ambientWind,
    IChumBucketProjectilePort chumBucketProjectile)
  {
    _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
    _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    _progressSink = progressSink ?? throw new ArgumentNullException(nameof(progressSink));
    _ambientWind = ambientWind ?? throw new ArgumentNullException(nameof(ambientWind));
    _chumBucketProjectile =
      chumBucketProjectile ?? throw new ArgumentNullException(nameof(chumBucketProjectile));
  }

  public TimeSpan Tick(
    WorldGenerationHostRuntime host,
    string passId,
    string? message = null)
  {
    ArgumentNullException.ThrowIfNull(host);
    ArgumentException.ThrowIfNullOrWhiteSpace(passId);
    if (host.Phase != WorldGenerationHostPhase.Generating)
    {
      throw new InvalidOperationException("The host must be generating before it can tick.");
    }

    TimeSpan elapsed = _clock.ReadElapsedTime();
    if (elapsed < TimeSpan.Zero)
    {
      throw new InvalidOperationException("The generation clock returned a negative duration.");
    }

    host.AdvanceGameUpdateCount();
    _graphics.Present(CreateGraphicsSnapshot(host));
    _progressSink.Publish(new WorldGenerationProgressProjection(
      host.GameUpdateCount,
      passId,
      message ?? host.ProgressMessage,
      host.Progress,
      host.Phase));
    _ambientWind.Update();
    _chumBucketProjectile.Update();
    return elapsed;
  }

  private static WorldGenerationGraphicsSnapshot CreateGraphicsSnapshot(
    WorldGenerationHostRuntime host)
  {
    return new WorldGenerationGraphicsSnapshot(
      host.FavoriteColor,
      host.MapEnabled,
      host.IsEnginePreloaded,
      host.SkipAssemblyLoad,
      host.RenderCount,
      host.ShimmerAlpha,
      host.ShimmerDarken,
      host.AfterPartyOfDoom);
  }
}
