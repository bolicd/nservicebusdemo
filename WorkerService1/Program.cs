using System.Diagnostics;
using System.Transactions;
using Infrastructure;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using WorkerService1;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .UseNServiceBus(context =>
    {
        var connectionString = context.Configuration.GetConnectionString("SbusConnection");
        var endpointConfiguration = new EndpointConfiguration("GenericHost1");

        var transport = new SqlServerTransport(connectionString)
        {
            TransportTransactionMode = TransportTransactionMode.TransactionScope,
            TransactionScope =
            {
                IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TransactionManager.MaximumTimeout
            }
        };

        endpointConfiguration.UseTransport(transport);

        endpointConfiguration.DefineCriticalErrorAction(CriticalAction.OnCriticalError);
        endpointConfiguration.EnableInstallers();

        return endpointConfiguration;
    })
    .ConfigureLogging((ctx, logging) =>
    {
        logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));
        logging.AddEventLog();
        logging.AddConsole();
    })
    .ConfigureServices((ctx, services) =>
    {
        TransactionManager.ImplicitDistributedTransactions = true;

        services.AddHostedService<Worker>();
        var connectionString = ctx.Configuration.GetConnectionString("SbusConnectionBusinessData");

        if (connectionString == null) throw new Exception("Connection string is null");
        services.AddScoped(x => new MainTableRepository(connectionString, "MainTable"));
    })
    .Build();

host.Run();