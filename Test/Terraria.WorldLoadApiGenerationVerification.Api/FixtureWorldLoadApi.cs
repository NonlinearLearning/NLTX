using Terraria.WorldStorage;

namespace Terraria.WorldLoadApiGenerationVerification.Api;

public sealed record FixtureSection(int Value);

public sealed record FixturePrepared(int Value);

public sealed class FixtureLoadState
{
  public FixtureLoadTrace Trace { get; } = new();

  public int Value { get; private set; }

  public void SetValue(int value)
  {
    Value = value;
  }
}

public sealed class FixtureOwnerContext(FixtureLoadState state)
{
  public FixtureLoadState State { get; } = state;

  public FixtureLoadTrace Trace => State.Trace;
}

[WorldLoadApi(
  "fixture.state",
  "fixture.owner",
  "fixture.section",
  1,
  1,
  WorldLoadSectionRequirement.Required)]
public sealed class FixtureWorldLoadApi :
  IWorldLoadApi<FixtureOwnerContext, FixtureSection, FixturePrepared>
{
  public int PrepareCallCount { get; private set; }

  public WorldLoadPrepareResult<FixturePrepared> PrepareLoad(
    FixtureOwnerContext ownerContext,
    WorldLoadSection<FixtureSection> section)
  {
    PrepareCallCount++;
    ownerContext.Trace.Record("prepare:fixture.state");
    return section.IsPresent
      ? WorldLoadPrepareResult<FixturePrepared>.Prepared(
        new FixturePrepared(section.Value.Value))
      : WorldLoadPrepareResult<FixturePrepared>.Rejected(
        WorldLoadApiFailure.Create("MissingSection", "The fixture section is required."));
  }

  public WorldLoadCommitResult CommitLoad(
    FixtureOwnerContext ownerContext,
    in FixturePrepared preparedData)
  {
    ownerContext.State.SetValue(preparedData.Value);
    ownerContext.Trace.Record("commit:fixture.state");
    return WorldLoadCommitResult.Committed();
  }

  public void DiscardPrepared(
    FixtureOwnerContext ownerContext,
    in FixturePrepared preparedData)
  {
  }
}
