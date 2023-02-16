using Dapper;
using Infrastructure;
using Infrastructure.Model;
using Infrastructure.Repository;
using Messages;
using Microsoft.Data.SqlClient;
using IsolationLevel = System.Data.IsolationLevel;

namespace WorkerService1;

public class Worker : BackgroundService
{
    public IServiceProvider ServiceProvider { get; }
    private readonly ILogger<Worker> _logger;

    private readonly IMessageSession _messageSession;
    private readonly IConfiguration _configuration;
    private string? _connectionString;

    public Worker(ILogger<Worker> logger, IMessageSession messageSession, 
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        ServiceProvider = serviceProvider;
        _logger = logger;
        _messageSession = messageSession;
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("SbusConnectionBusinessData");
    }

    private async Task SaveUsingGenericRepository(string data)
    {
        using var scope = ServiceProvider.CreateScope();

        var mainTableRepository = 
            scope.ServiceProvider
                .GetRequiredService<MainTableRepository>();
        
        await mainTableRepository.InsertAsync(new MainTable
        {
            Data = data,
            UpdatedDate = DateTime.Now
        });
    }

    private async Task SaveDummyDataIntoTable(int number, SqlConnection connection, SqlTransaction? transaction)
    {
        await connection.ExecuteAsync(
            "INSERT INTO [dbo].[MainTable](Data, UpdatedDate) values (@Data, @UpdatedDate);", new { Data = $"saved_{number}", UpdatedDate = DateTime.Now}, transaction);

        if (number % 2 == 0)
        {
            throw new Exception("Boom");
        }
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var number = 0;
        
        //Modify variant to check how it works with different settings
        var variant = Variants.TransactionScope;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                number += 1;

                switch (variant)
                {
                    //Works, no DTC using ambient connection(doesnt work if databases are on different server!)
                    case Variants.TransactionFromConnection:
                        await PromotesConnectionToAmbient(number, stoppingToken);
                        break;
                    //Doesnt work, hangs when trying to close the connection
                    case Variants.TransactionScopeWithConnection:
                        await DefaultNetShouldCreateDTC(number, stoppingToken);
                        break;
                    //Works, DTC is enforced (works if database is on different server)
                    case Variants.TransactionScope:
                        await ExplicitDistributedTransactionVariant(number, stoppingToken);
                        break;
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Exception: {ex}", e);
            throw;
        }
    }
   
    //Passes connection string 
    private async Task DefaultNetShouldCreateDTC(int number, CancellationToken stoppingToken)
    {
        try
        {
            using var transactionScope = TransactionUtils.CreateTransactionScope();

            await using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync(stoppingToken);

            await SaveDummyDataIntoTable(number, connection, null);

            _logger.LogInformation("Publishing message {number}", number);

            await _messageSession.Publish(new MyMessage { Number = number }, stoppingToken)
                .ConfigureAwait(false);

            transactionScope.Complete();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    //Uses explicit promotion to distributed transaction via EnsureDistributed extension
    private async Task ExplicitDistributedTransactionVariant(int number, CancellationToken stoppingToken)
    {
        using var transactionScope = TransactionUtils.CreateTransactionScope().EnsureDistributed();
        try
        {
            await SaveUsingGenericRepository($"saved {number}").ConfigureAwait(false);

            if (number % 2 == 0)
            {
                throw new Exception("Boom");
            }

            await _messageSession.Publish(new MyMessage { Number = number }, stoppingToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Publishing message {number}", number);

            transactionScope.Complete();

            _logger.LogInformation("Transaction complete");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    // explicitly sets the same transaction for both save into bussiness database
    private async Task PromotesConnectionToAmbient(int number, CancellationToken stoppingToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(stoppingToken);
        await using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        
        try
        {
            //We are using dapper here to be able to pass transaction directy
            await SaveDummyDataIntoTable(number, connection, transaction);

            await _messageSession.Publish(new MyMessage { Number = number }, stoppingToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Publishing message {number}", number);
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            await transaction.CommitAsync(stoppingToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            await transaction.RollbackAsync(stoppingToken);
        }
    }
}