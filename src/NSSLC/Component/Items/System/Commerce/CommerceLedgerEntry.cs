using Terraria.Items;

namespace Terraria.Items.Commerce;

public readonly record struct CommerceLedgerEntry(
  TransactionId TransactionId,
  OperationId OperationId,
  long Sequence,
  CommerceTransactionKind Kind,
  CommerceTransactionState State,
  long? CommittedAtTick,
  ExternalAccountId AccountId,
  ExternalAccountId? CounterpartyAccountId,
  ExternalContentId CurrencyId,
  long Amount,
  ExternalContentId OfferId,
  PersistentItemId? ResultItemId)
{
  public bool IsUnknownOutcome => State == CommerceTransactionState.Unknown;
}
