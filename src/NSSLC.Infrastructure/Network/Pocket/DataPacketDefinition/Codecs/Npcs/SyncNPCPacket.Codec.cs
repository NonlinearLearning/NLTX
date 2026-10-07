using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public delegate void SyncNPCWrite(
        PacketWireWriter writer,
        short npcSlot,
        float positionX,
        float positionY,
        float velocityX,
        float velocityY,
        ushort target,
        bool directionPositive,
        bool directionYPositive,
        bool spriteDirectionPositive,
        bool fullLife,
        bool spawnedFromStatue,
        bool spawnNeedsSyncing,
        bool shimmering,
        float[] ai,
        short netId,
        byte? playerCount,
        float? difficulty,
        int? life,
        byte? encodedLifeWidth,
        byte? catchableReleaseOwner,
        bool[] catchableTypes,
        Func<short, byte?, float?, byte> lifeWidthResolver);
public sealed partial class SyncNPCPacket
{
    public static readonly SyncNPCWrite WriteFunction = Write;
    public static void Write(
        PacketWireWriter writer,
        short npcSlot,
        float positionX,
        float positionY,
        float velocityX,
        float velocityY,
        ushort target,
        bool directionPositive,
        bool directionYPositive,
        bool spriteDirectionPositive,
        bool fullLife,
        bool spawnedFromStatue,
        bool spawnNeedsSyncing,
        bool shimmering,
        float[] ai,
        short netId,
        byte? playerCount,
        float? difficulty,
        int? life,
        byte? encodedLifeWidth,
        byte? catchableReleaseOwner,
        bool[] catchableTypes,
        Func<short, byte?, float?, byte> lifeWidthResolver)
    {
        ArgumentNullException.ThrowIfNull(lifeWidthResolver);
        ArgumentNullException.ThrowIfNull(catchableTypes);
        if (ai is null || ai.Length != 4)
            throw new PacketWireFormatException("Packet 23 requires exactly four AI values.");
        if (playerCount is <= 1)
            throw new PacketWireFormatException("Packet 23 only encodes a player count greater than one.");
        if (difficulty is 1f)
            throw new PacketWireFormatException("Packet 23 omits the default difficulty value of 1.");
        if (shimmering && !spawnNeedsSyncing)
            throw new PacketWireFormatException("Packet 23 only encodes shimmer state when spawn syncing is enabled.");
        bool isCatchable = IsCatchable(catchableTypes, netId);
        if (isCatchable && catchableReleaseOwner is null)
            throw new PacketWireFormatException("Packet 23 requires a release owner for catchable NPC types.");
        if (!isCatchable && catchableReleaseOwner is not null)
            throw new PacketWireFormatException("Packet 23 only encodes a release owner for catchable NPC types.");
        byte? lifeWidth = null;
        if (!fullLife)
        {
            if (life is not int lifeValue)
                throw new PacketWireFormatException("Packet 23 requires a life value when FullLife is not set.");
            lifeWidth = encodedLifeWidth ?? lifeWidthResolver(netId, playerCount, difficulty);
            if (lifeWidth is not (1 or 2 or 4))
                throw new PacketWireFormatException($"Packet 23 life width {lifeWidth} is invalid.");
            if (lifeWidth == 1 && lifeValue is < sbyte.MinValue or > sbyte.MaxValue)
                throw new PacketWireFormatException("Packet 23 life does not fit in a signed byte.");
            if (lifeWidth == 2 && lifeValue is < short.MinValue or > short.MaxValue)
                throw new PacketWireFormatException("Packet 23 life does not fit in a signed Int16.");
        }
        else if (encodedLifeWidth is not null || life is not null)
        {
            throw new PacketWireFormatException("Packet 23 carries neither life nor a width tag when FullLife is set.");
        }

        byte flags1 = 0;
        Set(ref flags1, 0, directionPositive);
        Set(ref flags1, 1, directionYPositive);
        for (int index = 0; index < 4; index++)
            Set(ref flags1, index + 2, ai[index] != 0f);
        Set(ref flags1, 6, spriteDirectionPositive);
        Set(ref flags1, 7, fullLife);
        byte flags2 = 0;
        Set(ref flags2, 0, playerCount is not null);
        Set(ref flags2, 1, spawnedFromStatue);
        Set(ref flags2, 2, difficulty is not null);
        Set(ref flags2, 3, spawnNeedsSyncing);
        Set(ref flags2, 4, shimmering);
        writer.WriteInt16(npcSlot);
        writer.WriteSingle(positionX);
        writer.WriteSingle(positionY);
        writer.WriteSingle(velocityX);
        writer.WriteSingle(velocityY);
        writer.WriteUInt16(target);
        writer.WriteByte(flags1);
        writer.WriteByte(flags2);
        foreach (float aiValue in ai)
        {
            if (aiValue != 0f)
                writer.WriteSingle(aiValue);
        }

        writer.WriteInt16(netId);
        if (playerCount is byte playerCountValue)
            writer.WriteByte(playerCountValue);
        if (difficulty is float difficultyValue)
            writer.WriteSingle(difficultyValue);
        if (!fullLife)
        {
            writer.WriteByte(lifeWidth!.Value);
            switch (lifeWidth.Value)
            {
                case 1:
                    writer.WriteSByte((sbyte)life!.Value);
                    break;
                case 2:
                    writer.WriteInt16((short)life!.Value);
                    break;
                default:
                    writer.WriteInt32(life!.Value);
                    break;
            }
        }

        if (isCatchable)
            writer.WriteByte(catchableReleaseOwner!.Value);
    }

    public static (
        short NpcSlot,
        float PositionX,
        float PositionY,
        float VelocityX,
        float VelocityY,
        ushort Target,
        bool DirectionPositive,
        bool DirectionYPositive,
        bool SpriteDirectionPositive,
        bool FullLife,
        bool SpawnedFromStatue,
        bool SpawnNeedsSyncing,
        bool Shimmering,
        float[] Ai,
        short NetId,
        byte? PlayerCount,
        float? Difficulty,
        int? Life,
        byte? EncodedLifeWidth,
        byte? CatchableReleaseOwner) Read(PacketWireReader reader, bool[] catchableTypes)
    {
        ArgumentNullException.ThrowIfNull(catchableTypes);
        short npcSlot = reader.ReadInt16();
        float positionX = reader.ReadSingle();
        float positionY = reader.ReadSingle();
        float velocityX = reader.ReadSingle();
        float velocityY = reader.ReadSingle();
        ushort target = reader.ReadUInt16();
        byte flags1 = reader.ReadByte();
        byte flags2 = reader.ReadByte();
        var ai = new float[4];
        for (int index = 0; index < ai.Length; index++)
            if (Has(flags1, index + 2))
                ai[index] = reader.ReadSingle();
        short netId = reader.ReadInt16();
        byte? playerCount = Has(flags2, 0) ? reader.ReadByte() : null;
        float? difficulty = Has(flags2, 2) ? reader.ReadSingle() : null;
        int life = 0;
        byte? lifeWidth = null;
        if (!Has(flags1, 7))
        {
            lifeWidth = reader.ReadByte();
            life = lifeWidth.Value switch
            {
                1 => reader.ReadSByte(),
                2 => reader.ReadInt16(),
                4 => reader.ReadInt32(),
                _ => throw new PacketWireFormatException($"Packet 23 life width {lifeWidth} is invalid.")};
        }

        byte? releaseOwner = IsCatchable(catchableTypes, netId) ? reader.ReadByte() : null;
        return (NpcSlot: npcSlot, PositionX: positionX, PositionY: positionY, VelocityX: velocityX, VelocityY: velocityY, Target: target, DirectionPositive: Has(flags1, 0), DirectionYPositive: Has(flags1, 1), SpriteDirectionPositive: Has(flags1, 6), FullLife: Has(flags1, 7), SpawnedFromStatue: Has(flags2, 1), SpawnNeedsSyncing: Has(flags2, 3), Shimmering: Has(flags2, 4), Ai: ai, NetId: netId, PlayerCount: playerCount, Difficulty: difficulty, Life: Has(flags1, 7) ? null : life, EncodedLifeWidth: lifeWidth, CatchableReleaseOwner: releaseOwner);
    }

    private static bool IsCatchable(bool[] catchableTypes, short npcType) => npcType >= 0 && npcType < catchableTypes.Length && catchableTypes[npcType];
    private static bool Has(byte flags, int bit) => (flags & (1 << bit)) != 0;
    private static void Set(ref byte flags, int bit, bool value)
    {
        if (value)
            flags |= (byte)(1 << bit);
    }
}
