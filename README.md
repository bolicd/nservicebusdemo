# nservicebusdemo
Example NServiceBus project using SQL transport, MSSQL database and Windows MSDTC for distributed transactions if needed

# how to run

1. create database for nservice bus. i suggest using database name from connection string
2. create bussiness mock database, use Infrastructure/Sql Scripts for this
3. once both databases are created Worker1. This should create servicebus tables 
4. run worker 2 to create subsription.

If all works worker1 should send messages to worker2, each even message should be thrown as an exception(for testing purposes)

modify variant in worker 1 to test other types messaging
