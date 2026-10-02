namespace Terraria.Npc;

// status: partial
// sourceMembers: IsABestiaryIconDummy, IsAPortraitDummy, ForcePartyHatOn,
// nameOverIncrement, nameOverDistance, nameOver, altTexture, townNpcVariationIndex
// crossSubsystemOwner: presentation snapshot integration-review
public sealed class NpcPresentationStateComponent
{
  public NpcPresentationStateComponent(
    bool isBestiaryIconDummy = false,
    bool isPortraitDummy = false,
    bool forcePartyHatOn = false,
    float nameOverIncrement = 0.0f,
    float nameOverDistance = 0.0f,
    float nameOver = 0.0f,
    int altTexture = 0,
    int townNpcVariationIndex = 0)
  {
    IsBestiaryIconDummy = isBestiaryIconDummy;
    IsPortraitDummy = isPortraitDummy;
    ForcePartyHatOn = forcePartyHatOn;
    NameOverIncrement = nameOverIncrement;
    NameOverDistance = nameOverDistance;
    NameOver = nameOver;
    AltTexture = altTexture;
    TownNpcVariationIndex = townNpcVariationIndex;
  }

  public bool IsBestiaryIconDummy { get; }

  public bool IsPortraitDummy { get; }

  public bool ForcePartyHatOn { get; }

  public float NameOverIncrement { get; }

  public float NameOverDistance { get; }

  public float NameOver { get; }

  public int AltTexture { get; }

  public int TownNpcVariationIndex { get; }
}
