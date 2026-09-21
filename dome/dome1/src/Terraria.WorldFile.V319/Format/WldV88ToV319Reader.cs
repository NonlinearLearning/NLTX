using System;
using System.Collections.Generic;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

internal static class WldV88ToV319Reader
{
  public static LegacyWorldDocument Read(
    int version,
    WldBinaryReader reader,
    WldReadLimits limits)
  {
    WldSectionPointerTable table = WldSectionPointerTable.Read(version, reader);
    using WldBinaryReader headerReader = table.CreateSectionReader(reader, sectionIndex: 0);
    LegacyWorldMetadata metadata = WldHeaderReader.Read(version, headerReader, limits);
    headerReader.RequireSectionEnd();
    using WldBinaryReader tileReader = table.CreateSectionReader(reader, sectionIndex: 1);
    IReadOnlyList<LegacyTile> tiles = WldTileRleReader.Read(
      version,
      tileReader,
      metadata.Width,
      metadata.Height,
      table.TileImportance);
    using WldBinaryReader chestReader = table.CreateSectionReader(reader, sectionIndex: 2);
    IReadOnlyList<LegacyChest> chests = WldChestReader.Read(version, chestReader, metadata, limits);
    using WldBinaryReader signReader = table.CreateSectionReader(reader, sectionIndex: 3);
    IReadOnlyList<LegacySign> signs = WldSignReader.Read(version, signReader, metadata, limits);
    using WldBinaryReader npcReader = table.CreateSectionReader(reader, sectionIndex: 4);
    IReadOnlyList<LegacyNpc> npcs = WldNpcReader.Read(version, npcReader, limits);
    IReadOnlyList<LegacyTileEntity> tileEntities = Array.Empty<LegacyTileEntity>();
    List<LegacySectionDiagnostic> diagnostics = [];
    int tailSectionIndex = 5;
    if (version >= 116)
    {
      using WldBinaryReader tileEntityReader = table.CreateSectionReader(reader, tailSectionIndex++);
      tileEntities = WldTileEntityReader.Read(version, tileEntityReader, metadata, limits);
    }

    if (version >= 170)
    {
      using WldBinaryReader pressurePlateReader = table.CreateSectionReader(reader, tailSectionIndex++);
      LegacySectionDiagnostic? diagnostic = WldPressurePlateReader.Read(
        pressurePlateReader,
        metadata,
        limits);
      if (diagnostic is not null)
      {
        diagnostics.Add(diagnostic);
      }
    }

    if (version >= 189)
    {
      using WldBinaryReader townManagerReader = table.CreateSectionReader(reader, tailSectionIndex++);
      LegacySectionDiagnostic? diagnostic = WldTownManagerReader.Read(townManagerReader, limits);
      if (diagnostic is not null)
      {
        diagnostics.Add(diagnostic);
      }
    }

    if (version >= 210)
    {
      using WldBinaryReader bestiaryReader = table.CreateSectionReader(reader, tailSectionIndex++);
      LegacySectionDiagnostic? diagnostic = WldBestiaryReader.Read(bestiaryReader, limits);
      if (diagnostic is not null)
      {
        diagnostics.Add(diagnostic);
      }
    }

    if (version >= 220)
    {
      using WldBinaryReader creativePowerReader = table.CreateSectionReader(reader, tailSectionIndex++);
      LegacySectionDiagnostic? diagnostic = WldCreativePowerReader.Read(creativePowerReader);
      if (diagnostic is not null)
      {
        diagnostics.Add(diagnostic);
      }
    }

    {
      using WldBinaryReader footerReader = table.CreateSectionReader(reader, tailSectionIndex);
      WldFooterReader.Read(version, footerReader, metadata);
    }

    return new LegacyWorldDocument(
      version,
      WldFormatVersion.PointerTableV88ToV319,
      metadata,
      tiles,
      chests,
      signs,
      npcs,
      tileEntities,
      diagnostics);
  }
}
