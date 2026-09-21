using System.Numerics;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public sealed class AmbientAudioVisualsComponent
{
  public bool ShouldUseWindyDayMusic { get; internal set; }

  public bool ShouldUseStormMusic { get; internal set; }

  public float MinWind { get; } = 0.34f;

  public float MaxWind { get; } = 0.4f;

  public float MinRain { get; } = 0.4f;

  public float MaxRain { get; } = 0.5f;

  public Vector2 WaterfallPosition { get; internal set; } = new(-1, -1);

  public float WaterfallStrength { get; internal set; }

  public Vector2 LavafallPosition { get; internal set; } = new(-1, -1);

  public float LavafallStrength { get; internal set; }

  public Vector2 LavaPosition { get; internal set; } = new(-1, -1);

  public float LavaStrength { get; internal set; }

  public int AmbientCounter { get; internal set; }

  public bool IsWaterfallMusicPlaying { get; internal set; }

  public bool IsLavafallMusicPlaying { get; internal set; }
}
