using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldGeneration;
using Terraria.Dome.Simulation.WorldModel;

WorldGrid world = new(200, 150);
TileChangeCommitSystem tileCommit = new();
long sectionVersion = world.GetSectionVersion(new WorldSectionCoordinates(0, 0));
TileChangeCommand validTile = new(
  1,
  10,
  10,
  TileChangeKind.Place,
  1,
  Priority: 2,
  Source: "terrain",
  ExpectedSectionVersion: sectionVersion);
if (!tileCommit.TryCommit(world, new[] { validTile }, out TileChangeCommitResult tileResult) ||
    !tileResult.Succeeded ||
    tileResult.AppliedCount != 1)
{
  throw new InvalidOperationException("A metadata-bearing tile command did not commit.");
}

TileChangeCommand staleTile = validTile with { Sequence = 2 };
if (tileCommit.TryCommit(world, new[] { staleTile }, out tileResult) ||
    tileResult.FailureReason is null ||
    !tileResult.FailureReason.Contains("metadata", StringComparison.Ordinal))
{
  throw new InvalidOperationException("A stale tile section revision was accepted.");
}

LiquidChangeCommitSystem liquidCommit = new();
IReadOnlyCollection<LiquidDefinition> definitions =
  new[] { new LiquidDefinition("water", 1, byte.MaxValue) };
LiquidChangeCommand invalidLiquid = new(
  long.MaxValue,
  10,
  10,
  1,
  1,
  Source: "liquid");
if (liquidCommit.TryCommit(world, new[] { invalidLiquid }, definitions, out LiquidChangeCommitResult liquidResult) ||
    liquidResult.FailureReason is null ||
    !liquidResult.FailureReason.Contains("metadata", StringComparison.Ordinal))
{
  throw new InvalidOperationException("A terminal liquid sequence was accepted.");
}

Console.WriteLine("PASS: generation command metadata and revision gates are enforced");

WorldMetadata mismatchedMetadata = new(
  "variant-mismatch",
  new WorldSeed(1456),
  200,
  150,
  seedVariant: "default");
try
{
  _ = new WorldGenerationRequest(
    mismatchedMetadata,
    100,
    40,
    seedVariant: "for-the-worthy",
    rules: new WorldRuleSnapshotComponent(0, "for-the-worthy", false));
  throw new InvalidOperationException("A seed-variant mismatch was accepted.");
}
catch (ArgumentException)
{
}

Console.WriteLine("PASS: generation requests reject metadata and rule variant mismatches");
