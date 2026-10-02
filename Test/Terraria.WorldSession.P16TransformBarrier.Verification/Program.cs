using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertEqual<T>(T expected, T actual, string message)
  where T : IEquatable<T>
{
  Assert(expected.Equals(actual), $"{message} Expected {expected}, got {actual}.");
}

static void AssertThrows<TException>(Action action, string message)
  where TException : Exception
{
  try
  {
    action();
  }
  catch (TException)
  {
    return;
  }

  throw new InvalidOperationException(message);
}

WorldTransformTransactionComponent transactions = new();
Assert(
  !WorldTransformTransactionSystem.HasActiveTransactions(transactions),
  "A new transform owner is clear.");
AssertEqual(1, WorldTransformTransactionSystem.Begin(transactions), "First transform count.");
AssertEqual(2, WorldTransformTransactionSystem.Begin(transactions), "Concurrent transform count.");
AssertEqual(2UL, transactions.Revision, "Begin advances the revision.");
AssertEqual(1, WorldTransformTransactionSystem.Complete(transactions), "One transform remains.");
AssertEqual(3UL, transactions.Revision, "Completion advances the revision.");
AssertEqual(0, WorldTransformTransactionSystem.Complete(transactions), "All transforms complete.");
Assert(
  !WorldTransformTransactionSystem.HasActiveTransactions(transactions),
  "The barrier clears after the final completion.");
AssertEqual(4UL, transactions.Revision, "Every state change advances the revision.");
AssertThrows<InvalidOperationException>(
  () => WorldTransformTransactionSystem.Complete(transactions),
  "Completion without an active transform is rejected.");
AssertEqual(0, transactions.ActiveCount, "An invalid completion leaves the count unchanged.");
Console.WriteLine("PASS: transform transaction state and underflow guard");

const int workerCount = 8;
const int iterationsPerWorker = 1000;
Parallel.For(0, workerCount, _ =>
{
  for (int iteration = 0; iteration < iterationsPerWorker; iteration++)
  {
    WorldTransformTransactionSystem.Begin(transactions);
    WorldTransformTransactionSystem.Complete(transactions);
  }
});

AssertEqual(0, transactions.ActiveCount, "Concurrent transactions leave no active count.");
AssertEqual(
  4UL + (ulong)(workerCount * iterationsPerWorker * 2),
  transactions.Revision,
  "Concurrent begin and completion operations are all observed.");
Console.WriteLine("PASS: concurrent transform transactions preserve the exact count");
