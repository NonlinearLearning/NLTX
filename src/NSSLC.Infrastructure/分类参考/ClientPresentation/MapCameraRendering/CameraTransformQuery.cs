using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public static class CameraTransformQuery
{
  public static CameraTransformSnapshot Read(
    CameraUiTransformStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return new CameraTransformSnapshot(
      state.CameraPosition,
      state.CameraSize,
      state.CameraPosition * state.Zoom,
      state.CameraSize * state.Zoom,
      state.Translation + state.Pan,
      state.TransformationMatrix,
      state.Revision);
  }
}
