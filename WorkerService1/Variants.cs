namespace WorkerService1
{
    public enum Variants
    {
        //Works, DTC is enforced (works if database is on different server)
        TransactionScope = 0,
        //Works, no DTC using ambient connection(doesnt work if databases are on different server!)
        TransactionFromConnection = 1,
        //Doesnt work, hangs when trying to close the connection
        TransactionScopeWithConnection = 2
    }
}
