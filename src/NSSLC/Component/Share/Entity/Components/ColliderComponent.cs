namespace EntityEcs.Components;

/// <summary>
/// 保存实体碰撞形状的尺寸、偏移和启用状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>主要源成员：width（第 22 行）； height（第 24 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/baseline/。</para>
/// <para>拆分依据文件：组件设计报告.md。</para>
/// <para>依据位置：第 206 行。</para>
/// </remarks>
public readonly record struct ColliderComponent
{
  public ColliderComponent()
    : this(0.0f, 0.0f)
  {
  }

  public ColliderComponent(
    float width,
    float height,
    float offsetX = 0.0f,
    float offsetY = 0.0f,
    CollisionShapeKind kind = CollisionShapeKind.Rectangle,
    bool isEnabled = true)
  {
    Width = width;
    Height = height;
    OffsetX = offsetX;
    OffsetY = offsetY;
    Kind = kind;
    IsEnabled = isEnabled;
  }

  public float Height { get; }
  public bool HasArea => Width > 0.0f && Height > 0.0f;
  public bool IsEnabled { get; }
  public CollisionShapeKind Kind { get; }
  public float OffsetX { get; }
  public float OffsetY { get; }
  public float Width { get; }
}
