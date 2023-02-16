using System.Transactions;

namespace Infrastructure;

public class TransactionUtils 
{
    public static TransactionScope CreateTransactionScope()
    {
        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TransactionManager.MaximumTimeout
        };

        return new TransactionScope(TransactionScopeOption.Required, transactionOptions,TransactionScopeAsyncFlowOption.Enabled);
    }
}