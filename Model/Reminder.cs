using Newtonsoft.Json;
using System;

namespace ToDoListApp.Model
{
    public class Reminder
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public Reminder()
        {
            Id = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
        }
        public static Reminder CreateFrom(DoList todo)
        {
            return new Reminder()
            {
                Id = todo.Id,
                Name = todo.Name
            };
        }

    }
}
