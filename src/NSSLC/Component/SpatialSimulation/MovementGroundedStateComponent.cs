namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.MOVEMENT_GROUNDED_STATE
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存移动主体是否接地的独立求解结果。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Collision 的移动碰撞和地面接触求解流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Collision.cs。</para>
/// <para>重组说明：IsGrounded 是求解后独立保存的派生事实，不对应原版同名字段。</para>
/// </remarks>
public readonly record struct MovementGroundedStateComponent(bool IsGrounded);
