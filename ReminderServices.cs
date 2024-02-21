using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using ToDoListApp;
using Azure.Messaging.EventGrid;
using System.Linq;

public static class ReminderService
{
    [FunctionName("ReminderServiceConsumer")]
    public static async Task Run(
        [EventGridTrigger] EventGridEvent eventGridEvent,
        ILogger log)
    {
        try
        {
            string data = eventGridEvent.Data.ToString();
            log.LogWarning($"here log event subject: {eventGridEvent.Subject}");
            log.LogWarning($"here event type: {eventGridEvent.EventType}");
            log.LogWarning($"here data: {data}");

            ToDoListApp.Model.DoList evgTodo = JsonConvert.DeserializeObject<ToDoListApp.Model.DoList>(data);

            if (evgTodo == null)
            {
                log.LogError("Failed to deserialize event data to ToDoListApp.Model.DoList");
                return;
            }

            var client = new CosmosClient(Utils.CONNECTION);
            Container container = client.GetDatabase(Utils.DATABASE)
                .GetContainer(Utils.CONTAINERREMINDER);
            ToDoListApp.Model.Reminder foundReminder =
                container.GetItemLinqQueryable<ToDoListApp.Model.Reminder>(true)
                    .Where(p => p.Id == evgTodo.Id)
                    .AsEnumerable()
                    .FirstOrDefault();

            if (foundReminder == null)
            {
                log.LogWarning("New Reminder");
                foundReminder = ToDoListApp.Model.Reminder.CreateFrom(evgTodo);
                var partition = new PartitionKey(foundReminder.Id);
                await container.CreateItemAsync(foundReminder, partition);
                log.LogWarning(JsonConvert.SerializeObject(foundReminder));
            }
            else
            {
                log.LogWarning("Edit Reminder");
                log.LogWarning(JsonConvert.SerializeObject(foundReminder));
                log.LogWarning("editing data");
                var partition = new PartitionKey(foundReminder.Id);
                await container.ReplaceItemAsync(foundReminder, foundReminder.Id,
                    partition);
                log.LogWarning(JsonConvert.SerializeObject(foundReminder));
            }
        }
        catch (Exception ex)
        {
            log.LogError($"An error occurred: {ex.Message}");
        }
    }
}