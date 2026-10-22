using System.Collections.ObjectModel;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Resolves a leashable critter NPC type to its Version4 runtime prototype.</summary>
public sealed class LeashedPrototypeResolver
{
  private readonly MechanismFactsCatalog _facts;
  private readonly IReadOnlyDictionary<short, LeashedPrototypeDefinition> _overrides;

  public LeashedPrototypeResolver(MechanismFactsCatalog facts)
  {
    _facts = facts ?? throw new ArgumentNullException(nameof(facts));
    var overrides = new Dictionary<short, LeashedPrototypeDefinition>();
    foreach (LeashedPrototypeDefinition definition in facts.LeashedPrototypes.Values
               .OrderBy(definition => definition.Type))
    {
      foreach (short npcType in definition.NpcTypes)
      {
        overrides[npcType] = definition;
      }
    }
    _overrides = new ReadOnlyDictionary<short, LeashedPrototypeDefinition>(overrides);
  }

  public bool TryResolve(short npcType, out LeashedPrototypeDefinition? definition)
  {
    if (npcType <= 0)
    {
      definition = null;
      return false;
    }
    return _overrides.TryGetValue(npcType, out definition);
  }

  /// <summary>Resolves an already-validated held-item type through its Version4 makeNPC value.</summary>
  public bool TryResolveHeldItem(
    short itemType,
    IReadOnlyDictionary<short, short> itemNpcTypes,
    out LeashedPrototypeDefinition? definition)
  {
    ArgumentNullException.ThrowIfNull(itemNpcTypes);
    if (itemType <= 0 || !itemNpcTypes.TryGetValue(itemType, out short npcType))
    {
      definition = null;
      return false;
    }
    return TryResolve(npcType, out definition);
  }
}
