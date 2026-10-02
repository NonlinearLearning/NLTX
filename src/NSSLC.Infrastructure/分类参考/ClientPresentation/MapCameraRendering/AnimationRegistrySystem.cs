namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class AnimationRegistrySystem
{
  public void QueueAdd(
    TileAnimationRegistryComponent component,
    TileAnimationDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.QueueAdd(definition);
  }

  public void QueueRemove(
    TileAnimationRegistryComponent component,
    TileAnimationKey key)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.QueueRemove(key);
  }

  public void Commit(TileAnimationRegistryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Commit();
  }
}
