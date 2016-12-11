using System;
using System.ComponentModel.DataAnnotations;
using AspNetCore.CrudDemo.Validators;
using Newtonsoft.Json;

namespace AspNetCore.CrudDemo.Models
{
    public class Product
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "html")]
        [Required]
        [Html]
        public string Html { get; set; }

        [JsonProperty(PropertyName = "created")]
        public DateTimeOffset Created { get; set; }

        [JsonProperty(PropertyName = "modified")]
        public DateTimeOffset? Modified { get; set; }
    }
}
