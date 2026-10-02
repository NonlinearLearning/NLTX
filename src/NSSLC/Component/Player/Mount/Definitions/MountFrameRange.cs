namespace Terraria.Player.Mount;

public readonly record struct MountFrameRange(int Start, int Count, int Delay)
{
  public bool IsUsable => Count > 0 && Delay >= 0;

  public bool Contains(int frame)
  {
    return Count > 0 && frame >= Start && frame < Start + Count;
  }

  public int Normalize(int frame)
  {
    if (Count <= 0)
    {
      return Start;
    }

    int offset = (frame - Start) % Count;
    if (offset < 0)
    {
      offset += Count;
    }

    return Start + offset;
  }
}
