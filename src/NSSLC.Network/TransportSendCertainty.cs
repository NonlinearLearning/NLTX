using Terraria.Communication;

namespace Terraria.Network;

public enum TransportSendCertainty
{
  NotSubmitted,
  Queued,
  LocallySent,
  OutcomeUnknown,
  Failed
}
