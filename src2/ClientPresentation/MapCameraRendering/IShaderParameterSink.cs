namespace NLTX.ClientPresentation.MapCameraRendering;

public interface IShaderParameterSink
{
  void Apply(
    ShaderHandle handle,
    IReadOnlyDictionary<string, ShaderParameterValue> parameters,
    IReadOnlyDictionary<string, ShaderParameterValue> familyParameters);
}
