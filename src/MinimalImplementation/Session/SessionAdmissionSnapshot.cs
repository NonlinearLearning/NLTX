using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Session;

public sealed record SessionAdmissionSnapshot(
  SessionEpoch Epoch,
  SessionStage Stage,
  PacketSequence? LastSequence,
  long Revision);
