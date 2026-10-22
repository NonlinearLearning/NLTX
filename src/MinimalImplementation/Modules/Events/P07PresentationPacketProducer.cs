using System.IO;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Modules.Events;

/// <summary>
/// Encodes P07-owned server presentation facts into outbound intents.
///
/// The caller must have already committed the event in its owning state owner. This class only
/// validates bounded wire values and creates packet bodies; it does not mutate world state or write
/// to a transport.
/// </summary>
public sealed class P07PresentationPacketProducer
{
  private const int MaximumRecipientCount = byte.MaxValue;
  private const int MaximumTravelMerchantSlots = 40;
  private const int MaximumNetworkTextWidth = short.MaxValue;

  private readonly ushort _maximumTowerShieldStrength;

  public P07PresentationPacketProducer(ushort maximumTowerShieldStrength)
  {
    if (maximumTowerShieldStrength == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumTowerShieldStrength));
    }

    _maximumTowerShieldStrength = maximumTowerShieldStrength;
  }

  public OutboundPacketIntent CreateMiscDataIntent(
    byte player,
    byte action,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    ValidatePlayerSlot(player);
    return CreateIntent(
      51,
      targets,
      commitVersion,
      writer => MiscDataSyncPacket.Write(writer, player, action));
  }

  public OutboundPacketIntent CreateTravelMerchantItemsIntent(
    IReadOnlyList<short> items,
    int slotCount,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(items);
    if (slotCount is < 0 or > MaximumTravelMerchantSlots || items.Count != slotCount)
    {
      throw new ArgumentOutOfRangeException(
        nameof(slotCount),
        "Travel merchant items must contain exactly the bounded Version4 slot count.");
    }

    return CreateIntent(
      72,
      targets,
      commitVersion,
      writer => TravelMerchantItemsPacket.Write(writer, items, slotCount));
  }

  public OutboundPacketIntent CreateAnglerQuestIntent(
    byte quest,
    bool alreadyFinished,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    return CreateIntent(
      74,
      targets,
      commitVersion,
      writer => AnglerQuestPacket.Write(writer, quest, alreadyFinished));
  }

  public OutboundPacketIntent CreateInvasionProgressIntent(
    int progress,
    int progressMax,
    sbyte icon,
    sbyte wave,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    if (progress < -1 || progressMax < 0 || icon is < 0 or > 7 || wave < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(progress),
        "Invasion progress requires a valid progress, maximum, icon and wave.");
    }

    return CreateIntent(
      78,
      targets,
      commitVersion,
      writer => InvasionProgressReportPacket.Write(
        writer,
        progress,
        progressMax,
        icon,
        wave));
  }

  public OutboundPacketIntent CreateCombatTextIntIntent(
    float x,
    float y,
    PacketRgb color,
    int amount,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    ValidateFinite(x, nameof(x));
    ValidateFinite(y, nameof(y));
    return CreateIntent(
      81,
      targets,
      commitVersion,
      writer => CombatTextIntPacket.Write(writer, x, y, color, amount));
  }

  public OutboundPacketIntent CreateAchievementNpcKilledIntent(
    short npcNetId,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    return CreateIntent(
      97,
      targets,
      commitVersion,
      writer => AchievementMessageNPCKilledPacket.Write(writer, npcNetId));
  }

  public OutboundPacketIntent CreateAchievementEventIntent(
    short eventId,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    return CreateIntent(
      98,
      targets,
      commitVersion,
      writer => AchievementMessageEventHappenedPacket.Write(writer, eventId));
  }

  public OutboundPacketIntent CreateTowerShieldIntent(
    ushort solar,
    ushort vortex,
    ushort nebula,
    ushort stardust,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    ValidateTowerShield(solar, nameof(solar));
    ValidateTowerShield(vortex, nameof(vortex));
    ValidateTowerShield(nebula, nameof(nebula));
    ValidateTowerShield(stardust, nameof(stardust));
    return CreateIntent(
      101,
      targets,
      commitVersion,
      writer => UpdateTowerShieldStrengthsPacket.Write(
        writer,
        solar,
        vortex,
        nebula,
        stardust));
  }

  public OutboundPacketIntent CreateMoonlordHorrorIntent(
    int maximumCountdown,
    int countdown,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    if (maximumCountdown < 0 || countdown < 0 || countdown > maximumCountdown)
    {
      throw new ArgumentOutOfRangeException(
        nameof(countdown),
        "Moonlord horror countdown must remain within the committed maximum.");
    }

    return CreateIntent(
      103,
      targets,
      commitVersion,
      writer => MoonlordHorrorPacket.Write(writer, maximumCountdown, countdown));
  }

  public OutboundPacketIntent CreatePoofOfSmokeIntent(
    uint packedPosition,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    return CreateIntent(
      106,
      targets,
      commitVersion,
      writer => PoofOfSmokePacket.Write(writer, packedPosition));
  }

  public OutboundPacketIntent CreateSmartTextIntent(
    PacketRgb color,
    NetworkText text,
    short widthLimit,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(text);
    if (widthLimit < 0 || widthLimit > MaximumNetworkTextWidth)
    {
      throw new ArgumentOutOfRangeException(nameof(widthLimit));
    }

    return CreateIntent(
      107,
      targets,
      commitVersion,
      writer =>
      {
        writer.WriteByte(color.Red);
        writer.WriteByte(color.Green);
        writer.WriteByte(color.Blue);
        NetworkTextCodec.Write(writer, text);
        writer.WriteInt16(widthLimit);
      });
  }

  public OutboundPacketIntent CreateInvasionWarningIntent(
    string localizationKey,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion,
    Packet82ModuleIntentProducer moduleProducer)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(localizationKey);
    ArgumentNullException.ThrowIfNull(moduleProducer);
    return moduleProducer.CreateIntent(
      (ushort)Packet82ModuleId.Text,
      new Packet82TextData(
        byte.MaxValue,
        NetworkText.Key(localizationKey),
        175,
        75,
        255),
      targets,
      commitVersion);
  }

  public OutboundPacketIntent CreateSpecialFxIntent(
    byte effectType,
    int x,
    int y,
    byte parameter,
    short style,
    byte flag,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    if (effectType is not 1 and not 2)
    {
      throw new ArgumentOutOfRangeException(
        nameof(effectType),
        "Version4 SpecialFX supports only tree-grow and fairy effect types.");
    }

    return CreateIntent(
      112,
      targets,
      commitVersion,
      writer => SpecialFXPacket.Write(writer, effectType, x, y, parameter, style, flag));
  }

  public OutboundPacketIntent CreateCrystalStartIntent(
    short x,
    short y,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    return CreateIntent(
      113,
      targets,
      commitVersion,
      writer => CrystalInvasionStartPacket.Write(writer, x, y));
  }

  public OutboundPacketIntent CreateCrystalWipeIntent(
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    return CreateEmptyIntent(114, targets, commitVersion);
  }

  public OutboundPacketIntent CreateCrystalWaitIntent(
    int waitTime,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    if (waitTime < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(waitTime));
    }

    return CreateIntent(
      116,
      targets,
      commitVersion,
      writer => CrystalInvasionSendWaitTimePacket.Write(writer, waitTime));
  }

  public OutboundPacketIntent CreateCombatTextStringIntent(
    float x,
    float y,
    PacketRgb color,
    NetworkText text,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    ValidateFinite(x, nameof(x));
    ValidateFinite(y, nameof(y));
    ArgumentNullException.ThrowIfNull(text);
    return CreateIntent(
      119,
      targets,
      commitVersion,
      writer => CombatTextStringPacket.Write(writer, x, y, color, text));
  }

  public OutboundPacketIntent CreateRemoveRevengeMarkerIntent(
    int markerId,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    return CreateIntent(
      127,
      targets,
      commitVersion,
      writer => RemoveRevengeMarkerPacket.Write(writer, markerId));
  }

  public OutboundPacketIntent CreateLegacySoundIntent(
    PacketVector2 position,
    ushort soundIndex,
    int? style,
    float? volume,
    float? pitchOffset,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    ValidateFinite(position.X, nameof(position));
    ValidateFinite(position.Y, nameof(position));
    ValidateOptionalFinite(volume, nameof(volume));
    ValidateOptionalFinite(pitchOffset, nameof(pitchOffset));
    return CreateIntent(
      132,
      targets,
      commitVersion,
      writer => PlayLegacySoundPacket.Write(
        writer,
        position,
        soundIndex,
        style,
        volume,
        pitchOffset));
  }

  public OutboundPacketIntent CreateSetMiscEventValuesIntent(
    byte eventKind,
    int value,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    if (eventKind > 2)
    {
      throw new ArgumentOutOfRangeException(nameof(eventKind));
    }

    return CreateIntent(
      140,
      targets,
      commitVersion,
      writer => SetMiscEventValuesPacket.Write(writer, eventKind, value));
  }

  public OutboundPacketIntent CreateLucyPopupIntent(
    byte messageSource,
    byte variation,
    PacketVector2 velocity,
    int positionX,
    int positionY,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    ValidateFinite(velocity.X, nameof(velocity));
    ValidateFinite(velocity.Y, nameof(velocity));
    return CreateIntent(
      141,
      targets,
      commitVersion,
      writer => RequestLucyPopupPacket.Write(
        writer,
        messageSource,
        variation,
        velocity,
        positionX,
        positionY));
  }

  public OutboundPacketIntent CreateCrystalSkipWaitIntent(
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    return CreateEmptyIntent(143, targets, commitVersion);
  }

  public OutboundPacketIntent CreateQuestEffectIntent(
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    return CreateEmptyIntent(144, targets, commitVersion);
  }

  private OutboundPacketIntent CreateEmptyIntent(
    byte messageId,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    return CreateIntent(messageId, targets, commitVersion, static _ => { });
  }

  private static OutboundPacketIntent CreateIntent(
    byte messageId,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion,
    Action<PacketWireWriter> writePayload)
  {
    ArgumentNullException.ThrowIfNull(targets);
    if (!commitVersion.IsValid)
    {
      throw new ArgumentException(
        "A presentation intent must reference an already committed version.",
        nameof(commitVersion));
    }

    SessionEpoch[] normalizedTargets = targets
      .Distinct()
      .OrderBy(epoch => epoch.Value)
      .ToArray();
    if (normalizedTargets.Length == 0)
    {
      throw new ArgumentException("A presentation intent needs at least one target.", nameof(targets));
    }

    if (normalizedTargets.Length > MaximumRecipientCount)
    {
      throw new ArgumentOutOfRangeException(
        nameof(targets),
        "A presentation intent exceeds the Version4 player recipient bound.");
    }

    if (normalizedTargets.Any(static epoch => !epoch.IsValid))
    {
      throw new ArgumentException(
        "Presentation recipients must carry valid session epochs.",
        nameof(targets));
    }

    ArgumentNullException.ThrowIfNull(writePayload);
    using MemoryStream stream = new();
    PacketWireWriter writer = new(stream, ushort.MaxValue - 3);
    writePayload(writer);
    return new OutboundPacketIntent(
      normalizedTargets,
      ignoreEpoch: null,
      messageId,
      stream.ToArray(),
      commitVersion);
  }

  private static void ValidatePlayerSlot(byte player)
  {
    if (player >= byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(player));
    }
  }

  private void ValidateTowerShield(ushort value, string parameterName)
  {
    if (value > _maximumTowerShieldStrength)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        "Tower shield strength exceeds the committed Version4 maximum.");
    }
  }

  private static void ValidateFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName, "Presentation coordinates must be finite.");
    }
  }

  private static void ValidateOptionalFinite(float? value, string parameterName)
  {
    if (value.HasValue)
    {
      ValidateFinite(value.Value, parameterName);
    }
  }
}
