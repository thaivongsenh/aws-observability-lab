// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using VotingData.Models;
using queue.datacontracts;

namespace VotingData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoteDataController : ControllerBase
    {
        private readonly ILogger<VoteDataController> logger;
        private readonly VotingDBContext context;

        private readonly MassTransit.IPublishEndpoint publishEndpoint;
        
        public VoteDataController(VotingDBContext context, ILogger<VoteDataController> logger,MassTransit.IPublishEndpoint publishEndpoint)
        {
            this.logger = logger;
            this.publishEndpoint = publishEndpoint;
            this.context = context;
            
        }

        [HttpGet("/Votes")]
        public async Task<ActionResult<IList<Counts>>> Get()
        {
            try
            {
                return await context.Counts.ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DB Exception");
                return BadRequest("Bad Request");
            }
        }

        [HttpPut("/SubmitVote/{name}")]
        public async Task<IActionResult> Put(string name)
        {
            try
            {
                var candidate = await context.Counts.FirstOrDefaultAsync(c => c.Candidate == name);
                var debugMessage = string.Empty;
                if (candidate == null)
                {
                    await context.Counts.AddAsync(new Counts
                    {
                        Candidate = name,
                        Count = 1
                    });
                    debugMessage = string.Format("Candidate name {0} has been created",name);
                }
                else
                {
                    candidate.Count++;
                    context.Entry(candidate).State = EntityState.Modified;
                    debugMessage = string.Format("Candidate name {0} has been increased the counter to {1}",name,candidate.Count);
                }
               
                await context.SaveChangesAsync();

                logger.LogInformation(debugMessage);

                return NoContent();
            }
            catch (Exception ex) when (ex is DbUpdateException ||
                                       ex is DbUpdateConcurrencyException)
            {
                logger.LogError(ex, "DB Exception Saving to Database");
                return BadRequest("Bad Request");
            }
        }
        
        [HttpPut("/VoteQueue/{id}")]        
        public async Task<IActionResult> VoteQueue(int id)
        {
            try
            {                                 
                await publishEndpoint.Publish<Message>(new {Id=id});        
                logger.LogInformation("A queue message has been sent id:" + id + ", the activity id is : " + Activity.Current?.Id);
                return this.Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception sending vote to the queue");
                return BadRequest("Bad Request");
            }
        }

        [HttpDelete("/Vote/Delete/{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            try
            {
                var candidate = await context.Counts.FirstOrDefaultAsync(c => c.Candidate == name);

                if (candidate != null)
                {
                    context.Counts.Remove(candidate);
                    await context.SaveChangesAsync();
                    
                }

                logger.LogInformation("Candidate name:{0} has been deleted", name);
                
                return Ok();
            }
            catch (Exception ex) when (ex is DbUpdateException ||
                                    ex is DbUpdateConcurrencyException)
            {
                logger.LogError(ex, "DB Exception Deleting from Database");
                return BadRequest("Bad Request");
            }
        }
    }

    
}
