namespace NSSLC.Infrastructure.Network;

public sealed record PacketGatewayOptions {
  public int MaximumSessions { get; init; } = 256;
  public int MaximumDispatchesPerMessage { get; init; } = 128;
  public TimeSpan StageTimeout { get; init; } = TimeSpan.FromSeconds(10);
  public TimeSpan HandshakeTimeout { get; init; } = TimeSpan.FromSeconds(30);
  public TimeSpan SynchronizationTimeout { get; init; } = TimeSpan.FromSeconds(60);
  public TimeSpan ActiveIdleTimeout { get; init; } = TimeSpan.FromSeconds(60);
  public TimeSpan OwnerTimeout { get; init; } = TimeSpan.FromSeconds(10);
  public TimeSpan CleanupTimeout { get; init; } = TimeSpan.FromSeconds(5);
  public TimeSpan RateWindow { get; init; } = TimeSpan.FromSeconds(1);
  public bool IgnoreClientVersion { get; init; }
  public bool UseSteamModuleIds { get; init; }
  public bool EnableHostAuthorization { get; init; }
  public bool EnablePing { get; init; }

  internal void Validate() {
    if (MaximumSessions < 1 || MaximumDispatchesPerMessage < 1
        || StageTimeout <= TimeSpan.Zero || HandshakeTimeout <= TimeSpan.Zero
        || SynchronizationTimeout <= TimeSpan.Zero || ActiveIdleTimeout <= TimeSpan.Zero
        || OwnerTimeout <= TimeSpan.Zero || CleanupTimeout <= TimeSpan.Zero
        || RateWindow <= TimeSpan.Zero) {
      throw new ArgumentOutOfRangeException(nameof(PacketGatewayOptions));
    }
  }
}
