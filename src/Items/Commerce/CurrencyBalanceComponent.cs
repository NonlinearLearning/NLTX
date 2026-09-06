using System.Collections.Generic;

namespace Terraria.Items.Commerce;

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
