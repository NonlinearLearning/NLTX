using System;
using System.Collections.Generic;
using System.IO;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldLoadApiGenerationVerification.Api;
using Terraria.WorldLoadApiGenerationVerification.SecondaryApi;
using Terraria.WorldLoadApiGenerationVerification.TransitiveApis;
using Terraria.WorldStorage;
using Terraria.WorldStorage.Generated.Terraria_WorldLoadApiGenerationVerification;

[assembly: WorldPersistenceSectionSchema(
  "fixture.section",
  typeof(FixtureSection),
  WorldLoadSectionRequirement.Required)]
[assembly: WorldPersistenceSectionSchema(
  "fixture.optional-section",
  typeof(FixtureOptionalSection),
  WorldLoadSectionRequirement.Optional)]

FixtureCompositionDependency.EnsureLoaded();
var state = new FixtureLoadState();
var api = new FixtureWorldLoadApi();
var secondaryApi = new FixtureSecondaryWorldLoadApi();
var runtimeBindings = new FixtureRuntimeBindings(
  api,
  new FixtureOwnerContext(state),
  secondaryApi,
  new FixtureSecondaryOwnerContext(state.Trace));
string path = Path.Combine(Path.GetTempPath(), $"world-load-api-{Guid.NewGuid():N}.wld");
WorldRecoveryOutcome outcome;
try
{
  File.WriteAllBytes(path, new byte[] { 1, 42 });
  WorldLoadCoordinator coordinator = WorldStorageCoordinatorFactory.CreateLoadCoordinator(
    new FixtureDocumentDecoder(),
    new FixtureDocumentValidator(),
    new GeneratedWorldLoadApiCatalog(
      WorldLoadApiCatalog.Descriptors,
      WorldLoadApiCatalog.Execute));
  outcome = coordinator.Load(
    path,
    runtimeBindings);
}
finally
{
  if (File.Exists(path))
  {
    File.Delete(path);
  }
}

if (!outcome.CanPublishWorldLoaded || state.Value != 42 || api.PrepareCallCount != 1)
{
  throw new InvalidOperationException(
    "The coordinator did not decode and dispatch the complete fixture document.");
}

string actualOrder = string.Join(",", state.Trace.Events);
string expectedOrder = string.Join(",", new[]
{
  "prepare:fixture.secondary",
  "prepare:fixture.state",
  "commit:fixture.state",
  "commit:fixture.secondary"
});
if (actualOrder != expectedOrder)
{
  throw new InvalidOperationException(
    $"The generated catalog used an unexpected prepare/commit order: {actualOrder}");
}

if (outcome.ApiExecution is null || !outcome.ApiExecution.Succeeded ||
    WorldLoadApiCatalog.Descriptors.Count != 2 ||
    WorldLoadApiCatalog.Descriptors[0].ApiId != "fixture.secondary" ||
    WorldLoadApiCatalog.Descriptors[1].ApiId != "fixture.state")
{
  throw new InvalidOperationException("The generated API catalog result is incomplete.");
}

Console.WriteLine("World load API generation verification passed.");

internal sealed class FixtureRuntimeBindings(
  FixtureWorldLoadApi api,
  FixtureOwnerContext ownerContext,
  FixtureSecondaryWorldLoadApi secondaryApi,
  FixtureSecondaryOwnerContext secondaryOwnerContext) : WorldLoadApiRuntimeBindings
{
  public override bool TryGetApi<TApi>(out TApi resolvedApi)
  {
    if (api is TApi typedApi)
    {
      resolvedApi = typedApi;
      return true;
    }

    if (secondaryApi is TApi typedSecondaryApi)
    {
      resolvedApi = typedSecondaryApi;
      return true;
    }

    resolvedApi = null!;
    return false;
  }

  public override bool TryGetOwnerContext<TOwnerContext>(
    string ownerId,
    out TOwnerContext resolvedContext)
  {
    if (ownerId == "fixture.owner" && ownerContext is TOwnerContext typedContext)
    {
      resolvedContext = typedContext;
      return true;
    }

    if (ownerId == "fixture.secondary-owner" &&
        secondaryOwnerContext is TOwnerContext typedSecondaryContext)
    {
      resolvedContext = typedSecondaryContext;
      return true;
    }

    resolvedContext = default!;
    return false;
  }
}

internal sealed class FixtureDocumentDecoder : IWorldPersistenceDocumentDecoder
{
  public WorldPersistenceDecodeResult Decode(ReadOnlyMemory<byte> fileBytes)
  {
    if (fileBytes.Length != 2 || fileBytes.Span[0] != 1)
    {
      return WorldPersistenceDecodeResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData));
    }

    var document = new WorldPersistenceDocument(
      fileBytes.Span[0],
      new[]
      {
        WorldPersistenceSection.Create(
          "fixture.section",
          new FixtureSection(fileBytes.Span[1]))
      });
    return WorldPersistenceDecodeResult.Decoded(document);
  }
}

internal sealed class FixtureDocumentValidator : IWorldPersistenceDocumentValidator
{
  public WorldStorageFailure Validate(WorldPersistenceDocument document)
  {
    return WorldStorageFailure.None;
  }
}
