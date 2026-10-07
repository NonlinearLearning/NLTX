using System.Collections.Generic;

namespace Terraria.Items.Commerce;

/// <summary>
/// 保存交易流水、序号和保留边界。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 CustomCurrencySystem 的货币计数、付款和回滚流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.UI/CustomCurrencySystem.cs。</para>
/// <para>重组说明：账户余额、交易编号、流水及其保留边界是 NLTX 交易模型，不是原字段的直接搬迁。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 2249 行。</para>
/// </remarks>
public sealed class CommerceLedgerComponent
{
  private readonly List<CommerceLedgerEntry> _entries;

  public CommerceLedgerComponent(
    IReadOnlyList<CommerceLedgerEntry>? entries = null,
    long lastSequence = 0,
    long retentionFloorSequence = 0)
  {
    _entries = entries is null ? [] : new List<CommerceLedgerEntry>(entries);
    LastSequence = lastSequence;
    RetentionFloorSequence = retentionFloorSequence;
  }

  public long LastSequence;
  public long RetentionFloorSequence;

  public IReadOnlyList<CommerceLedgerEntry> Entries => _entries;

  public bool ContainsUnknownOutcome
  {
    get
    {
      foreach (CommerceLedgerEntry entry in _entries)
      {
        if (entry.IsUnknownOutcome)
        {
          return true;
        }
      }
      return false;
    }
  }
}
