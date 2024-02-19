using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;


namespace ToDoListApp
{
    public static class ToDoListFunctions
    {
        private static string documentUri;
        private static Microsoft.Azure.Documents.Client.RequestOptions requestOptions;

        [FunctionName("GetToDoList")]
        public static async Task<IActionResult> GetToDoList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: Utils.DATABASE,
                containerName: Utils.CONTAINERNAME,
                Connection = Utils.CONNECTION,
                SqlQuery = "SELECT * FROM c")] IEnumerable<Model.DoList> toDoItems,
            ILogger log)
        {
            log.LogInformation("Getting To-Do list items.");

            try
            {
                return new OkObjectResult(new { status = "success", message = "To-Do list items retrieved successfully.", data = toDoItems });
            }
            catch (Exception ex)
            {
                log.LogError($"Error while getting To-Do list: {ex.Message}");
                return new ObjectResult(new { status = "error", message = $"Failed to retrieve To-Do list items: {ex.Message}" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        [FunctionName("AddToDoItem")]
        public static async Task<IActionResult> AddToDoItem(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: Utils.DATABASE,
                containerName: Utils.CONTAINERNAME,
                Connection = Utils.CONNECTION
                )] IAsyncCollector<Model.DoList> toDoItemsOut,
            ILogger log)
        {
            log.LogInformation("Adding a new item to the To-Do list.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var newItem = JsonConvert.DeserializeObject<Model.DoList>(requestBody);
                newItem.Id = Guid.NewGuid().ToString();

                await toDoItemsOut.AddAsync(newItem);

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

        [FunctionName("GetDetailItem")]
        public static async Task<IActionResult> GetDetailItem(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "detail/{id}")] HttpRequest req,
            [CosmosDB(
                databaseName: Utils.DATABASE,
                containerName: Utils.CONTAINERNAME,
                Connection = Utils.CONNECTION,
                Id = "{id}",
                PartitionKey ="{id}")] Model.DoList toDoItem,
            ILogger log,
            string id)
        {
            log.LogInformation($"Getting To-Do item with ID: {id}, ini item: {toDoItem}");

            try
            {
                if (toDoItem != null)
                {
                    return new OkObjectResult(new { status = "success", message = "To-Do item retrieved successfully.", data = toDoItem });
                }
                else
                {
                    return new NotFoundObjectResult(new { status = "error", message = "To-Do item not found." });
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error while getting To-Do item: {ex.Message}");
                return new ObjectResult(new { status = "error", message = $"Failed to retrieve To-Do item: {ex.Message}" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
        [FunctionName("UpdateToDoItem")]
        public static async Task<IActionResult> UpdateToDoItem(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "update/{id}")] HttpRequest req,
            [CosmosDB(
                databaseName: Utils.DATABASE,
                containerName: Utils.CONTAINERNAME,
                Connection = Utils.CONNECTION,
                Id = "{id}",
                PartitionKey ="{id}")]Model.DoList doItem,
            ILogger log,
            string id)
        {
            log.LogInformation($"Updating To-Do item with ID: {id}.");

            try
            {
                
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updatedTodo = JsonConvert.DeserializeObject<Model.DoList>(requestBody);

               
                var result = doItem.Update(updatedTodo);

                
                return new OkObjectResult(new { status = "success", message = "To-Do item updated successfully.", data = result });
            }
            catch (Exception ex)
            {
               
                log.LogError($"Error while updating to-do item: {ex.Message}");
                return new ObjectResult(new { status = "error", message = $"Failed to update To-Do item: {ex.Message}" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }


        [FunctionName("DeleteToDoItem")]
        public static async Task<IActionResult> DeleteToDoItem(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "delete/{id}")] HttpRequest req,
            [CosmosDB(
                databaseName: Utils.DATABASE,
                containerName: Utils.CONTAINERNAME,
                Connection = Utils.CONNECTION,
                Id = "{id}",
                PartitionKey ="{id}")] Model.DoList toDoItem,
            ILogger log, string id)
        {
            log.LogInformation($"Deleting item with ID: {id} from the To-Do list.");

            try
            {
                if (toDoItem == null)
                {
                    return new NotFoundObjectResult(new { status = "error", message = "To-Do item not found." });
                }

                return new OkObjectResult(new { status = "success", message = "To-Do item deleted successfully." });
            }
            catch (Exception ex)
            {
                log.LogError($"Error while deleting to-do item: {ex.Message}");
                return new ObjectResult(new { status = "error", message = $"Failed to delete To-Do item: {ex.Message}" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
