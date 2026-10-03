using Terraria.WorldLoadApiGenerationVerification.Api;
using Terraria.WorldStorage;

namespace Terraria.WorldLoadApiGenerationVerification.SecondaryApi;

[WorldLoadApi(
  "fixture.secondary",
  "fixture.secondary-owner",
  "fixture.optional-section",
  1,
  1,
  WorldLoadSectionRequirement.Optional,
  "fixture.state")]
public sealed class FixtureSecondaryWorldLoadApi :
  IWorldLoadApi<FixtureSecondaryOwnerContext, FixtureOptionalSection, FixtureSecondaryPrepared>
{
  public WorldLoadPrepareResult<FixtureSecondaryPrepared> PrepareLoad(
    FixtureSecondaryOwnerContext ownerContext,
    WorldLoadSection<FixtureOptionalSection> section)
  {
    ownerContext.Trace.Record("prepare:fixture.secondary");
    return WorldLoadPrepareResult<FixtureSecondaryPrepared>.Prepared(
      new FixtureSecondaryPrepared(section.IsPresent));
  }

  public WorldLoadCommitResult CommitLoad(
    FixtureSecondaryOwnerContext ownerContext,
    in FixtureSecondaryPrepared preparedData)
  {
    if (preparedData.IsPresent)
    {
      return WorldLoadCommitResult.Rejected(
        WorldLoadApiFailure.Create(
          "OptionalSectionShouldBeAbsent",
          "The fixture optional section should be absent."));
    }

    if (ownerContext.Trace.Events[^1] != "commit:fixture.state")
    {
      return WorldLoadCommitResult.Rejected(
        WorldLoadApiFailure.Create(
          "CommitOrderViolation",
          "The dependent API was committed before fixture.state."));
    }

    ownerContext.Trace.Record("commit:fixture.secondary");
    return WorldLoadCommitResult.Committed();
  }

  public void DiscardPrepared(
    FixtureSecondaryOwnerContext ownerContext,
    in FixtureSecondaryPrepared preparedData)
  {
  }
}
