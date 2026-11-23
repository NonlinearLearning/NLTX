using System.Text.Json.Serialization;

namespace NSSLC.Server.Content.ItemUse;

/// <summary>
/// Raw JSON shape of one item use entry.
/// </summary>
/// <remarks>
/// This is a parse target, not a domain value: every member is mutable and nullable so that
/// missing fields surface as diagnostics instead of deserialization exceptions. Validation and
/// conversion happen in <see cref="ItemUseCodec" />.
/// </remarks>
internal sealed class ItemUsePrototype
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("type")]
  public string Type { get; set; } = string.Empty;

  [JsonPropertyName("useTime")]
  public int UseTime { get; set; }

  [JsonPropertyName("useAnimation")]
  public int UseAnimation { get; set; }

  [JsonPropertyName("autoReuse")]
  public bool AutoReuse { get; set; }

  [JsonPropertyName("channel")]
  public bool Channel { get; set; }

  [JsonPropertyName("useAmmo")]
  public string UseAmmo { get; set; } = nameof(Terraria.Item.ItemId.None);

  [JsonPropertyName("consumeAmmo")]
  public bool ConsumeAmmo { get; set; }

  [JsonPropertyName("manaCost")]
  public int ManaCost { get; set; }
}
