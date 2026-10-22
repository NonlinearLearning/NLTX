namespace MinimalImplementation.Contracts;

public enum ProtocolErrorCode
{
  None,
  MalformedFrame,
  InvalidLength,
  UnknownPacket,
  UnknownModule,
  UnknownSubtype,
  WrongStage,
  Forbidden,
  StaleEpoch,
  StaleEntity,
  Conflict,
  RateLimited,
  TransportClosed,
  TransportFailure,
  NotReady
}
