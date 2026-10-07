#nullable enable
using System;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
namespace Terraria.NetWork.Generated;
public static class TerrariaV4Protocol
{
    public const string Name = "TerrariaV4";
    public const string Version = "4";
    public static class Packets
    {
        public static class AchievementMessageEventHappenedPacket
        {
            public const byte MessageId = 98;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("AchievementMessageEventHappenedPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AchievementMessageEventHappenedPacket), (PacketCodecDirections)3);
            public static TerrariaV4_AchievementMessageEventHappenedPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_AchievementMessageEventHappenedPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_AchievementMessageEventHappenedPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class AchievementMessageNPCKilledPacket
        {
            public const byte MessageId = 97;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("AchievementMessageNPCKilledPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AchievementMessageNPCKilledPacket), (PacketCodecDirections)3);
            public static TerrariaV4_AchievementMessageNPCKilledPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_AchievementMessageNPCKilledPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_AchievementMessageNPCKilledPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class AddNPCBuffPacket
        {
            public const byte MessageId = 53;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("AddNPCBuffPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AddNPCBuffPacket), (PacketCodecDirections)3);
            public static TerrariaV4_AddNPCBuffPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_AddNPCBuffPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_AddNPCBuffPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class AddPlayerBuffPvPPacket
        {
            public const byte MessageId = 55;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("AddPlayerBuffPvPPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AddPlayerBuffPvPPacket), (PacketCodecDirections)3);
            public static TerrariaV4_AddPlayerBuffPvPPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_AddPlayerBuffPvPPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_AddPlayerBuffPvPPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class AnglerQuestFinishedPacket
        {
            public const byte MessageId = 75;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("AnglerQuestFinishedPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AnglerQuestFinishedPacket), (PacketCodecDirections)3);
            public static TerrariaV4_AnglerQuestFinishedPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_AnglerQuestFinishedPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_AnglerQuestFinishedPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class AnglerQuestPacket
        {
            public const byte MessageId = 74;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("AnglerQuestPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AnglerQuestPacket), (PacketCodecDirections)3);
            public static TerrariaV4_AnglerQuestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_AnglerQuestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_AnglerQuestPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class AreaTileChangePacket
        {
            public const byte MessageId = 20;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("AreaTileChangePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket), (PacketCodecDirections)3);
            public static TerrariaV4_AreaTileChangePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_AreaTileChangePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_AreaTileChangePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class BugCatchingPacket
        {
            public const byte MessageId = 70;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("BugCatchingPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.BugCatchingPacket), (PacketCodecDirections)3);
            public static TerrariaV4_BugCatchingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_BugCatchingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_BugCatchingPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class BugReleasingPacket
        {
            public const byte MessageId = 71;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("BugReleasingPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.BugReleasingPacket), (PacketCodecDirections)3);
            public static TerrariaV4_BugReleasingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_BugReleasingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_BugReleasingPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ChestNameRequestPacket
        {
            public const byte MessageId = 69;
            public const WireDirection Direction = (WireDirection)1;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ChestNameRequestPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ChestNameRequestPacket), (PacketCodecDirections)3);
            public static TerrariaV4_ChestNameRequestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ChestNameRequestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ChestNameRequestPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ChestNameResponsePacket
        {
            public const byte MessageId = 69;
            public const WireDirection Direction = (WireDirection)2;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ChestNameResponsePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ChestNameResponsePacket), (PacketCodecDirections)3);
            public static TerrariaV4_ChestNameResponsePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ChestNameResponsePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ChestNameResponsePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ChestUpdatesPacket
        {
            public const byte MessageId = 34;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ChestUpdatesPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ChestUpdatesPacket), (PacketCodecDirections)3);
            public static TerrariaV4_ChestUpdatesPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ChestUpdatesPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ChestUpdatesPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ClientSyncedInventoryPacket
        {
            public const byte MessageId = 138;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ClientSyncedInventoryPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ClientSyncedInventoryPacket), (PacketCodecDirections)3);
            public static TerrariaV4_ClientSyncedInventoryPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ClientSyncedInventoryPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ClientSyncedInventoryPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class CombatTextIntPacket
        {
            public const byte MessageId = 81;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("CombatTextIntPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CombatTextIntPacket), (PacketCodecDirections)3);
            public static TerrariaV4_CombatTextIntPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_CombatTextIntPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_CombatTextIntPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class CombatTextStringPacket
        {
            public const byte MessageId = 119;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("CombatTextStringPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CombatTextStringPacket), (PacketCodecDirections)3);
            public static TerrariaV4_CombatTextStringPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_CombatTextStringPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_CombatTextStringPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class CrystalInvasionRequestedToSkipWaitTimePacket
        {
            public const byte MessageId = 143;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("CrystalInvasionRequestedToSkipWaitTimePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CrystalInvasionRequestedToSkipWaitTimePacket), (PacketCodecDirections)3);
            public static TerrariaV4_CrystalInvasionRequestedToSkipWaitTimePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_CrystalInvasionRequestedToSkipWaitTimePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_CrystalInvasionRequestedToSkipWaitTimePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class CrystalInvasionSendWaitTimePacket
        {
            public const byte MessageId = 116;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("CrystalInvasionSendWaitTimePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CrystalInvasionSendWaitTimePacket), (PacketCodecDirections)3);
            public static TerrariaV4_CrystalInvasionSendWaitTimePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_CrystalInvasionSendWaitTimePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_CrystalInvasionSendWaitTimePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class CrystalInvasionStartPacket
        {
            public const byte MessageId = 113;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("CrystalInvasionStartPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CrystalInvasionStartPacket), (PacketCodecDirections)3);
            public static TerrariaV4_CrystalInvasionStartPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_CrystalInvasionStartPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_CrystalInvasionStartPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class CrystalInvasionWipeAllTheThingsssPacket
        {
            public const byte MessageId = 114;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("CrystalInvasionWipeAllTheThingsssPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CrystalInvasionWipeAllTheThingsssPacket), (PacketCodecDirections)3);
            public static TerrariaV4_CrystalInvasionWipeAllTheThingsssPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_CrystalInvasionWipeAllTheThingsssPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_CrystalInvasionWipeAllTheThingsssPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class DamageNPCPacket
        {
            public const byte MessageId = 28;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("DamageNPCPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.DamageNPCPacket), (PacketCodecDirections)3);
            public static TerrariaV4_DamageNPCPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_DamageNPCPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_DamageNPCPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class DeadCellsDisplayJarTryPlacingPacket
        {
            public const byte MessageId = 149;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("DeadCellsDisplayJarTryPlacingPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.DeadCellsDisplayJarTryPlacingPacket), (PacketCodecDirections)3);
            public static TerrariaV4_DeadCellsDisplayJarTryPlacingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_DeadCellsDisplayJarTryPlacingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_DeadCellsDisplayJarTryPlacingPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class DeadPlayerPacket
        {
            public const byte MessageId = 135;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("DeadPlayerPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.DeadPlayerPacket), (PacketCodecDirections)3);
            public static TerrariaV4_DeadPlayerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_DeadPlayerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_DeadPlayerPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class DevCommandsPacket
        {
            public const byte MessageId = 94;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("DevCommandsPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.DevCommandsPacket), (PacketCodecDirections)3);
            public static TerrariaV4_DevCommandsPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_DevCommandsPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_DevCommandsPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class EmojiPacket
        {
            public const byte MessageId = 120;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("EmojiPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.EmojiPacket), (PacketCodecDirections)3);
            public static TerrariaV4_EmojiPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_EmojiPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_EmojiPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ExtraSpawnSectionLoadedPacket
        {
            public const byte MessageId = 158;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ExtraSpawnSectionLoadedPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ExtraSpawnSectionLoadedPacket), (PacketCodecDirections)3);
            public static TerrariaV4_ExtraSpawnSectionLoadedPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ExtraSpawnSectionLoadedPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ExtraSpawnSectionLoadedPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class FinishedConnectingToServerPacket
        {
            public const byte MessageId = 129;
            public const WireDirection Direction = (WireDirection)2;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("FinishedConnectingToServerPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.FinishedConnectingToServerPacket), (PacketCodecDirections)3);
            public static TerrariaV4_FinishedConnectingToServerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_FinishedConnectingToServerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_FinishedConnectingToServerPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class FishOutNPCPacket
        {
            public const byte MessageId = 130;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("FishOutNPCPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.FishOutNPCPacket), (PacketCodecDirections)3);
            public static TerrariaV4_FishOutNPCPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_FishOutNPCPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_FishOutNPCPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class FoodPlatterTryPlacingPacket
        {
            public const byte MessageId = 133;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("FoodPlatterTryPlacingPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.FoodPlatterTryPlacingPacket), (PacketCodecDirections)3);
            public static TerrariaV4_FoodPlatterTryPlacingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_FoodPlatterTryPlacingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_FoodPlatterTryPlacingPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class GemLockTogglePacket
        {
            public const byte MessageId = 105;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("GemLockTogglePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.GemLockTogglePacket), (PacketCodecDirections)3);
            public static TerrariaV4_GemLockTogglePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_GemLockTogglePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_GemLockTogglePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class HelloPacket
        {
            public const byte MessageId = 1;
            public const WireDirection Direction = (WireDirection)1;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("HelloPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.HelloPacket), (PacketCodecDirections)3);
            public static TerrariaV4_HelloPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_HelloPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_HelloPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class HitSwitchPacket
        {
            public const byte MessageId = 59;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("HitSwitchPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.HitSwitchPacket), (PacketCodecDirections)3);
            public static TerrariaV4_HitSwitchPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_HitSwitchPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_HitSwitchPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class HostTokenPacket
        {
            public const byte MessageId = 161;
            public const WireDirection Direction = (WireDirection)1;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("HostTokenPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.HostTokenPacket), (PacketCodecDirections)3);
            public static TerrariaV4_HostTokenPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_HostTokenPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_HostTokenPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class InitialSpawnPacket
        {
            public const byte MessageId = 49;
            public const WireDirection Direction = (WireDirection)2;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("InitialSpawnPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.InitialSpawnPacket), (PacketCodecDirections)3);
            public static TerrariaV4_InitialSpawnPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_InitialSpawnPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_InitialSpawnPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class InstancedItemPacket
        {
            public const byte MessageId = 90;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("InstancedItemPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.InstancedItemPacket), (PacketCodecDirections)3);
            public static TerrariaV4_InstancedItemPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_InstancedItemPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_InstancedItemPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class InstrumentSoundPacket
        {
            public const byte MessageId = 58;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("InstrumentSoundPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.InstrumentSoundPacket), (PacketCodecDirections)3);
            public static TerrariaV4_InstrumentSoundPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_InstrumentSoundPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_InstrumentSoundPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class InvasionProgressReportPacket
        {
            public const byte MessageId = 78;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("InvasionProgressReportPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.InvasionProgressReportPacket), (PacketCodecDirections)3);
            public static TerrariaV4_InvasionProgressReportPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_InvasionProgressReportPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_InvasionProgressReportPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ItemFrameTryPlacingPacket
        {
            public const byte MessageId = 89;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ItemFrameTryPlacingPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ItemFrameTryPlacingPacket), (PacketCodecDirections)3);
            public static TerrariaV4_ItemFrameTryPlacingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ItemFrameTryPlacingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ItemFrameTryPlacingPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ItemOwnerPacket
        {
            public const byte MessageId = 22;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ItemOwnerPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ItemOwnerPacket), (PacketCodecDirections)3);
            public static TerrariaV4_ItemOwnerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ItemOwnerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ItemOwnerPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ItemPositionPacket
        {
            public const byte MessageId = 160;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ItemPositionPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ItemPositionPacket), (PacketCodecDirections)3);
            public static TerrariaV4_ItemPositionPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ItemPositionPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ItemPositionPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ItemRotationAndAnimationPacket
        {
            public const byte MessageId = 41;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ItemRotationAndAnimationPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ItemRotationAndAnimationPacket), (PacketCodecDirections)3);
            public static TerrariaV4_ItemRotationAndAnimationPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ItemRotationAndAnimationPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ItemRotationAndAnimationPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ItemTweakerPacket
        {
            public const byte MessageId = 88;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ItemTweakerPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ItemTweakerPacket), (PacketCodecDirections)3);
            public static TerrariaV4_ItemTweakerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ItemTweakerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ItemTweakerPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ItemUseSoundPacket
        {
            public const byte MessageId = 152;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ItemUseSoundPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ItemUseSoundPacket), (PacketCodecDirections)3);
            public static TerrariaV4_ItemUseSoundPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ItemUseSoundPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ItemUseSoundPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class KickPacket
        {
            public const byte MessageId = 2;
            public const WireDirection Direction = (WireDirection)2;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("KickPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.KickPacket), (PacketCodecDirections)3);
            public static TerrariaV4_KickPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_KickPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_KickPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class KillProjectilePacket
        {
            public const byte MessageId = 29;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("KillProjectilePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.KillProjectilePacket), (PacketCodecDirections)3);
            public static TerrariaV4_KillProjectilePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_KillProjectilePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_KillProjectilePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class LandGolfBallInCupPacket
        {
            public const byte MessageId = 128;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("LandGolfBallInCupPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.LandGolfBallInCupPacket), (PacketCodecDirections)3);
            public static TerrariaV4_LandGolfBallInCupPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_LandGolfBallInCupPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_LandGolfBallInCupPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class LiquidUpdatePacket
        {
            public const byte MessageId = 48;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("LiquidUpdatePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.LiquidUpdatePacket), (PacketCodecDirections)3);
            public static TerrariaV4_LiquidUpdatePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_LiquidUpdatePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_LiquidUpdatePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class LockAndUnlockPacket
        {
            public const byte MessageId = 52;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("LockAndUnlockPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.LockAndUnlockPacket), (PacketCodecDirections)3);
            public static TerrariaV4_LockAndUnlockPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_LockAndUnlockPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_LockAndUnlockPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ManaEffectPacket
        {
            public const byte MessageId = 43;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ManaEffectPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ManaEffectPacket), (PacketCodecDirections)3);
            public static TerrariaV4_ManaEffectPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ManaEffectPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ManaEffectPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class MassWireOperationPacket
        {
            public const byte MessageId = 109;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("MassWireOperationPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.MassWireOperationPacket), (PacketCodecDirections)3);
            public static TerrariaV4_MassWireOperationPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_MassWireOperationPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_MassWireOperationPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class MassWireOperationPayPacket
        {
            public const byte MessageId = 110;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("MassWireOperationPayPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.MassWireOperationPayPacket), (PacketCodecDirections)3);
            public static TerrariaV4_MassWireOperationPayPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_MassWireOperationPayPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_MassWireOperationPayPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class MinionAttackTargetUpdatePacket
        {
            public const byte MessageId = 115;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("MinionAttackTargetUpdatePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.MinionAttackTargetUpdatePacket), (PacketCodecDirections)3);
            public static TerrariaV4_MinionAttackTargetUpdatePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_MinionAttackTargetUpdatePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_MinionAttackTargetUpdatePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class MinionRestTargetUpdatePacket
        {
            public const byte MessageId = 99;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("MinionRestTargetUpdatePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.MinionRestTargetUpdatePacket), (PacketCodecDirections)3);
            public static TerrariaV4_MinionRestTargetUpdatePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_MinionRestTargetUpdatePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_MinionRestTargetUpdatePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class MiscDataSyncPacket
        {
            public const byte MessageId = 51;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("MiscDataSyncPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.MiscDataSyncPacket), (PacketCodecDirections)3);
            public static TerrariaV4_MiscDataSyncPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_MiscDataSyncPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_MiscDataSyncPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class MoonlordHorrorPacket
        {
            public const byte MessageId = 103;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("MoonlordHorrorPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.MoonlordHorrorPacket), (PacketCodecDirections)3);
            public static TerrariaV4_MoonlordHorrorPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_MoonlordHorrorPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_MoonlordHorrorPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class MurderSomeoneElsesPortalPacket
        {
            public const byte MessageId = 95;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("MurderSomeoneElsesPortalPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.MurderSomeoneElsesPortalPacket), (PacketCodecDirections)3);
            public static TerrariaV4_MurderSomeoneElsesPortalPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_MurderSomeoneElsesPortalPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_MurderSomeoneElsesPortalPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class NPCBuffsPacket
        {
            public const byte MessageId = 54;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("NPCBuffsPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.NPCBuffsPacket), (PacketCodecDirections)3);
            public static TerrariaV4_NPCBuffsPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_NPCBuffsPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_NPCBuffsPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class NPCDebuffDamagePacket
        {
            public const byte MessageId = 153;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("NPCDebuffDamagePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.NPCDebuffDamagePacket), (PacketCodecDirections)3);
            public static TerrariaV4_NPCDebuffDamagePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_NPCDebuffDamagePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_NPCDebuffDamagePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class NebulaLevelupRequestPacket
        {
            public const byte MessageId = 102;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("NebulaLevelupRequestPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.NebulaLevelupRequestPacket), (PacketCodecDirections)3);
            public static TerrariaV4_NebulaLevelupRequestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_NebulaLevelupRequestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_NebulaLevelupRequestPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class NetModulesPacket
        {
            public const byte MessageId = 82;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("NetModulesPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.NetModulesPacket), (PacketCodecDirections)3);
            public static TerrariaV4_NetModulesPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_NetModulesPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_NetModulesPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class NeverCalledPacket
        {
            public const byte MessageId = 0;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("NeverCalledPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.NeverCalledPacket), (PacketCodecDirections)3);
            public static TerrariaV4_NeverCalledPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_NeverCalledPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_NeverCalledPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class OpenSignRequestPacket
        {
            public const byte MessageId = 46;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("OpenSignRequestPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.OpenSignRequestPacket), (PacketCodecDirections)3);
            public static TerrariaV4_OpenSignRequestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_OpenSignRequestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_OpenSignRequestPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class OpenSignResponsePacket
        {
            public const byte MessageId = 47;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("OpenSignResponsePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.OpenSignResponsePacket), (PacketCodecDirections)3);
            public static TerrariaV4_OpenSignResponsePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_OpenSignResponsePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_OpenSignResponsePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class PingPacket
        {
            public const byte MessageId = 154;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PingPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PingPacket), (PacketCodecDirections)3);
            public static TerrariaV4_PingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PingPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class PlaceObjectPacket
        {
            public const byte MessageId = 79;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PlaceObjectPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlaceObjectPacket), (PacketCodecDirections)3);
            public static TerrariaV4_PlaceObjectPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PlaceObjectPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PlaceObjectPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class PlayLegacySoundPacket
        {
            public const byte MessageId = 132;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PlayLegacySoundPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayLegacySoundPacket), (PacketCodecDirections)3);
            public static TerrariaV4_PlayLegacySoundPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PlayLegacySoundPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PlayLegacySoundPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class PlayerActivePacket
        {
            public const byte MessageId = 14;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PlayerActivePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerActivePacket), (PacketCodecDirections)3);
            public static TerrariaV4_PlayerActivePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PlayerActivePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PlayerActivePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class PlayerBuffs
        {
            public const byte MessageId = 50;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PlayerBuffs", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket), (PacketCodecDirections)3);
            public static TerrariaV4_PlayerBuffsPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PlayerBuffsPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PlayerBuffsPacketCodecWriter CreateWriter() => new();
        }
        public static class PlayerControls
        {
            public const byte MessageId = 13;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PlayerControls", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerControlsPacket), (PacketCodecDirections)3);
            public static TerrariaV4_PlayerControlsPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PlayerControlsPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PlayerControlsPacketCodecWriter CreateWriter() => new();
        }
        public static class PlayerDeathV2Packet
        {
            public const byte MessageId = 118;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PlayerDeathV2Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerDeathV2Packet), (PacketCodecDirections)3);
            public static TerrariaV4_PlayerDeathV2PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PlayerDeathV2PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PlayerDeathV2PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class PlayerHealPacket
        {
            public const byte MessageId = 35;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PlayerHealPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerHealPacket), (PacketCodecDirections)3);
            public static TerrariaV4_PlayerHealPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PlayerHealPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PlayerHealPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class PlayerHurtV2Packet
        {
            public const byte MessageId = 117;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PlayerHurtV2Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerHurtV2Packet), (PacketCodecDirections)3);
            public static TerrariaV4_PlayerHurtV2PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PlayerHurtV2PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PlayerHurtV2PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class PlayerInfoPacket
        {
            public const byte MessageId = 3;
            public const WireDirection Direction = (WireDirection)2;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PlayerInfoPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerInfoPacket), (PacketCodecDirections)3);
            public static TerrariaV4_PlayerInfoPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PlayerInfoPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PlayerInfoPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class PlayerLifeManaPacket
        {
            public const byte MessageId = 16;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PlayerLifeManaPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerLifeManaPacket), (PacketCodecDirections)3);
            public static TerrariaV4_PlayerLifeManaPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PlayerLifeManaPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PlayerLifeManaPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class PlayerSpawnPacket
        {
            public const byte MessageId = 12;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PlayerSpawnPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerSpawnPacket), (PacketCodecDirections)3);
            public static TerrariaV4_PlayerSpawnPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PlayerSpawnPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PlayerSpawnPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class PlayerStealthPacket
        {
            public const byte MessageId = 84;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PlayerStealthPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerStealthPacket), (PacketCodecDirections)3);
            public static TerrariaV4_PlayerStealthPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PlayerStealthPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PlayerStealthPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class PoofOfSmokePacket
        {
            public const byte MessageId = 106;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("PoofOfSmokePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PoofOfSmokePacket), (PacketCodecDirections)3);
            public static TerrariaV4_PoofOfSmokePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_PoofOfSmokePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_PoofOfSmokePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class QuestsCountSyncPacket
        {
            public const byte MessageId = 76;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("QuestsCountSyncPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.QuestsCountSyncPacket), (PacketCodecDirections)3);
            public static TerrariaV4_QuestsCountSyncPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_QuestsCountSyncPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_QuestsCountSyncPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class QuickStackChestsRequestPacket
        {
            public const byte MessageId = 85;
            public const WireDirection Direction = (WireDirection)1;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("QuickStackChestsRequestPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.QuickStackChestsRequestPacket), (PacketCodecDirections)3);
            public static TerrariaV4_QuickStackChestsRequestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_QuickStackChestsRequestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_QuickStackChestsRequestPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class QuickStackChestsResponsePacket
        {
            public const byte MessageId = 85;
            public const WireDirection Direction = (WireDirection)2;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("QuickStackChestsResponsePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.QuickStackChestsResponsePacket), (PacketCodecDirections)3);
            public static TerrariaV4_QuickStackChestsResponsePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_QuickStackChestsResponsePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_QuickStackChestsResponsePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ReleaseItemOwnershipPacket
        {
            public const byte MessageId = 39;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ReleaseItemOwnershipPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ReleaseItemOwnershipPacket), (PacketCodecDirections)3);
            public static TerrariaV4_ReleaseItemOwnershipPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ReleaseItemOwnershipPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ReleaseItemOwnershipPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class RemoveRevengeMarkerPacket
        {
            public const byte MessageId = 127;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("RemoveRevengeMarkerPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.RemoveRevengeMarkerPacket), (PacketCodecDirections)3);
            public static TerrariaV4_RemoveRevengeMarkerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_RemoveRevengeMarkerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_RemoveRevengeMarkerPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class RequestChestOpenPacket
        {
            public const byte MessageId = 31;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("RequestChestOpenPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.RequestChestOpenPacket), (PacketCodecDirections)3);
            public static TerrariaV4_RequestChestOpenPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_RequestChestOpenPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_RequestChestOpenPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class RequestLucyPopupPacket
        {
            public const byte MessageId = 141;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("RequestLucyPopupPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.RequestLucyPopupPacket), (PacketCodecDirections)3);
            public static TerrariaV4_RequestLucyPopupPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_RequestLucyPopupPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_RequestLucyPopupPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class RequestNPCBuffRemovalPacket
        {
            public const byte MessageId = 137;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("RequestNPCBuffRemovalPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.RequestNPCBuffRemovalPacket), (PacketCodecDirections)3);
            public static TerrariaV4_RequestNPCBuffRemovalPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_RequestNPCBuffRemovalPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_RequestNPCBuffRemovalPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class RequestPasswordPacket
        {
            public const byte MessageId = 37;
            public const WireDirection Direction = (WireDirection)2;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("RequestPasswordPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.RequestPasswordPacket), (PacketCodecDirections)3);
            public static TerrariaV4_RequestPasswordPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_RequestPasswordPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_RequestPasswordPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class RequestQuestEffectPacket
        {
            public const byte MessageId = 144;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("RequestQuestEffectPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.RequestQuestEffectPacket), (PacketCodecDirections)3);
            public static TerrariaV4_RequestQuestEffectPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_RequestQuestEffectPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_RequestQuestEffectPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class RequestSectionPacket
        {
            public const byte MessageId = 159;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("RequestSectionPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.RequestSectionPacket), (PacketCodecDirections)3);
            public static TerrariaV4_RequestSectionPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_RequestSectionPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_RequestSectionPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class RequestTeleportationByServerPacket
        {
            public const byte MessageId = 73;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("RequestTeleportationByServerPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.RequestTeleportationByServerPacket), (PacketCodecDirections)3);
            public static TerrariaV4_RequestTeleportationByServerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_RequestTeleportationByServerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_RequestTeleportationByServerPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class RequestTileEntityInteractionPacket
        {
            public const byte MessageId = 122;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("RequestTileEntityInteractionPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.RequestTileEntityInteractionPacket), (PacketCodecDirections)3);
            public static TerrariaV4_RequestTileEntityInteractionPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_RequestTileEntityInteractionPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_RequestTileEntityInteractionPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class RequestWorldDataPacket
        {
            public const byte MessageId = 6;
            public const WireDirection Direction = (WireDirection)1;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("RequestWorldDataPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.RequestWorldDataPacket), (PacketCodecDirections)3);
            public static TerrariaV4_RequestWorldDataPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_RequestWorldDataPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_RequestWorldDataPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SendPasswordPacket
        {
            public const byte MessageId = 38;
            public const WireDirection Direction = (WireDirection)1;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SendPasswordPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SendPasswordPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SendPasswordPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SendPasswordPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SendPasswordPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SetCountsAsHostForGameplayPacket
        {
            public const byte MessageId = 139;
            public const WireDirection Direction = (WireDirection)2;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SetCountsAsHostForGameplayPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SetCountsAsHostForGameplayPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SetCountsAsHostForGameplayPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SetCountsAsHostForGameplayPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SetCountsAsHostForGameplayPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SetMiscEventValuesPacket
        {
            public const byte MessageId = 140;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SetMiscEventValuesPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SetMiscEventValuesPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SetMiscEventValuesPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SetMiscEventValuesPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SetMiscEventValuesPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SetTimePacket
        {
            public const byte MessageId = 18;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SetTimePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SetTimePacket), (PacketCodecDirections)3);
            public static TerrariaV4_SetTimePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SetTimePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SetTimePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ShimmerActionsPacket
        {
            public const byte MessageId = 146;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ShimmerActionsPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ShimmerActionsPacket), (PacketCodecDirections)3);
            public static TerrariaV4_ShimmerActionsPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ShimmerActionsPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ShimmerActionsPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ShopOverridePacket
        {
            public const byte MessageId = 104;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ShopOverridePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ShopOverridePacket), (PacketCodecDirections)3);
            public static TerrariaV4_ShopOverridePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ShopOverridePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ShopOverridePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SmartTextMessagePacket
        {
            public const byte MessageId = 107;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SmartTextMessagePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SmartTextMessagePacket), (PacketCodecDirections)3);
            public static TerrariaV4_SmartTextMessagePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SmartTextMessagePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SmartTextMessagePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SocialHandshakePacket
        {
            public const byte MessageId = 93;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SocialHandshakePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SocialHandshakePacket), (PacketCodecDirections)3);
            public static TerrariaV4_SocialHandshakePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SocialHandshakePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SocialHandshakePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SpawnBossUseLicenseStartEventPacket
        {
            public const byte MessageId = 61;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SpawnBossUseLicenseStartEventPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SpawnBossUseLicenseStartEventPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SpawnBossUseLicenseStartEventPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SpawnBossUseLicenseStartEventPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SpawnBossUseLicenseStartEventPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SpawnTileDataPacket
        {
            public const byte MessageId = 8;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SpawnTileDataPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SpawnTileDataPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SpawnTileDataPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SpawnTileDataPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SpawnTileDataPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SpecialFXPacket
        {
            public const byte MessageId = 112;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SpecialFXPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SpecialFXPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SpecialFXPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SpecialFXPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SpecialFXPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SpectatePlayerPacket
        {
            public const byte MessageId = 150;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SpectatePlayerPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SpectatePlayerPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SpectatePlayerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SpectatePlayerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SpectatePlayerPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class StatusTextSizePacket
        {
            public const byte MessageId = 9;
            public const WireDirection Direction = (WireDirection)2;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("StatusTextSizePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.StatusTextSizePacket), (PacketCodecDirections)3);
            public static TerrariaV4_StatusTextSizePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_StatusTextSizePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_StatusTextSizePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncCavernMonsterTypePacket
        {
            public const byte MessageId = 136;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncCavernMonsterTypePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncCavernMonsterTypePacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncCavernMonsterTypePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncCavernMonsterTypePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncCavernMonsterTypePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncChestItemPacket
        {
            public const byte MessageId = 32;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncChestItemPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncChestItemPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncChestItemPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncChestItemPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncChestItemPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncChestSizePacket
        {
            public const byte MessageId = 155;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncChestSizePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncChestSizePacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncChestSizePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncChestSizePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncChestSizePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncEmoteBubblePacket
        {
            public const byte MessageId = 91;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncEmoteBubblePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncEmoteBubblePacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncEmoteBubblePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncEmoteBubblePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncEmoteBubblePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncEquipmentPacket
        {
            public const byte MessageId = 5;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncEquipmentPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncEquipmentPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncEquipmentPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncEquipmentPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncEquipmentPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncExtraValuePacket
        {
            public const byte MessageId = 92;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncExtraValuePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncExtraValuePacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncExtraValuePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncExtraValuePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncExtraValuePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncItemCannotBeTakenByEnemiesPacket
        {
            public const byte MessageId = 148;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncItemCannotBeTakenByEnemiesPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncItemCannotBeTakenByEnemiesPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncItemCannotBeTakenByEnemiesPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncItemCannotBeTakenByEnemiesPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncItemCannotBeTakenByEnemiesPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncItemDespawnPacket
        {
            public const byte MessageId = 151;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncItemDespawnPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncItemDespawnPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncItemDespawnPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncItemDespawnPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncItemDespawnPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncItemPacket
        {
            public const byte MessageId = 21;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncItemPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncItemPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncItemPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncItemPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncItemPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncItemsWithShimmerPacket
        {
            public const byte MessageId = 145;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncItemsWithShimmerPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncItemsWithShimmerPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncItemsWithShimmerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncItemsWithShimmerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncItemsWithShimmerPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncLoadoutPacket
        {
            public const byte MessageId = 147;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncLoadoutPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncLoadoutPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncLoadoutPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncLoadoutPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncLoadoutPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncNPCPacket
        {
            public const byte MessageId = 23;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncNPCPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncNPCPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncNPCPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncNPCPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncPlayerChestIndexPacket
        {
            public const byte MessageId = 80;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncPlayerChestIndexPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncPlayerChestIndexPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncPlayerChestIndexPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncPlayerChestIndexPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncPlayerChestIndexPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncPlayerChestPacket
        {
            public const byte MessageId = 33;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncPlayerChestPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncPlayerChestPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncPlayerChestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncPlayerChestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncPlayerChestPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncPlayerPacket
        {
            public const byte MessageId = 4;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncPlayerPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncPlayerPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncPlayerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncPlayerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncPlayerPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncPlayerZonePacket
        {
            public const byte MessageId = 36;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncPlayerZonePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncPlayerZonePacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncPlayerZonePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncPlayerZonePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncPlayerZonePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncProjectilePacket
        {
            public const byte MessageId = 27;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncProjectilePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncProjectilePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncProjectilePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncProjectilePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncProjectileTrackersPacket
        {
            public const byte MessageId = 142;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncProjectileTrackersPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectileTrackersPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncProjectileTrackersPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncProjectileTrackersPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncProjectileTrackersPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncRevengeMarkerPacket
        {
            public const byte MessageId = 126;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncRevengeMarkerPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncRevengeMarkerPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncRevengeMarkerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncRevengeMarkerPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncRevengeMarkerPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncTalkNPCPacket
        {
            public const byte MessageId = 40;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncTalkNPCPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncTalkNPCPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncTalkNPCPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncTalkNPCPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncTalkNPCPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncTilePaintOrCoatingPacket
        {
            public const byte MessageId = 63;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncTilePaintOrCoatingPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncTilePaintOrCoatingPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncTilePaintOrCoatingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncTilePaintOrCoatingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncTilePaintOrCoatingPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncTilePickingPacket
        {
            public const byte MessageId = 125;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncTilePickingPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncTilePickingPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncTilePickingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncTilePickingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncTilePickingPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class SyncWallPaintOrCoatingPacket
        {
            public const byte MessageId = 64;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("SyncWallPaintOrCoatingPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncWallPaintOrCoatingPacket), (PacketCodecDirections)3);
            public static TerrariaV4_SyncWallPaintOrCoatingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_SyncWallPaintOrCoatingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_SyncWallPaintOrCoatingPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TEDisplayDollDataSyncPacket
        {
            public const byte MessageId = 121;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TEDisplayDollDataSyncPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TEDisplayDollDataSyncPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TEDisplayDollDataSyncPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TEDisplayDollDataSyncPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TEDisplayDollDataSyncPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TEHatRackItemSyncPacket
        {
            public const byte MessageId = 124;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TEHatRackItemSyncPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TEHatRackItemSyncPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TEHatRackItemSyncPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TEHatRackItemSyncPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TEHatRackItemSyncPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TELeashedEntityAnchorPlaceItemPacket
        {
            public const byte MessageId = 156;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TELeashedEntityAnchorPlaceItemPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TELeashedEntityAnchorPlaceItemPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TELeashedEntityAnchorPlaceItemPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TELeashedEntityAnchorPlaceItemPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TELeashedEntityAnchorPlaceItemPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TamperWithNPCPacket
        {
            public const byte MessageId = 131;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TamperWithNPCPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TamperWithNPCPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TamperWithNPCPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TamperWithNPCPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TamperWithNPCPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TeamChangeFromUIPacket
        {
            public const byte MessageId = 157;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TeamChangeFromUIPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeamChangeFromUIPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TeamChangeFromUIPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TeamChangeFromUIPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TeamChangeFromUIPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TeamChangePacket
        {
            public const byte MessageId = 45;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TeamChangePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeamChangePacket), (PacketCodecDirections)3);
            public static TerrariaV4_TeamChangePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TeamChangePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TeamChangePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TeleportEntityPacket
        {
            public const byte MessageId = 65;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TeleportEntityPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TeleportEntityPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TeleportEntityPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TeleportEntityPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TeleportNPCThroughPortalPacket
        {
            public const byte MessageId = 100;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TeleportNPCThroughPortalPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportNPCThroughPortalPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TeleportNPCThroughPortalPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TeleportNPCThroughPortalPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TeleportNPCThroughPortalPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TeleportPlayerThroughPortalPacket
        {
            public const byte MessageId = 96;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TeleportPlayerThroughPortalPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportPlayerThroughPortalPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TeleportPlayerThroughPortalPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TeleportPlayerThroughPortalPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TeleportPlayerThroughPortalPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TemporaryAnimationPacket
        {
            public const byte MessageId = 77;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TemporaryAnimationPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TemporaryAnimationPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TemporaryAnimationPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TemporaryAnimationPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TemporaryAnimationPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TileEntityPlacementPacket
        {
            public const byte MessageId = 87;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TileEntityPlacementPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileEntityPlacementPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TileEntityPlacementPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TileEntityPlacementPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TileEntityPlacementPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TileEntitySharingPacket
        {
            public const byte MessageId = 86;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TileEntitySharingPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileEntitySharingPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TileEntitySharingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TileEntitySharingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TileEntitySharingPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TileFrameSectionPacket
        {
            public const byte MessageId = 11;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TileFrameSectionPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileFrameSectionPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TileFrameSectionPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TileFrameSectionPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TileFrameSectionPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TileManipulationPacket
        {
            public const byte MessageId = 17;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TileManipulationPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileManipulationPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TileManipulationPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TileManipulationPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TileManipulationPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TileSectionPacket
        {
            public const byte MessageId = 10;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TileSectionPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TileSectionPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TileSectionPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TileSectionPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class ToggleDoorStatePacket
        {
            public const byte MessageId = 19;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("ToggleDoorStatePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ToggleDoorStatePacket), (PacketCodecDirections)3);
            public static TerrariaV4_ToggleDoorStatePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_ToggleDoorStatePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_ToggleDoorStatePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TogglePVPPacket
        {
            public const byte MessageId = 30;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TogglePVPPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TogglePVPPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TogglePVPPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TogglePVPPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TogglePVPPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TogglePartyPacket
        {
            public const byte MessageId = 111;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TogglePartyPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TogglePartyPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TogglePartyPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TogglePartyPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TogglePartyPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class TravelMerchantItemsPacket
        {
            public const byte MessageId = 72;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("TravelMerchantItemsPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TravelMerchantItemsPacket), (PacketCodecDirections)3);
            public static TerrariaV4_TravelMerchantItemsPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_TravelMerchantItemsPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_TravelMerchantItemsPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class UniqueTownNPCInfoSyncRequestPacket
        {
            public const byte MessageId = 56;
            public const WireDirection Direction = (WireDirection)1;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("UniqueTownNPCInfoSyncRequestPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.UniqueTownNPCInfoSyncRequestPacket), (PacketCodecDirections)3);
            public static TerrariaV4_UniqueTownNPCInfoSyncRequestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_UniqueTownNPCInfoSyncRequestPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_UniqueTownNPCInfoSyncRequestPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class UniqueTownNPCInfoSyncResponsePacket
        {
            public const byte MessageId = 56;
            public const WireDirection Direction = (WireDirection)2;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("UniqueTownNPCInfoSyncResponsePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.UniqueTownNPCInfoSyncResponsePacket), (PacketCodecDirections)3);
            public static TerrariaV4_UniqueTownNPCInfoSyncResponsePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_UniqueTownNPCInfoSyncResponsePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_UniqueTownNPCInfoSyncResponsePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class Unknown15Packet
        {
            public const byte MessageId = 15;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("Unknown15Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown15Packet), (PacketCodecDirections)3);
            public static TerrariaV4_Unknown15PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_Unknown15PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_Unknown15PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class Unknown42Packet
        {
            public const byte MessageId = 42;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("Unknown42Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown42Packet), (PacketCodecDirections)3);
            public static TerrariaV4_Unknown42PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_Unknown42PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_Unknown42PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class Unknown44Packet
        {
            public const byte MessageId = 44;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("Unknown44Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown44Packet), (PacketCodecDirections)3);
            public static TerrariaV4_Unknown44PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_Unknown44PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_Unknown44PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class Unknown57Packet
        {
            public const byte MessageId = 57;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("Unknown57Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown57Packet), (PacketCodecDirections)3);
            public static TerrariaV4_Unknown57PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_Unknown57PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_Unknown57PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class Unknown60Packet
        {
            public const byte MessageId = 60;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("Unknown60Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown60Packet), (PacketCodecDirections)3);
            public static TerrariaV4_Unknown60PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_Unknown60PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_Unknown60PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class Unknown62Packet
        {
            public const byte MessageId = 62;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("Unknown62Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown62Packet), (PacketCodecDirections)3);
            public static TerrariaV4_Unknown62PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_Unknown62PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_Unknown62PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class Unknown66Packet
        {
            public const byte MessageId = 66;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("Unknown66Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown66Packet), (PacketCodecDirections)3);
            public static TerrariaV4_Unknown66PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_Unknown66PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_Unknown66PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class Unknown67Packet
        {
            public const byte MessageId = 67;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("Unknown67Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown67Packet), (PacketCodecDirections)3);
            public static TerrariaV4_Unknown67PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_Unknown67PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_Unknown67PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class Unknown68Packet
        {
            public const byte MessageId = 68;
            public const WireDirection Direction = (WireDirection)1;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("Unknown68Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown68Packet), (PacketCodecDirections)3);
            public static TerrariaV4_Unknown68PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_Unknown68PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_Unknown68PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class Unused25Packet
        {
            public const byte MessageId = 25;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("Unused25Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unused25Packet), (PacketCodecDirections)3);
            public static TerrariaV4_Unused25PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_Unused25PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_Unused25PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class Unused26Packet
        {
            public const byte MessageId = 26;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("Unused26Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unused26Packet), (PacketCodecDirections)3);
            public static TerrariaV4_Unused26PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_Unused26PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_Unused26PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class Unused83Packet
        {
            public const byte MessageId = 83;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("Unused83Packet", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unused83Packet), (PacketCodecDirections)3);
            public static TerrariaV4_Unused83PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_Unused83PacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_Unused83PacketPacketCodecWriter CreateWriter() => new();
        }
        public static class UnusedMeleeStrikePacket
        {
            public const byte MessageId = 24;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("UnusedMeleeStrikePacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.UnusedMeleeStrikePacket), (PacketCodecDirections)3);
            public static TerrariaV4_UnusedMeleeStrikePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_UnusedMeleeStrikePacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_UnusedMeleeStrikePacketPacketCodecWriter CreateWriter() => new();
        }
        public static class UpdatePlayerLuckFactorsPacket
        {
            public const byte MessageId = 134;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("UpdatePlayerLuckFactorsPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.UpdatePlayerLuckFactorsPacket), (PacketCodecDirections)3);
            public static TerrariaV4_UpdatePlayerLuckFactorsPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_UpdatePlayerLuckFactorsPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_UpdatePlayerLuckFactorsPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class UpdateTowerShieldStrengthsPacket
        {
            public const byte MessageId = 101;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("UpdateTowerShieldStrengthsPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.UpdateTowerShieldStrengthsPacket), (PacketCodecDirections)3);
            public static TerrariaV4_UpdateTowerShieldStrengthsPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_UpdateTowerShieldStrengthsPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_UpdateTowerShieldStrengthsPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class WeaponsRackTryPlacingPacket
        {
            public const byte MessageId = 123;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("WeaponsRackTryPlacingPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WeaponsRackTryPlacingPacket), (PacketCodecDirections)3);
            public static TerrariaV4_WeaponsRackTryPlacingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_WeaponsRackTryPlacingPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_WeaponsRackTryPlacingPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class WiredCannonShotPacket
        {
            public const byte MessageId = 108;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("WiredCannonShotPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WiredCannonShotPacket), (PacketCodecDirections)3);
            public static TerrariaV4_WiredCannonShotPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_WiredCannonShotPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_WiredCannonShotPacketPacketCodecWriter CreateWriter() => new();
        }
        public static class WorldDataPacket
        {
            public const byte MessageId = 7;
            public const WireDirection Direction = (WireDirection)3;
            public static ProtocolPacketDescriptor Descriptor { get; } = new("WorldDataPacket", MessageId, Direction, typeof(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket), (PacketCodecDirections)3);
            public static TerrariaV4_WorldDataPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> body) => new(body);
            public static TerrariaV4_WorldDataPacketPacketCodecReader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength) => new(buffer, bodyLength);
            public static TerrariaV4_WorldDataPacketPacketCodecWriter CreateWriter() => new();
        }
    }
    public static global::System.Collections.Generic.IReadOnlyList<ProtocolPacketDescriptor> All { get; } = Array.AsReadOnly(new ProtocolPacketDescriptor[] { Packets.AchievementMessageEventHappenedPacket.Descriptor, Packets.AchievementMessageNPCKilledPacket.Descriptor, Packets.AddNPCBuffPacket.Descriptor, Packets.AddPlayerBuffPvPPacket.Descriptor, Packets.AnglerQuestFinishedPacket.Descriptor, Packets.AnglerQuestPacket.Descriptor, Packets.AreaTileChangePacket.Descriptor, Packets.BugCatchingPacket.Descriptor, Packets.BugReleasingPacket.Descriptor, Packets.ChestNameRequestPacket.Descriptor, Packets.ChestNameResponsePacket.Descriptor, Packets.ChestUpdatesPacket.Descriptor, Packets.ClientSyncedInventoryPacket.Descriptor, Packets.CombatTextIntPacket.Descriptor, Packets.CombatTextStringPacket.Descriptor, Packets.CrystalInvasionRequestedToSkipWaitTimePacket.Descriptor, Packets.CrystalInvasionSendWaitTimePacket.Descriptor, Packets.CrystalInvasionStartPacket.Descriptor, Packets.CrystalInvasionWipeAllTheThingsssPacket.Descriptor, Packets.DamageNPCPacket.Descriptor, Packets.DeadCellsDisplayJarTryPlacingPacket.Descriptor, Packets.DeadPlayerPacket.Descriptor, Packets.DevCommandsPacket.Descriptor, Packets.EmojiPacket.Descriptor, Packets.ExtraSpawnSectionLoadedPacket.Descriptor, Packets.FinishedConnectingToServerPacket.Descriptor, Packets.FishOutNPCPacket.Descriptor, Packets.FoodPlatterTryPlacingPacket.Descriptor, Packets.GemLockTogglePacket.Descriptor, Packets.HelloPacket.Descriptor, Packets.HitSwitchPacket.Descriptor, Packets.HostTokenPacket.Descriptor, Packets.InitialSpawnPacket.Descriptor, Packets.InstancedItemPacket.Descriptor, Packets.InstrumentSoundPacket.Descriptor, Packets.InvasionProgressReportPacket.Descriptor, Packets.ItemFrameTryPlacingPacket.Descriptor, Packets.ItemOwnerPacket.Descriptor, Packets.ItemPositionPacket.Descriptor, Packets.ItemRotationAndAnimationPacket.Descriptor, Packets.ItemTweakerPacket.Descriptor, Packets.ItemUseSoundPacket.Descriptor, Packets.KickPacket.Descriptor, Packets.KillProjectilePacket.Descriptor, Packets.LandGolfBallInCupPacket.Descriptor, Packets.LiquidUpdatePacket.Descriptor, Packets.LockAndUnlockPacket.Descriptor, Packets.ManaEffectPacket.Descriptor, Packets.MassWireOperationPacket.Descriptor, Packets.MassWireOperationPayPacket.Descriptor, Packets.MinionAttackTargetUpdatePacket.Descriptor, Packets.MinionRestTargetUpdatePacket.Descriptor, Packets.MiscDataSyncPacket.Descriptor, Packets.MoonlordHorrorPacket.Descriptor, Packets.MurderSomeoneElsesPortalPacket.Descriptor, Packets.NPCBuffsPacket.Descriptor, Packets.NPCDebuffDamagePacket.Descriptor, Packets.NebulaLevelupRequestPacket.Descriptor, Packets.NetModulesPacket.Descriptor, Packets.NeverCalledPacket.Descriptor, Packets.OpenSignRequestPacket.Descriptor, Packets.OpenSignResponsePacket.Descriptor, Packets.PingPacket.Descriptor, Packets.PlaceObjectPacket.Descriptor, Packets.PlayLegacySoundPacket.Descriptor, Packets.PlayerActivePacket.Descriptor, Packets.PlayerBuffs.Descriptor, Packets.PlayerControls.Descriptor, Packets.PlayerDeathV2Packet.Descriptor, Packets.PlayerHealPacket.Descriptor, Packets.PlayerHurtV2Packet.Descriptor, Packets.PlayerInfoPacket.Descriptor, Packets.PlayerLifeManaPacket.Descriptor, Packets.PlayerSpawnPacket.Descriptor, Packets.PlayerStealthPacket.Descriptor, Packets.PoofOfSmokePacket.Descriptor, Packets.QuestsCountSyncPacket.Descriptor, Packets.QuickStackChestsRequestPacket.Descriptor, Packets.QuickStackChestsResponsePacket.Descriptor, Packets.ReleaseItemOwnershipPacket.Descriptor, Packets.RemoveRevengeMarkerPacket.Descriptor, Packets.RequestChestOpenPacket.Descriptor, Packets.RequestLucyPopupPacket.Descriptor, Packets.RequestNPCBuffRemovalPacket.Descriptor, Packets.RequestPasswordPacket.Descriptor, Packets.RequestQuestEffectPacket.Descriptor, Packets.RequestSectionPacket.Descriptor, Packets.RequestTeleportationByServerPacket.Descriptor, Packets.RequestTileEntityInteractionPacket.Descriptor, Packets.RequestWorldDataPacket.Descriptor, Packets.SendPasswordPacket.Descriptor, Packets.SetCountsAsHostForGameplayPacket.Descriptor, Packets.SetMiscEventValuesPacket.Descriptor, Packets.SetTimePacket.Descriptor, Packets.ShimmerActionsPacket.Descriptor, Packets.ShopOverridePacket.Descriptor, Packets.SmartTextMessagePacket.Descriptor, Packets.SocialHandshakePacket.Descriptor, Packets.SpawnBossUseLicenseStartEventPacket.Descriptor, Packets.SpawnTileDataPacket.Descriptor, Packets.SpecialFXPacket.Descriptor, Packets.SpectatePlayerPacket.Descriptor, Packets.StatusTextSizePacket.Descriptor, Packets.SyncCavernMonsterTypePacket.Descriptor, Packets.SyncChestItemPacket.Descriptor, Packets.SyncChestSizePacket.Descriptor, Packets.SyncEmoteBubblePacket.Descriptor, Packets.SyncEquipmentPacket.Descriptor, Packets.SyncExtraValuePacket.Descriptor, Packets.SyncItemCannotBeTakenByEnemiesPacket.Descriptor, Packets.SyncItemDespawnPacket.Descriptor, Packets.SyncItemPacket.Descriptor, Packets.SyncItemsWithShimmerPacket.Descriptor, Packets.SyncLoadoutPacket.Descriptor, Packets.SyncNPCPacket.Descriptor, Packets.SyncPlayerChestIndexPacket.Descriptor, Packets.SyncPlayerChestPacket.Descriptor, Packets.SyncPlayerPacket.Descriptor, Packets.SyncPlayerZonePacket.Descriptor, Packets.SyncProjectilePacket.Descriptor, Packets.SyncProjectileTrackersPacket.Descriptor, Packets.SyncRevengeMarkerPacket.Descriptor, Packets.SyncTalkNPCPacket.Descriptor, Packets.SyncTilePaintOrCoatingPacket.Descriptor, Packets.SyncTilePickingPacket.Descriptor, Packets.SyncWallPaintOrCoatingPacket.Descriptor, Packets.TEDisplayDollDataSyncPacket.Descriptor, Packets.TEHatRackItemSyncPacket.Descriptor, Packets.TELeashedEntityAnchorPlaceItemPacket.Descriptor, Packets.TamperWithNPCPacket.Descriptor, Packets.TeamChangeFromUIPacket.Descriptor, Packets.TeamChangePacket.Descriptor, Packets.TeleportEntityPacket.Descriptor, Packets.TeleportNPCThroughPortalPacket.Descriptor, Packets.TeleportPlayerThroughPortalPacket.Descriptor, Packets.TemporaryAnimationPacket.Descriptor, Packets.TileEntityPlacementPacket.Descriptor, Packets.TileEntitySharingPacket.Descriptor, Packets.TileFrameSectionPacket.Descriptor, Packets.TileManipulationPacket.Descriptor, Packets.TileSectionPacket.Descriptor, Packets.ToggleDoorStatePacket.Descriptor, Packets.TogglePVPPacket.Descriptor, Packets.TogglePartyPacket.Descriptor, Packets.TravelMerchantItemsPacket.Descriptor, Packets.UniqueTownNPCInfoSyncRequestPacket.Descriptor, Packets.UniqueTownNPCInfoSyncResponsePacket.Descriptor, Packets.Unknown15Packet.Descriptor, Packets.Unknown42Packet.Descriptor, Packets.Unknown44Packet.Descriptor, Packets.Unknown57Packet.Descriptor, Packets.Unknown60Packet.Descriptor, Packets.Unknown62Packet.Descriptor, Packets.Unknown66Packet.Descriptor, Packets.Unknown67Packet.Descriptor, Packets.Unknown68Packet.Descriptor, Packets.Unused25Packet.Descriptor, Packets.Unused26Packet.Descriptor, Packets.Unused83Packet.Descriptor, Packets.UnusedMeleeStrikePacket.Descriptor, Packets.UpdatePlayerLuckFactorsPacket.Descriptor, Packets.UpdateTowerShieldStrengthsPacket.Descriptor, Packets.WeaponsRackTryPlacingPacket.Descriptor, Packets.WiredCannonShotPacket.Descriptor, Packets.WorldDataPacket.Descriptor });
    public static ProtocolPacketDescriptor? Find(WireDirection direction, byte messageId)
    {
        if (direction is not (WireDirection.ClientToServer or WireDirection.ServerToClient)) throw new ArgumentOutOfRangeException(nameof(direction));
        foreach (var packet in All) if (packet.MessageId == messageId && (packet.WireDirection & direction) != 0) return packet;
        return null;
    }
}
