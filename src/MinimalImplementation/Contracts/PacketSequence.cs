namespace MinimalImplementation.Contracts;

public readonly record struct PacketSequence(long Value)
{
  public bool IsValid => Value >= 0;
}
