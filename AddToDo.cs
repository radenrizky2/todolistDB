using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TodoAppFunction;
using ToDoListApp.Model;


namespace ToDoListApp
{
    public static class AddListToDo
    {

        private static async Task SendDataToEventHub(IAsyncCollector<string> evenhubName, ILogger log, Model.DoList postNameData)
        {
            await evenhubName.AddAsync(JsonConvert.SerializeObject(postNameData));
            log.LogInformation($"ID: {postNameData.Id} Send to eventhub");
        }

        [FunctionName("AddToDoItem")]
        public static async Task<IActionResult> AddToDoItem(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(
            databaseName: Utils.DATABASE,
            containerName: Utils.CONTAINERNAME,
            Connection = Utils.CONNECTION
            )] IAsyncCollector<dynamic> toDoItemsOut,
            ILogger log)
        {
            log.LogInformation("Adding a new item to the To-Do list.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation($"Request body: {requestBody}");

                var newItem = JsonConvert.DeserializeObject<Model.DoList>(requestBody);
                log.LogInformation($"Deserialized new item: {JsonConvert.SerializeObject(newItem)}");

                newItem.Id = Guid.NewGuid().ToString();

                log.LogInformation($"Generated new item ID: {newItem.Id}");

                await toDoItemsOut.AddAsync(newItem);
                log.LogInformation($"Added new item to Cosmos DB.");

                await EventGridUtil.SendDataToEventGrid(newItem, "Create/");
                log.LogInformation($"Sent data to Event Grid.");

                return new OkObjectResult(new { status = "success", message = "To-Do item added successfully.", data = newItem });
            }
            catch (Exception ex)
            {
                log.LogError($"Error while adding to-do item: {ex.Message}");
                return new ObjectResult(new { status = "error", message = $"Failed to add To-Do item: {ex.Message}" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        [FunctionName("AddToDoItemToEVH")]
        public static async Task<IActionResult> AddToDoItemToEVH(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [EventHub("name", Connection = "Evh-pdpazure-rizky-send")] IAsyncCollector<EventData> outputEvents,
            [CosmosDB(
                    databaseName: Utils.DATABASE,
                    containerName: Utils.CONTAINERNAME,
                    Connection = Utils.CONNECTION)] IAsyncCollector<DoList> cosmosDBCollector,
                ILogger log)
           {
                log.LogInformation("Adding a new item to the To-Do list.");

                try
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    log.LogInformation($"Request body: {requestBody}");

                    var newItem = JsonConvert.DeserializeObject<DoList>(requestBody);
                    log.LogInformation($"Deserialized new item: {JsonConvert.SerializeObject(newItem)}");

                    newItem.Id = Guid.NewGuid().ToString();
                    log.LogInformation($"Generated new item ID: {newItem.Id}");

                    // Save to Cosmos DB
                    await cosmosDBCollector.AddAsync(newItem);
                    log.LogInformation($"Added new item to Cosmos DB.");

                    // Add the EventData to the Event Hub
                    await SendDataToEventHub(outputEvents, log, newItem);
                    log.LogInformation($"Added new item to Event Hub.");

                    return new OkObjectResult(new { status = "success", message = "To-Do item added successfully.", data = newItem });
                }
                catch (Exception ex)
                {
                    log.LogError($"Error while adding to-do item: {ex.Message}");
                    return new ObjectResult(new { status = "error", message = $"Failed to add To-Do item: {ex.Message}" })
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }
            }

        private static async Task SendDataToEventHub(IAsyncCollector<EventData> eventHub, ILogger log, DoList newItem)
        {
            string serializedItem = JsonConvert.SerializeObject(newItem);
            var eventData = new EventData(Encoding.UTF8.GetBytes(serializedItem));
            await eventHub.AddAsync(eventData);
            log.LogInformation($"ID: {newItem.Id} sent to Event Hub.");
        }
    }
}
