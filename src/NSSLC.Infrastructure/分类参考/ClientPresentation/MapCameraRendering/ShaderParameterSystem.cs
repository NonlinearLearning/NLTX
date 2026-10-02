namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class ShaderParameterSystem
{
  public void Stage(
    ShaderParameterComponent component,
    string name,
    ShaderParameterValue value)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Stage(name, value);
  }

  public void StageFamily(
    ShaderFamilyParameterComponent component,
    string name,
    ShaderParameterValue value)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Stage(name, value);
  }

  public void SetDisabled(ShaderParameterComponent component, bool disabled)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.SetDisabled(disabled);
  }
}
