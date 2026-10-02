namespace Terraria.NpcTownBestiary;

public sealed class TownNpcProfileDefinition
{
  public TownNpcProfileDefinition(
    NpcTypeId npcType,
    string defaultRoot,
    string shimmeredRoot,
    IEnumerable<int> headIds)
  {
    if (string.IsNullOrWhiteSpace(defaultRoot))
    {
      throw new ArgumentException("Default profile root is required.", nameof(defaultRoot));
    }

    if (string.IsNullOrWhiteSpace(shimmeredRoot))
    {
      throw new ArgumentException(
        "Shimmered profile root is required.",
        nameof(shimmeredRoot));
    }

    ArgumentNullException.ThrowIfNull(headIds);
    NpcType = npcType;
    DefaultRoot = defaultRoot;
    ShimmeredRoot = shimmeredRoot;
    HeadIds = headIds.ToArray();
  }

  public NpcTypeId NpcType { get; }

  public string DefaultRoot { get; }

  public string ShimmeredRoot { get; }

  public IReadOnlyList<int> HeadIds { get; }
}
