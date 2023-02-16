using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using WorkerService1;
using WorkerService2;


IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .UseNServiceBus(context =>
    {
        var connectionString = context.Configuration.GetConnectionString("SbusConnection");
        var endpointConfiguration = new EndpointConfiguration("GenericHost2");
        endpointConfiguration.UseTransport<SqlServerTransport>()
            .Transactions(TransportTransactionMode.TransactionScope)
            .ConnectionString(connectionString);

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
    .ConfigureServices(services => { services.AddHostedService<Worker>(); })
    .Build();

host.Run();