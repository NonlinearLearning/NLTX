namespace Terraria.Network;

/// <summary>
/// The frame data an inbound packet carries across the ECS ingress boundary.
/// </summary>
/// <remarks>
/// 这是领域侧的载体，不含任何帧格式知识：头部长度、模块 id 编码等都由网络侧解析后填入。
/// 领域层只把它当作"一包原始字节 + 两个已解出的标识"使用，因此不依赖网络程序集的任何类型。
/// </remarks>
public readonly record struct InboundPacketFrame(
    byte MessageId,
    ushort? ModuleId,
    ReadOnlyMemory<byte> FrameBytes,
    ReadOnlyMemory<byte> Payload);
