namespace NLTX.ClientPresentation.MapCameraRendering;

public interface ISpriteBatchSink
{
  void Submit(DrawCommand command);
}
