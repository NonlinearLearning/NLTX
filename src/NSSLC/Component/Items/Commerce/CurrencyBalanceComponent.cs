using System.Collections.Generic;

namespace Terraria.Items.Commerce;

/// <summary>
/// 保存账户的各币种余额、上限及最近交易状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 CustomCurrencySystem 的货币计数、付款和回滚流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.UI/CustomCurrencySystem.cs。</para>
/// <para>重组说明：账户余额、交易编号、流水及其保留边界是 NLTX 交易模型，不是原字段的直接搬迁。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 2248 行。</para>
/// </remarks>
public sealed class CurrencyBalanceComponent
{
  private readonly Dictionary<ExternalContentId, long> _balances;
  private readonly Dictionary<ExternalContentId, long> _currencyCaps;

  public CurrencyBalanceComponent(
    ExternalAccountId accountId,
    IReadOnlyDictionary<ExternalContentId, long>? balances = null,
    IReadOnlyDictionary<ExternalContentId, long>? currencyCaps = null,
    long revision = 0,
    TransactionId? lastTransactionId = null)
  {
    AccountId = accountId;
    _balances = balances is null
      ? []
      : new Dictionary<ExternalContentId, long>(balances);
    _currencyCaps = currencyCaps is null
      ? []
      : new Dictionary<ExternalContentId, long>(currencyCaps);
    Revision = revision;
    LastTransactionId = lastTransactionId;
  }

  public ExternalAccountId AccountId;
  public long Revision;
  public TransactionId? LastTransactionId;

  public IReadOnlyDictionary<ExternalContentId, long> Balances => _balances;

  public IReadOnlyDictionary<ExternalContentId, long> CurrencyCaps =>
    _currencyCaps;

  public bool IsInitialized => AccountId.IsAssigned;

  public int TotalCurrencyKinds
  {
    get
    {
      int totalCurrencyKinds = 0;
      foreach (long balance in _balances.Values)
      {
        if (balance > 0)
        {
          totalCurrencyKinds++;
        }
      }
      return totalCurrencyKinds;
    }
  }
}
