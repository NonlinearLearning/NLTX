using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Session;

public sealed record SessionBinding(
  SessionEpoch Epoch,
  byte PlayerSlot,
  EntityHandle PlayerHandle);
