namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public sealed class BackgroundVariantCatalogValue
{
  public BackgroundVariantCatalogValue(
    BackgroundVariantValue pure,
    BackgroundVariantValue corrupt,
    BackgroundVariantValue crimson,
    BackgroundVariantValue hallow)
  {
    Pure = pure ?? throw new ArgumentNullException(nameof(pure));
    Corrupt = corrupt ?? throw new ArgumentNullException(nameof(corrupt));
    Crimson = crimson ?? throw new ArgumentNullException(nameof(crimson));
    Hallow = hallow ?? throw new ArgumentNullException(nameof(hallow));
  }

  public BackgroundVariantValue Pure { get; }

  public BackgroundVariantValue Corrupt { get; }

  public BackgroundVariantValue Crimson { get; }

  public BackgroundVariantValue Hallow { get; }
}
