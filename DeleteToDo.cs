using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;


namespace ToDoListApp
{
    public static class DeleteToDo
    {

        [FunctionName("DeleteToDoItem")]
        public static async Task<IActionResult> DeleteToDoItem(
         [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "delete/{id}")] HttpRequest req,
         [CosmosDB(
            databaseName: Utils.DATABASE,
            containerName: Utils.CONTAINERNAME,
            Connection = Utils.CONNECTION)] CosmosClient client,
         ILogger log, string id)
        {
            log.LogInformation($"Deleting item with ID: {id} from the To-Do list.");

            try
            {
                var container = client.GetContainer(Utils.DATABASE, Utils.CONTAINERNAME);
                var partitionKey = new Microsoft.Azure.Cosmos.PartitionKey(id);

                var response = await container.DeleteItemAsync<dynamic>(id, partitionKey);

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return new OkObjectResult(new { status = "success", message = "To-Do item deleted successfully." });
                }
                else
                {
                    return new ObjectResult(new { status = "error", message = $"Failed to delete To-Do item." })
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new NotFoundObjectResult(new { status = "error", message = "To-Do item not found." });
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
