using MassTransit;

namespace worker;

public class Worker : IHostedService
{
    private readonly ILogger<Worker> _logger;
    private readonly IBusControl _bus;

   
    public Worker(ILogger<Worker> logger,IBusControl bus)
    {
        _logger = logger;
        _bus = bus;
        
    }

    public Task StartAsync(CancellationToken cancellationToken){
                  
         _bus.StartAsync(cancellationToken);

         return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => _bus.StopAsync(cancellationToken);
}
