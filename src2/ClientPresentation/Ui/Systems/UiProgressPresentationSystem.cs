using Terraria.ClientPresentation.Ui.Collections;

namespace Terraria.ClientPresentation.Ui.Systems;

public sealed class UiProgressPresentationSystem
{
  private readonly UiCollectionProgressComponent _collection;

  public UiProgressPresentationSystem(UiCollectionProgressComponent collection)
  {
    _collection = collection ?? throw new ArgumentNullException(nameof(collection));
  }

  public bool ApplySample(float overallProgress, float currentProgress)
  {
    return _collection.ApplyProgress(overallProgress, currentProgress);
  }
}
