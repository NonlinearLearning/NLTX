using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.WorldCompatibility.Model;
using Terraria.WorldCompatibility.Projection;
using Terraria.WorldFile.V319.Format;
using Terraria.WorldFile.V319.Model;

LegacyWorldDocument document = new(
  319,
  WldFormatVersion.PointerTableV88ToV319,
  new LegacyWorldMetadata("Compatibility", 319, 2, 2, 0, 0),
  [new LegacyTile(
    true,
    7,
    LiquidAmount: 100,
    LiquidKind: 3,
    IsInvisibleBlock: true,
    IsFullbrightWall: true)],
  [new LegacyChest(0, 0, "Chest", [new LegacyChestItem(2, 73, 4)])],
  [new LegacySign(1, 1, "Sign")],
  [new LegacyNpc(17, "Guide", 1, 2, false, 0, 0)],
  [new LegacyTileEntity(1, 250, 1, 0, [0xAA], isOpaque: true)],
  [new LegacySectionDiagnostic(9, 42, "Creative records were preserved.", [1, 2, 3])]);

CompatibilityWorldSnapshot snapshot = WldToCompatibilityProjection.Project(document);
if (snapshot.Metadata.Name != "Compatibility" ||
    snapshot.Tiles.Count != 1 ||
    snapshot.Tiles[0].LiquidKind != 3 ||
    snapshot.Chests[0].Items[0] != new CompatibilityChestItem(2, 73, 4) ||
    snapshot.Signs[0].Text != "Sign" ||
    snapshot.Npcs[0].Type != 17 ||
    snapshot.TileEntities[0].IsOpaque != true ||
    snapshot.LoadReport.UnsupportedRecords.Count != 3 ||
    !snapshot.LoadReport.UnsupportedRecords.Any(record =>
      record.SectionIndex == 5 && record.Type == 250 && record.X == 1 && record.Y == 0) ||
    !snapshot.LoadReport.UnsupportedRecords.Any(record => record.SectionIndex == 9))
{
  throw new InvalidOperationException("The compatibility projection did not preserve world state.");
}

if (snapshot.Tiles is IList<CompatibilityTile> ||
    snapshot.LoadReport.UnsupportedRecords is IList<CompatibilityUnsupportedRecord>)
{
  throw new InvalidOperationException("The compatibility projection exposed mutable collections.");
}

LegacyWorldDocument coordinateDocument = new(
  319,
  WldFormatVersion.PointerTableV88ToV319,
  new LegacyWorldMetadata("Coordinates", 319, 2, 3, 0, 0),
  [
    new LegacyTile(true, 1),
    new LegacyTile(true, 2),
    new LegacyTile(true, 3),
    new LegacyTile(true, 4),
    new LegacyTile(true, 5),
    new LegacyTile(true, 6)
  ],
  [],
  [],
  [],
  [],
  []);
CompatibilityWorldSnapshot coordinateSnapshot =
  WldToCompatibilityProjection.Project(coordinateDocument);
for (int x = 0; x < coordinateDocument.Metadata.Width; x++)
{
  for (int y = 0; y < coordinateDocument.Metadata.Height; y++)
  {
    int index = (x * coordinateDocument.Metadata.Height) + y;
    CompatibilityTile tile = coordinateSnapshot.Tiles[index];
    if (tile.X != x || tile.Y != y || tile.TileType != index + 1)
    {
      throw new InvalidOperationException(
        "The compatibility projection did not preserve column-major tile coordinates.");
    }
  }
}

Console.WriteLine("PASS: compatibility projection preserves state and loss diagnostics");
