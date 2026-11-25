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
  public string UseAmmo { get; set; } = nameof(Terraria.Share.Items.ItemId.None);

  [JsonPropertyName("consumeAmmo")]
  public bool ConsumeAmmo { get; set; }

  [JsonPropertyName("manaCost")]
  public int ManaCost { get; set; }

  [JsonPropertyName("shoot")]
  public ItemShootPrototype? Shoot { get; set; }
}

/// <summary>射击参数：生成哪个投射物、以什么速度从哪里出发。</summary>
internal sealed class ItemShootPrototype
{
  [JsonPropertyName("projectileId")]
  public string ProjectileId { get; set; } = string.Empty;

  [JsonPropertyName("velocityX")]
  public float VelocityX { get; set; }

  [JsonPropertyName("velocityY")]
  public float VelocityY { get; set; }

  [JsonPropertyName("offsetX")]
  public float OffsetX { get; set; }

  [JsonPropertyName("offsetY")]
  public float OffsetY { get; set; }
}
