using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ToDoListApp
{
    public static class DB
    {
        [FunctionName("DB")]
        public static void Run([CosmosDBTrigger(
            databaseName: Utils.DATABASE,
            containerName: Utils.CONTAINERNAME,
            Connection = "CosmosDBConnection",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)]IReadOnlyList<ToDoItem> input,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].id);
            }
        }

        public class ToDoItem
        {
            public string id { get; set; }
            public string Description { get; set; }
        }
    }
}
