using System.Collections.Generic;

namespace Terraria.Items;

/// <summary>
/// 保存合成事务的配方、数量、阶段及来源版本前提。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Recipe 的配方判定、材料消费与产物创建流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Recipe.cs。</para>
/// <para>重组说明：材料预留、事务阶段、来源版本和事务序号是合成流程拆分时新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 2175 行。</para>
/// </remarks>
public sealed class CraftingStateComponent
{
  private readonly Dictionary<PersistentContainerId, long> _expectedSourceRevisions;

  public CraftingStateComponent(
    TransactionId? transactionId = null,
    RecipeDefinitionRef? recipeRef = null,
    int requestedQuantity = 0,
    IReadOnlyDictionary<PersistentContainerId, long>? expectedSourceRevisions = null,
    long? acceptedAtTick = null,
    long craftSequence = 0,
    CraftingPhase phase = CraftingPhase.Idle)
  {
    TransactionId = transactionId;
    RecipeRef = recipeRef;
    RequestedQuantity = requestedQuantity;
    _expectedSourceRevisions = expectedSourceRevisions is null
      ? []
      : new Dictionary<PersistentContainerId, long>(expectedSourceRevisions);
    AcceptedAtTick = acceptedAtTick;
    CraftSequence = craftSequence;
    Phase = phase;
  }

  public TransactionId? TransactionId;
  public RecipeDefinitionRef? RecipeRef;
  public int RequestedQuantity;
  public long? AcceptedAtTick;
  public long CraftSequence;
  public CraftingPhase Phase;

  public IReadOnlyDictionary<PersistentContainerId, long> ExpectedSourceRevisions =>
    _expectedSourceRevisions;

  public bool HasTransaction => TransactionId.HasValue;

  public bool HasActiveCraft =>
    TransactionId.HasValue &&
    RecipeRef.HasValue &&
    RequestedQuantity > 0 &&
    Phase is CraftingPhase.Accepted or CraftingPhase.Consuming or CraftingPhase.Producing;
}
