using System.Collections.ObjectModel;
using MinimalImplementation.Server.Modules.Bestiary;

namespace MinimalImplementation.Server.Host;

/// <summary>Maps the persisted NPC types supported by the host to their wire identities.</summary>
internal static class BestiaryNpcCatalog {
  internal static IReadOnlyDictionary<string, short> NetIds { get; } =
    new ReadOnlyDictionary<string, short>(new Dictionary<string, short>(StringComparer.Ordinal) {
      ["BlueSlime"] = 1,
      ["DemonEye"] = 2,
      ["Zombie"] = 3
    });

  internal static void Validate(BestiarySnapshot snapshot) {
    string[] unknown = snapshot.KillCounts.Keys.Concat(snapshot.SeenNpcIds)
      .Concat(snapshot.ChattedNpcIds).Distinct(StringComparer.Ordinal)
      .Where(id => !NetIds.ContainsKey(id)).Order(StringComparer.Ordinal).ToArray();
    if (unknown.Length != 0) {
      throw new InvalidDataException(
        $"The bounded NPC catalog cannot project persisted IDs: {string.Join(", ", unknown)}.");
    }
  }
}
