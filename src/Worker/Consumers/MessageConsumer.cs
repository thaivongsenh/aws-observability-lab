using System.Collections.Generic;
using MassTransit;
using VotingData.Db;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

using queue.datacontracts;

namespace worker.Consumers;

public class MessageConsumer : IConsumer<Message>
{
    private readonly ILogger _logger;     
    private readonly VotingDBContext dbContext; 

    public MessageConsumer(ILogger<MessageConsumer> logger,VotingDBContext context)
    {
        _logger = logger;
        this.dbContext = context;                
    }

    public async Task Consume(ConsumeContext<Message> context)
    {        
        try
            {
                var candidate = await dbContext.Counts.FirstOrDefaultAsync(c => c.ID ==context.Message.Id);
                
                if (candidate != null)
                {                    
                    candidate.Count++;
                    dbContext.Entry(candidate).State = EntityState.Modified;                    

                    _logger.LogInformation(String.Format("Candidate name {0} has been increased the counter to {1}",candidate.Candidate,candidate.Count));

                    await dbContext.SaveChangesAsync();
                }

            }
            catch (Exception ex) when (ex is DbUpdateException ||
                                       ex is DbUpdateConcurrencyException)
            {
                _logger.LogError(ex, "DB Exception Saving to Database");
            
            }

            
        await Task.Delay(TimeSpan.FromMilliseconds(200), context.CancellationToken);
    }
}