namespace NSSLC.Infrastructure.Network;

public sealed record ReconnectRunResult(long Attempts, string Reason, Exception? Error = null);
