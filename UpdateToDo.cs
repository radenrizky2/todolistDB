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
    public static class UpdateToDo
    {
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


                return new OkObjectResult(new { status = "success", message = "To-Do item updated successfully.", data = result});
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
    }
}
