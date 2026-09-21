using System.Numerics;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public static class AmbientAudioQuery
{
  public static IReadOnlyList<AmbientAudioCommand> CreateCommands(
    AmbientAudioInput input,
    float minWind = 0.34f,
    float maxWind = 0.4f,
    float minRain = 0.4f,
    float maxRain = 0.5f)
  {
    ValidateThresholds(minWind, maxWind, minRain, maxRain);
    List<AmbientAudioCommand> commands = new();

    if (input.Wind >= maxWind)
    {
      commands.Add(new AmbientAudioCommand(AmbientAudioKind.Wind, Vector2.Zero, input.Wind));
    }

    if (input.Rain >= maxRain && input.Wind >= minWind)
    {
      commands.Add(new AmbientAudioCommand(AmbientAudioKind.Storm, Vector2.Zero, input.Rain));
    }

    AddIfActive(commands, AmbientAudioKind.Waterfall, input.WaterfallPosition, input.WaterfallStrength);
    AddIfActive(commands, AmbientAudioKind.Lavafall, input.LavafallPosition, input.LavafallStrength);
    AddIfActive(commands, AmbientAudioKind.Lava, input.LavaPosition, input.LavaStrength);
    return commands;
  }

  private static void AddIfActive(
    ICollection<AmbientAudioCommand> commands,
    AmbientAudioKind kind,
    Vector2 position,
    float strength)
  {
    if (strength > 0)
    {
      commands.Add(new AmbientAudioCommand(kind, position, strength));
    }
  }

  private static void ValidateThresholds(float minWind, float maxWind, float minRain, float maxRain)
  {
    if (minWind < 0 || maxWind < minWind || minRain < 0 || maxRain < minRain)
    {
      throw new ArgumentOutOfRangeException(nameof(minWind));
    }
  }
}
