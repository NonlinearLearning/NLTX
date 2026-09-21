namespace NLTX.ClientPresentation.AudioParticlesCinematics.Audio;

public sealed class AudioDefinitionCatalogComponent
{
  public AudioDefinitionCatalogComponent(
    int styleCode,
    int variationCount,
    int soundId,
    float volume = 1f,
    float pitchVariance = 0f,
    int typeCode = 0,
    bool isTrackable = false,
    int maxTrackedInstances = 0)
  {
    if (variationCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(variationCount));
    }

    if (!float.IsFinite(volume) || volume < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(volume));
    }

    if (!float.IsFinite(pitchVariance) || pitchVariance < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(pitchVariance));
    }

    if (maxTrackedInstances < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxTrackedInstances));
    }

    StyleCode = styleCode;
    VariationCount = variationCount;
    SoundId = soundId;
    Volume = volume;
    PitchVariance = pitchVariance;
    TypeCode = typeCode;
    IsTrackable = isTrackable;
    MaxTrackedInstances = maxTrackedInstances;
  }

  public int StyleCode { get; }

  public int VariationCount { get; }

  public int SoundId { get; }

  public float Volume { get; }

  public float PitchVariance { get; }

  public int TypeCode { get; }

  public bool IsTrackable { get; }

  public int MaxTrackedInstances { get; }
}
