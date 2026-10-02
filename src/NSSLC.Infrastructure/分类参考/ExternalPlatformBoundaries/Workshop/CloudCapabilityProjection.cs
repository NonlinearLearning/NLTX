namespace Terraria.ExternalPlatformBoundaries.Workshop;

public static class CloudCapabilityProjection
{
  public static CloudCapabilitySnapshot FromProviderFlag(bool enabledByDefault)
  {
    return new CloudCapabilitySnapshot(enabledByDefault);
  }
}
