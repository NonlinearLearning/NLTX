using System.Numerics;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Audio;

public sealed class ActiveSoundVisualsComponent
{
  public ActiveSoundVisualsComponent(
    bool isGlobal,
    Vector2 position,
    float volume = 1f,
    float pitch = 0,
    int conditionCode = 0,
    bool isPlaying = false)
  {
    if (!float.IsFinite(volume) || volume < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(volume));
    }

    if (!float.IsFinite(pitch))
    {
      throw new ArgumentOutOfRangeException(nameof(pitch));
    }

    IsGlobal = isGlobal;
    Position = position;
    Volume = volume;
    Pitch = pitch;
    ConditionCode = conditionCode;
    IsPlaying = isPlaying;
  }

  public bool IsGlobal { get; }

  public Vector2 Position { get; internal set; }

  public float Volume { get; internal set; }

  public float Pitch { get; internal set; }

  public int ConditionCode { get; }

  public bool IsPlaying { get; internal set; }
}
