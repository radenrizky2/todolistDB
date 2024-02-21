using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace ToDoListApp
{
    public static class GetToDo
    {
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
    }
}