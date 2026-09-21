using System.Collections.ObjectModel;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public static class ParticleDrawProjection
{
  public static IReadOnlyList<StarVisualState> Project(StarVisualsComponent stars)
  {
    ArgumentNullException.ThrowIfNull(stars);
    return new ReadOnlyCollection<StarVisualState>(
      stars.Entries.Select(entry => entry.Value).ToList());
  }

  public static IReadOnlyList<CloudVisualState> Project(CloudVisualsComponent clouds)
  {
    ArgumentNullException.ThrowIfNull(clouds);
    return new ReadOnlyCollection<CloudVisualState>(
      clouds.Entries.Select(entry => entry.Value).ToList());
  }

  public static IReadOnlyList<RainVisualState> Project(RainVisualsComponent rain)
  {
    ArgumentNullException.ThrowIfNull(rain);
    return new ReadOnlyCollection<RainVisualState>(
      rain.Entries.Select(entry => entry.Value).ToList());
  }

  public static IReadOnlyList<DustVisualState> Project(DustVisualsComponent dust)
  {
    ArgumentNullException.ThrowIfNull(dust);
    return new ReadOnlyCollection<DustVisualState>(
      dust.Entries.Select(entry => entry.Value).ToList());
  }

  public static IReadOnlyList<GoreVisualState> Project(GoreVisualsComponent gore)
  {
    ArgumentNullException.ThrowIfNull(gore);
    return new ReadOnlyCollection<GoreVisualState>(
      gore.Entries.Select(entry => entry.Value).ToList());
  }
}
