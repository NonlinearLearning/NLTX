using MinimalImplementation.Contracts;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Modules.Events;

public sealed record LanternNightSnapshot(
    CommitVersion Version,
    bool Manual,
    bool Genuine,
    bool NextNightIsLanternNight,
    int Cooldown) {
  public bool LanternsUp => Manual || Genuine;

  public WorldFileEventSection ToPersistenceSection(WorldFileEventSection existingSection) {
    ArgumentNullException.ThrowIfNull(existingSection);
    return new WorldFileEventSection(
        existingSection.CombatBookWasUsed, Cooldown, Genuine, Manual, NextNightIsLanternNight);
  }
}
