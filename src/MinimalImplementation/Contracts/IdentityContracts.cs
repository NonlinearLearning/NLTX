namespace MinimalImplementation.Contracts;

public readonly record struct EntityUuid(Guid Value)
{
  public bool IsValid => Value != Guid.Empty;

  public static EntityUuid Create()
  {
    return new EntityUuid(Guid.NewGuid());
  }

    public static EntityUuid From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EntityUuid cannot be empty.", nameof(value));

        return new EntityUuid(value);
    }
}
