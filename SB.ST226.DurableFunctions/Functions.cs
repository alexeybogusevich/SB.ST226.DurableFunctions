using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;

namespace SB.ST226.DurableFunctions
{
    public static class Functions
    {
        [FunctionName("Orchestration_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Orchestration", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("Orchestration")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("Orchestration_Hello", "Guests"));
            outputs.Add(await context.CallActivityAsync<string>("Orchestration_Hello", "SMART Business"));
            outputs.Add(await context.CallActivityAsync<string>("Orchestration_Hello", "SMART TALKS 226"));

            // returns ["Hello Guests!", "SMART Business!", "Hello SMART TALKS 226!"]
            return outputs;
        }

        [FunctionName("Orchestration_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("Counter")]
        public static void Counter([EntityTrigger] IDurableEntityContext ctx)
        {
            switch (ctx.OperationName.ToLowerInvariant())
            {
                case "add":
                    ctx.SetState(ctx.GetState<int>() + ctx.GetInput<int>());
                    break;
                case "reset":
                    ctx.SetState(0);
                    break;
                case "get":
                    ctx.Return(ctx.GetState<int>());
                    break;
            }
        }
    }
}
