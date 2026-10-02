namespace Terraria.Network.Section;

public sealed class RemoteClientSectionObservationStateComponent
{
  private readonly bool[,] _tileSections;
  private readonly uint[,] _tileSectionsCheckTime;

  public RemoteClientSectionObservationStateComponent(
    int sectionWidth,
    int sectionHeight,
    bool[,]? tileSections = null,
    uint[,]? tileSectionsCheckTime = null,
    bool checkingSections = false)
  {
    if (sectionWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sectionWidth));
    }

    if (sectionHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sectionHeight));
    }

    if ((tileSections is null) != (tileSectionsCheckTime is null))
    {
      throw new ArgumentException(
        "Section observations and check times must be supplied together.");
    }

    if (tileSections is not null && tileSectionsCheckTime is not null)
    {
      ValidateDimensions(tileSections, sectionWidth, sectionHeight, nameof(tileSections));
      ValidateDimensions(
        tileSectionsCheckTime,
        sectionWidth,
        sectionHeight,
        nameof(tileSectionsCheckTime));
      _tileSections = (bool[,])tileSections.Clone();
      _tileSectionsCheckTime = (uint[,])tileSectionsCheckTime.Clone();
    }
    else
    {
      _tileSections = new bool[sectionWidth, sectionHeight];
      _tileSectionsCheckTime = new uint[sectionWidth, sectionHeight];
    }

    SectionWidth = sectionWidth;
    SectionHeight = sectionHeight;
    CheckingSections = checkingSections;
  }

  public int SectionWidth { get; }

  public int SectionHeight { get; }

  public bool CheckingSections { get; }

  public bool[,] TileSections => (bool[,])_tileSections.Clone();

  public uint[,] TileSectionsCheckTime => (uint[,])_tileSectionsCheckTime.Clone();

  private static void ValidateDimensions<T>(
    T[,] values,
    int expectedWidth,
    int expectedHeight,
    string parameterName)
  {
    if (values.GetLength(0) != expectedWidth ||
        values.GetLength(1) != expectedHeight)
    {
      throw new ArgumentException(
        "Section state dimensions do not match the component dimensions.",
        parameterName);
    }
  }
}
