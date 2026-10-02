namespace NLTX.ClientPresentation.MapCameraRendering;

public static class ShaderLookupQuery
{
  public static bool TryFind(
    ShaderRegistryComponent component,
    string key,
    out ShaderHandle handle)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.TryFind(key, out handle);
  }
}
