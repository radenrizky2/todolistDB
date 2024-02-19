using Newtonsoft.Json;
using System;

namespace ToDoListApp.Model
{
    public class DoList
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public DoList()
        {
            Id = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
        }

        public DoList Update(DoList updatedList)
        {
            if (updatedList == null) return this;
            Name = updatedList.Name ?? Name;
            ModifiedDate = DateTime.Now;
            return this;
        }
    }
}
