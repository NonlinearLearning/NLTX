namespace MinimalImplementation.Server.Modules;

/// <summary>
/// P07's direction-owned proposal for the Version4 module registry. The total registry remains
/// a root-composition responsibility; this table is deliberately data-only and is not a runtime
/// replacement for that registry.
/// </summary>
public sealed record ModuleRegistrationProposalEntry(
  ushort ModuleId,
  string Name,
  string Owner,
  bool ClientToServer,
  bool ServerToClient,
  string CodecKey);

public static class ModuleRegistrationProposal
{
  public const ushort Version4ModuleCount = 15;

  public static IReadOnlyList<ModuleRegistrationProposalEntry> Create()
  {
    ModuleRegistrationProposalEntry[] entries =
    [
      new(0, "Liquid", "P03", true, true, "Liquid"),
      new(1, "Text", "P07", true, true, "Text"),
      new(2, "Ping", "P02", true, true, "Ping"),
      new(3, "Ambience", "P07", false, true, "Ambience"),
      new(4, "Bestiary", "P07", false, true, "Bestiary"),
      new(5, "CreativePowers", "P07", true, true, "CreativePowers"),
      new(6, "CreativeUnlocksPlayerReport", "P07", true, true, "CreativeUnlocksPlayerReport"),
      new(7, "TeleportPylon", "P06", true, true, "TeleportPylon"),
      new(8, "Particles", "P07", true, true, "Particles"),
      new(9, "CreativePowerPermissions", "P07", false, true, "CreativePowerPermissions"),
      new(10, "Banners", "P07", true, true, "Banners"),
      new(11, "CraftingRequests", "P04", true, true, "CraftingRequests"),
      new(12, "TagEffectState", "P05", true, true, "TagEffectState"),
      new(13, "LeashedEntity", "P06", false, true, "LeashedEntity"),
      new(14, "UnbreakableWallScan", "P03", false, true, "UnbreakableWallScan")
    ];

    return Array.AsReadOnly(entries);
  }

  public static bool IsVersion4Complete(
    IReadOnlyList<ModuleRegistrationProposalEntry>? entries = null)
  {
    entries ??= Create();
    if (entries.Count != Version4ModuleCount)
    {
      return false;
    }

    for (ushort moduleId = 0; moduleId < Version4ModuleCount; moduleId++)
    {
      ModuleRegistrationProposalEntry entry = entries[moduleId];
      if (entry.ModuleId != moduleId
          || string.IsNullOrWhiteSpace(entry.Name)
          || string.IsNullOrWhiteSpace(entry.Owner)
          || string.IsNullOrWhiteSpace(entry.CodecKey)
          || !entry.ClientToServer && !entry.ServerToClient)
      {
        return false;
      }
    }

    return true;
  }
}
