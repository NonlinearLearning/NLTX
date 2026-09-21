using Terraria.ClientPresentation.Ui.Collections;

namespace Terraria.ClientPresentation.Ui.Systems;

public sealed class UiCollectionLayoutSystem
{
  private readonly UiCollectionProgressComponent _collection;

  public UiCollectionLayoutSystem(UiCollectionProgressComponent collection)
  {
    _collection = collection ?? throw new ArgumentNullException(nameof(collection));
  }

  public void Recalculate(float itemExtent, float viewSize)
  {
    float effectiveViewSize = _collection.RequestedViewSize > 0f
      ? _collection.RequestedViewSize
      : viewSize;
    _collection.RecalculateLayout(itemExtent, effectiveViewSize);
  }
}
