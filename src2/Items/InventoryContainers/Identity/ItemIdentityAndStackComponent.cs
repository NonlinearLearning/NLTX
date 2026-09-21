namespace Terraria.Items.InventoryContainers;

public sealed class ItemIdentityAndStackComponent
{
  public ItemIdentityAndStackComponent(
    int contentType,
    int stack,
    int maxStack,
    bool uniqueStack,
    byte prefix,
    int variant,
    bool favorited,
    string? nameOverride)
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

    if (uniqueStack && maxStack > 1)
    {
      throw new ArgumentException(
        "A unique-stack item cannot expose a stack capacity greater than one.",
        nameof(maxStack));
    }

    ContentType = contentType;
    Stack = stack;
    MaxStack = maxStack;
    UniqueStack = uniqueStack;
    Prefix = prefix;
    Variant = variant;
    Favorited = favorited;
    NameOverride = nameOverride;
  }

  public int ContentType { get; }

  public int Stack { get; private set; }

  public int MaxStack { get; private set; }

  public bool UniqueStack { get; }

  public byte Prefix { get; private set; }

  public int Variant { get; private set; }

  public bool Favorited { get; private set; }

  public string? NameOverride { get; private set; }

  public long Revision { get; private set; }

  public bool TrySetStack(int stack)
  {
    if (stack < 0 || stack > MaxStack)
    {
      return false;
    }

    if (Stack == stack)
    {
      return true;
    }

    Stack = stack;
    Revision++;
    return true;
  }

  public bool TrySetMaxStack(int maxStack)
  {
    if (maxStack < 0 || Stack > maxStack || (UniqueStack && maxStack > 1))
    {
      return false;
    }

    if (MaxStack == maxStack)
    {
      return true;
    }

    MaxStack = maxStack;
    Revision++;
    return true;
  }

  public void SetFavorited(bool favorited)
  {
    if (Favorited != favorited)
    {
      Favorited = favorited;
      Revision++;
    }
  }

  public void SetPrefix(byte prefix)
  {
    if (Prefix != prefix)
    {
      Prefix = prefix;
      Revision++;
    }
  }

  public void SetVariant(int variant)
  {
    if (Variant != variant)
    {
      Variant = variant;
      Revision++;
    }
  }

  public void SetNameOverride(string? nameOverride)
  {
    if (!string.Equals(NameOverride, nameOverride, StringComparison.Ordinal))
    {
      NameOverride = nameOverride;
      Revision++;
    }
  }

  public ItemStackSnapshot CreateSnapshot()
  {
    return new ItemStackSnapshot(
      ContentType,
      Stack,
      MaxStack,
      UniqueStack,
      Prefix,
      Variant,
      Favorited,
      NameOverride,
      Revision);
  }
}
