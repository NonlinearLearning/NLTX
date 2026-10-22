using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules;

public sealed record ModuleRecipient(SessionEpoch Epoch, byte PlayerSlot, byte Team);
