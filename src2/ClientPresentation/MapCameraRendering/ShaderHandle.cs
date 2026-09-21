namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct ShaderHandle(string Key, uint RegistryRevision)
{
  public bool IsValid => !string.IsNullOrWhiteSpace(Key) && RegistryRevision > 0;
}
