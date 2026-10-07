using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public static class PacketWireFormats
{
    public static WireFormat<PacketRgb> Rgb { get; } = CreateRgb();
    public static WireFormat<NetworkText> NetworkText { get; } = WireFormat<NetworkText>.FromCodec("NetworkText", NetworkTextCodec.Write, NetworkTextCodec.Read);

    private static WireFormat<PacketRgb> CreateRgb()
    {
        var format = new WireFormatBuilder<PacketRgb>("RgbBytes");
        var red = format.Field(value => value.Red);
        var green = format.Field(value => value.Green);
        var blue = format.Field(value => value.Blue);
        format.BindConstructor(red, green, blue);
        return format.Build();
    }
}

public static class PacketAllDefinitions
{
    public static ProtocolManifest CreateProtocol()
    {
        var protocol = new ProtocolDefinition("TerrariaV4", version: "4", frame: PacketFrameLimits.Layout);
        foreach (var packet in CreateInput().Manifests)
        {
            string name = packet.MessageId switch
            {
                13 => "PlayerControls",
                50 => "PlayerBuffs",
                _ => packet.PacketType.Name
            };
            protocol.Register(name, packet);
        }

        return protocol.Build();
    }

    public static CompilationInputBundle CreateInput() => new(
    [
        PacketDefinitions.CreateKickPacket(),
        PacketDefinitions.CreateStatusTextSizePacket(),
        PacketDefinitions.CreateTileSectionPacket(),
        PacketDefinitions.CreateAreaTileChangePacket(),
        PacketDefinitions.CreateSyncNPCPacket(),
        PacketDefinitions.CreateNetModulesPacket(),
        PacketDefinitions.CreateTileEntitySharingPacket(),
        PacketDefinitions.CreateSmartTextMessagePacket(),
        PacketDefinitions.CreatePlayerHurtV2Packet(),
        PacketDefinitions.CreatePlayerDeathV2Packet(),
        PacketDefinitions.CreateHelloPacket(),
        PacketDefinitions.CreatePlayerInfoPacket(),
        PacketDefinitions.CreateSyncEquipmentPacket(),
        PacketDefinitions.CreateRequestWorldDataPacket(),
        PacketDefinitions.CreateSpawnTileDataPacket(),
        PacketDefinitions.CreateTileFrameSectionPacket(),
        PacketDefinitions.CreatePlayerSpawnPacket(),
        PacketDefinitions.CreatePlayerControlsPacket(),
        PacketDefinitions.CreatePlayerActivePacket(),
        PacketDefinitions.CreateSyncPlayerPacket(),
        PacketDefinitions.CreatePlayerLifeManaPacket(),
        PacketDefinitions.CreateTileManipulationPacket(),
        PacketDefinitions.CreateSetTimePacket(),
        PacketDefinitions.CreateToggleDoorStatePacket(),
        PacketDefinitions.CreateSyncItemPacket(),
        PacketDefinitions.CreateItemOwnerPacket(),
        PacketDefinitions.CreateUnusedMeleeStrikePacket(),
        PacketDefinitions.CreateSyncProjectilePacket(),
        PacketDefinitions.CreateDamageNPCPacket(),
        PacketDefinitions.CreateKillProjectilePacket(),
        PacketDefinitions.CreateTogglePVPPacket(),
        PacketDefinitions.CreateRequestChestOpenPacket(),
        PacketDefinitions.CreateSyncChestItemPacket(),
        PacketDefinitions.CreateSyncPlayerChestPacket(),
        PacketDefinitions.CreateChestUpdatesPacket(),
        PacketDefinitions.CreatePlayerHealPacket(),
        PacketDefinitions.CreateSyncPlayerZonePacket(),
        PacketDefinitions.CreateSendPasswordPacket(),
        PacketDefinitions.CreateReleaseItemOwnershipPacket(),
        PacketDefinitions.CreateSyncTalkNPCPacket(),
        PacketDefinitions.CreateItemRotationAndAnimationPacket(),
        PacketDefinitions.CreateUnknown42Packet(),
        PacketDefinitions.CreateManaEffectPacket(),
        PacketDefinitions.CreateTeamChangePacket(),
        PacketDefinitions.CreateOpenSignRequestPacket(),
        PacketDefinitions.CreateOpenSignResponsePacket(),
        PacketDefinitions.CreatePlayerBuffsPacket(),
        PacketDefinitions.CreateMiscDataSyncPacket(),
        PacketDefinitions.CreateLockAndUnlockPacket(),
        PacketDefinitions.CreateAddNPCBuffPacket(),
        PacketDefinitions.CreateNPCBuffsPacket(),
        PacketDefinitions.CreateAddPlayerBuffPvPPacket(),
        PacketDefinitions.CreateInstrumentSoundPacket(),
        PacketDefinitions.CreateHitSwitchPacket(),
        PacketDefinitions.CreateUnknown60Packet(),
        PacketDefinitions.CreateUnknown62Packet(),
        PacketDefinitions.CreateSyncTilePaintOrCoatingPacket(),
        PacketDefinitions.CreateSyncWallPaintOrCoatingPacket(),
        PacketDefinitions.CreateUnknown66Packet(),
        PacketDefinitions.CreateUnknown68Packet(),
        PacketDefinitions.CreateBugCatchingPacket(),
        PacketDefinitions.CreateBugReleasingPacket(),
        PacketDefinitions.CreateTravelMerchantItemsPacket(),
        PacketDefinitions.CreateRequestTeleportationByServerPacket(),
        PacketDefinitions.CreateAnglerQuestPacket(),
        PacketDefinitions.CreateQuestsCountSyncPacket(),
        PacketDefinitions.CreateTemporaryAnimationPacket(),
        PacketDefinitions.CreateInvasionProgressReportPacket(),
        PacketDefinitions.CreatePlaceObjectPacket(),
        PacketDefinitions.CreateSyncPlayerChestIndexPacket(),
        PacketDefinitions.CreateCombatTextIntPacket(),
        PacketDefinitions.CreatePlayerStealthPacket(),
        PacketDefinitions.CreateTileEntityPlacementPacket(),
        PacketDefinitions.CreateItemTweakerPacket(),
        PacketDefinitions.CreateItemFrameTryPlacingPacket(),
        PacketDefinitions.CreateInstancedItemPacket(),
        PacketDefinitions.CreateSyncExtraValuePacket(),
        PacketDefinitions.CreateMurderSomeoneElsesPortalPacket(),
        PacketDefinitions.CreateTeleportPlayerThroughPortalPacket(),
        PacketDefinitions.CreateMinionRestTargetUpdatePacket(),
        PacketDefinitions.CreateTeleportNPCThroughPortalPacket(),
        PacketDefinitions.CreateUpdateTowerShieldStrengthsPacket(),
        PacketDefinitions.CreateNebulaLevelupRequestPacket(),
        PacketDefinitions.CreateMoonlordHorrorPacket(),
        PacketDefinitions.CreateShopOverridePacket(),
        PacketDefinitions.CreateGemLockTogglePacket(),
        PacketDefinitions.CreatePoofOfSmokePacket(),
        PacketDefinitions.CreateWiredCannonShotPacket(),
        PacketDefinitions.CreateMassWireOperationPacket(),
        PacketDefinitions.CreateMassWireOperationPayPacket(),
        PacketDefinitions.CreateTogglePartyPacket(),
        PacketDefinitions.CreateSpecialFXPacket(),
        PacketDefinitions.CreateCrystalInvasionStartPacket(),
        PacketDefinitions.CreateMinionAttackTargetUpdatePacket(),
        PacketDefinitions.CreateCrystalInvasionSendWaitTimePacket(),
        PacketDefinitions.CreateCombatTextStringPacket(),
        PacketDefinitions.CreateEmojiPacket(),
        PacketDefinitions.CreateSyncItemsWithShimmerPacket(),
        PacketDefinitions.CreateSyncItemCannotBeTakenByEnemiesPacket(),
        PacketDefinitions.CreateSyncItemDespawnPacket(),
        PacketDefinitions.CreateTeamChangeFromUIPacket(),
        PacketDefinitions.CreateTEDisplayDollDataSyncPacket(),
        PacketDefinitions.CreateRequestTileEntityInteractionPacket(),
        PacketDefinitions.CreateWeaponsRackTryPlacingPacket(),
        PacketDefinitions.CreateTEHatRackItemSyncPacket(),
        PacketDefinitions.CreateSyncTilePickingPacket(),
        PacketDefinitions.CreateSyncRevengeMarkerPacket(),
        PacketDefinitions.CreateRemoveRevengeMarkerPacket(),
        PacketDefinitions.CreateLandGolfBallInCupPacket(),
        PacketDefinitions.CreateFinishedConnectingToServerPacket(),
        PacketDefinitions.CreateFishOutNPCPacket(),
        PacketDefinitions.CreateTamperWithNPCPacket(),
        PacketDefinitions.CreatePlayLegacySoundPacket(),
        PacketDefinitions.CreateFoodPlatterTryPlacingPacket(),
        PacketDefinitions.CreateUpdatePlayerLuckFactorsPacket(),
        PacketDefinitions.CreateDeadPlayerPacket(),
        PacketDefinitions.CreateSyncCavernMonsterTypePacket(),
        PacketDefinitions.CreateRequestNPCBuffRemovalPacket(),
        PacketDefinitions.CreateSetCountsAsHostForGameplayPacket(),
        PacketDefinitions.CreateSetMiscEventValuesPacket(),
        PacketDefinitions.CreateRequestLucyPopupPacket(),
        PacketDefinitions.CreateSyncProjectileTrackersPacket(),
        PacketDefinitions.CreateCrystalInvasionRequestedToSkipWaitTimePacket(),
        PacketDefinitions.CreateRequestQuestEffectPacket(),
        PacketDefinitions.CreateShimmerActionsPacket(),
        PacketDefinitions.CreateSyncLoadoutPacket(),
        PacketDefinitions.CreateDeadCellsDisplayJarTryPlacingPacket(),
        PacketDefinitions.CreateSpectatePlayerPacket(),
        PacketDefinitions.CreateItemUseSoundPacket(),
        PacketDefinitions.CreateNPCDebuffDamagePacket(),
        PacketDefinitions.CreatePingPacket(),
        PacketDefinitions.CreateSyncChestSizePacket(),
        PacketDefinitions.CreateTELeashedEntityAnchorPlaceItemPacket(),
        PacketDefinitions.CreateExtraSpawnSectionLoadedPacket(),
        PacketDefinitions.CreateRequestSectionPacket(),
        PacketDefinitions.CreateItemPositionPacket(),
        PacketDefinitions.CreateHostTokenPacket(),
        PacketDefinitions.CreateRequestPasswordPacket(),
        PacketDefinitions.CreateLiquidUpdatePacket(),
        PacketDefinitions.CreateInitialSpawnPacket(),
        PacketDefinitions.CreateUniqueTownNPCInfoSyncRequestPacket(),
        PacketDefinitions.CreateUniqueTownNPCInfoSyncResponsePacket(),
        PacketDefinitions.CreateUnknown57Packet(),
        PacketDefinitions.CreateSpawnBossUseLicenseStartEventPacket(),
        PacketDefinitions.CreateTeleportEntityPacket(),
        PacketDefinitions.CreateChestNameRequestPacket(),
        PacketDefinitions.CreateChestNameResponsePacket(),
        PacketDefinitions.CreateAnglerQuestFinishedPacket(),
        PacketDefinitions.CreateQuickStackChestsRequestPacket(),
        PacketDefinitions.CreateQuickStackChestsResponsePacket(),
        PacketDefinitions.CreateSyncEmoteBubblePacket(),
        PacketDefinitions.CreateDevCommandsPacket(),
        PacketDefinitions.CreateAchievementMessageNPCKilledPacket(),
        PacketDefinitions.CreateAchievementMessageEventHappenedPacket(),
        PacketDefinitions.CreateCrystalInvasionWipeAllTheThingsssPacket(),
        PacketDefinitions.CreateWorldDataPacket(),
        PacketDefinitions.CreateUnknown15Packet(),
        PacketDefinitions.CreateUnused25Packet(),
        PacketDefinitions.CreateUnused26Packet(),
        PacketDefinitions.CreateUnknown44Packet(),
        PacketDefinitions.CreateUnknown67Packet(),
        PacketDefinitions.CreateUnused83Packet(),
        PacketDefinitions.CreateSocialHandshakePacket(),
        PacketDefinitions.CreateNeverCalledPacket(),
        PacketDefinitions.CreateClientSyncedInventoryPacket()
    ], frame: PacketFrameLimits.Layout);
}

public static class PacketTopTenDefinitions
{
    public static CompilationInputBundle CreateInput() => new(
    [
        PacketDefinitions.CreateKickPacket(),
        PacketDefinitions.CreateStatusTextSizePacket(),
        PacketDefinitions.CreateTileSectionPacket(),
        PacketDefinitions.CreateAreaTileChangePacket(),
        PacketDefinitions.CreateSyncNPCPacket(),
        PacketDefinitions.CreateNetModulesPacket(),
        PacketDefinitions.CreateTileEntitySharingPacket(),
        PacketDefinitions.CreateSmartTextMessagePacket(),
        PacketDefinitions.CreatePlayerHurtV2Packet(),
        PacketDefinitions.CreatePlayerDeathV2Packet()
    ], frame: PacketFrameLimits.Layout);
}

public static class TileSectionDefinitionHost
{
    public const int MaximumCompressedBodyBytes = PacketFrameLimits.MaximumPayloadBytes;
    public static CompilationInputBundle CreateInput() => new([PacketDefinitions.CreateTileSectionPacket()], frame: PacketFrameLimits.Layout);
}

public static class AreaTileChangeDefinitionHost
{
    public static CompilationInputBundle CreateInput() => new([PacketDefinitions.CreateAreaTileChangePacket()], frame: PacketFrameLimits.Layout);
}

public static class PacketComplexDefinitionHosts
{
    public static CompilationInputBundle CreateSyncNPCInput() => new([PacketDefinitions.CreateSyncNPCPacket()], frame: PacketFrameLimits.Layout);
    public static CompilationInputBundle CreateNetModulesInput() => new([PacketDefinitions.CreateNetModulesPacket()], frame: PacketFrameLimits.Layout);
    public static CompilationInputBundle CreateTileEntitySharingInput() => new([PacketDefinitions.CreateTileEntitySharingPacket()], frame: PacketFrameLimits.Layout);
}

public static class WorldDataDefinitions
{
    public static CompilationInputBundle CreateInput() => new([PacketDefinitions.CreateWorldDataPacket()], frame: PacketFrameLimits.Layout);
}

public static class PacketDefinitionHosts
{
    public static CompilationInputBundle CreateKickInput() => new([PacketDefinitions.CreateKickPacket()], frame: PacketFrameLimits.Layout);
    public static CompilationInputBundle CreateStatusTextSizeInput() => new([PacketDefinitions.CreateStatusTextSizePacket()], frame: PacketFrameLimits.Layout);
    public static CompilationInputBundle CreateSmartTextMessageInput() => new([PacketDefinitions.CreateSmartTextMessagePacket()], frame: PacketFrameLimits.Layout);
    public static CompilationInputBundle CreatePlayerHurtV2Input() => new([PacketDefinitions.CreatePlayerHurtV2Packet()], frame: PacketFrameLimits.Layout);
    public static CompilationInputBundle CreatePlayerDeathV2Input() => new([PacketDefinitions.CreatePlayerDeathV2Packet()], frame: PacketFrameLimits.Layout);
}

public static class PacketBasicDefinitionHosts
{
    public static CompilationInputBundle CreateInput() => new(
    [
        PacketDefinitions.CreateHelloPacket(),
        PacketDefinitions.CreatePlayerInfoPacket(),
        PacketDefinitions.CreateSyncEquipmentPacket(),
        PacketDefinitions.CreateRequestWorldDataPacket(),
        PacketDefinitions.CreateSpawnTileDataPacket(),
        PacketDefinitions.CreateTileFrameSectionPacket(),
        PacketDefinitions.CreatePlayerSpawnPacket(),
        PacketDefinitions.CreatePlayerControlsPacket(),
        PacketDefinitions.CreatePlayerActivePacket(),
        PacketDefinitions.CreateSyncPlayerPacket(),
        PacketDefinitions.CreatePlayerLifeManaPacket(),
        PacketDefinitions.CreateTileManipulationPacket(),
        PacketDefinitions.CreateSetTimePacket(),
        PacketDefinitions.CreateToggleDoorStatePacket(),
        PacketDefinitions.CreateSyncItemPacket(),
        PacketDefinitions.CreateItemOwnerPacket(),
        PacketDefinitions.CreateUnusedMeleeStrikePacket(),
        PacketDefinitions.CreateSyncProjectilePacket(),
        PacketDefinitions.CreateDamageNPCPacket(),
        PacketDefinitions.CreateKillProjectilePacket(),
        PacketDefinitions.CreateTogglePVPPacket(),
        PacketDefinitions.CreateRequestChestOpenPacket(),
        PacketDefinitions.CreateSyncChestItemPacket(),
        PacketDefinitions.CreateSyncPlayerChestPacket(),
        PacketDefinitions.CreateChestUpdatesPacket(),
        PacketDefinitions.CreatePlayerHealPacket(),
        PacketDefinitions.CreateSyncPlayerZonePacket(),
        PacketDefinitions.CreateSendPasswordPacket(),
        PacketDefinitions.CreateReleaseItemOwnershipPacket(),
        PacketDefinitions.CreateSyncTalkNPCPacket(),
        PacketDefinitions.CreateItemRotationAndAnimationPacket(),
        PacketDefinitions.CreateUnknown42Packet(),
        PacketDefinitions.CreateManaEffectPacket(),
        PacketDefinitions.CreateTeamChangePacket(),
        PacketDefinitions.CreateOpenSignRequestPacket(),
        PacketDefinitions.CreateOpenSignResponsePacket(),
        PacketDefinitions.CreatePlayerBuffsPacket(),
        PacketDefinitions.CreateMiscDataSyncPacket(),
        PacketDefinitions.CreateLockAndUnlockPacket(),
        PacketDefinitions.CreateAddNPCBuffPacket(),
        PacketDefinitions.CreateNPCBuffsPacket(),
        PacketDefinitions.CreateAddPlayerBuffPvPPacket(),
        PacketDefinitions.CreateInstrumentSoundPacket(),
        PacketDefinitions.CreateHitSwitchPacket(),
        PacketDefinitions.CreateUnknown60Packet(),
        PacketDefinitions.CreateUnknown62Packet(),
        PacketDefinitions.CreateSyncTilePaintOrCoatingPacket(),
        PacketDefinitions.CreateSyncWallPaintOrCoatingPacket(),
        PacketDefinitions.CreateUnknown66Packet(),
        PacketDefinitions.CreateUnknown68Packet(),
        PacketDefinitions.CreateBugCatchingPacket(),
        PacketDefinitions.CreateBugReleasingPacket(),
        PacketDefinitions.CreateTravelMerchantItemsPacket(),
        PacketDefinitions.CreateRequestTeleportationByServerPacket(),
        PacketDefinitions.CreateAnglerQuestPacket(),
        PacketDefinitions.CreateQuestsCountSyncPacket(),
        PacketDefinitions.CreateTemporaryAnimationPacket(),
        PacketDefinitions.CreateInvasionProgressReportPacket(),
        PacketDefinitions.CreatePlaceObjectPacket(),
        PacketDefinitions.CreateSyncPlayerChestIndexPacket(),
        PacketDefinitions.CreateCombatTextIntPacket(),
        PacketDefinitions.CreatePlayerStealthPacket(),
        PacketDefinitions.CreateTileEntityPlacementPacket(),
        PacketDefinitions.CreateItemTweakerPacket(),
        PacketDefinitions.CreateItemFrameTryPlacingPacket(),
        PacketDefinitions.CreateInstancedItemPacket(),
        PacketDefinitions.CreateSyncExtraValuePacket(),
        PacketDefinitions.CreateMurderSomeoneElsesPortalPacket(),
        PacketDefinitions.CreateTeleportPlayerThroughPortalPacket(),
        PacketDefinitions.CreateMinionRestTargetUpdatePacket(),
        PacketDefinitions.CreateTeleportNPCThroughPortalPacket(),
        PacketDefinitions.CreateUpdateTowerShieldStrengthsPacket(),
        PacketDefinitions.CreateNebulaLevelupRequestPacket(),
        PacketDefinitions.CreateMoonlordHorrorPacket(),
        PacketDefinitions.CreateShopOverridePacket(),
        PacketDefinitions.CreateGemLockTogglePacket(),
        PacketDefinitions.CreatePoofOfSmokePacket(),
        PacketDefinitions.CreateWiredCannonShotPacket(),
        PacketDefinitions.CreateMassWireOperationPacket(),
        PacketDefinitions.CreateMassWireOperationPayPacket(),
        PacketDefinitions.CreateTogglePartyPacket(),
        PacketDefinitions.CreateSpecialFXPacket(),
        PacketDefinitions.CreateCrystalInvasionStartPacket(),
        PacketDefinitions.CreateMinionAttackTargetUpdatePacket(),
        PacketDefinitions.CreateCrystalInvasionSendWaitTimePacket(),
        PacketDefinitions.CreateCombatTextStringPacket(),
        PacketDefinitions.CreateEmojiPacket(),
        PacketDefinitions.CreateSyncItemsWithShimmerPacket(),
        PacketDefinitions.CreateSyncItemCannotBeTakenByEnemiesPacket(),
        PacketDefinitions.CreateSyncItemDespawnPacket(),
        PacketDefinitions.CreateTeamChangeFromUIPacket()
    ], frame: PacketFrameLimits.Layout);
}

public static class Packet48To114Definitions
{
    public static CompilationInputBundle CreateInput() => new(
    [
        PacketDefinitions.CreateRequestPasswordPacket(),
        PacketDefinitions.CreateLiquidUpdatePacket(),
        PacketDefinitions.CreateInitialSpawnPacket(),
        PacketDefinitions.CreateUniqueTownNPCInfoSyncRequestPacket(),
        PacketDefinitions.CreateUniqueTownNPCInfoSyncResponsePacket(),
        PacketDefinitions.CreateUnknown57Packet(),
        PacketDefinitions.CreateSpawnBossUseLicenseStartEventPacket(),
        PacketDefinitions.CreateTeleportEntityPacket(),
        PacketDefinitions.CreateChestNameRequestPacket(),
        PacketDefinitions.CreateChestNameResponsePacket(),
        PacketDefinitions.CreateAnglerQuestFinishedPacket(),
        PacketDefinitions.CreateQuickStackChestsRequestPacket(),
        PacketDefinitions.CreateQuickStackChestsResponsePacket(),
        PacketDefinitions.CreateSyncEmoteBubblePacket(),
        PacketDefinitions.CreateDevCommandsPacket(),
        PacketDefinitions.CreateAchievementMessageNPCKilledPacket(),
        PacketDefinitions.CreateAchievementMessageEventHappenedPacket(),
        PacketDefinitions.CreateCrystalInvasionWipeAllTheThingsssPacket()
    ], frame: PacketFrameLimits.Layout);
}

public static class Packet121To140Definitions
{
    public static CompilationInputBundle CreateInput() => new(
    [
        PacketDefinitions.CreateTEDisplayDollDataSyncPacket(),
        PacketDefinitions.CreateRequestTileEntityInteractionPacket(),
        PacketDefinitions.CreateWeaponsRackTryPlacingPacket(),
        PacketDefinitions.CreateTEHatRackItemSyncPacket(),
        PacketDefinitions.CreateSyncTilePickingPacket(),
        PacketDefinitions.CreateSyncRevengeMarkerPacket(),
        PacketDefinitions.CreateRemoveRevengeMarkerPacket(),
        PacketDefinitions.CreateLandGolfBallInCupPacket(),
        PacketDefinitions.CreateFinishedConnectingToServerPacket(),
        PacketDefinitions.CreateFishOutNPCPacket(),
        PacketDefinitions.CreateTamperWithNPCPacket(),
        PacketDefinitions.CreatePlayLegacySoundPacket(),
        PacketDefinitions.CreateFoodPlatterTryPlacingPacket(),
        PacketDefinitions.CreateUpdatePlayerLuckFactorsPacket(),
        PacketDefinitions.CreateDeadPlayerPacket(),
        PacketDefinitions.CreateSyncCavernMonsterTypePacket(),
        PacketDefinitions.CreateRequestNPCBuffRemovalPacket(),
        PacketDefinitions.CreateSetCountsAsHostForGameplayPacket(),
        PacketDefinitions.CreateSetMiscEventValuesPacket()
    ], frame: PacketFrameLimits.Layout);
}

public static class Packet141To161Definitions
{
    public static CompilationInputBundle CreateInput() => new(
    [
        PacketDefinitions.CreateRequestLucyPopupPacket(),
        PacketDefinitions.CreateSyncProjectileTrackersPacket(),
        PacketDefinitions.CreateCrystalInvasionRequestedToSkipWaitTimePacket(),
        PacketDefinitions.CreateRequestQuestEffectPacket(),
        PacketDefinitions.CreateShimmerActionsPacket(),
        PacketDefinitions.CreateSyncLoadoutPacket(),
        PacketDefinitions.CreateDeadCellsDisplayJarTryPlacingPacket(),
        PacketDefinitions.CreateSpectatePlayerPacket(),
        PacketDefinitions.CreateItemUseSoundPacket(),
        PacketDefinitions.CreateNPCDebuffDamagePacket(),
        PacketDefinitions.CreatePingPacket(),
        PacketDefinitions.CreateSyncChestSizePacket(),
        PacketDefinitions.CreateTELeashedEntityAnchorPlaceItemPacket(),
        PacketDefinitions.CreateExtraSpawnSectionLoadedPacket(),
        PacketDefinitions.CreateRequestSectionPacket(),
        PacketDefinitions.CreateItemPositionPacket(),
        PacketDefinitions.CreateHostTokenPacket()
    ], frame: PacketFrameLimits.Layout);
}

public static class PacketReservedDefinitions
{
    public static CompilationInputBundle CreateInput() => new(
    [
        PacketDefinitions.CreateUnknown15Packet(),
        PacketDefinitions.CreateUnused25Packet(),
        PacketDefinitions.CreateUnused26Packet(),
        PacketDefinitions.CreateUnknown44Packet(),
        PacketDefinitions.CreateUnknown67Packet(),
        PacketDefinitions.CreateUnused83Packet(),
        PacketDefinitions.CreateSocialHandshakePacket(),
        PacketDefinitions.CreateNeverCalledPacket(),
        PacketDefinitions.CreateClientSyncedInventoryPacket()
    ], frame: PacketFrameLimits.Layout);
}

internal static class PacketDefinitions
{
    internal static PacketGraphManifest CreateKickPacket()
    {
        var graph = new PacketGraph<KickPacket>(2, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        graph.Field(KickPacket.Members.Text);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(KickPacket.Write)
            .ParameterList(KickPacket.Members.Text)
            .Sizeof(KickPacket.Members.Text);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(KickPacket.Read)
            .ReturnValue(KickPacket.Members.Text);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateStatusTextSizePacket()
    {
        var graph = new PacketGraph<StatusTextSizePacket>(9, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        graph.Field(StatusTextSizePacket.Members.Value);
        graph.Field(StatusTextSizePacket.Members.Text);
        graph.Field(StatusTextSizePacket.Members.Flags);
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(StatusTextSizePacket.Write)
            .ParameterList(StatusTextSizePacket.Members.Value, StatusTextSizePacket.Members.Text, StatusTextSizePacket.Members.Flags)
            .Sizeof(StatusTextSizePacket.Members.Value, StatusTextSizePacket.Members.Text, StatusTextSizePacket.Members.Flags);
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(StatusTextSizePacket.Read)
            .ReturnValue(StatusTextSizePacket.Members.Value, StatusTextSizePacket.Members.Text, StatusTextSizePacket.Members.Flags);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTileSectionPacket()
    {
        var graph = new PacketGraph<TileSectionPacket>(10, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies(
                ProtocolInputs.Members.FrameImportant,
                ProtocolInputs.Members.AllowsSaveCompressionBatching,
                ProtocolInputs.Members.TileEntityCodecs
            );
        graph.Field(TileSectionPacket.Members.StartX);
        graph.Field(TileSectionPacket.Members.StartY);
        graph.Field(TileSectionPacket.Members.Width);
        graph.Field(TileSectionPacket.Members.Height);
        graph.Field(TileSectionPacket.Members.Tiles);
        graph.Field(TileSectionPacket.Members.Chests);
        graph.Field(TileSectionPacket.Members.Signs);
        graph.Field(TileSectionPacket.Members.TileEntities);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(TileSectionPacket.WriteCompressedBody)
            .ParameterList(
                TileSectionPacket.Members.StartX,
                TileSectionPacket.Members.StartY,
                TileSectionPacket.Members.Width,
                TileSectionPacket.Members.Height,
                TileSectionPacket.Members.Tiles,
                TileSectionPacket.Members.Chests,
                TileSectionPacket.Members.Signs,
                TileSectionPacket.Members.TileEntities,
                ProtocolInputs.Members.FrameImportant,
                ProtocolInputs.Members.AllowsSaveCompressionBatching,
                ProtocolInputs.Members.TileEntityCodecs
            )
            .Sizeof(
                TileSectionPacket.Members.StartX,
                TileSectionPacket.Members.StartY,
                TileSectionPacket.Members.Width,
                TileSectionPacket.Members.Height,
                TileSectionPacket.Members.Tiles,
                TileSectionPacket.Members.Chests,
                TileSectionPacket.Members.Signs,
                TileSectionPacket.Members.TileEntities
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(TileSectionPacket.ReadCompressedBody)
            .ParameterList(ProtocolInputs.Members.FrameImportant, ProtocolInputs.Members.TileEntityCodecs)
            .ReturnValue(
                TileSectionPacket.Members.StartX,
                TileSectionPacket.Members.StartY,
                TileSectionPacket.Members.Width,
                TileSectionPacket.Members.Height,
                TileSectionPacket.Members.Tiles,
                TileSectionPacket.Members.Chests,
                TileSectionPacket.Members.Signs,
                TileSectionPacket.Members.TileEntities
            )
            .Sizeof(
                TileSectionPacket.Members.StartX,
                TileSectionPacket.Members.StartY,
                TileSectionPacket.Members.Width,
                TileSectionPacket.Members.Height,
                TileSectionPacket.Members.Tiles,
                TileSectionPacket.Members.Chests,
                TileSectionPacket.Members.Signs,
                TileSectionPacket.Members.TileEntities
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateAreaTileChangePacket()
    {
        var graph = new PacketGraph<AreaTileChangePacket>(20, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies(ProtocolInputs.Members.FrameImportant, ProtocolInputs.Members.IsServer);
        graph.Field(AreaTileChangePacket.Members.StartX);
        graph.Field(AreaTileChangePacket.Members.StartY);
        graph.Field(AreaTileChangePacket.Members.Width);
        graph.Field(AreaTileChangePacket.Members.Height);
        graph.Field(AreaTileChangePacket.Members.ChangeType);
        graph.Field(AreaTileChangePacket.Members.Tiles);
        graph.FunctionDeclaration()
            .FunctionName(AreaTileChangePacket.WriteTileBytes)
            .ParameterList(
                AreaTileChangePacket.Members.Width,
                AreaTileChangePacket.Members.Height,
                ProtocolInputs.Members.FrameImportant,
                ProtocolInputs.Members.IsServer,
                AreaTileChangePacket.Members.Tiles
            )
            .Sizeof(AreaTileChangePacket.Members.Tiles);
        graph.FunctionDeclaration()
            .FunctionName(AreaTileChangePacket.ReadTileBytes)
            .ParameterList(AreaTileChangePacket.Members.Width, AreaTileChangePacket.Members.Height, ProtocolInputs.Members.FrameImportant)
            .ReturnValue(AreaTileChangePacket.Members.Tiles)
            .Sizeof(AreaTileChangePacket.Members.Tiles);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncNPCPacket()
    {
        var graph = new PacketGraph<SyncNPCPacket>(23, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies(ProtocolInputs.Members.CatchableTypes, ProtocolInputs.Members.LifeWidthResolver);
        graph.Field(SyncNPCPacket.Members.NpcSlot);
        graph.Field(SyncNPCPacket.Members.PositionX);
        graph.Field(SyncNPCPacket.Members.PositionY);
        graph.Field(SyncNPCPacket.Members.VelocityX);
        graph.Field(SyncNPCPacket.Members.VelocityY);
        graph.Field(SyncNPCPacket.Members.Target);
        graph.Field(SyncNPCPacket.Members.DirectionPositive);
        graph.Field(SyncNPCPacket.Members.DirectionYPositive);
        graph.Field(SyncNPCPacket.Members.SpriteDirectionPositive);
        graph.Field(SyncNPCPacket.Members.FullLife);
        graph.Field(SyncNPCPacket.Members.SpawnedFromStatue);
        graph.Field(SyncNPCPacket.Members.SpawnNeedsSyncing);
        graph.Field(SyncNPCPacket.Members.Shimmering);
        graph.Field(SyncNPCPacket.Members.Ai);
        graph.Field(SyncNPCPacket.Members.NetId);
        graph.Field(SyncNPCPacket.Members.PlayerCount);
        graph.Field(SyncNPCPacket.Members.Difficulty);
        graph.Field(SyncNPCPacket.Members.Life);
        graph.Field(SyncNPCPacket.Members.EncodedLifeWidth);
        graph.Field(SyncNPCPacket.Members.CatchableReleaseOwner);
        graph.FunctionDeclaration(lengthRange: new ByteRange(24, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(SyncNPCPacket.WriteFunction)
            .ParameterList(
                SyncNPCPacket.Members.NpcSlot,
                SyncNPCPacket.Members.PositionX,
                SyncNPCPacket.Members.PositionY,
                SyncNPCPacket.Members.VelocityX,
                SyncNPCPacket.Members.VelocityY,
                SyncNPCPacket.Members.Target,
                SyncNPCPacket.Members.DirectionPositive,
                SyncNPCPacket.Members.DirectionYPositive,
                SyncNPCPacket.Members.SpriteDirectionPositive,
                SyncNPCPacket.Members.FullLife,
                SyncNPCPacket.Members.SpawnedFromStatue,
                SyncNPCPacket.Members.SpawnNeedsSyncing,
                SyncNPCPacket.Members.Shimmering,
                SyncNPCPacket.Members.Ai,
                SyncNPCPacket.Members.NetId,
                SyncNPCPacket.Members.PlayerCount,
                SyncNPCPacket.Members.Difficulty,
                SyncNPCPacket.Members.Life,
                SyncNPCPacket.Members.EncodedLifeWidth,
                SyncNPCPacket.Members.CatchableReleaseOwner,
                ProtocolInputs.Members.CatchableTypes,
                ProtocolInputs.Members.LifeWidthResolver
            )
            .Sizeof(
                SyncNPCPacket.Members.NpcSlot,
                SyncNPCPacket.Members.PositionX,
                SyncNPCPacket.Members.PositionY,
                SyncNPCPacket.Members.VelocityX,
                SyncNPCPacket.Members.VelocityY,
                SyncNPCPacket.Members.Target,
                SyncNPCPacket.Members.DirectionPositive,
                SyncNPCPacket.Members.DirectionYPositive,
                SyncNPCPacket.Members.SpriteDirectionPositive,
                SyncNPCPacket.Members.FullLife,
                SyncNPCPacket.Members.SpawnedFromStatue,
                SyncNPCPacket.Members.SpawnNeedsSyncing,
                SyncNPCPacket.Members.Shimmering,
                SyncNPCPacket.Members.Ai,
                SyncNPCPacket.Members.NetId,
                SyncNPCPacket.Members.PlayerCount,
                SyncNPCPacket.Members.Difficulty,
                SyncNPCPacket.Members.Life,
                SyncNPCPacket.Members.EncodedLifeWidth,
                SyncNPCPacket.Members.CatchableReleaseOwner
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(24, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(SyncNPCPacket.Read)
            .ParameterList(ProtocolInputs.Members.CatchableTypes)
            .ReturnValue(
                SyncNPCPacket.Members.NpcSlot,
                SyncNPCPacket.Members.PositionX,
                SyncNPCPacket.Members.PositionY,
                SyncNPCPacket.Members.VelocityX,
                SyncNPCPacket.Members.VelocityY,
                SyncNPCPacket.Members.Target,
                SyncNPCPacket.Members.DirectionPositive,
                SyncNPCPacket.Members.DirectionYPositive,
                SyncNPCPacket.Members.SpriteDirectionPositive,
                SyncNPCPacket.Members.FullLife,
                SyncNPCPacket.Members.SpawnedFromStatue,
                SyncNPCPacket.Members.SpawnNeedsSyncing,
                SyncNPCPacket.Members.Shimmering,
                SyncNPCPacket.Members.Ai,
                SyncNPCPacket.Members.NetId,
                SyncNPCPacket.Members.PlayerCount,
                SyncNPCPacket.Members.Difficulty,
                SyncNPCPacket.Members.Life,
                SyncNPCPacket.Members.EncodedLifeWidth,
                SyncNPCPacket.Members.CatchableReleaseOwner
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateNetModulesPacket()
    {
        var graph = new PacketGraph<NetModulesPacket>(82, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies(
                ProtocolInputs.Members.ModuleCodecs,
                ProtocolInputs.Members.TagEffectNpcSlotCount,
                ProtocolInputs.Members.TagEffectUsesProcTimes
            );
        graph.Field(NetModulesPacket.Members.ModuleId);
        graph.Field(NetModulesPacket.Members.Data);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(NetModulesPacket.Write)
            .ParameterList(
                NetModulesPacket.Members.ModuleId,
                NetModulesPacket.Members.Data,
                ProtocolInputs.Members.ModuleCodecs,
                ProtocolInputs.Members.TagEffectNpcSlotCount,
                ProtocolInputs.Members.TagEffectUsesProcTimes
            )
            .Sizeof(NetModulesPacket.Members.ModuleId, NetModulesPacket.Members.Data);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(NetModulesPacket.Read)
            .ParameterList(
                ProtocolInputs.Members.ModuleCodecs,
                ProtocolInputs.Members.TagEffectNpcSlotCount,
                ProtocolInputs.Members.TagEffectUsesProcTimes
            )
            .ReturnValue(NetModulesPacket.Members.ModuleId, NetModulesPacket.Members.Data);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTileEntitySharingPacket()
    {
        var graph = new PacketGraph<TileEntitySharingPacket>(86, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies(ProtocolInputs.Members.TileEntityCodecs);
        graph.Field(TileEntitySharingPacket.Members.EntityId);
        graph.Field(TileEntitySharingPacket.Members.Exists);
        graph.Field(TileEntitySharingPacket.Members.Entity);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(TileEntitySharingPacket.Write)
            .ParameterList(
                TileEntitySharingPacket.Members.EntityId,
                TileEntitySharingPacket.Members.Exists,
                TileEntitySharingPacket.Members.Entity,
                ProtocolInputs.Members.TileEntityCodecs
            )
            .Sizeof(TileEntitySharingPacket.Members.EntityId, TileEntitySharingPacket.Members.Exists, TileEntitySharingPacket.Members.Entity);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(TileEntitySharingPacket.Read)
            .ParameterList(ProtocolInputs.Members.TileEntityCodecs)
            .ReturnValue(TileEntitySharingPacket.Members.EntityId, TileEntitySharingPacket.Members.Exists, TileEntitySharingPacket.Members.Entity);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSmartTextMessagePacket()
    {
        var graph = new PacketGraph<SmartTextMessagePacket>(107, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        graph.Field(SmartTextMessagePacket.Members.Color, PacketWireFormats.Rgb);
        graph.Field(SmartTextMessagePacket.Members.Text, PacketWireFormats.NetworkText);
        graph.Field(SmartTextMessagePacket.Members.WidthLimit);
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePlayerHurtV2Packet()
    {
        var graph = new PacketGraph<PlayerHurtV2Packet>(117, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(PlayerHurtV2Packet.Members.Player);
        graph.Field(PlayerHurtV2Packet.Members.Reason);
        graph.Field(PlayerHurtV2Packet.Members.Damage);
        graph.Field(PlayerHurtV2Packet.Members.DirectionCode);
        graph.Field(PlayerHurtV2Packet.Members.HitFlags);
        graph.Field(PlayerHurtV2Packet.Members.CooldownCounter);
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(PlayerHurtV2Packet.Write)
            .ParameterList(
                PlayerHurtV2Packet.Members.Player,
                PlayerHurtV2Packet.Members.Reason,
                PlayerHurtV2Packet.Members.Damage,
                PlayerHurtV2Packet.Members.DirectionCode,
                PlayerHurtV2Packet.Members.HitFlags,
                PlayerHurtV2Packet.Members.CooldownCounter
            )
            .Sizeof(
                PlayerHurtV2Packet.Members.Player,
                PlayerHurtV2Packet.Members.Reason,
                PlayerHurtV2Packet.Members.Damage,
                PlayerHurtV2Packet.Members.DirectionCode,
                PlayerHurtV2Packet.Members.HitFlags,
                PlayerHurtV2Packet.Members.CooldownCounter
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(PlayerHurtV2Packet.Read)
            .ReturnValue(
                PlayerHurtV2Packet.Members.Player,
                PlayerHurtV2Packet.Members.Reason,
                PlayerHurtV2Packet.Members.Damage,
                PlayerHurtV2Packet.Members.DirectionCode,
                PlayerHurtV2Packet.Members.HitFlags,
                PlayerHurtV2Packet.Members.CooldownCounter
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePlayerDeathV2Packet()
    {
        var graph = new PacketGraph<PlayerDeathV2Packet>(118, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(PlayerDeathV2Packet.Members.Player);
        graph.Field(PlayerDeathV2Packet.Members.Reason);
        graph.Field(PlayerDeathV2Packet.Members.Damage);
        graph.Field(PlayerDeathV2Packet.Members.DirectionCode);
        graph.Field(PlayerDeathV2Packet.Members.DeathFlags);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(PlayerDeathV2Packet.Write)
            .ParameterList(
                PlayerDeathV2Packet.Members.Player,
                PlayerDeathV2Packet.Members.Reason,
                PlayerDeathV2Packet.Members.Damage,
                PlayerDeathV2Packet.Members.DirectionCode,
                PlayerDeathV2Packet.Members.DeathFlags
            )
            .Sizeof(
                PlayerDeathV2Packet.Members.Player,
                PlayerDeathV2Packet.Members.Reason,
                PlayerDeathV2Packet.Members.Damage,
                PlayerDeathV2Packet.Members.DirectionCode,
                PlayerDeathV2Packet.Members.DeathFlags
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(PlayerDeathV2Packet.Read)
            .ReturnValue(
                PlayerDeathV2Packet.Members.Player,
                PlayerDeathV2Packet.Members.Reason,
                PlayerDeathV2Packet.Members.Damage,
                PlayerDeathV2Packet.Members.DirectionCode,
                PlayerDeathV2Packet.Members.DeathFlags
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateHelloPacket()
    {
        var graph = new PacketGraph<HelloPacket>(1, wireDirection: WireDirection.ClientToServer)
            .ExternalDependencies();
        graph.Field(HelloPacket.Members.Version);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(HelloPacket.Write)
            .ParameterList(HelloPacket.Members.Version)
            .Sizeof(HelloPacket.Members.Version);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(HelloPacket.Read)
            .ReturnValue(HelloPacket.Members.Version);
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePlayerInfoPacket()
    {
        var graph = new PacketGraph<PlayerInfoPacket>(3, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        graph.Field(PlayerInfoPacket.Members.Player);
        graph.Field(PlayerInfoPacket.Members.Accepted);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(PlayerInfoPacket.Write)
            .ParameterList(PlayerInfoPacket.Members.Player, PlayerInfoPacket.Members.Accepted)
            .Sizeof(PlayerInfoPacket.Members.Player, PlayerInfoPacket.Members.Accepted);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(PlayerInfoPacket.Read)
            .ReturnValue(PlayerInfoPacket.Members.Player, PlayerInfoPacket.Members.Accepted);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncEquipmentPacket()
    {
        var graph = new PacketGraph<SyncEquipmentPacket>(5, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncEquipmentPacket.Members.Player);
        graph.Field(SyncEquipmentPacket.Members.Slot);
        graph.Field(SyncEquipmentPacket.Members.Stack);
        graph.Field(SyncEquipmentPacket.Members.Prefix);
        graph.Field(SyncEquipmentPacket.Members.ItemType);
        graph.Field(SyncEquipmentPacket.Members.Favorited);
        graph.Field(SyncEquipmentPacket.Members.IsCreativeItem);
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(SyncEquipmentPacket.Write)
            .ParameterList(
                SyncEquipmentPacket.Members.Player,
                SyncEquipmentPacket.Members.Slot,
                SyncEquipmentPacket.Members.Stack,
                SyncEquipmentPacket.Members.Prefix,
                SyncEquipmentPacket.Members.ItemType,
                SyncEquipmentPacket.Members.Favorited,
                SyncEquipmentPacket.Members.IsCreativeItem
            )
            .Sizeof(
                SyncEquipmentPacket.Members.Player,
                SyncEquipmentPacket.Members.Slot,
                SyncEquipmentPacket.Members.Stack,
                SyncEquipmentPacket.Members.Prefix,
                SyncEquipmentPacket.Members.ItemType,
                SyncEquipmentPacket.Members.Favorited,
                SyncEquipmentPacket.Members.IsCreativeItem
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(SyncEquipmentPacket.Read)
            .ReturnValue(
                SyncEquipmentPacket.Members.Player,
                SyncEquipmentPacket.Members.Slot,
                SyncEquipmentPacket.Members.Stack,
                SyncEquipmentPacket.Members.Prefix,
                SyncEquipmentPacket.Members.ItemType,
                SyncEquipmentPacket.Members.Favorited,
                SyncEquipmentPacket.Members.IsCreativeItem
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateRequestWorldDataPacket()
    {
        var graph = new PacketGraph<RequestWorldDataPacket>(6, wireDirection: WireDirection.ClientToServer)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSpawnTileDataPacket()
    {
        var graph = new PacketGraph<SpawnTileDataPacket>(8, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SpawnTileDataPacket.Members.X);
        graph.Field(SpawnTileDataPacket.Members.Y);
        graph.Field(SpawnTileDataPacket.Members.Team);
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(SpawnTileDataPacket.Write)
            .ParameterList(SpawnTileDataPacket.Members.X, SpawnTileDataPacket.Members.Y, SpawnTileDataPacket.Members.Team)
            .Sizeof(SpawnTileDataPacket.Members.X, SpawnTileDataPacket.Members.Y, SpawnTileDataPacket.Members.Team);
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(SpawnTileDataPacket.Read)
            .ReturnValue(SpawnTileDataPacket.Members.X, SpawnTileDataPacket.Members.Y, SpawnTileDataPacket.Members.Team);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTileFrameSectionPacket()
    {
        var graph = new PacketGraph<TileFrameSectionPacket>(11, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TileFrameSectionPacket.Members.X);
        graph.Field(TileFrameSectionPacket.Members.Y);
        graph.Field(TileFrameSectionPacket.Members.Width);
        graph.Field(TileFrameSectionPacket.Members.Height);
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 8))
            .FunctionName(TileFrameSectionPacket.Write)
            .ParameterList(
                TileFrameSectionPacket.Members.X,
                TileFrameSectionPacket.Members.Y,
                TileFrameSectionPacket.Members.Width,
                TileFrameSectionPacket.Members.Height
            )
            .Sizeof(
                TileFrameSectionPacket.Members.X,
                TileFrameSectionPacket.Members.Y,
                TileFrameSectionPacket.Members.Width,
                TileFrameSectionPacket.Members.Height
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 8))
            .FunctionName(TileFrameSectionPacket.Read)
            .ReturnValue(
                TileFrameSectionPacket.Members.X,
                TileFrameSectionPacket.Members.Y,
                TileFrameSectionPacket.Members.Width,
                TileFrameSectionPacket.Members.Height
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePlayerSpawnPacket()
    {
        var graph = new PacketGraph<PlayerSpawnPacket>(12, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(PlayerSpawnPacket.Members.Player);
        graph.Field(PlayerSpawnPacket.Members.SpawnX);
        graph.Field(PlayerSpawnPacket.Members.SpawnY);
        graph.Field(PlayerSpawnPacket.Members.RespawnTimer);
        graph.Field(PlayerSpawnPacket.Members.PveDeaths);
        graph.Field(PlayerSpawnPacket.Members.PvpDeaths);
        graph.Field(PlayerSpawnPacket.Members.Team);
        graph.Field(PlayerSpawnPacket.Members.SpawnContext);
        graph.FunctionDeclaration(lengthRange: new ByteRange(15, 15))
            .FunctionName(PlayerSpawnPacket.Write)
            .ParameterList(
                PlayerSpawnPacket.Members.Player,
                PlayerSpawnPacket.Members.SpawnX,
                PlayerSpawnPacket.Members.SpawnY,
                PlayerSpawnPacket.Members.RespawnTimer,
                PlayerSpawnPacket.Members.PveDeaths,
                PlayerSpawnPacket.Members.PvpDeaths,
                PlayerSpawnPacket.Members.Team,
                PlayerSpawnPacket.Members.SpawnContext
            )
            .Sizeof(
                PlayerSpawnPacket.Members.Player,
                PlayerSpawnPacket.Members.SpawnX,
                PlayerSpawnPacket.Members.SpawnY,
                PlayerSpawnPacket.Members.RespawnTimer,
                PlayerSpawnPacket.Members.PveDeaths,
                PlayerSpawnPacket.Members.PvpDeaths,
                PlayerSpawnPacket.Members.Team,
                PlayerSpawnPacket.Members.SpawnContext
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(15, 15))
            .FunctionName(PlayerSpawnPacket.Read)
            .ReturnValue(
                PlayerSpawnPacket.Members.Player,
                PlayerSpawnPacket.Members.SpawnX,
                PlayerSpawnPacket.Members.SpawnY,
                PlayerSpawnPacket.Members.RespawnTimer,
                PlayerSpawnPacket.Members.PveDeaths,
                PlayerSpawnPacket.Members.PvpDeaths,
                PlayerSpawnPacket.Members.Team,
                PlayerSpawnPacket.Members.SpawnContext
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePlayerControlsPacket()
    {
        var graph = new PacketGraph<PlayerControlsPacket>(13, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(PlayerControlsPacket.Members.Player);
        graph.Field(PlayerControlsPacket.Members.ControlFlags);
        graph.Field(PlayerControlsPacket.Members.MovementFlags);
        graph.Field(PlayerControlsPacket.Members.PlayerFeatureFlags);
        graph.Field(PlayerControlsPacket.Members.ActionFlags);
        graph.Field(PlayerControlsPacket.Members.SelectedItem);
        graph.Field(PlayerControlsPacket.Members.Position);
        graph.Field(PlayerControlsPacket.Members.Velocity);
        graph.Field(PlayerControlsPacket.Members.MountType);
        graph.Field(PlayerControlsPacket.Members.PotionOfReturnUsePosition);
        graph.Field(PlayerControlsPacket.Members.PotionOfReturnHomePosition);
        graph.Field(PlayerControlsPacket.Members.CameraTarget);
        graph.FunctionDeclaration(lengthRange: new ByteRange(14, 48))
            .FunctionName(PlayerControlsPacket.Write)
            .ParameterList(
                PlayerControlsPacket.Members.Player,
                PlayerControlsPacket.Members.ControlFlags,
                PlayerControlsPacket.Members.MovementFlags,
                PlayerControlsPacket.Members.PlayerFeatureFlags,
                PlayerControlsPacket.Members.ActionFlags,
                PlayerControlsPacket.Members.SelectedItem,
                PlayerControlsPacket.Members.Position,
                PlayerControlsPacket.Members.Velocity,
                PlayerControlsPacket.Members.MountType,
                PlayerControlsPacket.Members.PotionOfReturnUsePosition,
                PlayerControlsPacket.Members.PotionOfReturnHomePosition,
                PlayerControlsPacket.Members.CameraTarget
            )
            .Sizeof(
                PlayerControlsPacket.Members.Player,
                PlayerControlsPacket.Members.ControlFlags,
                PlayerControlsPacket.Members.MovementFlags,
                PlayerControlsPacket.Members.PlayerFeatureFlags,
                PlayerControlsPacket.Members.ActionFlags,
                PlayerControlsPacket.Members.SelectedItem,
                PlayerControlsPacket.Members.Position,
                PlayerControlsPacket.Members.Velocity,
                PlayerControlsPacket.Members.MountType,
                PlayerControlsPacket.Members.PotionOfReturnUsePosition,
                PlayerControlsPacket.Members.PotionOfReturnHomePosition,
                PlayerControlsPacket.Members.CameraTarget
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(14, 48))
            .FunctionName(PlayerControlsPacket.Read)
            .ReturnValue(
                PlayerControlsPacket.Members.Player,
                PlayerControlsPacket.Members.ControlFlags,
                PlayerControlsPacket.Members.MovementFlags,
                PlayerControlsPacket.Members.PlayerFeatureFlags,
                PlayerControlsPacket.Members.ActionFlags,
                PlayerControlsPacket.Members.SelectedItem,
                PlayerControlsPacket.Members.Position,
                PlayerControlsPacket.Members.Velocity,
                PlayerControlsPacket.Members.MountType,
                PlayerControlsPacket.Members.PotionOfReturnUsePosition,
                PlayerControlsPacket.Members.PotionOfReturnHomePosition,
                PlayerControlsPacket.Members.CameraTarget
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePlayerActivePacket()
    {
        var graph = new PacketGraph<PlayerActivePacket>(14, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(PlayerActivePacket.Members.Player);
        graph.Field(PlayerActivePacket.Members.ActiveState);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(PlayerActivePacket.Write)
            .ParameterList(PlayerActivePacket.Members.Player, PlayerActivePacket.Members.ActiveState)
            .Sizeof(PlayerActivePacket.Members.Player, PlayerActivePacket.Members.ActiveState);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(PlayerActivePacket.Read)
            .ReturnValue(PlayerActivePacket.Members.Player, PlayerActivePacket.Members.ActiveState);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncPlayerPacket()
    {
        var graph = new PacketGraph<SyncPlayerPacket>(4, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncPlayerPacket.Members.Player);
        graph.Field(SyncPlayerPacket.Members.SkinVariant);
        graph.Field(SyncPlayerPacket.Members.VoiceVariant);
        graph.Field(SyncPlayerPacket.Members.VoicePitchOffset);
        graph.Field(SyncPlayerPacket.Members.Hair);
        graph.Field(SyncPlayerPacket.Members.Name);
        graph.Field(SyncPlayerPacket.Members.HairDye);
        graph.Field(SyncPlayerPacket.Members.HiddenAccessories);
        graph.Field(SyncPlayerPacket.Members.HideMisc);
        graph.Field(SyncPlayerPacket.Members.HairColor);
        graph.Field(SyncPlayerPacket.Members.SkinColor);
        graph.Field(SyncPlayerPacket.Members.EyeColor);
        graph.Field(SyncPlayerPacket.Members.ShirtColor);
        graph.Field(SyncPlayerPacket.Members.UnderShirtColor);
        graph.Field(SyncPlayerPacket.Members.PantsColor);
        graph.Field(SyncPlayerPacket.Members.ShoeColor);
        graph.Field(SyncPlayerPacket.Members.DifficultyAndAccessoryFlags);
        graph.Field(SyncPlayerPacket.Members.BiomeAndCartFlags);
        graph.Field(SyncPlayerPacket.Members.PermanentUpgradeFlags);
        graph.FunctionDeclaration(lengthRange: new ByteRange(37, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(SyncPlayerPacket.WriteFunction)
            .ParameterList(
                SyncPlayerPacket.Members.Player,
                SyncPlayerPacket.Members.SkinVariant,
                SyncPlayerPacket.Members.VoiceVariant,
                SyncPlayerPacket.Members.VoicePitchOffset,
                SyncPlayerPacket.Members.Hair,
                SyncPlayerPacket.Members.Name,
                SyncPlayerPacket.Members.HairDye,
                SyncPlayerPacket.Members.HiddenAccessories,
                SyncPlayerPacket.Members.HideMisc,
                SyncPlayerPacket.Members.HairColor,
                SyncPlayerPacket.Members.SkinColor,
                SyncPlayerPacket.Members.EyeColor,
                SyncPlayerPacket.Members.ShirtColor,
                SyncPlayerPacket.Members.UnderShirtColor,
                SyncPlayerPacket.Members.PantsColor,
                SyncPlayerPacket.Members.ShoeColor,
                SyncPlayerPacket.Members.DifficultyAndAccessoryFlags,
                SyncPlayerPacket.Members.BiomeAndCartFlags,
                SyncPlayerPacket.Members.PermanentUpgradeFlags
            )
            .Sizeof(
                SyncPlayerPacket.Members.Player,
                SyncPlayerPacket.Members.SkinVariant,
                SyncPlayerPacket.Members.VoiceVariant,
                SyncPlayerPacket.Members.VoicePitchOffset,
                SyncPlayerPacket.Members.Hair,
                SyncPlayerPacket.Members.Name,
                SyncPlayerPacket.Members.HairDye,
                SyncPlayerPacket.Members.HiddenAccessories,
                SyncPlayerPacket.Members.HideMisc,
                SyncPlayerPacket.Members.HairColor,
                SyncPlayerPacket.Members.SkinColor,
                SyncPlayerPacket.Members.EyeColor,
                SyncPlayerPacket.Members.ShirtColor,
                SyncPlayerPacket.Members.UnderShirtColor,
                SyncPlayerPacket.Members.PantsColor,
                SyncPlayerPacket.Members.ShoeColor,
                SyncPlayerPacket.Members.DifficultyAndAccessoryFlags,
                SyncPlayerPacket.Members.BiomeAndCartFlags,
                SyncPlayerPacket.Members.PermanentUpgradeFlags
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(37, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(SyncPlayerPacket.Read)
            .ReturnValue(
                SyncPlayerPacket.Members.Player,
                SyncPlayerPacket.Members.SkinVariant,
                SyncPlayerPacket.Members.VoiceVariant,
                SyncPlayerPacket.Members.VoicePitchOffset,
                SyncPlayerPacket.Members.Hair,
                SyncPlayerPacket.Members.Name,
                SyncPlayerPacket.Members.HairDye,
                SyncPlayerPacket.Members.HiddenAccessories,
                SyncPlayerPacket.Members.HideMisc,
                SyncPlayerPacket.Members.HairColor,
                SyncPlayerPacket.Members.SkinColor,
                SyncPlayerPacket.Members.EyeColor,
                SyncPlayerPacket.Members.ShirtColor,
                SyncPlayerPacket.Members.UnderShirtColor,
                SyncPlayerPacket.Members.PantsColor,
                SyncPlayerPacket.Members.ShoeColor,
                SyncPlayerPacket.Members.DifficultyAndAccessoryFlags,
                SyncPlayerPacket.Members.BiomeAndCartFlags,
                SyncPlayerPacket.Members.PermanentUpgradeFlags
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePlayerLifeManaPacket()
    {
        var graph = new PacketGraph<PlayerLifeManaPacket>(16, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(PlayerLifeManaPacket.Members.Player);
        graph.Field(PlayerLifeManaPacket.Members.Life);
        graph.Field(PlayerLifeManaPacket.Members.MaximumLife);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(PlayerLifeManaPacket.Write)
            .ParameterList(PlayerLifeManaPacket.Members.Player, PlayerLifeManaPacket.Members.Life, PlayerLifeManaPacket.Members.MaximumLife)
            .Sizeof(PlayerLifeManaPacket.Members.Player, PlayerLifeManaPacket.Members.Life, PlayerLifeManaPacket.Members.MaximumLife);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(PlayerLifeManaPacket.Read)
            .ReturnValue(PlayerLifeManaPacket.Members.Player, PlayerLifeManaPacket.Members.Life, PlayerLifeManaPacket.Members.MaximumLife);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTileManipulationPacket()
    {
        var graph = new PacketGraph<TileManipulationPacket>(17, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TileManipulationPacket.Members.Action);
        graph.Field(TileManipulationPacket.Members.X);
        graph.Field(TileManipulationPacket.Members.Y);
        graph.Field(TileManipulationPacket.Members.TileOrWallType);
        graph.Field(TileManipulationPacket.Members.Style);
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 8))
            .FunctionName(TileManipulationPacket.Write)
            .ParameterList(
                TileManipulationPacket.Members.Action,
                TileManipulationPacket.Members.X,
                TileManipulationPacket.Members.Y,
                TileManipulationPacket.Members.TileOrWallType,
                TileManipulationPacket.Members.Style
            )
            .Sizeof(
                TileManipulationPacket.Members.Action,
                TileManipulationPacket.Members.X,
                TileManipulationPacket.Members.Y,
                TileManipulationPacket.Members.TileOrWallType,
                TileManipulationPacket.Members.Style
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 8))
            .FunctionName(TileManipulationPacket.Read)
            .ReturnValue(
                TileManipulationPacket.Members.Action,
                TileManipulationPacket.Members.X,
                TileManipulationPacket.Members.Y,
                TileManipulationPacket.Members.TileOrWallType,
                TileManipulationPacket.Members.Style
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSetTimePacket()
    {
        var graph = new PacketGraph<SetTimePacket>(18, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SetTimePacket.Members.DayTime);
        graph.Field(SetTimePacket.Members.Time);
        graph.Field(SetTimePacket.Members.SunModY);
        graph.Field(SetTimePacket.Members.MoonModY);
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(SetTimePacket.Write)
            .ParameterList(
                SetTimePacket.Members.DayTime,
                SetTimePacket.Members.Time,
                SetTimePacket.Members.SunModY,
                SetTimePacket.Members.MoonModY
            )
            .Sizeof(
                SetTimePacket.Members.DayTime,
                SetTimePacket.Members.Time,
                SetTimePacket.Members.SunModY,
                SetTimePacket.Members.MoonModY
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(SetTimePacket.Read)
            .ReturnValue(
                SetTimePacket.Members.DayTime,
                SetTimePacket.Members.Time,
                SetTimePacket.Members.SunModY,
                SetTimePacket.Members.MoonModY
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateToggleDoorStatePacket()
    {
        var graph = new PacketGraph<ToggleDoorStatePacket>(19, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(ToggleDoorStatePacket.Members.Action);
        graph.Field(ToggleDoorStatePacket.Members.X);
        graph.Field(ToggleDoorStatePacket.Members.Y);
        graph.Field(ToggleDoorStatePacket.Members.Direction);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(ToggleDoorStatePacket.Write)
            .ParameterList(
                ToggleDoorStatePacket.Members.Action,
                ToggleDoorStatePacket.Members.X,
                ToggleDoorStatePacket.Members.Y,
                ToggleDoorStatePacket.Members.Direction
            )
            .Sizeof(
                ToggleDoorStatePacket.Members.Action,
                ToggleDoorStatePacket.Members.X,
                ToggleDoorStatePacket.Members.Y,
                ToggleDoorStatePacket.Members.Direction
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(ToggleDoorStatePacket.Read)
            .ReturnValue(
                ToggleDoorStatePacket.Members.Action,
                ToggleDoorStatePacket.Members.X,
                ToggleDoorStatePacket.Members.Y,
                ToggleDoorStatePacket.Members.Direction
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncItemPacket()
    {
        var graph = new PacketGraph<SyncItemPacket>(21, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncItemPacket.Members.ItemIndex);
        graph.Field(SyncItemPacket.Members.PositionX);
        graph.Field(SyncItemPacket.Members.PositionY);
        graph.Field(SyncItemPacket.Members.VelocityX);
        graph.Field(SyncItemPacket.Members.VelocityY);
        graph.Field(SyncItemPacket.Members.Stack);
        graph.Field(SyncItemPacket.Members.Prefix);
        graph.Field(SyncItemPacket.Members.StateFlags);
        graph.Field(SyncItemPacket.Members.ItemType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(24, 24))
            .FunctionName(PacketItemSyncCodec.WriteFields)
            .ParameterList(
                SyncItemPacket.Members.ItemIndex,
                SyncItemPacket.Members.PositionX,
                SyncItemPacket.Members.PositionY,
                SyncItemPacket.Members.VelocityX,
                SyncItemPacket.Members.VelocityY,
                SyncItemPacket.Members.Stack,
                SyncItemPacket.Members.Prefix,
                SyncItemPacket.Members.StateFlags,
                SyncItemPacket.Members.ItemType
            )
            .Sizeof(
                SyncItemPacket.Members.ItemIndex,
                SyncItemPacket.Members.PositionX,
                SyncItemPacket.Members.PositionY,
                SyncItemPacket.Members.VelocityX,
                SyncItemPacket.Members.VelocityY,
                SyncItemPacket.Members.Stack,
                SyncItemPacket.Members.Prefix,
                SyncItemPacket.Members.StateFlags,
                SyncItemPacket.Members.ItemType
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(24, 24))
            .FunctionName(PacketItemSyncCodec.ReadFields)
            .ReturnValue(
                SyncItemPacket.Members.ItemIndex,
                SyncItemPacket.Members.PositionX,
                SyncItemPacket.Members.PositionY,
                SyncItemPacket.Members.VelocityX,
                SyncItemPacket.Members.VelocityY,
                SyncItemPacket.Members.Stack,
                SyncItemPacket.Members.Prefix,
                SyncItemPacket.Members.StateFlags,
                SyncItemPacket.Members.ItemType
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateItemOwnerPacket()
    {
        var graph = new PacketGraph<ItemOwnerPacket>(22, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        graph.Field(ItemOwnerPacket.Members.ItemIndex);
        graph.Field(ItemOwnerPacket.Members.ReservedForPlayer);
        graph.Field(ItemOwnerPacket.Members.PositionX);
        graph.Field(ItemOwnerPacket.Members.PositionY);
        graph.FunctionDeclaration(lengthRange: new ByteRange(11, 11))
            .FunctionName(ItemOwnerPacket.Write)
            .ParameterList(
                ItemOwnerPacket.Members.ItemIndex,
                ItemOwnerPacket.Members.ReservedForPlayer,
                ItemOwnerPacket.Members.PositionX,
                ItemOwnerPacket.Members.PositionY
            )
            .Sizeof(
                ItemOwnerPacket.Members.ItemIndex,
                ItemOwnerPacket.Members.ReservedForPlayer,
                ItemOwnerPacket.Members.PositionX,
                ItemOwnerPacket.Members.PositionY
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(11, 11))
            .FunctionName(ItemOwnerPacket.Read)
            .ReturnValue(
                ItemOwnerPacket.Members.ItemIndex,
                ItemOwnerPacket.Members.ReservedForPlayer,
                ItemOwnerPacket.Members.PositionX,
                ItemOwnerPacket.Members.PositionY
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUnusedMeleeStrikePacket()
    {
        var graph = new PacketGraph<UnusedMeleeStrikePacket>(24, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(UnusedMeleeStrikePacket.Members.NpcIndex);
        graph.Field(UnusedMeleeStrikePacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(UnusedMeleeStrikePacket.Write)
            .ParameterList(UnusedMeleeStrikePacket.Members.NpcIndex, UnusedMeleeStrikePacket.Members.Player)
            .Sizeof(UnusedMeleeStrikePacket.Members.NpcIndex, UnusedMeleeStrikePacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(UnusedMeleeStrikePacket.Read)
            .ReturnValue(UnusedMeleeStrikePacket.Members.NpcIndex, UnusedMeleeStrikePacket.Members.Player);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncProjectilePacket()
    {
        var graph = new PacketGraph<SyncProjectilePacket>(27, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies(ProtocolInputs.Members.NeedsUuid);
        graph.Field(SyncProjectilePacket.Members.Identity);
        graph.Field(SyncProjectilePacket.Members.Position);
        graph.Field(SyncProjectilePacket.Members.Velocity);
        graph.Field(SyncProjectilePacket.Members.Owner);
        graph.Field(SyncProjectilePacket.Members.ProjectileType);
        graph.Field(SyncProjectilePacket.Members.Ai0);
        graph.Field(SyncProjectilePacket.Members.Ai1);
        graph.Field(SyncProjectilePacket.Members.BannerId);
        graph.Field(SyncProjectilePacket.Members.Damage);
        graph.Field(SyncProjectilePacket.Members.Knockback);
        graph.Field(SyncProjectilePacket.Members.OriginalDamage);
        graph.Field(SyncProjectilePacket.Members.ProjectileUuid);
        graph.Field(SyncProjectilePacket.Members.Ai2);
        graph.FunctionDeclaration(lengthRange: new ByteRange(22, 47))
            .FunctionName(SyncProjectilePacket.Write)
            .ParameterList(
                SyncProjectilePacket.Members.Identity,
                SyncProjectilePacket.Members.Position,
                SyncProjectilePacket.Members.Velocity,
                SyncProjectilePacket.Members.Owner,
                SyncProjectilePacket.Members.ProjectileType,
                SyncProjectilePacket.Members.Ai0,
                SyncProjectilePacket.Members.Ai1,
                SyncProjectilePacket.Members.BannerId,
                SyncProjectilePacket.Members.Damage,
                SyncProjectilePacket.Members.Knockback,
                SyncProjectilePacket.Members.OriginalDamage,
                SyncProjectilePacket.Members.ProjectileUuid,
                SyncProjectilePacket.Members.Ai2,
                ProtocolInputs.Members.NeedsUuid
            )
            .Sizeof(
                SyncProjectilePacket.Members.Identity,
                SyncProjectilePacket.Members.Position,
                SyncProjectilePacket.Members.Velocity,
                SyncProjectilePacket.Members.Owner,
                SyncProjectilePacket.Members.ProjectileType,
                SyncProjectilePacket.Members.Ai0,
                SyncProjectilePacket.Members.Ai1,
                SyncProjectilePacket.Members.BannerId,
                SyncProjectilePacket.Members.Damage,
                SyncProjectilePacket.Members.Knockback,
                SyncProjectilePacket.Members.OriginalDamage,
                SyncProjectilePacket.Members.ProjectileUuid,
                SyncProjectilePacket.Members.Ai2
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(22, 47))
            .FunctionName(SyncProjectilePacket.Read)
            .ReturnValue(
                SyncProjectilePacket.Members.Identity,
                SyncProjectilePacket.Members.Position,
                SyncProjectilePacket.Members.Velocity,
                SyncProjectilePacket.Members.Owner,
                SyncProjectilePacket.Members.ProjectileType,
                SyncProjectilePacket.Members.Ai0,
                SyncProjectilePacket.Members.Ai1,
                SyncProjectilePacket.Members.BannerId,
                SyncProjectilePacket.Members.Damage,
                SyncProjectilePacket.Members.Knockback,
                SyncProjectilePacket.Members.OriginalDamage,
                SyncProjectilePacket.Members.ProjectileUuid,
                SyncProjectilePacket.Members.Ai2
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateDamageNPCPacket()
    {
        var graph = new PacketGraph<DamageNPCPacket>(28, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(DamageNPCPacket.Members.NpcIndex);
        graph.Field(DamageNPCPacket.Members.Damage);
        graph.Field(DamageNPCPacket.Members.Knockback);
        graph.Field(DamageNPCPacket.Members.EncodedDirection);
        graph.Field(DamageNPCPacket.Members.HitDirection);
        graph.FunctionDeclaration(lengthRange: new ByteRange(10, 10))
            .FunctionName(DamageNPCPacket.Write)
            .ParameterList(
                DamageNPCPacket.Members.NpcIndex,
                DamageNPCPacket.Members.Damage,
                DamageNPCPacket.Members.Knockback,
                DamageNPCPacket.Members.EncodedDirection,
                DamageNPCPacket.Members.HitDirection
            )
            .Sizeof(
                DamageNPCPacket.Members.NpcIndex,
                DamageNPCPacket.Members.Damage,
                DamageNPCPacket.Members.Knockback,
                DamageNPCPacket.Members.EncodedDirection,
                DamageNPCPacket.Members.HitDirection
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(10, 10))
            .FunctionName(DamageNPCPacket.Read)
            .ReturnValue(
                DamageNPCPacket.Members.NpcIndex,
                DamageNPCPacket.Members.Damage,
                DamageNPCPacket.Members.Knockback,
                DamageNPCPacket.Members.EncodedDirection,
                DamageNPCPacket.Members.HitDirection
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateKillProjectilePacket()
    {
        var graph = new PacketGraph<KillProjectilePacket>(29, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(KillProjectilePacket.Members.ProjectileIdentity);
        graph.Field(KillProjectilePacket.Members.Owner);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(KillProjectilePacket.Write)
            .ParameterList(KillProjectilePacket.Members.ProjectileIdentity, KillProjectilePacket.Members.Owner)
            .Sizeof(KillProjectilePacket.Members.ProjectileIdentity, KillProjectilePacket.Members.Owner);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(KillProjectilePacket.Read)
            .ReturnValue(KillProjectilePacket.Members.ProjectileIdentity, KillProjectilePacket.Members.Owner);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTogglePVPPacket()
    {
        var graph = new PacketGraph<TogglePVPPacket>(30, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TogglePVPPacket.Members.Player);
        graph.Field(TogglePVPPacket.Members.Hostile);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(TogglePVPPacket.Write)
            .ParameterList(TogglePVPPacket.Members.Player, TogglePVPPacket.Members.Hostile)
            .Sizeof(TogglePVPPacket.Members.Player, TogglePVPPacket.Members.Hostile);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(TogglePVPPacket.Read)
            .ReturnValue(TogglePVPPacket.Members.Player, TogglePVPPacket.Members.Hostile);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateRequestChestOpenPacket()
    {
        var graph = new PacketGraph<RequestChestOpenPacket>(31, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(RequestChestOpenPacket.Members.X);
        graph.Field(RequestChestOpenPacket.Members.Y);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(RequestChestOpenPacket.Write)
            .ParameterList(RequestChestOpenPacket.Members.X, RequestChestOpenPacket.Members.Y)
            .Sizeof(RequestChestOpenPacket.Members.X, RequestChestOpenPacket.Members.Y);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(RequestChestOpenPacket.Read)
            .ReturnValue(RequestChestOpenPacket.Members.X, RequestChestOpenPacket.Members.Y);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncChestItemPacket()
    {
        var graph = new PacketGraph<SyncChestItemPacket>(32, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncChestItemPacket.Members.ChestIndex);
        graph.Field(SyncChestItemPacket.Members.Slot);
        graph.Field(SyncChestItemPacket.Members.Stack);
        graph.Field(SyncChestItemPacket.Members.Prefix);
        graph.Field(SyncChestItemPacket.Members.ItemType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 8))
            .FunctionName(SyncChestItemPacket.Write)
            .ParameterList(
                SyncChestItemPacket.Members.ChestIndex,
                SyncChestItemPacket.Members.Slot,
                SyncChestItemPacket.Members.Stack,
                SyncChestItemPacket.Members.Prefix,
                SyncChestItemPacket.Members.ItemType
            )
            .Sizeof(
                SyncChestItemPacket.Members.ChestIndex,
                SyncChestItemPacket.Members.Slot,
                SyncChestItemPacket.Members.Stack,
                SyncChestItemPacket.Members.Prefix,
                SyncChestItemPacket.Members.ItemType
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 8))
            .FunctionName(SyncChestItemPacket.Read)
            .ReturnValue(
                SyncChestItemPacket.Members.ChestIndex,
                SyncChestItemPacket.Members.Slot,
                SyncChestItemPacket.Members.Stack,
                SyncChestItemPacket.Members.Prefix,
                SyncChestItemPacket.Members.ItemType
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncPlayerChestPacket()
    {
        var graph = new PacketGraph<SyncPlayerChestPacket>(33, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncPlayerChestPacket.Members.ChestIndex);
        graph.Field(SyncPlayerChestPacket.Members.ChestX);
        graph.Field(SyncPlayerChestPacket.Members.ChestY);
        graph.Field(SyncPlayerChestPacket.Members.NameLengthIndicator);
        graph.Field(SyncPlayerChestPacket.Members.Name);
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(SyncPlayerChestPacket.Write)
            .ParameterList(
                SyncPlayerChestPacket.Members.ChestIndex,
                SyncPlayerChestPacket.Members.ChestX,
                SyncPlayerChestPacket.Members.ChestY,
                SyncPlayerChestPacket.Members.NameLengthIndicator,
                SyncPlayerChestPacket.Members.Name
            )
            .Sizeof(
                SyncPlayerChestPacket.Members.ChestIndex,
                SyncPlayerChestPacket.Members.ChestX,
                SyncPlayerChestPacket.Members.ChestY,
                SyncPlayerChestPacket.Members.NameLengthIndicator,
                SyncPlayerChestPacket.Members.Name
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(SyncPlayerChestPacket.Read)
            .ReturnValue(
                SyncPlayerChestPacket.Members.ChestIndex,
                SyncPlayerChestPacket.Members.ChestX,
                SyncPlayerChestPacket.Members.ChestY,
                SyncPlayerChestPacket.Members.NameLengthIndicator,
                SyncPlayerChestPacket.Members.Name
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateChestUpdatesPacket()
    {
        var graph = new PacketGraph<ChestUpdatesPacket>(34, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(ChestUpdatesPacket.Members.Action);
        graph.Field(ChestUpdatesPacket.Members.X);
        graph.Field(ChestUpdatesPacket.Members.Y);
        graph.Field(ChestUpdatesPacket.Members.ObjectType);
        graph.Field(ChestUpdatesPacket.Members.ChestIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(ChestUpdatesPacket.Write)
            .ParameterList(
                ChestUpdatesPacket.Members.Action,
                ChestUpdatesPacket.Members.X,
                ChestUpdatesPacket.Members.Y,
                ChestUpdatesPacket.Members.ObjectType,
                ChestUpdatesPacket.Members.ChestIndex
            )
            .Sizeof(
                ChestUpdatesPacket.Members.Action,
                ChestUpdatesPacket.Members.X,
                ChestUpdatesPacket.Members.Y,
                ChestUpdatesPacket.Members.ObjectType,
                ChestUpdatesPacket.Members.ChestIndex
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(ChestUpdatesPacket.Read)
            .ReturnValue(
                ChestUpdatesPacket.Members.Action,
                ChestUpdatesPacket.Members.X,
                ChestUpdatesPacket.Members.Y,
                ChestUpdatesPacket.Members.ObjectType,
                ChestUpdatesPacket.Members.ChestIndex
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePlayerHealPacket()
    {
        var graph = new PacketGraph<PlayerHealPacket>(35, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(PlayerHealPacket.Members.Player);
        graph.Field(PlayerHealPacket.Members.HealAmount);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(PlayerHealPacket.Write)
            .ParameterList(PlayerHealPacket.Members.Player, PlayerHealPacket.Members.HealAmount)
            .Sizeof(PlayerHealPacket.Members.Player, PlayerHealPacket.Members.HealAmount);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(PlayerHealPacket.Read)
            .ReturnValue(PlayerHealPacket.Members.Player, PlayerHealPacket.Members.HealAmount);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncPlayerZonePacket()
    {
        var graph = new PacketGraph<SyncPlayerZonePacket>(36, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncPlayerZonePacket.Members.Player);
        graph.Field(SyncPlayerZonePacket.Members.Zone1);
        graph.Field(SyncPlayerZonePacket.Members.Zone2);
        graph.Field(SyncPlayerZonePacket.Members.Zone3);
        graph.Field(SyncPlayerZonePacket.Members.Zone4);
        graph.Field(SyncPlayerZonePacket.Members.Zone5);
        graph.Field(SyncPlayerZonePacket.Members.TownNpcCount);
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, 7))
            .FunctionName(SyncPlayerZonePacket.Write)
            .ParameterList(
                SyncPlayerZonePacket.Members.Player,
                SyncPlayerZonePacket.Members.Zone1,
                SyncPlayerZonePacket.Members.Zone2,
                SyncPlayerZonePacket.Members.Zone3,
                SyncPlayerZonePacket.Members.Zone4,
                SyncPlayerZonePacket.Members.Zone5,
                SyncPlayerZonePacket.Members.TownNpcCount
            )
            .Sizeof(
                SyncPlayerZonePacket.Members.Player,
                SyncPlayerZonePacket.Members.Zone1,
                SyncPlayerZonePacket.Members.Zone2,
                SyncPlayerZonePacket.Members.Zone3,
                SyncPlayerZonePacket.Members.Zone4,
                SyncPlayerZonePacket.Members.Zone5,
                SyncPlayerZonePacket.Members.TownNpcCount
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, 7))
            .FunctionName(SyncPlayerZonePacket.Read)
            .ReturnValue(
                SyncPlayerZonePacket.Members.Player,
                SyncPlayerZonePacket.Members.Zone1,
                SyncPlayerZonePacket.Members.Zone2,
                SyncPlayerZonePacket.Members.Zone3,
                SyncPlayerZonePacket.Members.Zone4,
                SyncPlayerZonePacket.Members.Zone5,
                SyncPlayerZonePacket.Members.TownNpcCount
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSendPasswordPacket()
    {
        var graph = new PacketGraph<SendPasswordPacket>(38, wireDirection: WireDirection.ClientToServer)
            .ExternalDependencies();
        graph.Field(SendPasswordPacket.Members.Password);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(SendPasswordPacket.Write)
            .ParameterList(SendPasswordPacket.Members.Password)
            .Sizeof(SendPasswordPacket.Members.Password);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(SendPasswordPacket.Read)
            .ReturnValue(SendPasswordPacket.Members.Password);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateReleaseItemOwnershipPacket()
    {
        var graph = new PacketGraph<ReleaseItemOwnershipPacket>(39, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(ReleaseItemOwnershipPacket.Members.ItemIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(ReleaseItemOwnershipPacket.Write)
            .ParameterList(ReleaseItemOwnershipPacket.Members.ItemIndex)
            .Sizeof(ReleaseItemOwnershipPacket.Members.ItemIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(ReleaseItemOwnershipPacket.Read)
            .ReturnValue(ReleaseItemOwnershipPacket.Members.ItemIndex);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncTalkNPCPacket()
    {
        var graph = new PacketGraph<SyncTalkNPCPacket>(40, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncTalkNPCPacket.Members.Player);
        graph.Field(SyncTalkNPCPacket.Members.NpcIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(SyncTalkNPCPacket.Write)
            .ParameterList(SyncTalkNPCPacket.Members.Player, SyncTalkNPCPacket.Members.NpcIndex)
            .Sizeof(SyncTalkNPCPacket.Members.Player, SyncTalkNPCPacket.Members.NpcIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(SyncTalkNPCPacket.Read)
            .ReturnValue(SyncTalkNPCPacket.Members.Player, SyncTalkNPCPacket.Members.NpcIndex);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateItemRotationAndAnimationPacket()
    {
        var graph = new PacketGraph<ItemRotationAndAnimationPacket>(41, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(ItemRotationAndAnimationPacket.Members.Player);
        graph.Field(ItemRotationAndAnimationPacket.Members.ItemRotation);
        graph.Field(ItemRotationAndAnimationPacket.Members.ItemAnimation);
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, 7))
            .FunctionName(ItemRotationAndAnimationPacket.Write)
            .ParameterList(ItemRotationAndAnimationPacket.Members.Player, ItemRotationAndAnimationPacket.Members.ItemRotation, ItemRotationAndAnimationPacket.Members.ItemAnimation)
            .Sizeof(ItemRotationAndAnimationPacket.Members.Player, ItemRotationAndAnimationPacket.Members.ItemRotation, ItemRotationAndAnimationPacket.Members.ItemAnimation);
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, 7))
            .FunctionName(ItemRotationAndAnimationPacket.Read)
            .ReturnValue(ItemRotationAndAnimationPacket.Members.Player, ItemRotationAndAnimationPacket.Members.ItemRotation, ItemRotationAndAnimationPacket.Members.ItemAnimation);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUnknown42Packet()
    {
        var graph = new PacketGraph<Unknown42Packet>(42, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(Unknown42Packet.Members.Player);
        graph.Field(Unknown42Packet.Members.Mana);
        graph.Field(Unknown42Packet.Members.MaximumMana);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(Unknown42Packet.Write)
            .ParameterList(Unknown42Packet.Members.Player, Unknown42Packet.Members.Mana, Unknown42Packet.Members.MaximumMana)
            .Sizeof(Unknown42Packet.Members.Player, Unknown42Packet.Members.Mana, Unknown42Packet.Members.MaximumMana);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(Unknown42Packet.Read)
            .ReturnValue(Unknown42Packet.Members.Player, Unknown42Packet.Members.Mana, Unknown42Packet.Members.MaximumMana);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateManaEffectPacket()
    {
        var graph = new PacketGraph<ManaEffectPacket>(43, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(ManaEffectPacket.Members.Player);
        graph.Field(ManaEffectPacket.Members.ManaEffect);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(ManaEffectPacket.Write)
            .ParameterList(ManaEffectPacket.Members.Player, ManaEffectPacket.Members.ManaEffect)
            .Sizeof(ManaEffectPacket.Members.Player, ManaEffectPacket.Members.ManaEffect);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(ManaEffectPacket.Read)
            .ReturnValue(ManaEffectPacket.Members.Player, ManaEffectPacket.Members.ManaEffect);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTeamChangePacket()
    {
        var graph = new PacketGraph<TeamChangePacket>(45, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TeamChangePacket.Members.Player);
        graph.Field(TeamChangePacket.Members.Team);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(TeamChangeCodec.Write)
            .ParameterList(TeamChangePacket.Members.Player, TeamChangePacket.Members.Team)
            .Sizeof(TeamChangePacket.Members.Player, TeamChangePacket.Members.Team);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(TeamChangeCodec.Read)
            .ReturnValue(TeamChangePacket.Members.Player, TeamChangePacket.Members.Team);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateOpenSignRequestPacket()
    {
        var graph = new PacketGraph<OpenSignRequestPacket>(46, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(OpenSignRequestPacket.Members.X);
        graph.Field(OpenSignRequestPacket.Members.Y);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(OpenSignRequestPacket.Write)
            .ParameterList(OpenSignRequestPacket.Members.X, OpenSignRequestPacket.Members.Y)
            .Sizeof(OpenSignRequestPacket.Members.X, OpenSignRequestPacket.Members.Y);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(OpenSignRequestPacket.Read)
            .ReturnValue(OpenSignRequestPacket.Members.X, OpenSignRequestPacket.Members.Y);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateOpenSignResponsePacket()
    {
        var graph = new PacketGraph<OpenSignResponsePacket>(47, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(OpenSignResponsePacket.Members.SignIndex);
        graph.Field(OpenSignResponsePacket.Members.X);
        graph.Field(OpenSignResponsePacket.Members.Y);
        graph.Field(OpenSignResponsePacket.Members.Text);
        graph.Field(OpenSignResponsePacket.Members.Player);
        graph.Field(OpenSignResponsePacket.Members.Flags);
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(OpenSignResponsePacket.Write)
            .ParameterList(
                OpenSignResponsePacket.Members.SignIndex,
                OpenSignResponsePacket.Members.X,
                OpenSignResponsePacket.Members.Y,
                OpenSignResponsePacket.Members.Text,
                OpenSignResponsePacket.Members.Player,
                OpenSignResponsePacket.Members.Flags
            )
            .Sizeof(
                OpenSignResponsePacket.Members.SignIndex,
                OpenSignResponsePacket.Members.X,
                OpenSignResponsePacket.Members.Y,
                OpenSignResponsePacket.Members.Text,
                OpenSignResponsePacket.Members.Player,
                OpenSignResponsePacket.Members.Flags
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(OpenSignResponsePacket.Read)
            .ReturnValue(
                OpenSignResponsePacket.Members.SignIndex,
                OpenSignResponsePacket.Members.X,
                OpenSignResponsePacket.Members.Y,
                OpenSignResponsePacket.Members.Text,
                OpenSignResponsePacket.Members.Player,
                OpenSignResponsePacket.Members.Flags
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePlayerBuffsPacket()
    {
        var graph = new PacketGraph<PlayerBuffsPacket>(50, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(PlayerBuffsPacket.Members.Player);
        graph.Field(PlayerBuffsPacket.Members.BuffTypes);
        graph.FunctionDeclaration()
            .FunctionName(PlayerBuffsPacket.WriteBuffTypes)
            .ParameterList(PlayerBuffsPacket.Members.BuffTypes)
            .Sizeof(PlayerBuffsPacket.Members.BuffTypes);
        graph.FunctionDeclaration()
            .FunctionName(PlayerBuffsPacket.ReadBuffTypes)
            .ReturnValue(PlayerBuffsPacket.Members.BuffTypes)
            .Sizeof(PlayerBuffsPacket.Members.BuffTypes);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateMiscDataSyncPacket()
    {
        var graph = new PacketGraph<MiscDataSyncPacket>(51, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(MiscDataSyncPacket.Members.Player);
        graph.Field(MiscDataSyncPacket.Members.Action);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(MiscDataSyncPacket.Write)
            .ParameterList(MiscDataSyncPacket.Members.Player, MiscDataSyncPacket.Members.Action)
            .Sizeof(MiscDataSyncPacket.Members.Player, MiscDataSyncPacket.Members.Action);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(MiscDataSyncPacket.Read)
            .ReturnValue(MiscDataSyncPacket.Members.Player, MiscDataSyncPacket.Members.Action);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateLockAndUnlockPacket()
    {
        var graph = new PacketGraph<LockAndUnlockPacket>(52, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(LockAndUnlockPacket.Members.Action);
        graph.Field(LockAndUnlockPacket.Members.X);
        graph.Field(LockAndUnlockPacket.Members.Y);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(LockAndUnlockPacket.Write)
            .ParameterList(LockAndUnlockPacket.Members.Action, LockAndUnlockPacket.Members.X, LockAndUnlockPacket.Members.Y)
            .Sizeof(LockAndUnlockPacket.Members.Action, LockAndUnlockPacket.Members.X, LockAndUnlockPacket.Members.Y);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(LockAndUnlockPacket.Read)
            .ReturnValue(LockAndUnlockPacket.Members.Action, LockAndUnlockPacket.Members.X, LockAndUnlockPacket.Members.Y);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateAddNPCBuffPacket()
    {
        var graph = new PacketGraph<AddNPCBuffPacket>(53, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(AddNPCBuffPacket.Members.NpcIndex);
        graph.Field(AddNPCBuffPacket.Members.BuffType);
        graph.Field(AddNPCBuffPacket.Members.BuffTime);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(AddNPCBuffPacket.Write)
            .ParameterList(AddNPCBuffPacket.Members.NpcIndex, AddNPCBuffPacket.Members.BuffType, AddNPCBuffPacket.Members.BuffTime)
            .Sizeof(AddNPCBuffPacket.Members.NpcIndex, AddNPCBuffPacket.Members.BuffType, AddNPCBuffPacket.Members.BuffTime);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(AddNPCBuffPacket.Read)
            .ReturnValue(AddNPCBuffPacket.Members.NpcIndex, AddNPCBuffPacket.Members.BuffType, AddNPCBuffPacket.Members.BuffTime);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateNPCBuffsPacket()
    {
        var graph = new PacketGraph<NPCBuffsPacket>(54, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(NPCBuffsPacket.Members.NpcIndex);
        graph.Field(NPCBuffsPacket.Members.Buffs);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(NPCBuffsPacket.Write)
            .ParameterList(NPCBuffsPacket.Members.NpcIndex, NPCBuffsPacket.Members.Buffs)
            .Sizeof(NPCBuffsPacket.Members.NpcIndex, NPCBuffsPacket.Members.Buffs);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(NPCBuffsPacket.Read)
            .ReturnValue(NPCBuffsPacket.Members.NpcIndex, NPCBuffsPacket.Members.Buffs);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateAddPlayerBuffPvPPacket()
    {
        var graph = new PacketGraph<AddPlayerBuffPvPPacket>(55, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(AddPlayerBuffPvPPacket.Members.Player);
        graph.Field(AddPlayerBuffPvPPacket.Members.BuffType);
        graph.Field(AddPlayerBuffPvPPacket.Members.BuffTime);
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, 7))
            .FunctionName(AddPlayerBuffPvPPacket.Write)
            .ParameterList(AddPlayerBuffPvPPacket.Members.Player, AddPlayerBuffPvPPacket.Members.BuffType, AddPlayerBuffPvPPacket.Members.BuffTime)
            .Sizeof(AddPlayerBuffPvPPacket.Members.Player, AddPlayerBuffPvPPacket.Members.BuffType, AddPlayerBuffPvPPacket.Members.BuffTime);
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, 7))
            .FunctionName(AddPlayerBuffPvPPacket.Read)
            .ReturnValue(AddPlayerBuffPvPPacket.Members.Player, AddPlayerBuffPvPPacket.Members.BuffType, AddPlayerBuffPvPPacket.Members.BuffTime);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateInstrumentSoundPacket()
    {
        var graph = new PacketGraph<InstrumentSoundPacket>(58, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(InstrumentSoundPacket.Members.Player);
        graph.Field(InstrumentSoundPacket.Members.Pitch);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(InstrumentSoundPacket.Write)
            .ParameterList(InstrumentSoundPacket.Members.Player, InstrumentSoundPacket.Members.Pitch)
            .Sizeof(InstrumentSoundPacket.Members.Player, InstrumentSoundPacket.Members.Pitch);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(InstrumentSoundPacket.Read)
            .ReturnValue(InstrumentSoundPacket.Members.Player, InstrumentSoundPacket.Members.Pitch);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateHitSwitchPacket()
    {
        var graph = new PacketGraph<HitSwitchPacket>(59, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(HitSwitchPacket.Members.X);
        graph.Field(HitSwitchPacket.Members.Y);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(HitSwitchPacket.Write)
            .ParameterList(HitSwitchPacket.Members.X, HitSwitchPacket.Members.Y)
            .Sizeof(HitSwitchPacket.Members.X, HitSwitchPacket.Members.Y);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(HitSwitchPacket.Read)
            .ReturnValue(HitSwitchPacket.Members.X, HitSwitchPacket.Members.Y);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUnknown60Packet()
    {
        var graph = new PacketGraph<Unknown60Packet>(60, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(Unknown60Packet.Members.NpcIndex);
        graph.Field(Unknown60Packet.Members.RoomX);
        graph.Field(Unknown60Packet.Members.RoomY);
        graph.Field(Unknown60Packet.Members.Action);
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, 7))
            .FunctionName(Unknown60Packet.Write)
            .ParameterList(
                Unknown60Packet.Members.NpcIndex,
                Unknown60Packet.Members.RoomX,
                Unknown60Packet.Members.RoomY,
                Unknown60Packet.Members.Action
            )
            .Sizeof(
                Unknown60Packet.Members.NpcIndex,
                Unknown60Packet.Members.RoomX,
                Unknown60Packet.Members.RoomY,
                Unknown60Packet.Members.Action
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, 7))
            .FunctionName(Unknown60Packet.Read)
            .ReturnValue(
                Unknown60Packet.Members.NpcIndex,
                Unknown60Packet.Members.RoomX,
                Unknown60Packet.Members.RoomY,
                Unknown60Packet.Members.Action
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUnknown62Packet()
    {
        var graph = new PacketGraph<Unknown62Packet>(62, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(Unknown62Packet.Members.Player);
        graph.Field(Unknown62Packet.Members.DodgeType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(Unknown62Packet.Write)
            .ParameterList(Unknown62Packet.Members.Player, Unknown62Packet.Members.DodgeType)
            .Sizeof(Unknown62Packet.Members.Player, Unknown62Packet.Members.DodgeType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(Unknown62Packet.Read)
            .ReturnValue(Unknown62Packet.Members.Player, Unknown62Packet.Members.DodgeType);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncTilePaintOrCoatingPacket()
    {
        var graph = new PacketGraph<SyncTilePaintOrCoatingPacket>(63, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncTilePaintOrCoatingPacket.Members.X);
        graph.Field(SyncTilePaintOrCoatingPacket.Members.Y);
        graph.Field(SyncTilePaintOrCoatingPacket.Members.PaintOrCoating);
        graph.Field(SyncTilePaintOrCoatingPacket.Members.CoatingMode);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(PacketPaintCodec.Write)
            .ParameterList(
                SyncTilePaintOrCoatingPacket.Members.X,
                SyncTilePaintOrCoatingPacket.Members.Y,
                SyncTilePaintOrCoatingPacket.Members.PaintOrCoating,
                SyncTilePaintOrCoatingPacket.Members.CoatingMode
            )
            .Sizeof(
                SyncTilePaintOrCoatingPacket.Members.X,
                SyncTilePaintOrCoatingPacket.Members.Y,
                SyncTilePaintOrCoatingPacket.Members.PaintOrCoating,
                SyncTilePaintOrCoatingPacket.Members.CoatingMode
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(PacketPaintCodec.Read)
            .ReturnValue(
                SyncTilePaintOrCoatingPacket.Members.X,
                SyncTilePaintOrCoatingPacket.Members.Y,
                SyncTilePaintOrCoatingPacket.Members.PaintOrCoating,
                SyncTilePaintOrCoatingPacket.Members.CoatingMode
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncWallPaintOrCoatingPacket()
    {
        var graph = new PacketGraph<SyncWallPaintOrCoatingPacket>(64, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncWallPaintOrCoatingPacket.Members.X);
        graph.Field(SyncWallPaintOrCoatingPacket.Members.Y);
        graph.Field(SyncWallPaintOrCoatingPacket.Members.PaintOrCoating);
        graph.Field(SyncWallPaintOrCoatingPacket.Members.CoatingMode);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(PacketPaintCodec.Write)
            .ParameterList(
                SyncWallPaintOrCoatingPacket.Members.X,
                SyncWallPaintOrCoatingPacket.Members.Y,
                SyncWallPaintOrCoatingPacket.Members.PaintOrCoating,
                SyncWallPaintOrCoatingPacket.Members.CoatingMode
            )
            .Sizeof(
                SyncWallPaintOrCoatingPacket.Members.X,
                SyncWallPaintOrCoatingPacket.Members.Y,
                SyncWallPaintOrCoatingPacket.Members.PaintOrCoating,
                SyncWallPaintOrCoatingPacket.Members.CoatingMode
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(PacketPaintCodec.Read)
            .ReturnValue(
                SyncWallPaintOrCoatingPacket.Members.X,
                SyncWallPaintOrCoatingPacket.Members.Y,
                SyncWallPaintOrCoatingPacket.Members.PaintOrCoating,
                SyncWallPaintOrCoatingPacket.Members.CoatingMode
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUnknown66Packet()
    {
        var graph = new PacketGraph<Unknown66Packet>(66, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(Unknown66Packet.Members.Player);
        graph.Field(Unknown66Packet.Members.LifeAmount);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(Unknown66Packet.Write)
            .ParameterList(Unknown66Packet.Members.Player, Unknown66Packet.Members.LifeAmount)
            .Sizeof(Unknown66Packet.Members.Player, Unknown66Packet.Members.LifeAmount);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(Unknown66Packet.Read)
            .ReturnValue(Unknown66Packet.Members.Player, Unknown66Packet.Members.LifeAmount);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUnknown68Packet()
    {
        var graph = new PacketGraph<Unknown68Packet>(68, wireDirection: WireDirection.ClientToServer)
            .ExternalDependencies();
        graph.Field(Unknown68Packet.Members.ClientUuid);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(Unknown68Packet.Write)
            .ParameterList(Unknown68Packet.Members.ClientUuid)
            .Sizeof(Unknown68Packet.Members.ClientUuid);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(Unknown68Packet.Read)
            .ReturnValue(Unknown68Packet.Members.ClientUuid);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateBugCatchingPacket()
    {
        var graph = new PacketGraph<BugCatchingPacket>(70, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(BugCatchingPacket.Members.NpcIndex);
        graph.Field(BugCatchingPacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(BugCatchingPacket.Write)
            .ParameterList(BugCatchingPacket.Members.NpcIndex, BugCatchingPacket.Members.Player)
            .Sizeof(BugCatchingPacket.Members.NpcIndex, BugCatchingPacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(BugCatchingPacket.Read)
            .ReturnValue(BugCatchingPacket.Members.NpcIndex, BugCatchingPacket.Members.Player);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateBugReleasingPacket()
    {
        var graph = new PacketGraph<BugReleasingPacket>(71, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(BugReleasingPacket.Members.X);
        graph.Field(BugReleasingPacket.Members.Y);
        graph.Field(BugReleasingPacket.Members.NpcType);
        graph.Field(BugReleasingPacket.Members.Style);
        graph.FunctionDeclaration(lengthRange: new ByteRange(11, 11))
            .FunctionName(BugReleasingPacket.Write)
            .ParameterList(
                BugReleasingPacket.Members.X,
                BugReleasingPacket.Members.Y,
                BugReleasingPacket.Members.NpcType,
                BugReleasingPacket.Members.Style
            )
            .Sizeof(
                BugReleasingPacket.Members.X,
                BugReleasingPacket.Members.Y,
                BugReleasingPacket.Members.NpcType,
                BugReleasingPacket.Members.Style
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(11, 11))
            .FunctionName(BugReleasingPacket.Read)
            .ReturnValue(
                BugReleasingPacket.Members.X,
                BugReleasingPacket.Members.Y,
                BugReleasingPacket.Members.NpcType,
                BugReleasingPacket.Members.Style
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTravelMerchantItemsPacket()
    {
        var graph = new PacketGraph<TravelMerchantItemsPacket>(72, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies(ProtocolInputs.Members.SlotCount);
        graph.Field(TravelMerchantItemsPacket.Members.Items);
        graph.FunctionDeclaration(lengthRange: new ByteRange(0, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(TravelMerchantItemsPacket.Write)
            .ParameterList(TravelMerchantItemsPacket.Members.Items, ProtocolInputs.Members.SlotCount)
            .Sizeof(TravelMerchantItemsPacket.Members.Items);
        graph.FunctionDeclaration(lengthRange: new ByteRange(0, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(TravelMerchantItemsPacket.Read)
            .ParameterList(ProtocolInputs.Members.SlotCount)
            .ReturnValue(TravelMerchantItemsPacket.Members.Items);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateRequestTeleportationByServerPacket()
    {
        var graph = new PacketGraph<RequestTeleportationByServerPacket>(73, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(RequestTeleportationByServerPacket.Members.TeleportationKind);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, 1))
            .FunctionName(RequestTeleportationByServerPacket.Write)
            .ParameterList(RequestTeleportationByServerPacket.Members.TeleportationKind)
            .Sizeof(RequestTeleportationByServerPacket.Members.TeleportationKind);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, 1))
            .FunctionName(RequestTeleportationByServerPacket.Read)
            .ReturnValue(RequestTeleportationByServerPacket.Members.TeleportationKind);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateAnglerQuestPacket()
    {
        var graph = new PacketGraph<AnglerQuestPacket>(74, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(AnglerQuestPacket.Members.Quest);
        graph.Field(AnglerQuestPacket.Members.AlreadyFinished);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(AnglerQuestPacket.Write)
            .ParameterList(AnglerQuestPacket.Members.Quest, AnglerQuestPacket.Members.AlreadyFinished)
            .Sizeof(AnglerQuestPacket.Members.Quest, AnglerQuestPacket.Members.AlreadyFinished);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(AnglerQuestPacket.Read)
            .ReturnValue(AnglerQuestPacket.Members.Quest, AnglerQuestPacket.Members.AlreadyFinished);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateQuestsCountSyncPacket()
    {
        var graph = new PacketGraph<QuestsCountSyncPacket>(76, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(QuestsCountSyncPacket.Members.Player);
        graph.Field(QuestsCountSyncPacket.Members.AnglerQuestsFinished);
        graph.Field(QuestsCountSyncPacket.Members.GolferScore);
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(QuestsCountSyncPacket.Write)
            .ParameterList(
                QuestsCountSyncPacket.Members.Player,
                QuestsCountSyncPacket.Members.AnglerQuestsFinished,
                QuestsCountSyncPacket.Members.GolferScore
            )
            .Sizeof(
                QuestsCountSyncPacket.Members.Player,
                QuestsCountSyncPacket.Members.AnglerQuestsFinished,
                QuestsCountSyncPacket.Members.GolferScore
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(QuestsCountSyncPacket.Read)
            .ReturnValue(
                QuestsCountSyncPacket.Members.Player,
                QuestsCountSyncPacket.Members.AnglerQuestsFinished,
                QuestsCountSyncPacket.Members.GolferScore
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTemporaryAnimationPacket()
    {
        var graph = new PacketGraph<TemporaryAnimationPacket>(77, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TemporaryAnimationPacket.Members.AnimationType);
        graph.Field(TemporaryAnimationPacket.Members.TileType);
        graph.Field(TemporaryAnimationPacket.Members.X);
        graph.Field(TemporaryAnimationPacket.Members.Y);
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 8))
            .FunctionName(TemporaryAnimationPacket.Write)
            .ParameterList(
                TemporaryAnimationPacket.Members.AnimationType,
                TemporaryAnimationPacket.Members.TileType,
                TemporaryAnimationPacket.Members.X,
                TemporaryAnimationPacket.Members.Y
            )
            .Sizeof(
                TemporaryAnimationPacket.Members.AnimationType,
                TemporaryAnimationPacket.Members.TileType,
                TemporaryAnimationPacket.Members.X,
                TemporaryAnimationPacket.Members.Y
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 8))
            .FunctionName(TemporaryAnimationPacket.Read)
            .ReturnValue(
                TemporaryAnimationPacket.Members.AnimationType,
                TemporaryAnimationPacket.Members.TileType,
                TemporaryAnimationPacket.Members.X,
                TemporaryAnimationPacket.Members.Y
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateInvasionProgressReportPacket()
    {
        var graph = new PacketGraph<InvasionProgressReportPacket>(78, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(InvasionProgressReportPacket.Members.InvasionType);
        graph.Field(InvasionProgressReportPacket.Members.Progress);
        graph.Field(InvasionProgressReportPacket.Members.Wave);
        graph.Field(InvasionProgressReportPacket.Members.MaxWave);
        graph.FunctionDeclaration(lengthRange: new ByteRange(10, 10))
            .FunctionName(InvasionProgressReportPacket.Write)
            .ParameterList(
                InvasionProgressReportPacket.Members.InvasionType,
                InvasionProgressReportPacket.Members.Progress,
                InvasionProgressReportPacket.Members.Wave,
                InvasionProgressReportPacket.Members.MaxWave
            )
            .Sizeof(
                InvasionProgressReportPacket.Members.InvasionType,
                InvasionProgressReportPacket.Members.Progress,
                InvasionProgressReportPacket.Members.Wave,
                InvasionProgressReportPacket.Members.MaxWave
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(10, 10))
            .FunctionName(InvasionProgressReportPacket.Read)
            .ReturnValue(
                InvasionProgressReportPacket.Members.InvasionType,
                InvasionProgressReportPacket.Members.Progress,
                InvasionProgressReportPacket.Members.Wave,
                InvasionProgressReportPacket.Members.MaxWave
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePlaceObjectPacket()
    {
        var graph = new PacketGraph<PlaceObjectPacket>(79, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(PlaceObjectPacket.Members.X);
        graph.Field(PlaceObjectPacket.Members.Y);
        graph.Field(PlaceObjectPacket.Members.ObjectType);
        graph.Field(PlaceObjectPacket.Members.Style);
        graph.Field(PlaceObjectPacket.Members.Alternate);
        graph.Field(PlaceObjectPacket.Members.Random);
        graph.Field(PlaceObjectPacket.Members.DirectionPositive);
        graph.FunctionDeclaration(lengthRange: new ByteRange(11, 11))
            .FunctionName(PlaceObjectPacket.Write)
            .ParameterList(
                PlaceObjectPacket.Members.X,
                PlaceObjectPacket.Members.Y,
                PlaceObjectPacket.Members.ObjectType,
                PlaceObjectPacket.Members.Style,
                PlaceObjectPacket.Members.Alternate,
                PlaceObjectPacket.Members.Random,
                PlaceObjectPacket.Members.DirectionPositive
            )
            .Sizeof(
                PlaceObjectPacket.Members.X,
                PlaceObjectPacket.Members.Y,
                PlaceObjectPacket.Members.ObjectType,
                PlaceObjectPacket.Members.Style,
                PlaceObjectPacket.Members.Alternate,
                PlaceObjectPacket.Members.Random,
                PlaceObjectPacket.Members.DirectionPositive
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(11, 11))
            .FunctionName(PlaceObjectPacket.Read)
            .ReturnValue(
                PlaceObjectPacket.Members.X,
                PlaceObjectPacket.Members.Y,
                PlaceObjectPacket.Members.ObjectType,
                PlaceObjectPacket.Members.Style,
                PlaceObjectPacket.Members.Alternate,
                PlaceObjectPacket.Members.Random,
                PlaceObjectPacket.Members.DirectionPositive
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncPlayerChestIndexPacket()
    {
        var graph = new PacketGraph<SyncPlayerChestIndexPacket>(80, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncPlayerChestIndexPacket.Members.Player);
        graph.Field(SyncPlayerChestIndexPacket.Members.ChestIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(SyncPlayerChestIndexPacket.Write)
            .ParameterList(SyncPlayerChestIndexPacket.Members.Player, SyncPlayerChestIndexPacket.Members.ChestIndex)
            .Sizeof(SyncPlayerChestIndexPacket.Members.Player, SyncPlayerChestIndexPacket.Members.ChestIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(SyncPlayerChestIndexPacket.Read)
            .ReturnValue(SyncPlayerChestIndexPacket.Members.Player, SyncPlayerChestIndexPacket.Members.ChestIndex);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateCombatTextIntPacket()
    {
        var graph = new PacketGraph<CombatTextIntPacket>(81, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(CombatTextIntPacket.Members.X);
        graph.Field(CombatTextIntPacket.Members.Y);
        graph.Field(CombatTextIntPacket.Members.Color);
        graph.Field(CombatTextIntPacket.Members.Amount);
        graph.FunctionDeclaration(lengthRange: new ByteRange(15, 15))
            .FunctionName(CombatTextIntPacket.Write)
            .ParameterList(
                CombatTextIntPacket.Members.X,
                CombatTextIntPacket.Members.Y,
                CombatTextIntPacket.Members.Color,
                CombatTextIntPacket.Members.Amount
            )
            .Sizeof(
                CombatTextIntPacket.Members.X,
                CombatTextIntPacket.Members.Y,
                CombatTextIntPacket.Members.Color,
                CombatTextIntPacket.Members.Amount
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(15, 15))
            .FunctionName(CombatTextIntPacket.Read)
            .ReturnValue(
                CombatTextIntPacket.Members.X,
                CombatTextIntPacket.Members.Y,
                CombatTextIntPacket.Members.Color,
                CombatTextIntPacket.Members.Amount
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePlayerStealthPacket()
    {
        var graph = new PacketGraph<PlayerStealthPacket>(84, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(PlayerStealthPacket.Members.Player);
        graph.Field(PlayerStealthPacket.Members.Stealth);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(PlayerStealthPacket.Write)
            .ParameterList(PlayerStealthPacket.Members.Player, PlayerStealthPacket.Members.Stealth)
            .Sizeof(PlayerStealthPacket.Members.Player, PlayerStealthPacket.Members.Stealth);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(PlayerStealthPacket.Read)
            .ReturnValue(PlayerStealthPacket.Members.Player, PlayerStealthPacket.Members.Stealth);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTileEntityPlacementPacket()
    {
        var graph = new PacketGraph<TileEntityPlacementPacket>(87, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TileEntityPlacementPacket.Members.X);
        graph.Field(TileEntityPlacementPacket.Members.Y);
        graph.Field(TileEntityPlacementPacket.Members.EntityType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(TileEntityPlacementPacket.Write)
            .ParameterList(TileEntityPlacementPacket.Members.X, TileEntityPlacementPacket.Members.Y, TileEntityPlacementPacket.Members.EntityType)
            .Sizeof(TileEntityPlacementPacket.Members.X, TileEntityPlacementPacket.Members.Y, TileEntityPlacementPacket.Members.EntityType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(TileEntityPlacementPacket.Read)
            .ReturnValue(TileEntityPlacementPacket.Members.X, TileEntityPlacementPacket.Members.Y, TileEntityPlacementPacket.Members.EntityType);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateItemTweakerPacket()
    {
        var graph = new PacketGraph<ItemTweakerPacket>(88, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        graph.Field(ItemTweakerPacket.Members.ItemIndex);
        graph.Field(ItemTweakerPacket.Members.Flags);
        graph.Field(ItemTweakerPacket.Members.Color);
        graph.Field(ItemTweakerPacket.Members.Damage);
        graph.Field(ItemTweakerPacket.Members.Knockback);
        graph.Field(ItemTweakerPacket.Members.UseAnimation);
        graph.Field(ItemTweakerPacket.Members.UseTime);
        graph.Field(ItemTweakerPacket.Members.Shoot);
        graph.Field(ItemTweakerPacket.Members.ShootSpeed);
        graph.Field(ItemTweakerPacket.Members.Extra);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 37))
            .FunctionName(ItemTweakerPacket.Write)
            .ParameterList(
                ItemTweakerPacket.Members.ItemIndex,
                ItemTweakerPacket.Members.Flags,
                ItemTweakerPacket.Members.Color,
                ItemTweakerPacket.Members.Damage,
                ItemTweakerPacket.Members.Knockback,
                ItemTweakerPacket.Members.UseAnimation,
                ItemTweakerPacket.Members.UseTime,
                ItemTweakerPacket.Members.Shoot,
                ItemTweakerPacket.Members.ShootSpeed,
                ItemTweakerPacket.Members.Extra
            )
            .Sizeof(
                ItemTweakerPacket.Members.ItemIndex,
                ItemTweakerPacket.Members.Flags,
                ItemTweakerPacket.Members.Color,
                ItemTweakerPacket.Members.Damage,
                ItemTweakerPacket.Members.Knockback,
                ItemTweakerPacket.Members.UseAnimation,
                ItemTweakerPacket.Members.UseTime,
                ItemTweakerPacket.Members.Shoot,
                ItemTweakerPacket.Members.ShootSpeed,
                ItemTweakerPacket.Members.Extra
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 37))
            .FunctionName(ItemTweakerPacket.Read)
            .ReturnValue(
                ItemTweakerPacket.Members.ItemIndex,
                ItemTweakerPacket.Members.Flags,
                ItemTweakerPacket.Members.Color,
                ItemTweakerPacket.Members.Damage,
                ItemTweakerPacket.Members.Knockback,
                ItemTweakerPacket.Members.UseAnimation,
                ItemTweakerPacket.Members.UseTime,
                ItemTweakerPacket.Members.Shoot,
                ItemTweakerPacket.Members.ShootSpeed,
                ItemTweakerPacket.Members.Extra
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateItemFrameTryPlacingPacket()
    {
        var graph = new PacketGraph<ItemFrameTryPlacingPacket>(89, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(ItemFrameTryPlacingPacket.Members.X);
        graph.Field(ItemFrameTryPlacingPacket.Members.Y);
        graph.Field(ItemFrameTryPlacingPacket.Members.ItemType);
        graph.Field(ItemFrameTryPlacingPacket.Members.Prefix);
        graph.Field(ItemFrameTryPlacingPacket.Members.Stack);
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(ItemFrameTryPlacingPacket.Write)
            .ParameterList(
                ItemFrameTryPlacingPacket.Members.X,
                ItemFrameTryPlacingPacket.Members.Y,
                ItemFrameTryPlacingPacket.Members.ItemType,
                ItemFrameTryPlacingPacket.Members.Prefix,
                ItemFrameTryPlacingPacket.Members.Stack
            )
            .Sizeof(
                ItemFrameTryPlacingPacket.Members.X,
                ItemFrameTryPlacingPacket.Members.Y,
                ItemFrameTryPlacingPacket.Members.ItemType,
                ItemFrameTryPlacingPacket.Members.Prefix,
                ItemFrameTryPlacingPacket.Members.Stack
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(ItemFrameTryPlacingPacket.Read)
            .ReturnValue(
                ItemFrameTryPlacingPacket.Members.X,
                ItemFrameTryPlacingPacket.Members.Y,
                ItemFrameTryPlacingPacket.Members.ItemType,
                ItemFrameTryPlacingPacket.Members.Prefix,
                ItemFrameTryPlacingPacket.Members.Stack
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateInstancedItemPacket()
    {
        var graph = new PacketGraph<InstancedItemPacket>(90, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(InstancedItemPacket.Members.ItemIndex);
        graph.Field(InstancedItemPacket.Members.PositionX);
        graph.Field(InstancedItemPacket.Members.PositionY);
        graph.Field(InstancedItemPacket.Members.VelocityX);
        graph.Field(InstancedItemPacket.Members.VelocityY);
        graph.Field(InstancedItemPacket.Members.Stack);
        graph.Field(InstancedItemPacket.Members.Prefix);
        graph.Field(InstancedItemPacket.Members.StateFlags);
        graph.Field(InstancedItemPacket.Members.ItemType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(24, 24))
            .FunctionName(PacketItemSyncCodec.WriteFields)
            .ParameterList(
                InstancedItemPacket.Members.ItemIndex,
                InstancedItemPacket.Members.PositionX,
                InstancedItemPacket.Members.PositionY,
                InstancedItemPacket.Members.VelocityX,
                InstancedItemPacket.Members.VelocityY,
                InstancedItemPacket.Members.Stack,
                InstancedItemPacket.Members.Prefix,
                InstancedItemPacket.Members.StateFlags,
                InstancedItemPacket.Members.ItemType
            )
            .Sizeof(
                InstancedItemPacket.Members.ItemIndex,
                InstancedItemPacket.Members.PositionX,
                InstancedItemPacket.Members.PositionY,
                InstancedItemPacket.Members.VelocityX,
                InstancedItemPacket.Members.VelocityY,
                InstancedItemPacket.Members.Stack,
                InstancedItemPacket.Members.Prefix,
                InstancedItemPacket.Members.StateFlags,
                InstancedItemPacket.Members.ItemType
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(24, 24))
            .FunctionName(PacketItemSyncCodec.ReadFields)
            .ReturnValue(
                InstancedItemPacket.Members.ItemIndex,
                InstancedItemPacket.Members.PositionX,
                InstancedItemPacket.Members.PositionY,
                InstancedItemPacket.Members.VelocityX,
                InstancedItemPacket.Members.VelocityY,
                InstancedItemPacket.Members.Stack,
                InstancedItemPacket.Members.Prefix,
                InstancedItemPacket.Members.StateFlags,
                InstancedItemPacket.Members.ItemType
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncExtraValuePacket()
    {
        var graph = new PacketGraph<SyncExtraValuePacket>(92, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncExtraValuePacket.Members.NpcIndex);
        graph.Field(SyncExtraValuePacket.Members.ExtraValue);
        graph.Field(SyncExtraValuePacket.Members.ValueX);
        graph.Field(SyncExtraValuePacket.Members.ValueY);
        graph.FunctionDeclaration(lengthRange: new ByteRange(14, 14))
            .FunctionName(SyncExtraValuePacket.Write)
            .ParameterList(
                SyncExtraValuePacket.Members.NpcIndex,
                SyncExtraValuePacket.Members.ExtraValue,
                SyncExtraValuePacket.Members.ValueX,
                SyncExtraValuePacket.Members.ValueY
            )
            .Sizeof(
                SyncExtraValuePacket.Members.NpcIndex,
                SyncExtraValuePacket.Members.ExtraValue,
                SyncExtraValuePacket.Members.ValueX,
                SyncExtraValuePacket.Members.ValueY
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(14, 14))
            .FunctionName(SyncExtraValuePacket.Read)
            .ReturnValue(
                SyncExtraValuePacket.Members.NpcIndex,
                SyncExtraValuePacket.Members.ExtraValue,
                SyncExtraValuePacket.Members.ValueX,
                SyncExtraValuePacket.Members.ValueY
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateMurderSomeoneElsesPortalPacket()
    {
        var graph = new PacketGraph<MurderSomeoneElsesPortalPacket>(95, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(MurderSomeoneElsesPortalPacket.Members.ProjectileOwner);
        graph.Field(MurderSomeoneElsesPortalPacket.Members.PortalIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(MurderSomeoneElsesPortalPacket.Write)
            .ParameterList(MurderSomeoneElsesPortalPacket.Members.ProjectileOwner, MurderSomeoneElsesPortalPacket.Members.PortalIndex)
            .Sizeof(MurderSomeoneElsesPortalPacket.Members.ProjectileOwner, MurderSomeoneElsesPortalPacket.Members.PortalIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(MurderSomeoneElsesPortalPacket.Read)
            .ReturnValue(MurderSomeoneElsesPortalPacket.Members.ProjectileOwner, MurderSomeoneElsesPortalPacket.Members.PortalIndex);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTeleportPlayerThroughPortalPacket()
    {
        var graph = new PacketGraph<TeleportPlayerThroughPortalPacket>(96, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TeleportPlayerThroughPortalPacket.Members.Player);
        graph.Field(TeleportPlayerThroughPortalPacket.Members.PortalColorIndex);
        graph.Field(TeleportPlayerThroughPortalPacket.Members.PositionX);
        graph.Field(TeleportPlayerThroughPortalPacket.Members.PositionY);
        graph.Field(TeleportPlayerThroughPortalPacket.Members.VelocityX);
        graph.Field(TeleportPlayerThroughPortalPacket.Members.VelocityY);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateMinionRestTargetUpdatePacket()
    {
        var graph = new PacketGraph<MinionRestTargetUpdatePacket>(99, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(MinionRestTargetUpdatePacket.Members.Player);
        graph.Field(MinionRestTargetUpdatePacket.Members.TargetX);
        graph.Field(MinionRestTargetUpdatePacket.Members.TargetY);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTeleportNPCThroughPortalPacket()
    {
        var graph = new PacketGraph<TeleportNPCThroughPortalPacket>(100, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TeleportNPCThroughPortalPacket.Members.NpcIndex);
        graph.Field(TeleportNPCThroughPortalPacket.Members.PortalColorIndex);
        graph.Field(TeleportNPCThroughPortalPacket.Members.PositionX);
        graph.Field(TeleportNPCThroughPortalPacket.Members.PositionY);
        graph.Field(TeleportNPCThroughPortalPacket.Members.VelocityX);
        graph.Field(TeleportNPCThroughPortalPacket.Members.VelocityY);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUpdateTowerShieldStrengthsPacket()
    {
        var graph = new PacketGraph<UpdateTowerShieldStrengthsPacket>(101, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(UpdateTowerShieldStrengthsPacket.Members.Solar);
        graph.Field(UpdateTowerShieldStrengthsPacket.Members.Vortex);
        graph.Field(UpdateTowerShieldStrengthsPacket.Members.Nebula);
        graph.Field(UpdateTowerShieldStrengthsPacket.Members.Stardust);
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 8))
            .FunctionName(UpdateTowerShieldStrengthsPacket.Write)
            .ParameterList(
                UpdateTowerShieldStrengthsPacket.Members.Solar,
                UpdateTowerShieldStrengthsPacket.Members.Vortex,
                UpdateTowerShieldStrengthsPacket.Members.Nebula,
                UpdateTowerShieldStrengthsPacket.Members.Stardust
            )
            .Sizeof(
                UpdateTowerShieldStrengthsPacket.Members.Solar,
                UpdateTowerShieldStrengthsPacket.Members.Vortex,
                UpdateTowerShieldStrengthsPacket.Members.Nebula,
                UpdateTowerShieldStrengthsPacket.Members.Stardust
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 8))
            .FunctionName(UpdateTowerShieldStrengthsPacket.Read)
            .ReturnValue(
                UpdateTowerShieldStrengthsPacket.Members.Solar,
                UpdateTowerShieldStrengthsPacket.Members.Vortex,
                UpdateTowerShieldStrengthsPacket.Members.Nebula,
                UpdateTowerShieldStrengthsPacket.Members.Stardust
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateNebulaLevelupRequestPacket()
    {
        var graph = new PacketGraph<NebulaLevelupRequestPacket>(102, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(NebulaLevelupRequestPacket.Members.Player);
        graph.Field(NebulaLevelupRequestPacket.Members.ItemType);
        graph.Field(NebulaLevelupRequestPacket.Members.Position);
        graph.FunctionDeclaration(lengthRange: new ByteRange(11, 11))
            .FunctionName(NebulaLevelupRequestPacket.Write)
            .ParameterList(NebulaLevelupRequestPacket.Members.Player, NebulaLevelupRequestPacket.Members.ItemType, NebulaLevelupRequestPacket.Members.Position)
            .Sizeof(NebulaLevelupRequestPacket.Members.Player, NebulaLevelupRequestPacket.Members.ItemType, NebulaLevelupRequestPacket.Members.Position);
        graph.FunctionDeclaration(lengthRange: new ByteRange(11, 11))
            .FunctionName(NebulaLevelupRequestPacket.Read)
            .ReturnValue(NebulaLevelupRequestPacket.Members.Player, NebulaLevelupRequestPacket.Members.ItemType, NebulaLevelupRequestPacket.Members.Position);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateMoonlordHorrorPacket()
    {
        var graph = new PacketGraph<MoonlordHorrorPacket>(103, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(MoonlordHorrorPacket.Members.MaximumCountdown);
        graph.Field(MoonlordHorrorPacket.Members.Countdown);
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 8))
            .FunctionName(MoonlordHorrorPacket.Write)
            .ParameterList(MoonlordHorrorPacket.Members.MaximumCountdown, MoonlordHorrorPacket.Members.Countdown)
            .Sizeof(MoonlordHorrorPacket.Members.MaximumCountdown, MoonlordHorrorPacket.Members.Countdown);
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 8))
            .FunctionName(MoonlordHorrorPacket.Read)
            .ReturnValue(MoonlordHorrorPacket.Members.MaximumCountdown, MoonlordHorrorPacket.Members.Countdown);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateShopOverridePacket()
    {
        var graph = new PacketGraph<ShopOverridePacket>(104, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(ShopOverridePacket.Members.Player);
        graph.Field(ShopOverridePacket.Members.NpcType);
        graph.Field(ShopOverridePacket.Members.PriceAdjustment);
        graph.Field(ShopOverridePacket.Members.ShopId);
        graph.Field(ShopOverridePacket.Members.SpecialCurrency);
        graph.Field(ShopOverridePacket.Members.ShopType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(13, 13))
            .FunctionName(ShopOverridePacket.Write)
            .ParameterList(
                ShopOverridePacket.Members.Player,
                ShopOverridePacket.Members.NpcType,
                ShopOverridePacket.Members.PriceAdjustment,
                ShopOverridePacket.Members.ShopId,
                ShopOverridePacket.Members.SpecialCurrency,
                ShopOverridePacket.Members.ShopType
            )
            .Sizeof(
                ShopOverridePacket.Members.Player,
                ShopOverridePacket.Members.NpcType,
                ShopOverridePacket.Members.PriceAdjustment,
                ShopOverridePacket.Members.ShopId,
                ShopOverridePacket.Members.SpecialCurrency,
                ShopOverridePacket.Members.ShopType
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(13, 13))
            .FunctionName(ShopOverridePacket.Read)
            .ReturnValue(
                ShopOverridePacket.Members.Player,
                ShopOverridePacket.Members.NpcType,
                ShopOverridePacket.Members.PriceAdjustment,
                ShopOverridePacket.Members.ShopId,
                ShopOverridePacket.Members.SpecialCurrency,
                ShopOverridePacket.Members.ShopType
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateGemLockTogglePacket()
    {
        var graph = new PacketGraph<GemLockTogglePacket>(105, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(GemLockTogglePacket.Members.X);
        graph.Field(GemLockTogglePacket.Members.Y);
        graph.Field(GemLockTogglePacket.Members.Enabled);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(GemLockTogglePacket.Write)
            .ParameterList(GemLockTogglePacket.Members.X, GemLockTogglePacket.Members.Y, GemLockTogglePacket.Members.Enabled)
            .Sizeof(GemLockTogglePacket.Members.X, GemLockTogglePacket.Members.Y, GemLockTogglePacket.Members.Enabled);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(GemLockTogglePacket.Read)
            .ReturnValue(GemLockTogglePacket.Members.X, GemLockTogglePacket.Members.Y, GemLockTogglePacket.Members.Enabled);
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePoofOfSmokePacket()
    {
        var graph = new PacketGraph<PoofOfSmokePacket>(106, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(PoofOfSmokePacket.Members.PackedPosition);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(PoofOfSmokePacket.Write)
            .ParameterList(PoofOfSmokePacket.Members.PackedPosition)
            .Sizeof(PoofOfSmokePacket.Members.PackedPosition);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(PoofOfSmokePacket.Read)
            .ReturnValue(PoofOfSmokePacket.Members.PackedPosition);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateWiredCannonShotPacket()
    {
        var graph = new PacketGraph<WiredCannonShotPacket>(108, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(WiredCannonShotPacket.Members.Damage);
        graph.Field(WiredCannonShotPacket.Members.Knockback);
        graph.Field(WiredCannonShotPacket.Members.X);
        graph.Field(WiredCannonShotPacket.Members.Y);
        graph.Field(WiredCannonShotPacket.Members.Angle);
        graph.Field(WiredCannonShotPacket.Members.Ammo);
        graph.Field(WiredCannonShotPacket.Members.Owner);
        graph.FunctionDeclaration(lengthRange: new ByteRange(15, 15))
            .FunctionName(WiredCannonShotPacket.Write)
            .ParameterList(
                WiredCannonShotPacket.Members.Damage,
                WiredCannonShotPacket.Members.Knockback,
                WiredCannonShotPacket.Members.X,
                WiredCannonShotPacket.Members.Y,
                WiredCannonShotPacket.Members.Angle,
                WiredCannonShotPacket.Members.Ammo,
                WiredCannonShotPacket.Members.Owner
            )
            .Sizeof(
                WiredCannonShotPacket.Members.Damage,
                WiredCannonShotPacket.Members.Knockback,
                WiredCannonShotPacket.Members.X,
                WiredCannonShotPacket.Members.Y,
                WiredCannonShotPacket.Members.Angle,
                WiredCannonShotPacket.Members.Ammo,
                WiredCannonShotPacket.Members.Owner
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(15, 15))
            .FunctionName(WiredCannonShotPacket.Read)
            .ReturnValue(
                WiredCannonShotPacket.Members.Damage,
                WiredCannonShotPacket.Members.Knockback,
                WiredCannonShotPacket.Members.X,
                WiredCannonShotPacket.Members.Y,
                WiredCannonShotPacket.Members.Angle,
                WiredCannonShotPacket.Members.Ammo,
                WiredCannonShotPacket.Members.Owner
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateMassWireOperationPacket()
    {
        var graph = new PacketGraph<MassWireOperationPacket>(109, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(MassWireOperationPacket.Members.StartX);
        graph.Field(MassWireOperationPacket.Members.StartY);
        graph.Field(MassWireOperationPacket.Members.EndX);
        graph.Field(MassWireOperationPacket.Members.EndY);
        graph.Field(MassWireOperationPacket.Members.ToolMode);
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(MassWireOperationPacket.Write)
            .ParameterList(
                MassWireOperationPacket.Members.StartX,
                MassWireOperationPacket.Members.StartY,
                MassWireOperationPacket.Members.EndX,
                MassWireOperationPacket.Members.EndY,
                MassWireOperationPacket.Members.ToolMode
            )
            .Sizeof(
                MassWireOperationPacket.Members.StartX,
                MassWireOperationPacket.Members.StartY,
                MassWireOperationPacket.Members.EndX,
                MassWireOperationPacket.Members.EndY,
                MassWireOperationPacket.Members.ToolMode
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(MassWireOperationPacket.Read)
            .ReturnValue(
                MassWireOperationPacket.Members.StartX,
                MassWireOperationPacket.Members.StartY,
                MassWireOperationPacket.Members.EndX,
                MassWireOperationPacket.Members.EndY,
                MassWireOperationPacket.Members.ToolMode
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateMassWireOperationPayPacket()
    {
        var graph = new PacketGraph<MassWireOperationPayPacket>(110, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(MassWireOperationPayPacket.Members.ItemType);
        graph.Field(MassWireOperationPayPacket.Members.Count);
        graph.Field(MassWireOperationPayPacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(MassWireOperationPayPacket.Write)
            .ParameterList(MassWireOperationPayPacket.Members.ItemType, MassWireOperationPayPacket.Members.Count, MassWireOperationPayPacket.Members.Player)
            .Sizeof(MassWireOperationPayPacket.Members.ItemType, MassWireOperationPayPacket.Members.Count, MassWireOperationPayPacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(MassWireOperationPayPacket.Read)
            .ReturnValue(MassWireOperationPayPacket.Members.ItemType, MassWireOperationPayPacket.Members.Count, MassWireOperationPayPacket.Members.Player);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTogglePartyPacket()
    {
        var graph = new PacketGraph<TogglePartyPacket>(111, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSpecialFXPacket()
    {
        var graph = new PacketGraph<SpecialFXPacket>(112, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SpecialFXPacket.Members.EffectType);
        graph.Field(SpecialFXPacket.Members.X);
        graph.Field(SpecialFXPacket.Members.Y);
        graph.Field(SpecialFXPacket.Members.Parameter);
        graph.Field(SpecialFXPacket.Members.Style);
        graph.Field(SpecialFXPacket.Members.Flag);
        graph.FunctionDeclaration(lengthRange: new ByteRange(13, 13))
            .FunctionName(SpecialFXPacket.Write)
            .ParameterList(
                SpecialFXPacket.Members.EffectType,
                SpecialFXPacket.Members.X,
                SpecialFXPacket.Members.Y,
                SpecialFXPacket.Members.Parameter,
                SpecialFXPacket.Members.Style,
                SpecialFXPacket.Members.Flag
            )
            .Sizeof(
                SpecialFXPacket.Members.EffectType,
                SpecialFXPacket.Members.X,
                SpecialFXPacket.Members.Y,
                SpecialFXPacket.Members.Parameter,
                SpecialFXPacket.Members.Style,
                SpecialFXPacket.Members.Flag
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(13, 13))
            .FunctionName(SpecialFXPacket.Read)
            .ReturnValue(
                SpecialFXPacket.Members.EffectType,
                SpecialFXPacket.Members.X,
                SpecialFXPacket.Members.Y,
                SpecialFXPacket.Members.Parameter,
                SpecialFXPacket.Members.Style,
                SpecialFXPacket.Members.Flag
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateCrystalInvasionStartPacket()
    {
        var graph = new PacketGraph<CrystalInvasionStartPacket>(113, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(CrystalInvasionStartPacket.Members.X);
        graph.Field(CrystalInvasionStartPacket.Members.Y);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(CrystalInvasionStartPacket.Write)
            .ParameterList(CrystalInvasionStartPacket.Members.X, CrystalInvasionStartPacket.Members.Y)
            .Sizeof(CrystalInvasionStartPacket.Members.X, CrystalInvasionStartPacket.Members.Y);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(CrystalInvasionStartPacket.Read)
            .ReturnValue(CrystalInvasionStartPacket.Members.X, CrystalInvasionStartPacket.Members.Y);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateMinionAttackTargetUpdatePacket()
    {
        var graph = new PacketGraph<MinionAttackTargetUpdatePacket>(115, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(MinionAttackTargetUpdatePacket.Members.Player);
        graph.Field(MinionAttackTargetUpdatePacket.Members.NpcIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(MinionAttackTargetUpdatePacket.Write)
            .ParameterList(MinionAttackTargetUpdatePacket.Members.Player, MinionAttackTargetUpdatePacket.Members.NpcIndex)
            .Sizeof(MinionAttackTargetUpdatePacket.Members.Player, MinionAttackTargetUpdatePacket.Members.NpcIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(MinionAttackTargetUpdatePacket.Read)
            .ReturnValue(MinionAttackTargetUpdatePacket.Members.Player, MinionAttackTargetUpdatePacket.Members.NpcIndex);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateCrystalInvasionSendWaitTimePacket()
    {
        var graph = new PacketGraph<CrystalInvasionSendWaitTimePacket>(116, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(CrystalInvasionSendWaitTimePacket.Members.WaitTime);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(CrystalInvasionSendWaitTimePacket.Write)
            .ParameterList(CrystalInvasionSendWaitTimePacket.Members.WaitTime)
            .Sizeof(CrystalInvasionSendWaitTimePacket.Members.WaitTime);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(CrystalInvasionSendWaitTimePacket.Read)
            .ReturnValue(CrystalInvasionSendWaitTimePacket.Members.WaitTime);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateCombatTextStringPacket()
    {
        var graph = new PacketGraph<CombatTextStringPacket>(119, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(CombatTextStringPacket.Members.X);
        graph.Field(CombatTextStringPacket.Members.Y);
        graph.Field(CombatTextStringPacket.Members.Color, PacketWireFormats.Rgb);
        graph.Field(CombatTextStringPacket.Members.Text, PacketWireFormats.NetworkText);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateEmojiPacket()
    {
        var graph = new PacketGraph<EmojiPacket>(120, wireDirection: WireDirection.ClientToServer)
            .ExternalDependencies();
        graph.Field(EmojiPacket.Members.Player);
        graph.Field(EmojiPacket.Members.Emote);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(EmojiPacket.Write)
            .ParameterList(EmojiPacket.Members.Player, EmojiPacket.Members.Emote)
            .Sizeof(EmojiPacket.Members.Player, EmojiPacket.Members.Emote);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(EmojiPacket.Read)
            .ReturnValue(EmojiPacket.Members.Player, EmojiPacket.Members.Emote);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncItemsWithShimmerPacket()
    {
        var graph = new PacketGraph<SyncItemsWithShimmerPacket>(145, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncItemsWithShimmerPacket.Members.Item);
        graph.Field(SyncItemsWithShimmerPacket.Members.Shimmered);
        graph.Field(SyncItemsWithShimmerPacket.Members.ShimmerTime);
        graph.FunctionDeclaration(lengthRange: new ByteRange(29, 29))
            .FunctionName(SyncItemsWithShimmerPacket.Write)
            .ParameterList(SyncItemsWithShimmerPacket.Members.Item, SyncItemsWithShimmerPacket.Members.Shimmered, SyncItemsWithShimmerPacket.Members.ShimmerTime)
            .Sizeof(SyncItemsWithShimmerPacket.Members.Item, SyncItemsWithShimmerPacket.Members.Shimmered, SyncItemsWithShimmerPacket.Members.ShimmerTime);
        graph.FunctionDeclaration(lengthRange: new ByteRange(29, 29))
            .FunctionName(SyncItemsWithShimmerPacket.Read)
            .ReturnValue(SyncItemsWithShimmerPacket.Members.Item, SyncItemsWithShimmerPacket.Members.Shimmered, SyncItemsWithShimmerPacket.Members.ShimmerTime);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncItemCannotBeTakenByEnemiesPacket()
    {
        var graph = new PacketGraph<SyncItemCannotBeTakenByEnemiesPacket>(148, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncItemCannotBeTakenByEnemiesPacket.Members.Item);
        graph.Field(SyncItemCannotBeTakenByEnemiesPacket.Members.CannotBeTakenTimer);
        graph.FunctionDeclaration(lengthRange: new ByteRange(25, 25))
            .FunctionName(SyncItemCannotBeTakenByEnemiesPacket.Write)
            .ParameterList(SyncItemCannotBeTakenByEnemiesPacket.Members.Item, SyncItemCannotBeTakenByEnemiesPacket.Members.CannotBeTakenTimer)
            .Sizeof(SyncItemCannotBeTakenByEnemiesPacket.Members.Item, SyncItemCannotBeTakenByEnemiesPacket.Members.CannotBeTakenTimer);
        graph.FunctionDeclaration(lengthRange: new ByteRange(25, 25))
            .FunctionName(SyncItemCannotBeTakenByEnemiesPacket.Read)
            .ReturnValue(SyncItemCannotBeTakenByEnemiesPacket.Members.Item, SyncItemCannotBeTakenByEnemiesPacket.Members.CannotBeTakenTimer);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncItemDespawnPacket()
    {
        var graph = new PacketGraph<SyncItemDespawnPacket>(151, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncItemDespawnPacket.Members.ItemIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(SyncItemDespawnPacket.Write)
            .ParameterList(SyncItemDespawnPacket.Members.ItemIndex)
            .Sizeof(SyncItemDespawnPacket.Members.ItemIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(SyncItemDespawnPacket.Read)
            .ReturnValue(SyncItemDespawnPacket.Members.ItemIndex);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTeamChangeFromUIPacket()
    {
        var graph = new PacketGraph<TeamChangeFromUIPacket>(157, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TeamChangeFromUIPacket.Members.Player);
        graph.Field(TeamChangeFromUIPacket.Members.Team);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(TeamChangeCodec.Write)
            .ParameterList(TeamChangeFromUIPacket.Members.Player, TeamChangeFromUIPacket.Members.Team)
            .Sizeof(TeamChangeFromUIPacket.Members.Player, TeamChangeFromUIPacket.Members.Team);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(TeamChangeCodec.Read)
            .ReturnValue(TeamChangeFromUIPacket.Members.Player, TeamChangeFromUIPacket.Members.Team);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTEDisplayDollDataSyncPacket()
    {
        var graph = new PacketGraph<TEDisplayDollDataSyncPacket>(121, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TEDisplayDollDataSyncPacket.Members.Player);
        graph.Field(TEDisplayDollDataSyncPacket.Members.EntityId);
        graph.Field(TEDisplayDollDataSyncPacket.Members.ItemIndex);
        graph.Field(TEDisplayDollDataSyncPacket.Members.Command);
        graph.Field(TEDisplayDollDataSyncPacket.Members.Item);
        graph.Field(TEDisplayDollDataSyncPacket.Members.Pose);
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 12))
            .FunctionName(TEDisplayDollDataSyncPacket.Write)
            .ParameterList(
                TEDisplayDollDataSyncPacket.Members.Player,
                TEDisplayDollDataSyncPacket.Members.EntityId,
                TEDisplayDollDataSyncPacket.Members.ItemIndex,
                TEDisplayDollDataSyncPacket.Members.Command,
                TEDisplayDollDataSyncPacket.Members.Item,
                TEDisplayDollDataSyncPacket.Members.Pose
            )
            .Sizeof(
                TEDisplayDollDataSyncPacket.Members.Player,
                TEDisplayDollDataSyncPacket.Members.EntityId,
                TEDisplayDollDataSyncPacket.Members.ItemIndex,
                TEDisplayDollDataSyncPacket.Members.Command,
                TEDisplayDollDataSyncPacket.Members.Item,
                TEDisplayDollDataSyncPacket.Members.Pose
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(8, 12))
            .FunctionName(TEDisplayDollDataSyncPacket.Read)
            .ReturnValue(
                TEDisplayDollDataSyncPacket.Members.Player,
                TEDisplayDollDataSyncPacket.Members.EntityId,
                TEDisplayDollDataSyncPacket.Members.ItemIndex,
                TEDisplayDollDataSyncPacket.Members.Command,
                TEDisplayDollDataSyncPacket.Members.Item,
                TEDisplayDollDataSyncPacket.Members.Pose
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateRequestTileEntityInteractionPacket()
    {
        var graph = new PacketGraph<RequestTileEntityInteractionPacket>(122, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(RequestTileEntityInteractionPacket.Members.EntityId);
        graph.Field(RequestTileEntityInteractionPacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(RequestTileEntityInteractionPacket.Write)
            .ParameterList(RequestTileEntityInteractionPacket.Members.EntityId, RequestTileEntityInteractionPacket.Members.Player)
            .Sizeof(RequestTileEntityInteractionPacket.Members.EntityId, RequestTileEntityInteractionPacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(RequestTileEntityInteractionPacket.Read)
            .ReturnValue(RequestTileEntityInteractionPacket.Members.EntityId, RequestTileEntityInteractionPacket.Members.Player);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateWeaponsRackTryPlacingPacket()
    {
        var graph = new PacketGraph<WeaponsRackTryPlacingPacket>(123, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(WeaponsRackTryPlacingPacket.Members.X);
        graph.Field(WeaponsRackTryPlacingPacket.Members.Y);
        graph.Field(WeaponsRackTryPlacingPacket.Members.ItemType);
        graph.Field(WeaponsRackTryPlacingPacket.Members.Prefix);
        graph.Field(WeaponsRackTryPlacingPacket.Members.Stack);
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(WeaponsRackTryPlacingPacket.Write)
            .ParameterList(
                WeaponsRackTryPlacingPacket.Members.X,
                WeaponsRackTryPlacingPacket.Members.Y,
                WeaponsRackTryPlacingPacket.Members.ItemType,
                WeaponsRackTryPlacingPacket.Members.Prefix,
                WeaponsRackTryPlacingPacket.Members.Stack
            )
            .Sizeof(
                WeaponsRackTryPlacingPacket.Members.X,
                WeaponsRackTryPlacingPacket.Members.Y,
                WeaponsRackTryPlacingPacket.Members.ItemType,
                WeaponsRackTryPlacingPacket.Members.Prefix,
                WeaponsRackTryPlacingPacket.Members.Stack
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(WeaponsRackTryPlacingPacket.Read)
            .ReturnValue(
                WeaponsRackTryPlacingPacket.Members.X,
                WeaponsRackTryPlacingPacket.Members.Y,
                WeaponsRackTryPlacingPacket.Members.ItemType,
                WeaponsRackTryPlacingPacket.Members.Prefix,
                WeaponsRackTryPlacingPacket.Members.Stack
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTEHatRackItemSyncPacket()
    {
        var graph = new PacketGraph<TEHatRackItemSyncPacket>(124, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TEHatRackItemSyncPacket.Members.Player);
        graph.Field(TEHatRackItemSyncPacket.Members.EntityId);
        graph.Field(TEHatRackItemSyncPacket.Members.EncodedSlot);
        graph.Field(TEHatRackItemSyncPacket.Members.Item);
        graph.FunctionDeclaration(lengthRange: new ByteRange(11, 11))
            .FunctionName(TEHatRackItemSyncPacket.Write)
            .ParameterList(
                TEHatRackItemSyncPacket.Members.Player,
                TEHatRackItemSyncPacket.Members.EntityId,
                TEHatRackItemSyncPacket.Members.EncodedSlot,
                TEHatRackItemSyncPacket.Members.Item
            )
            .Sizeof(
                TEHatRackItemSyncPacket.Members.Player,
                TEHatRackItemSyncPacket.Members.EntityId,
                TEHatRackItemSyncPacket.Members.EncodedSlot,
                TEHatRackItemSyncPacket.Members.Item
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(11, 11))
            .FunctionName(TEHatRackItemSyncPacket.Read)
            .ReturnValue(
                TEHatRackItemSyncPacket.Members.Player,
                TEHatRackItemSyncPacket.Members.EntityId,
                TEHatRackItemSyncPacket.Members.EncodedSlot,
                TEHatRackItemSyncPacket.Members.Item
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncTilePickingPacket()
    {
        var graph = new PacketGraph<SyncTilePickingPacket>(125, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncTilePickingPacket.Members.Player);
        graph.Field(SyncTilePickingPacket.Members.X);
        graph.Field(SyncTilePickingPacket.Members.Y);
        graph.Field(SyncTilePickingPacket.Members.TileType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(SyncTilePickingPacket.Write)
            .ParameterList(
                SyncTilePickingPacket.Members.Player,
                SyncTilePickingPacket.Members.X,
                SyncTilePickingPacket.Members.Y,
                SyncTilePickingPacket.Members.TileType
            )
            .Sizeof(
                SyncTilePickingPacket.Members.Player,
                SyncTilePickingPacket.Members.X,
                SyncTilePickingPacket.Members.Y,
                SyncTilePickingPacket.Members.TileType
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(SyncTilePickingPacket.Read)
            .ReturnValue(
                SyncTilePickingPacket.Members.Player,
                SyncTilePickingPacket.Members.X,
                SyncTilePickingPacket.Members.Y,
                SyncTilePickingPacket.Members.TileType
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncRevengeMarkerPacket()
    {
        var graph = new PacketGraph<SyncRevengeMarkerPacket>(126, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncRevengeMarkerPacket.Members.UniqueId);
        graph.Field(SyncRevengeMarkerPacket.Members.Position);
        graph.Field(SyncRevengeMarkerPacket.Members.NpcNetId);
        graph.Field(SyncRevengeMarkerPacket.Members.NpcHpPercent);
        graph.Field(SyncRevengeMarkerPacket.Members.NpcType);
        graph.Field(SyncRevengeMarkerPacket.Members.NpcAiStyle);
        graph.Field(SyncRevengeMarkerPacket.Members.CoinsValue);
        graph.Field(SyncRevengeMarkerPacket.Members.BaseValue);
        graph.Field(SyncRevengeMarkerPacket.Members.SpawnedFromStatue);
        graph.FunctionDeclaration(lengthRange: new ByteRange(37, 37))
            .FunctionName(SyncRevengeMarkerPacket.Write)
            .ParameterList(
                SyncRevengeMarkerPacket.Members.UniqueId,
                SyncRevengeMarkerPacket.Members.Position,
                SyncRevengeMarkerPacket.Members.NpcNetId,
                SyncRevengeMarkerPacket.Members.NpcHpPercent,
                SyncRevengeMarkerPacket.Members.NpcType,
                SyncRevengeMarkerPacket.Members.NpcAiStyle,
                SyncRevengeMarkerPacket.Members.CoinsValue,
                SyncRevengeMarkerPacket.Members.BaseValue,
                SyncRevengeMarkerPacket.Members.SpawnedFromStatue
            )
            .Sizeof(
                SyncRevengeMarkerPacket.Members.UniqueId,
                SyncRevengeMarkerPacket.Members.Position,
                SyncRevengeMarkerPacket.Members.NpcNetId,
                SyncRevengeMarkerPacket.Members.NpcHpPercent,
                SyncRevengeMarkerPacket.Members.NpcType,
                SyncRevengeMarkerPacket.Members.NpcAiStyle,
                SyncRevengeMarkerPacket.Members.CoinsValue,
                SyncRevengeMarkerPacket.Members.BaseValue,
                SyncRevengeMarkerPacket.Members.SpawnedFromStatue
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(37, 37))
            .FunctionName(SyncRevengeMarkerPacket.Read)
            .ReturnValue(
                SyncRevengeMarkerPacket.Members.UniqueId,
                SyncRevengeMarkerPacket.Members.Position,
                SyncRevengeMarkerPacket.Members.NpcNetId,
                SyncRevengeMarkerPacket.Members.NpcHpPercent,
                SyncRevengeMarkerPacket.Members.NpcType,
                SyncRevengeMarkerPacket.Members.NpcAiStyle,
                SyncRevengeMarkerPacket.Members.CoinsValue,
                SyncRevengeMarkerPacket.Members.BaseValue,
                SyncRevengeMarkerPacket.Members.SpawnedFromStatue
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateRemoveRevengeMarkerPacket()
    {
        var graph = new PacketGraph<RemoveRevengeMarkerPacket>(127, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(RemoveRevengeMarkerPacket.Members.MarkerId);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(RemoveRevengeMarkerPacket.Write)
            .ParameterList(RemoveRevengeMarkerPacket.Members.MarkerId)
            .Sizeof(RemoveRevengeMarkerPacket.Members.MarkerId);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(RemoveRevengeMarkerPacket.Read)
            .ReturnValue(RemoveRevengeMarkerPacket.Members.MarkerId);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateLandGolfBallInCupPacket()
    {
        var graph = new PacketGraph<LandGolfBallInCupPacket>(128, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(LandGolfBallInCupPacket.Members.Player);
        graph.Field(LandGolfBallInCupPacket.Members.BallX);
        graph.Field(LandGolfBallInCupPacket.Members.BallY);
        graph.Field(LandGolfBallInCupPacket.Members.CupX);
        graph.Field(LandGolfBallInCupPacket.Members.CupY);
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(LandGolfBallInCupPacket.Write)
            .ParameterList(
                LandGolfBallInCupPacket.Members.Player,
                LandGolfBallInCupPacket.Members.BallX,
                LandGolfBallInCupPacket.Members.BallY,
                LandGolfBallInCupPacket.Members.CupX,
                LandGolfBallInCupPacket.Members.CupY
            )
            .Sizeof(
                LandGolfBallInCupPacket.Members.Player,
                LandGolfBallInCupPacket.Members.BallX,
                LandGolfBallInCupPacket.Members.BallY,
                LandGolfBallInCupPacket.Members.CupX,
                LandGolfBallInCupPacket.Members.CupY
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(LandGolfBallInCupPacket.Read)
            .ReturnValue(
                LandGolfBallInCupPacket.Members.Player,
                LandGolfBallInCupPacket.Members.BallX,
                LandGolfBallInCupPacket.Members.BallY,
                LandGolfBallInCupPacket.Members.CupX,
                LandGolfBallInCupPacket.Members.CupY
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateFinishedConnectingToServerPacket()
    {
        var graph = new PacketGraph<FinishedConnectingToServerPacket>(129, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateFishOutNPCPacket()
    {
        var graph = new PacketGraph<FishOutNPCPacket>(130, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(FishOutNPCPacket.Members.X);
        graph.Field(FishOutNPCPacket.Members.Y);
        graph.Field(FishOutNPCPacket.Members.NpcType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(FishOutNPCPacket.Write)
            .ParameterList(FishOutNPCPacket.Members.X, FishOutNPCPacket.Members.Y, FishOutNPCPacket.Members.NpcType)
            .Sizeof(FishOutNPCPacket.Members.X, FishOutNPCPacket.Members.Y, FishOutNPCPacket.Members.NpcType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(FishOutNPCPacket.Read)
            .ReturnValue(FishOutNPCPacket.Members.X, FishOutNPCPacket.Members.Y, FishOutNPCPacket.Members.NpcType);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTamperWithNPCPacket()
    {
        var graph = new PacketGraph<TamperWithNPCPacket>(131, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TamperWithNPCPacket.Members.NpcIndex);
        graph.Field(TamperWithNPCPacket.Members.Action);
        graph.Field(TamperWithNPCPacket.Members.Value);
        graph.Field(TamperWithNPCPacket.Members.Style);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 9))
            .FunctionName(TamperWithNPCPacket.Write)
            .ParameterList(
                TamperWithNPCPacket.Members.NpcIndex,
                TamperWithNPCPacket.Members.Action,
                TamperWithNPCPacket.Members.Value,
                TamperWithNPCPacket.Members.Style
            )
            .Sizeof(
                TamperWithNPCPacket.Members.NpcIndex,
                TamperWithNPCPacket.Members.Action,
                TamperWithNPCPacket.Members.Value,
                TamperWithNPCPacket.Members.Style
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 9))
            .FunctionName(TamperWithNPCPacket.Read)
            .ReturnValue(
                TamperWithNPCPacket.Members.NpcIndex,
                TamperWithNPCPacket.Members.Action,
                TamperWithNPCPacket.Members.Value,
                TamperWithNPCPacket.Members.Style
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePlayLegacySoundPacket()
    {
        var graph = new PacketGraph<PlayLegacySoundPacket>(132, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        graph.Field(PlayLegacySoundPacket.Members.Position);
        graph.Field(PlayLegacySoundPacket.Members.SoundIndex);
        graph.Field(PlayLegacySoundPacket.Members.Style);
        graph.Field(PlayLegacySoundPacket.Members.Volume);
        graph.Field(PlayLegacySoundPacket.Members.PitchOffset);
        graph.FunctionDeclaration(lengthRange: new ByteRange(11, 23))
            .FunctionName(PlayLegacySoundPacket.Write)
            .ParameterList(
                PlayLegacySoundPacket.Members.Position,
                PlayLegacySoundPacket.Members.SoundIndex,
                PlayLegacySoundPacket.Members.Style,
                PlayLegacySoundPacket.Members.Volume,
                PlayLegacySoundPacket.Members.PitchOffset
            )
            .Sizeof(
                PlayLegacySoundPacket.Members.Position,
                PlayLegacySoundPacket.Members.SoundIndex,
                PlayLegacySoundPacket.Members.Style,
                PlayLegacySoundPacket.Members.Volume,
                PlayLegacySoundPacket.Members.PitchOffset
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(11, 23))
            .FunctionName(PlayLegacySoundPacket.Read)
            .ReturnValue(
                PlayLegacySoundPacket.Members.Position,
                PlayLegacySoundPacket.Members.SoundIndex,
                PlayLegacySoundPacket.Members.Style,
                PlayLegacySoundPacket.Members.Volume,
                PlayLegacySoundPacket.Members.PitchOffset
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateFoodPlatterTryPlacingPacket()
    {
        var graph = new PacketGraph<FoodPlatterTryPlacingPacket>(133, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(FoodPlatterTryPlacingPacket.Members.X);
        graph.Field(FoodPlatterTryPlacingPacket.Members.Y);
        graph.Field(FoodPlatterTryPlacingPacket.Members.ItemType);
        graph.Field(FoodPlatterTryPlacingPacket.Members.Prefix);
        graph.Field(FoodPlatterTryPlacingPacket.Members.Stack);
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(FoodPlatterTryPlacingPacket.Write)
            .ParameterList(
                FoodPlatterTryPlacingPacket.Members.X,
                FoodPlatterTryPlacingPacket.Members.Y,
                FoodPlatterTryPlacingPacket.Members.ItemType,
                FoodPlatterTryPlacingPacket.Members.Prefix,
                FoodPlatterTryPlacingPacket.Members.Stack
            )
            .Sizeof(
                FoodPlatterTryPlacingPacket.Members.X,
                FoodPlatterTryPlacingPacket.Members.Y,
                FoodPlatterTryPlacingPacket.Members.ItemType,
                FoodPlatterTryPlacingPacket.Members.Prefix,
                FoodPlatterTryPlacingPacket.Members.Stack
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(FoodPlatterTryPlacingPacket.Read)
            .ReturnValue(
                FoodPlatterTryPlacingPacket.Members.X,
                FoodPlatterTryPlacingPacket.Members.Y,
                FoodPlatterTryPlacingPacket.Members.ItemType,
                FoodPlatterTryPlacingPacket.Members.Prefix,
                FoodPlatterTryPlacingPacket.Members.Stack
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUpdatePlayerLuckFactorsPacket()
    {
        var graph = new PacketGraph<UpdatePlayerLuckFactorsPacket>(134, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(UpdatePlayerLuckFactorsPacket.Members.Player);
        graph.Field(UpdatePlayerLuckFactorsPacket.Members.LadyBugLuckTime);
        graph.Field(UpdatePlayerLuckFactorsPacket.Members.TorchLuck);
        graph.Field(UpdatePlayerLuckFactorsPacket.Members.LuckPotion);
        graph.Field(UpdatePlayerLuckFactorsPacket.Members.HasGardenGnome);
        graph.Field(UpdatePlayerLuckFactorsPacket.Members.BrokenMirrorBadLuck);
        graph.Field(UpdatePlayerLuckFactorsPacket.Members.EquipmentLuckBonus);
        graph.Field(UpdatePlayerLuckFactorsPacket.Members.CoinLuck);
        graph.Field(UpdatePlayerLuckFactorsPacket.Members.KiteLuckLevel);
        graph.FunctionDeclaration(lengthRange: new ByteRange(21, 21))
            .FunctionName(UpdatePlayerLuckFactorsPacket.Write)
            .ParameterList(
                UpdatePlayerLuckFactorsPacket.Members.Player,
                UpdatePlayerLuckFactorsPacket.Members.LadyBugLuckTime,
                UpdatePlayerLuckFactorsPacket.Members.TorchLuck,
                UpdatePlayerLuckFactorsPacket.Members.LuckPotion,
                UpdatePlayerLuckFactorsPacket.Members.HasGardenGnome,
                UpdatePlayerLuckFactorsPacket.Members.BrokenMirrorBadLuck,
                UpdatePlayerLuckFactorsPacket.Members.EquipmentLuckBonus,
                UpdatePlayerLuckFactorsPacket.Members.CoinLuck,
                UpdatePlayerLuckFactorsPacket.Members.KiteLuckLevel
            )
            .Sizeof(
                UpdatePlayerLuckFactorsPacket.Members.Player,
                UpdatePlayerLuckFactorsPacket.Members.LadyBugLuckTime,
                UpdatePlayerLuckFactorsPacket.Members.TorchLuck,
                UpdatePlayerLuckFactorsPacket.Members.LuckPotion,
                UpdatePlayerLuckFactorsPacket.Members.HasGardenGnome,
                UpdatePlayerLuckFactorsPacket.Members.BrokenMirrorBadLuck,
                UpdatePlayerLuckFactorsPacket.Members.EquipmentLuckBonus,
                UpdatePlayerLuckFactorsPacket.Members.CoinLuck,
                UpdatePlayerLuckFactorsPacket.Members.KiteLuckLevel
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(21, 21))
            .FunctionName(UpdatePlayerLuckFactorsPacket.Read)
            .ReturnValue(
                UpdatePlayerLuckFactorsPacket.Members.Player,
                UpdatePlayerLuckFactorsPacket.Members.LadyBugLuckTime,
                UpdatePlayerLuckFactorsPacket.Members.TorchLuck,
                UpdatePlayerLuckFactorsPacket.Members.LuckPotion,
                UpdatePlayerLuckFactorsPacket.Members.HasGardenGnome,
                UpdatePlayerLuckFactorsPacket.Members.BrokenMirrorBadLuck,
                UpdatePlayerLuckFactorsPacket.Members.EquipmentLuckBonus,
                UpdatePlayerLuckFactorsPacket.Members.CoinLuck,
                UpdatePlayerLuckFactorsPacket.Members.KiteLuckLevel
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateDeadPlayerPacket()
    {
        var graph = new PacketGraph<DeadPlayerPacket>(135, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(DeadPlayerPacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, 1))
            .FunctionName(DeadPlayerPacket.Write)
            .ParameterList(DeadPlayerPacket.Members.Player)
            .Sizeof(DeadPlayerPacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, 1))
            .FunctionName(DeadPlayerPacket.Read)
            .ReturnValue(DeadPlayerPacket.Members.Player);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncCavernMonsterTypePacket()
    {
        var graph = new PacketGraph<SyncCavernMonsterTypePacket>(136, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncCavernMonsterTypePacket.Members.Type00);
        graph.Field(SyncCavernMonsterTypePacket.Members.Type01);
        graph.Field(SyncCavernMonsterTypePacket.Members.Type02);
        graph.Field(SyncCavernMonsterTypePacket.Members.Type10);
        graph.Field(SyncCavernMonsterTypePacket.Members.Type11);
        graph.Field(SyncCavernMonsterTypePacket.Members.Type12);
        graph.FunctionDeclaration(lengthRange: new ByteRange(12, 12))
            .FunctionName(SyncCavernMonsterTypePacket.Write)
            .ParameterList(
                SyncCavernMonsterTypePacket.Members.Type00,
                SyncCavernMonsterTypePacket.Members.Type01,
                SyncCavernMonsterTypePacket.Members.Type02,
                SyncCavernMonsterTypePacket.Members.Type10,
                SyncCavernMonsterTypePacket.Members.Type11,
                SyncCavernMonsterTypePacket.Members.Type12
            )
            .Sizeof(
                SyncCavernMonsterTypePacket.Members.Type00,
                SyncCavernMonsterTypePacket.Members.Type01,
                SyncCavernMonsterTypePacket.Members.Type02,
                SyncCavernMonsterTypePacket.Members.Type10,
                SyncCavernMonsterTypePacket.Members.Type11,
                SyncCavernMonsterTypePacket.Members.Type12
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(12, 12))
            .FunctionName(SyncCavernMonsterTypePacket.Read)
            .ReturnValue(
                SyncCavernMonsterTypePacket.Members.Type00,
                SyncCavernMonsterTypePacket.Members.Type01,
                SyncCavernMonsterTypePacket.Members.Type02,
                SyncCavernMonsterTypePacket.Members.Type10,
                SyncCavernMonsterTypePacket.Members.Type11,
                SyncCavernMonsterTypePacket.Members.Type12
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateRequestNPCBuffRemovalPacket()
    {
        var graph = new PacketGraph<RequestNPCBuffRemovalPacket>(137, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(RequestNPCBuffRemovalPacket.Members.NpcIndex);
        graph.Field(RequestNPCBuffRemovalPacket.Members.BuffType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(RequestNPCBuffRemovalPacket.Write)
            .ParameterList(RequestNPCBuffRemovalPacket.Members.NpcIndex, RequestNPCBuffRemovalPacket.Members.BuffType)
            .Sizeof(RequestNPCBuffRemovalPacket.Members.NpcIndex, RequestNPCBuffRemovalPacket.Members.BuffType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(RequestNPCBuffRemovalPacket.Read)
            .ReturnValue(RequestNPCBuffRemovalPacket.Members.NpcIndex, RequestNPCBuffRemovalPacket.Members.BuffType);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSetCountsAsHostForGameplayPacket()
    {
        var graph = new PacketGraph<SetCountsAsHostForGameplayPacket>(139, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        graph.Field(SetCountsAsHostForGameplayPacket.Members.Player);
        graph.Field(SetCountsAsHostForGameplayPacket.Members.CountsAsHost);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(SetCountsAsHostForGameplayPacket.Write)
            .ParameterList(SetCountsAsHostForGameplayPacket.Members.Player, SetCountsAsHostForGameplayPacket.Members.CountsAsHost)
            .Sizeof(SetCountsAsHostForGameplayPacket.Members.Player, SetCountsAsHostForGameplayPacket.Members.CountsAsHost);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(SetCountsAsHostForGameplayPacket.Read)
            .ReturnValue(SetCountsAsHostForGameplayPacket.Members.Player, SetCountsAsHostForGameplayPacket.Members.CountsAsHost);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSetMiscEventValuesPacket()
    {
        var graph = new PacketGraph<SetMiscEventValuesPacket>(140, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SetMiscEventValuesPacket.Members.EventKind);
        graph.Field(SetMiscEventValuesPacket.Members.Value);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(SetMiscEventValuesPacket.Write)
            .ParameterList(SetMiscEventValuesPacket.Members.EventKind, SetMiscEventValuesPacket.Members.Value)
            .Sizeof(SetMiscEventValuesPacket.Members.EventKind, SetMiscEventValuesPacket.Members.Value);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 5))
            .FunctionName(SetMiscEventValuesPacket.Read)
            .ReturnValue(SetMiscEventValuesPacket.Members.EventKind, SetMiscEventValuesPacket.Members.Value);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateRequestLucyPopupPacket()
    {
        var graph = new PacketGraph<RequestLucyPopupPacket>(141, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(RequestLucyPopupPacket.Members.MessageSource);
        graph.Field(RequestLucyPopupPacket.Members.Variation);
        graph.Field(RequestLucyPopupPacket.Members.Velocity);
        graph.Field(RequestLucyPopupPacket.Members.PositionX);
        graph.Field(RequestLucyPopupPacket.Members.PositionY);
        graph.FunctionDeclaration(lengthRange: new ByteRange(18, 18))
            .FunctionName(RequestLucyPopupPacket.Write)
            .ParameterList(
                RequestLucyPopupPacket.Members.MessageSource,
                RequestLucyPopupPacket.Members.Variation,
                RequestLucyPopupPacket.Members.Velocity,
                RequestLucyPopupPacket.Members.PositionX,
                RequestLucyPopupPacket.Members.PositionY
            )
            .Sizeof(
                RequestLucyPopupPacket.Members.MessageSource,
                RequestLucyPopupPacket.Members.Variation,
                RequestLucyPopupPacket.Members.Velocity,
                RequestLucyPopupPacket.Members.PositionX,
                RequestLucyPopupPacket.Members.PositionY
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(18, 18))
            .FunctionName(RequestLucyPopupPacket.Read)
            .ReturnValue(
                RequestLucyPopupPacket.Members.MessageSource,
                RequestLucyPopupPacket.Members.Variation,
                RequestLucyPopupPacket.Members.Velocity,
                RequestLucyPopupPacket.Members.PositionX,
                RequestLucyPopupPacket.Members.PositionY
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncProjectileTrackersPacket()
    {
        var graph = new PacketGraph<SyncProjectileTrackersPacket>(142, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncProjectileTrackersPacket.Members.Player);
        graph.Field(SyncProjectileTrackersPacket.Members.PiggyBank);
        graph.Field(SyncProjectileTrackersPacket.Members.VoidLens);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 13))
            .FunctionName(SyncProjectileTrackersPacket.Write)
            .ParameterList(SyncProjectileTrackersPacket.Members.Player, SyncProjectileTrackersPacket.Members.PiggyBank, SyncProjectileTrackersPacket.Members.VoidLens)
            .Sizeof(SyncProjectileTrackersPacket.Members.Player, SyncProjectileTrackersPacket.Members.PiggyBank, SyncProjectileTrackersPacket.Members.VoidLens);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 13))
            .FunctionName(SyncProjectileTrackersPacket.Read)
            .ReturnValue(SyncProjectileTrackersPacket.Members.Player, SyncProjectileTrackersPacket.Members.PiggyBank, SyncProjectileTrackersPacket.Members.VoidLens);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateCrystalInvasionRequestedToSkipWaitTimePacket()
    {
        var graph = new PacketGraph<CrystalInvasionRequestedToSkipWaitTimePacket>(143, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateRequestQuestEffectPacket()
    {
        var graph = new PacketGraph<RequestQuestEffectPacket>(144, wireDirection: WireDirection.ClientToServer)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateShimmerActionsPacket()
    {
        var graph = new PacketGraph<ShimmerActionsPacket>(146, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(ShimmerActionsPacket.Members.Action);
        graph.Field(ShimmerActionsPacket.Members.Position);
        graph.Field(ShimmerActionsPacket.Members.CoinAmount);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, 13))
            .FunctionName(ShimmerActionsPacket.Write)
            .ParameterList(ShimmerActionsPacket.Members.Action, ShimmerActionsPacket.Members.Position, ShimmerActionsPacket.Members.CoinAmount)
            .Sizeof(ShimmerActionsPacket.Members.Action, ShimmerActionsPacket.Members.Position, ShimmerActionsPacket.Members.CoinAmount);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, 13))
            .FunctionName(ShimmerActionsPacket.Read)
            .ReturnValue(ShimmerActionsPacket.Members.Action, ShimmerActionsPacket.Members.Position, ShimmerActionsPacket.Members.CoinAmount);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncLoadoutPacket()
    {
        var graph = new PacketGraph<SyncLoadoutPacket>(147, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncLoadoutPacket.Members.Player);
        graph.Field(SyncLoadoutPacket.Members.LoadoutIndex);
        graph.Field(SyncLoadoutPacket.Members.AccessoryVisibilityMask);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(SyncLoadoutPacket.Write)
            .ParameterList(
                SyncLoadoutPacket.Members.Player,
                SyncLoadoutPacket.Members.LoadoutIndex,
                SyncLoadoutPacket.Members.AccessoryVisibilityMask
            )
            .Sizeof(
                SyncLoadoutPacket.Members.Player,
                SyncLoadoutPacket.Members.LoadoutIndex,
                SyncLoadoutPacket.Members.AccessoryVisibilityMask
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(SyncLoadoutPacket.Read)
            .ReturnValue(
                SyncLoadoutPacket.Members.Player,
                SyncLoadoutPacket.Members.LoadoutIndex,
                SyncLoadoutPacket.Members.AccessoryVisibilityMask
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateDeadCellsDisplayJarTryPlacingPacket()
    {
        var graph = new PacketGraph<DeadCellsDisplayJarTryPlacingPacket>(149, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(DeadCellsDisplayJarTryPlacingPacket.Members.X);
        graph.Field(DeadCellsDisplayJarTryPlacingPacket.Members.Y);
        graph.Field(DeadCellsDisplayJarTryPlacingPacket.Members.ItemType);
        graph.Field(DeadCellsDisplayJarTryPlacingPacket.Members.Prefix);
        graph.Field(DeadCellsDisplayJarTryPlacingPacket.Members.Stack);
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(DeadCellsDisplayJarTryPlacingPacket.Write)
            .ParameterList(
                DeadCellsDisplayJarTryPlacingPacket.Members.X,
                DeadCellsDisplayJarTryPlacingPacket.Members.Y,
                DeadCellsDisplayJarTryPlacingPacket.Members.ItemType,
                DeadCellsDisplayJarTryPlacingPacket.Members.Prefix,
                DeadCellsDisplayJarTryPlacingPacket.Members.Stack
            )
            .Sizeof(
                DeadCellsDisplayJarTryPlacingPacket.Members.X,
                DeadCellsDisplayJarTryPlacingPacket.Members.Y,
                DeadCellsDisplayJarTryPlacingPacket.Members.ItemType,
                DeadCellsDisplayJarTryPlacingPacket.Members.Prefix,
                DeadCellsDisplayJarTryPlacingPacket.Members.Stack
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(9, 9))
            .FunctionName(DeadCellsDisplayJarTryPlacingPacket.Read)
            .ReturnValue(
                DeadCellsDisplayJarTryPlacingPacket.Members.X,
                DeadCellsDisplayJarTryPlacingPacket.Members.Y,
                DeadCellsDisplayJarTryPlacingPacket.Members.ItemType,
                DeadCellsDisplayJarTryPlacingPacket.Members.Prefix,
                DeadCellsDisplayJarTryPlacingPacket.Members.Stack
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSpectatePlayerPacket()
    {
        var graph = new PacketGraph<SpectatePlayerPacket>(150, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SpectatePlayerPacket.Members.Player);
        graph.Field(SpectatePlayerPacket.Members.TargetPlayer);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(SpectatePlayerPacket.Write)
            .ParameterList(SpectatePlayerPacket.Members.Player, SpectatePlayerPacket.Members.TargetPlayer)
            .Sizeof(SpectatePlayerPacket.Members.Player, SpectatePlayerPacket.Members.TargetPlayer);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(SpectatePlayerPacket.Read)
            .ReturnValue(SpectatePlayerPacket.Members.Player, SpectatePlayerPacket.Members.TargetPlayer);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateItemUseSoundPacket()
    {
        var graph = new PacketGraph<ItemUseSoundPacket>(152, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(ItemUseSoundPacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, 1))
            .FunctionName(ItemUseSoundPacket.Write)
            .ParameterList(ItemUseSoundPacket.Members.Player)
            .Sizeof(ItemUseSoundPacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, 1))
            .FunctionName(ItemUseSoundPacket.Read)
            .ReturnValue(ItemUseSoundPacket.Members.Player);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateNPCDebuffDamagePacket()
    {
        var graph = new PacketGraph<NPCDebuffDamagePacket>(153, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(NPCDebuffDamagePacket.Members.NpcIndex);
        graph.Field(NPCDebuffDamagePacket.Members.BuffType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(NPCDebuffDamagePacket.Write)
            .ParameterList(NPCDebuffDamagePacket.Members.NpcIndex, NPCDebuffDamagePacket.Members.BuffType)
            .Sizeof(NPCDebuffDamagePacket.Members.NpcIndex, NPCDebuffDamagePacket.Members.BuffType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(NPCDebuffDamagePacket.Read)
            .ReturnValue(NPCDebuffDamagePacket.Members.NpcIndex, NPCDebuffDamagePacket.Members.BuffType);
        return graph.Build();
    }

    internal static PacketGraphManifest CreatePingPacket()
    {
        var graph = new PacketGraph<PingPacket>(154, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncChestSizePacket()
    {
        var graph = new PacketGraph<SyncChestSizePacket>(155, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SyncChestSizePacket.Members.ChestIndex);
        graph.Field(SyncChestSizePacket.Members.Size);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(SyncChestSizePacket.Write)
            .ParameterList(SyncChestSizePacket.Members.ChestIndex, SyncChestSizePacket.Members.Size)
            .Sizeof(SyncChestSizePacket.Members.ChestIndex, SyncChestSizePacket.Members.Size);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(SyncChestSizePacket.Read)
            .ReturnValue(SyncChestSizePacket.Members.ChestIndex, SyncChestSizePacket.Members.Size);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTELeashedEntityAnchorPlaceItemPacket()
    {
        var graph = new PacketGraph<TELeashedEntityAnchorPlaceItemPacket>(156, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TELeashedEntityAnchorPlaceItemPacket.Members.X);
        graph.Field(TELeashedEntityAnchorPlaceItemPacket.Members.Y);
        graph.Field(TELeashedEntityAnchorPlaceItemPacket.Members.ItemType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(TELeashedEntityAnchorPlaceItemPacket.Write)
            .ParameterList(TELeashedEntityAnchorPlaceItemPacket.Members.X, TELeashedEntityAnchorPlaceItemPacket.Members.Y, TELeashedEntityAnchorPlaceItemPacket.Members.ItemType)
            .Sizeof(TELeashedEntityAnchorPlaceItemPacket.Members.X, TELeashedEntityAnchorPlaceItemPacket.Members.Y, TELeashedEntityAnchorPlaceItemPacket.Members.ItemType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(TELeashedEntityAnchorPlaceItemPacket.Read)
            .ReturnValue(TELeashedEntityAnchorPlaceItemPacket.Members.X, TELeashedEntityAnchorPlaceItemPacket.Members.Y, TELeashedEntityAnchorPlaceItemPacket.Members.ItemType);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateExtraSpawnSectionLoadedPacket()
    {
        var graph = new PacketGraph<ExtraSpawnSectionLoadedPacket>(158, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(ExtraSpawnSectionLoadedPacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, 1))
            .FunctionName(ExtraSpawnSectionLoadedPacket.Write)
            .ParameterList(ExtraSpawnSectionLoadedPacket.Members.Player)
            .Sizeof(ExtraSpawnSectionLoadedPacket.Members.Player);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, 1))
            .FunctionName(ExtraSpawnSectionLoadedPacket.Read)
            .ReturnValue(ExtraSpawnSectionLoadedPacket.Members.Player);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateRequestSectionPacket()
    {
        var graph = new PacketGraph<RequestSectionPacket>(159, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(RequestSectionPacket.Members.SectionX);
        graph.Field(RequestSectionPacket.Members.SectionY);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(RequestSectionPacket.Write)
            .ParameterList(RequestSectionPacket.Members.SectionX, RequestSectionPacket.Members.SectionY)
            .Sizeof(RequestSectionPacket.Members.SectionX, RequestSectionPacket.Members.SectionY);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(RequestSectionPacket.Read)
            .ReturnValue(RequestSectionPacket.Members.SectionX, RequestSectionPacket.Members.SectionY);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateItemPositionPacket()
    {
        var graph = new PacketGraph<ItemPositionPacket>(160, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        graph.Field(ItemPositionPacket.Members.ItemIndex);
        graph.Field(ItemPositionPacket.Members.Position);
        graph.FunctionDeclaration(lengthRange: new ByteRange(10, 10))
            .FunctionName(ItemPositionPacket.Write)
            .ParameterList(ItemPositionPacket.Members.ItemIndex, ItemPositionPacket.Members.Position)
            .Sizeof(ItemPositionPacket.Members.ItemIndex, ItemPositionPacket.Members.Position);
        graph.FunctionDeclaration(lengthRange: new ByteRange(10, 10))
            .FunctionName(ItemPositionPacket.Read)
            .ReturnValue(ItemPositionPacket.Members.ItemIndex, ItemPositionPacket.Members.Position);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateHostTokenPacket()
    {
        var graph = new PacketGraph<HostTokenPacket>(161, wireDirection: WireDirection.ClientToServer)
            .ExternalDependencies();
        graph.Field(HostTokenPacket.Members.HostToken);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(HostTokenPacket.Write)
            .ParameterList(HostTokenPacket.Members.HostToken)
            .Sizeof(HostTokenPacket.Members.HostToken);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(HostTokenPacket.Read)
            .ReturnValue(HostTokenPacket.Members.HostToken);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateRequestPasswordPacket()
    {
        var graph = new PacketGraph<RequestPasswordPacket>(37, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateLiquidUpdatePacket()
    {
        var graph = new PacketGraph<LiquidUpdatePacket>(48, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(LiquidUpdatePacket.Members.X);
        graph.Field(LiquidUpdatePacket.Members.Y);
        graph.Field(LiquidUpdatePacket.Members.LiquidAmount);
        graph.Field(LiquidUpdatePacket.Members.LiquidType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(LiquidUpdatePacket.Write)
            .ParameterList(
                LiquidUpdatePacket.Members.X,
                LiquidUpdatePacket.Members.Y,
                LiquidUpdatePacket.Members.LiquidAmount,
                LiquidUpdatePacket.Members.LiquidType
            )
            .Sizeof(
                LiquidUpdatePacket.Members.X,
                LiquidUpdatePacket.Members.Y,
                LiquidUpdatePacket.Members.LiquidAmount,
                LiquidUpdatePacket.Members.LiquidType
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(LiquidUpdatePacket.Read)
            .ReturnValue(
                LiquidUpdatePacket.Members.X,
                LiquidUpdatePacket.Members.Y,
                LiquidUpdatePacket.Members.LiquidAmount,
                LiquidUpdatePacket.Members.LiquidType
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateInitialSpawnPacket()
    {
        var graph = new PacketGraph<InitialSpawnPacket>(49, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUniqueTownNPCInfoSyncRequestPacket()
    {
        var graph = new PacketGraph<UniqueTownNPCInfoSyncRequestPacket>(56, wireDirection: WireDirection.ClientToServer)
            .ExternalDependencies();
        graph.Field(UniqueTownNPCInfoSyncRequestPacket.Members.NpcIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(UniqueTownNPCInfoSyncRequestPacket.Write)
            .ParameterList(UniqueTownNPCInfoSyncRequestPacket.Members.NpcIndex)
            .Sizeof(UniqueTownNPCInfoSyncRequestPacket.Members.NpcIndex);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(UniqueTownNPCInfoSyncRequestPacket.Read)
            .ReturnValue(UniqueTownNPCInfoSyncRequestPacket.Members.NpcIndex);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUniqueTownNPCInfoSyncResponsePacket()
    {
        var graph = new PacketGraph<UniqueTownNPCInfoSyncResponsePacket>(56, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        graph.Field(UniqueTownNPCInfoSyncResponsePacket.Members.NpcIndex);
        graph.Field(UniqueTownNPCInfoSyncResponsePacket.Members.GivenName);
        graph.Field(UniqueTownNPCInfoSyncResponsePacket.Members.Variation);
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(UniqueTownNPCInfoSyncResponsePacket.Write)
            .ParameterList(
                UniqueTownNPCInfoSyncResponsePacket.Members.NpcIndex,
                UniqueTownNPCInfoSyncResponsePacket.Members.GivenName,
                UniqueTownNPCInfoSyncResponsePacket.Members.Variation
            )
            .Sizeof(
                UniqueTownNPCInfoSyncResponsePacket.Members.NpcIndex,
                UniqueTownNPCInfoSyncResponsePacket.Members.GivenName,
                UniqueTownNPCInfoSyncResponsePacket.Members.Variation
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(UniqueTownNPCInfoSyncResponsePacket.Read)
            .ReturnValue(
                UniqueTownNPCInfoSyncResponsePacket.Members.NpcIndex,
                UniqueTownNPCInfoSyncResponsePacket.Members.GivenName,
                UniqueTownNPCInfoSyncResponsePacket.Members.Variation
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUnknown57Packet()
    {
        var graph = new PacketGraph<Unknown57Packet>(57, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(Unknown57Packet.Members.Good);
        graph.Field(Unknown57Packet.Members.Evil);
        graph.Field(Unknown57Packet.Members.Blood);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(Unknown57Packet.Write)
            .ParameterList(Unknown57Packet.Members.Good, Unknown57Packet.Members.Evil, Unknown57Packet.Members.Blood)
            .Sizeof(Unknown57Packet.Members.Good, Unknown57Packet.Members.Evil, Unknown57Packet.Members.Blood);
        graph.FunctionDeclaration(lengthRange: new ByteRange(3, 3))
            .FunctionName(Unknown57Packet.Read)
            .ReturnValue(Unknown57Packet.Members.Good, Unknown57Packet.Members.Evil, Unknown57Packet.Members.Blood);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSpawnBossUseLicenseStartEventPacket()
    {
        var graph = new PacketGraph<SpawnBossUseLicenseStartEventPacket>(61, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(SpawnBossUseLicenseStartEventPacket.Members.Player);
        graph.Field(SpawnBossUseLicenseStartEventPacket.Members.EventOrNpcType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(SpawnBossUseLicenseStartEventPacket.Write)
            .ParameterList(SpawnBossUseLicenseStartEventPacket.Members.Player, SpawnBossUseLicenseStartEventPacket.Members.EventOrNpcType)
            .Sizeof(SpawnBossUseLicenseStartEventPacket.Members.Player, SpawnBossUseLicenseStartEventPacket.Members.EventOrNpcType);
        graph.FunctionDeclaration(lengthRange: new ByteRange(4, 4))
            .FunctionName(SpawnBossUseLicenseStartEventPacket.Read)
            .ReturnValue(SpawnBossUseLicenseStartEventPacket.Members.Player, SpawnBossUseLicenseStartEventPacket.Members.EventOrNpcType);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateTeleportEntityPacket()
    {
        var graph = new PacketGraph<TeleportEntityPacket>(65, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(TeleportEntityPacket.Members.EntityKind);
        graph.Field(TeleportEntityPacket.Members.EntityIndex);
        graph.Field(TeleportEntityPacket.Members.Position);
        graph.Field(TeleportEntityPacket.Members.Style);
        graph.Field(TeleportEntityPacket.Members.UseCurrentEntityPosition);
        graph.Field(TeleportEntityPacket.Members.TeleportValue);
        graph.FunctionDeclaration(lengthRange: new ByteRange(12, 16))
            .FunctionName(TeleportEntityPacket.Write)
            .ParameterList(
                TeleportEntityPacket.Members.EntityKind,
                TeleportEntityPacket.Members.EntityIndex,
                TeleportEntityPacket.Members.Position,
                TeleportEntityPacket.Members.Style,
                TeleportEntityPacket.Members.UseCurrentEntityPosition,
                TeleportEntityPacket.Members.TeleportValue
            )
            .Sizeof(
                TeleportEntityPacket.Members.EntityKind,
                TeleportEntityPacket.Members.EntityIndex,
                TeleportEntityPacket.Members.Position,
                TeleportEntityPacket.Members.Style,
                TeleportEntityPacket.Members.UseCurrentEntityPosition,
                TeleportEntityPacket.Members.TeleportValue
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(12, 16))
            .FunctionName(TeleportEntityPacket.Read)
            .ReturnValue(
                TeleportEntityPacket.Members.EntityKind,
                TeleportEntityPacket.Members.EntityIndex,
                TeleportEntityPacket.Members.Position,
                TeleportEntityPacket.Members.Style,
                TeleportEntityPacket.Members.UseCurrentEntityPosition,
                TeleportEntityPacket.Members.TeleportValue
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateChestNameRequestPacket()
    {
        var graph = new PacketGraph<ChestNameRequestPacket>(69, wireDirection: WireDirection.ClientToServer)
            .ExternalDependencies();
        graph.Field(ChestNameRequestPacket.Members.ChestIndex);
        graph.Field(ChestNameRequestPacket.Members.X);
        graph.Field(ChestNameRequestPacket.Members.Y);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(ChestNameRequestPacket.Write)
            .ParameterList(ChestNameRequestPacket.Members.ChestIndex, ChestNameRequestPacket.Members.X, ChestNameRequestPacket.Members.Y)
            .Sizeof(ChestNameRequestPacket.Members.ChestIndex, ChestNameRequestPacket.Members.X, ChestNameRequestPacket.Members.Y);
        graph.FunctionDeclaration(lengthRange: new ByteRange(6, 6))
            .FunctionName(ChestNameRequestPacket.Read)
            .ReturnValue(ChestNameRequestPacket.Members.ChestIndex, ChestNameRequestPacket.Members.X, ChestNameRequestPacket.Members.Y);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateChestNameResponsePacket()
    {
        var graph = new PacketGraph<ChestNameResponsePacket>(69, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        graph.Field(ChestNameResponsePacket.Members.ChestIndex);
        graph.Field(ChestNameResponsePacket.Members.X);
        graph.Field(ChestNameResponsePacket.Members.Y);
        graph.Field(ChestNameResponsePacket.Members.ChestName);
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(ChestNameResponsePacket.Write)
            .ParameterList(
                ChestNameResponsePacket.Members.ChestIndex,
                ChestNameResponsePacket.Members.X,
                ChestNameResponsePacket.Members.Y,
                ChestNameResponsePacket.Members.ChestName
            )
            .Sizeof(
                ChestNameResponsePacket.Members.ChestIndex,
                ChestNameResponsePacket.Members.X,
                ChestNameResponsePacket.Members.Y,
                ChestNameResponsePacket.Members.ChestName
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(7, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(ChestNameResponsePacket.Read)
            .ReturnValue(
                ChestNameResponsePacket.Members.ChestIndex,
                ChestNameResponsePacket.Members.X,
                ChestNameResponsePacket.Members.Y,
                ChestNameResponsePacket.Members.ChestName
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateAnglerQuestFinishedPacket()
    {
        var graph = new PacketGraph<AnglerQuestFinishedPacket>(75, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateQuickStackChestsRequestPacket()
    {
        var graph = new PacketGraph<QuickStackChestsRequestPacket>(85, wireDirection: WireDirection.ClientToServer)
            .ExternalDependencies();
        graph.Field(QuickStackChestsRequestPacket.Members.SmartStack);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, 1))
            .FunctionName(QuickStackChestsRequestPacket.Write)
            .ParameterList(QuickStackChestsRequestPacket.Members.SmartStack)
            .Sizeof(QuickStackChestsRequestPacket.Members.SmartStack);
        graph.FunctionDeclaration(lengthRange: new ByteRange(1, 1))
            .FunctionName(QuickStackChestsRequestPacket.Read)
            .ReturnValue(QuickStackChestsRequestPacket.Members.SmartStack);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateQuickStackChestsResponsePacket()
    {
        var graph = new PacketGraph<QuickStackChestsResponsePacket>(85, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSyncEmoteBubblePacket()
    {
        var graph = new PacketGraph<SyncEmoteBubblePacket>(91, wireDirection: WireDirection.ServerToClient)
            .ExternalDependencies();
        graph.Field(SyncEmoteBubblePacket.Members.BubbleId);
        graph.Field(SyncEmoteBubblePacket.Members.AnchorKind);
        graph.Field(SyncEmoteBubblePacket.Members.AnchorId);
        graph.Field(SyncEmoteBubblePacket.Members.Lifetime);
        graph.Field(SyncEmoteBubblePacket.Members.Emote);
        graph.Field(SyncEmoteBubblePacket.Members.Metadata);
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 12))
            .FunctionName(SyncEmoteBubblePacket.Write)
            .ParameterList(
                SyncEmoteBubblePacket.Members.BubbleId,
                SyncEmoteBubblePacket.Members.AnchorKind,
                SyncEmoteBubblePacket.Members.AnchorId,
                SyncEmoteBubblePacket.Members.Lifetime,
                SyncEmoteBubblePacket.Members.Emote,
                SyncEmoteBubblePacket.Members.Metadata
            )
            .Sizeof(
                SyncEmoteBubblePacket.Members.BubbleId,
                SyncEmoteBubblePacket.Members.AnchorKind,
                SyncEmoteBubblePacket.Members.AnchorId,
                SyncEmoteBubblePacket.Members.Lifetime,
                SyncEmoteBubblePacket.Members.Emote,
                SyncEmoteBubblePacket.Members.Metadata
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(5, 12))
            .FunctionName(SyncEmoteBubblePacket.Read)
            .ReturnValue(
                SyncEmoteBubblePacket.Members.BubbleId,
                SyncEmoteBubblePacket.Members.AnchorKind,
                SyncEmoteBubblePacket.Members.AnchorId,
                SyncEmoteBubblePacket.Members.Lifetime,
                SyncEmoteBubblePacket.Members.Emote,
                SyncEmoteBubblePacket.Members.Metadata
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateDevCommandsPacket()
    {
        var graph = new PacketGraph<DevCommandsPacket>(94, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(DevCommandsPacket.Members.Command);
        graph.Field(DevCommandsPacket.Members.IntegerArgument);
        graph.Field(DevCommandsPacket.Members.Value);
        graph.Field(DevCommandsPacket.Members.TrailingValue);
        graph.FunctionDeclaration(lengthRange: new ByteRange(13, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(DevCommandsPacket.Write)
            .ParameterList(
                DevCommandsPacket.Members.Command,
                DevCommandsPacket.Members.IntegerArgument,
                DevCommandsPacket.Members.Value,
                DevCommandsPacket.Members.TrailingValue
            )
            .Sizeof(
                DevCommandsPacket.Members.Command,
                DevCommandsPacket.Members.IntegerArgument,
                DevCommandsPacket.Members.Value,
                DevCommandsPacket.Members.TrailingValue
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(13, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(DevCommandsPacket.Read)
            .ReturnValue(
                DevCommandsPacket.Members.Command,
                DevCommandsPacket.Members.IntegerArgument,
                DevCommandsPacket.Members.Value,
                DevCommandsPacket.Members.TrailingValue
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateAchievementMessageNPCKilledPacket()
    {
        var graph = new PacketGraph<AchievementMessageNPCKilledPacket>(97, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(AchievementMessageNPCKilledPacket.Members.NpcNetId);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(AchievementMessageNPCKilledPacket.Write)
            .ParameterList(AchievementMessageNPCKilledPacket.Members.NpcNetId)
            .Sizeof(AchievementMessageNPCKilledPacket.Members.NpcNetId);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(AchievementMessageNPCKilledPacket.Read)
            .ReturnValue(AchievementMessageNPCKilledPacket.Members.NpcNetId);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateAchievementMessageEventHappenedPacket()
    {
        var graph = new PacketGraph<AchievementMessageEventHappenedPacket>(98, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(AchievementMessageEventHappenedPacket.Members.EventId);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(AchievementMessageEventHappenedPacket.Write)
            .ParameterList(AchievementMessageEventHappenedPacket.Members.EventId)
            .Sizeof(AchievementMessageEventHappenedPacket.Members.EventId);
        graph.FunctionDeclaration(lengthRange: new ByteRange(2, 2))
            .FunctionName(AchievementMessageEventHappenedPacket.Read)
            .ReturnValue(AchievementMessageEventHappenedPacket.Members.EventId);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateCrystalInvasionWipeAllTheThingsssPacket()
    {
        var graph = new PacketGraph<CrystalInvasionWipeAllTheThingsssPacket>(114, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateWorldDataPacket()
    {
        var graph = new PacketGraph<WorldDataPacket>(7, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(WorldDataPacket.Members.Time);
        graph.Field(WorldDataPacket.Members.TimeFlags);
        graph.Field(WorldDataPacket.Members.MoonPhase);
        graph.Field(WorldDataPacket.Members.MaxTilesX);
        graph.Field(WorldDataPacket.Members.MaxTilesY);
        graph.Field(WorldDataPacket.Members.SpawnTileX);
        graph.Field(WorldDataPacket.Members.SpawnTileY);
        graph.Field(WorldDataPacket.Members.WorldSurface);
        graph.Field(WorldDataPacket.Members.RockLayer);
        graph.Field(WorldDataPacket.Members.WorldId);
        graph.Field(WorldDataPacket.Members.WorldName);
        graph.Field(WorldDataPacket.Members.GameMode);
        graph.Field(WorldDataPacket.Members.WorldGuid);
        graph.Field(WorldDataPacket.Members.WorldGeneratorVersion);
        graph.Field(WorldDataPacket.Members.MoonType);
        graph.Field(WorldDataPacket.Members.BackgroundTypes);
        graph.Field(WorldDataPacket.Members.SpecialBackgroundStyles);
        graph.Field(WorldDataPacket.Members.WindSpeedTarget);
        graph.Field(WorldDataPacket.Members.CloudCount);
        graph.Field(WorldDataPacket.Members.TreePositions);
        graph.Field(WorldDataPacket.Members.TreeStyles);
        graph.Field(WorldDataPacket.Members.CaveBackgroundPositions);
        graph.Field(WorldDataPacket.Members.CaveBackgroundStyles);
        graph.Field(WorldDataPacket.Members.TreeTopStyles);
        graph.Field(WorldDataPacket.Members.MaximumRain);
        graph.Field(WorldDataPacket.Members.WorldFlagGroups);
        graph.Field(WorldDataPacket.Members.SundialCooldown);
        graph.Field(WorldDataPacket.Members.MoondialCooldown);
        graph.Field(WorldDataPacket.Members.SavedOreTiers);
        graph.Field(WorldDataPacket.Members.InvasionType);
        graph.Field(WorldDataPacket.Members.LobbyId);
        graph.Field(WorldDataPacket.Members.SandstormSeverity);
        graph.Field(WorldDataPacket.Members.ExtraSpawnPoints);
        graph.FunctionDeclaration(lengthRange: new ByteRange(160, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(WorldDataPacket.WriteFunction)
            .ParameterList(
                WorldDataPacket.Members.Time,
                WorldDataPacket.Members.TimeFlags,
                WorldDataPacket.Members.MoonPhase,
                WorldDataPacket.Members.MaxTilesX,
                WorldDataPacket.Members.MaxTilesY,
                WorldDataPacket.Members.SpawnTileX,
                WorldDataPacket.Members.SpawnTileY,
                WorldDataPacket.Members.WorldSurface,
                WorldDataPacket.Members.RockLayer,
                WorldDataPacket.Members.WorldId,
                WorldDataPacket.Members.WorldName,
                WorldDataPacket.Members.GameMode,
                WorldDataPacket.Members.WorldGuid,
                WorldDataPacket.Members.WorldGeneratorVersion,
                WorldDataPacket.Members.MoonType,
                WorldDataPacket.Members.BackgroundTypes,
                WorldDataPacket.Members.SpecialBackgroundStyles,
                WorldDataPacket.Members.WindSpeedTarget,
                WorldDataPacket.Members.CloudCount,
                WorldDataPacket.Members.TreePositions,
                WorldDataPacket.Members.TreeStyles,
                WorldDataPacket.Members.CaveBackgroundPositions,
                WorldDataPacket.Members.CaveBackgroundStyles,
                WorldDataPacket.Members.TreeTopStyles,
                WorldDataPacket.Members.MaximumRain,
                WorldDataPacket.Members.WorldFlagGroups,
                WorldDataPacket.Members.SundialCooldown,
                WorldDataPacket.Members.MoondialCooldown,
                WorldDataPacket.Members.SavedOreTiers,
                WorldDataPacket.Members.InvasionType,
                WorldDataPacket.Members.LobbyId,
                WorldDataPacket.Members.SandstormSeverity,
                WorldDataPacket.Members.ExtraSpawnPoints
            )
            .Sizeof(
                WorldDataPacket.Members.Time,
                WorldDataPacket.Members.TimeFlags,
                WorldDataPacket.Members.MoonPhase,
                WorldDataPacket.Members.MaxTilesX,
                WorldDataPacket.Members.MaxTilesY,
                WorldDataPacket.Members.SpawnTileX,
                WorldDataPacket.Members.SpawnTileY,
                WorldDataPacket.Members.WorldSurface,
                WorldDataPacket.Members.RockLayer,
                WorldDataPacket.Members.WorldId,
                WorldDataPacket.Members.WorldName,
                WorldDataPacket.Members.GameMode,
                WorldDataPacket.Members.WorldGuid,
                WorldDataPacket.Members.WorldGeneratorVersion,
                WorldDataPacket.Members.MoonType,
                WorldDataPacket.Members.BackgroundTypes,
                WorldDataPacket.Members.SpecialBackgroundStyles,
                WorldDataPacket.Members.WindSpeedTarget,
                WorldDataPacket.Members.CloudCount,
                WorldDataPacket.Members.TreePositions,
                WorldDataPacket.Members.TreeStyles,
                WorldDataPacket.Members.CaveBackgroundPositions,
                WorldDataPacket.Members.CaveBackgroundStyles,
                WorldDataPacket.Members.TreeTopStyles,
                WorldDataPacket.Members.MaximumRain,
                WorldDataPacket.Members.WorldFlagGroups,
                WorldDataPacket.Members.SundialCooldown,
                WorldDataPacket.Members.MoondialCooldown,
                WorldDataPacket.Members.SavedOreTiers,
                WorldDataPacket.Members.InvasionType,
                WorldDataPacket.Members.LobbyId,
                WorldDataPacket.Members.SandstormSeverity,
                WorldDataPacket.Members.ExtraSpawnPoints
            );
        graph.FunctionDeclaration(lengthRange: new ByteRange(160, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(WorldDataPacket.Read)
            .ReturnValue(
                WorldDataPacket.Members.Time,
                WorldDataPacket.Members.TimeFlags,
                WorldDataPacket.Members.MoonPhase,
                WorldDataPacket.Members.MaxTilesX,
                WorldDataPacket.Members.MaxTilesY,
                WorldDataPacket.Members.SpawnTileX,
                WorldDataPacket.Members.SpawnTileY,
                WorldDataPacket.Members.WorldSurface,
                WorldDataPacket.Members.RockLayer,
                WorldDataPacket.Members.WorldId,
                WorldDataPacket.Members.WorldName,
                WorldDataPacket.Members.GameMode,
                WorldDataPacket.Members.WorldGuid,
                WorldDataPacket.Members.WorldGeneratorVersion,
                WorldDataPacket.Members.MoonType,
                WorldDataPacket.Members.BackgroundTypes,
                WorldDataPacket.Members.SpecialBackgroundStyles,
                WorldDataPacket.Members.WindSpeedTarget,
                WorldDataPacket.Members.CloudCount,
                WorldDataPacket.Members.TreePositions,
                WorldDataPacket.Members.TreeStyles,
                WorldDataPacket.Members.CaveBackgroundPositions,
                WorldDataPacket.Members.CaveBackgroundStyles,
                WorldDataPacket.Members.TreeTopStyles,
                WorldDataPacket.Members.MaximumRain,
                WorldDataPacket.Members.WorldFlagGroups,
                WorldDataPacket.Members.SundialCooldown,
                WorldDataPacket.Members.MoondialCooldown,
                WorldDataPacket.Members.SavedOreTiers,
                WorldDataPacket.Members.InvasionType,
                WorldDataPacket.Members.LobbyId,
                WorldDataPacket.Members.SandstormSeverity,
                WorldDataPacket.Members.ExtraSpawnPoints
            );
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUnknown15Packet()
    {
        var graph = new PacketGraph<Unknown15Packet>(15, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUnused25Packet()
    {
        var graph = new PacketGraph<Unused25Packet>(25, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUnused26Packet()
    {
        var graph = new PacketGraph<Unused26Packet>(26, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUnknown44Packet()
    {
        var graph = new PacketGraph<Unknown44Packet>(44, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUnknown67Packet()
    {
        var graph = new PacketGraph<Unknown67Packet>(67, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateUnused83Packet()
    {
        var graph = new PacketGraph<Unused83Packet>(83, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateSocialHandshakePacket()
    {
        var graph = new PacketGraph<SocialHandshakePacket>(93, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        return graph.Build();
    }

    internal static PacketGraphManifest CreateNeverCalledPacket()
    {
        var graph = new PacketGraph<NeverCalledPacket>(0, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(NeverCalledPacket.Members.Bytes);
        graph.FunctionDeclaration(lengthRange: new ByteRange(0, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(OpaquePacketBodyCodec.Write)
            .ParameterList(NeverCalledPacket.Members.Bytes)
            .Sizeof(NeverCalledPacket.Members.Bytes);
        graph.FunctionDeclaration(lengthRange: new ByteRange(0, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(OpaquePacketBodyCodec.Read)
            .ReturnValue(NeverCalledPacket.Members.Bytes);
        return graph.Build();
    }

    internal static PacketGraphManifest CreateClientSyncedInventoryPacket()
    {
        var graph = new PacketGraph<ClientSyncedInventoryPacket>(138, wireDirection: WireDirection.Bidirectional)
            .ExternalDependencies();
        graph.Field(ClientSyncedInventoryPacket.Members.Bytes);
        graph.FunctionDeclaration(lengthRange: new ByteRange(0, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(OpaquePacketBodyCodec.Write)
            .ParameterList(ClientSyncedInventoryPacket.Members.Bytes)
            .Sizeof(ClientSyncedInventoryPacket.Members.Bytes);
        graph.FunctionDeclaration(lengthRange: new ByteRange(0, PacketFrameLimits.MaximumPayloadBytes))
            .FunctionName(OpaquePacketBodyCodec.Read)
            .ReturnValue(ClientSyncedInventoryPacket.Members.Bytes);
        return graph.Build();
    }
}
