namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct SpriteFrameSnapshot(
  int PaddingX,
  int PaddingY,
  byte CurrentColumn,
  byte CurrentRow,
  byte ColumnCount,
  byte RowCount);
