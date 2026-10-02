namespace Terraria.EntityLifecycleAttribution;

public sealed class WorldItemComponent
{
  public WorldItemComponent(int contentType, int stack)
  {
    if (contentType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(contentType));
    }

    if (stack <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(stack));
    }

    ContentType = contentType;
    Stack = stack;
  }

  public int ContentType { get; }

  public int Stack { get; }
}
