namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class ShaderRegistryBuildSystem
{
  public ShaderHandle Register(
    ShaderRegistryComponent component,
    ShaderDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.Register(definition);
  }

  public void Rebuild(
    ShaderRegistryComponent component,
    uint contentRevision,
    IReadOnlyList<ShaderDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Rebuild(contentRevision, definitions);
  }
}
