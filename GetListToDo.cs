using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;


namespace ToDoListApp
{
    public static class GetListToDo
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
    }
}
