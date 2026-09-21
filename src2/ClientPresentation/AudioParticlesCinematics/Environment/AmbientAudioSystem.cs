namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public sealed class AmbientAudioSystem
{
  public IReadOnlyList<AmbientAudioCommand> Update(
    AmbientAudioVisualsComponent state,
    AmbientAudioInput input)
  {
    ArgumentNullException.ThrowIfNull(state);
    IReadOnlyList<AmbientAudioCommand> commands = AmbientAudioQuery.CreateCommands(
      input,
      state.MinWind,
      state.MaxWind,
      state.MinRain,
      state.MaxRain);

    state.ShouldUseWindyDayMusic = input.Wind >= state.MaxWind;
    state.ShouldUseStormMusic = input.Rain >= state.MaxRain && input.Wind >= state.MinWind;
    state.WaterfallPosition = input.WaterfallPosition;
    state.WaterfallStrength = input.WaterfallStrength;
    state.LavafallPosition = input.LavafallPosition;
    state.LavafallStrength = input.LavafallStrength;
    state.LavaPosition = input.LavaPosition;
    state.LavaStrength = input.LavaStrength;
    state.IsWaterfallMusicPlaying = input.WaterfallStrength > 0;
    state.IsLavafallMusicPlaying = input.LavafallStrength > 0;
    state.AmbientCounter++;
    return commands;
  }
}
