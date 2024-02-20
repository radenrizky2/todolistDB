using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace ToDoListApp
{
    public static class AddListToDo
    {

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
    }
}
