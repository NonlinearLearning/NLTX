using Terraria.WorldStorage;

[assembly: Terraria.NonAuthoritative.Persistence.WorldPersistenceSectionSchemaAttribute(
  Terraria.NonAuthoritative.Persistence.WorldFileTreeTopsSection.SectionId,
  typeof(Terraria.NonAuthoritative.Persistence.WorldFileTreeTopsSection),
  WorldLoadSectionRequirement.Optional)]
