namespace NLTX.ClientPresentation.MapCameraRendering;

public enum MapPersistenceFailureKind
{
  None,
  InvalidInput,
  LockBusy,
  DirectoryMissing,
  IoFailure,
  CorruptDocument,
  UnsupportedVersion,
  CodecFailure
}
