using Terraria.Item;

namespace NSSLC.Server.Content.Prototypes;

[Prototype("itemUse")]
internal sealed class ItemUseDefinitionPrototype : IPrototype
{
  [IdDataField]
  [DataField(required: true)]
  public string Id { get; init; } = string.Empty;

  [DataField]
  public string Type { get; init; } = "itemUse";

  [DataField(required: true)]
  public int UseTime { get; init; }

  [DataField(required: true)]
  public int UseAnimation { get; init; }

  [DataField]
  public bool AutoReuse { get; init; }

  [DataField]
  public bool Channel { get; init; }

  [DataField]
  public string UseAmmo { get; init; } = nameof(ItemId.None);

  [DataField]
  public bool ConsumeAmmo { get; init; }

  [DataField]
  public int ManaCost { get; init; }
}

