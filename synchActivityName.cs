using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ToDoListApp.Model;

namespace ToDoListApp
{
    public class SynchActivityName
    {
        [FunctionName("SynchActivityName")]
        public static async Task Run([EventHubTrigger("name", Connection = "Evh-pdpazure-rizky-listen", ConsumerGroup = "log-synchactivityname")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    var messageBody = Encoding.UTF8.GetString(eventData.Body.ToArray());
                    log.LogInformation($"Event Hub message body: {messageBody}");

                    var userData = JsonConvert.DeserializeObject<DoList>(messageBody);
                    log.LogInformation($"User data: {JsonConvert.SerializeObject(userData)}");

                    ItemResponse<DoList> createdItem = await InsertDataToDB(eventData);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    log.LogError($"Error processing event: {e.Message}");
                }
            }

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions[0];
        }

        static async Task<ItemResponse<DoList>> InsertDataToDB(EventData eventData)
        {
            try
            {
                // Extract data from EventData object
                string messageBody = Encoding.UTF8.GetString(eventData.Body.ToArray());
                var userData = JsonConvert.DeserializeObject<DoList>(messageBody);

                // Initialize Cosmos DB client and container
                var client = new CosmosClient(Utils.CONNECTION);
                var cosmosContainer = client.GetDatabase(Utils.DATABASE).GetContainer(Utils.CONTAINERNAME);

                // Insert the data into Cosmos DB
                var createdItem = await cosmosContainer.CreateItemAsync(userData);

                return createdItem;
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                throw new Exception("Failed to insert data to Cosmos DB", ex);
            }
        }

    }
}
