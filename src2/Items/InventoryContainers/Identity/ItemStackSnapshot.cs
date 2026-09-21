namespace Terraria.Items.InventoryContainers;

public sealed class ItemStackSnapshot
{
  public ItemStackSnapshot(
    int contentType,
    int stack,
    int maxStack,
    bool uniqueStack,
    byte prefix,
    int variant,
    bool favorited,
    string? nameOverride,
    long revision = 0)
  {
    if (contentType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(contentType));
    }

    if (maxStack < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxStack));
    }

    if (stack < 0 || stack > maxStack)
    {
      throw new ArgumentOutOfRangeException(nameof(stack));
    }

    ContentType = contentType;
    Stack = stack;
    MaxStack = maxStack;
    UniqueStack = uniqueStack;
    Prefix = prefix;
    Variant = variant;
    Favorited = favorited;
    NameOverride = nameOverride;
    Revision = revision;
  }

  public int ContentType { get; }

  public int Stack { get; }

  public int MaxStack { get; }

  public bool UniqueStack { get; }

  public byte Prefix { get; }

  public int Variant { get; }

  public bool Favorited { get; }

  public string? NameOverride { get; }

  public long Revision { get; }

  public int AvailableCapacity => Math.Max(0, MaxStack - Stack);
}
