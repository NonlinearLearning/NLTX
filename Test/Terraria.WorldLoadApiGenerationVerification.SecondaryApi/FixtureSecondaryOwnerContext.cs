using Terraria.WorldLoadApiGenerationVerification.Api;

namespace Terraria.WorldLoadApiGenerationVerification.SecondaryApi;

public sealed class FixtureSecondaryOwnerContext(FixtureLoadTrace trace)
{
  public FixtureLoadTrace Trace { get; } = trace;
}
