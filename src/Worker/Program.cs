using System;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using worker;
using VotingData.Db;
using worker.Consumers;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureLogging((hostBuilderContext,logging) =>
{
    logging.ClearProviders();
    logging.AddConsole();
    //Add code to configurate ILogger for OpenTelemtry here

});
builder.ConfigureServices((hostBuilderContext, services) =>
{    
    //add code block to register opentelemetry for metrics and traces
    
    var connectionString = hostBuilderContext.Configuration.GetConnectionString("SqlDbConnection");
    services.AddDbContext<VotingDBContext>(options =>options.UseNpgsql(connectionString));

    services.AddHostedService<Worker>();
    
    //RabittMQ over Masstransit
    services.AddMassTransit(x =>
    {
        x.AddConsumer<MessageConsumer>();
        x.UsingRabbitMq((context, cfg) =>
            {
                    cfg.Host(hostBuilderContext.Configuration.GetValue<string>("MassTransit:RabbitMq:Host"));
                    cfg.ConfigureEndpoints(context);
                });
    });
    
    //Add code block to register opentelemetr metric provider here

    //Add code block to register opentelemetr trace provider here
    
});

await builder.Build().RunAsync();
