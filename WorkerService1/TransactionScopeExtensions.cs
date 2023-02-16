using System.Transactions;

namespace WorkerService1;

public static class TransactionScopeExtensions
{
    // This method ensures that a transaction scope is distributed.
    public static TransactionScope EnsureDistributed(this TransactionScope ts)
    {
        // Enlist a dummy enlistment notification in the current transaction if it exists.
        Transaction.Current?.EnlistDurable(DummyEnlistmentNotification.Id, new DummyEnlistmentNotification(),
            EnlistmentOptions.None);

        // Return the transaction scope.
        return ts;
    }

    // This class provides a dummy implementation of the IEnlistmentNotification interface.
    private class DummyEnlistmentNotification : IEnlistmentNotification
    {
        // The ID of the dummy enlistment notification.
        internal static readonly Guid Id = new("8d952615-7f67-4579-94fa-5c36f0c61478");

        // This method is called by the transaction manager to instruct the participant to prepare for a transaction.
        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            // Indicate that the participant is prepared for the transaction.
            preparingEnlistment.Prepared();
        }

        // This method is called by the transaction manager to instruct the participant to commit the transaction.
        public void Commit(Enlistment enlistment)
        {
            // Indicate that the participant has committed the transaction.
            enlistment.Done();
        }

        // This method is called by the transaction manager to instruct the participant to roll back the transaction.
        public void Rollback(Enlistment enlistment)
        {
            // Indicate that the participant has rolled back the transaction.
            enlistment.Done();
        }

        // This method is called by the transaction manager to indicate that the outcome of the transaction is in doubt.
        public void InDoubt(Enlistment enlistment)
        {
            // Indicate that the participant is in doubt about the outcome of the transaction.
            enlistment.Done();
        }
    }
}