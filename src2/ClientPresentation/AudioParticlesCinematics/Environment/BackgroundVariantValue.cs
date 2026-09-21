using System.Collections.ObjectModel;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public sealed class BackgroundVariantValue
{
  public BackgroundVariantValue(IEnumerable<int> layerIds)
  {
    ArgumentNullException.ThrowIfNull(layerIds);
    int[] values = layerIds.ToArray();
    if (values.Length == 0)
    {
      throw new ArgumentException("A background variant needs at least one layer.", nameof(layerIds));
    }

    LayerIds = Array.AsReadOnly(values);
  }

  public ReadOnlyCollection<int> LayerIds { get; }
}
