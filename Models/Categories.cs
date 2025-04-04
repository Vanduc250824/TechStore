using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace TechStore.Models
{
    public class Category {
        public int CategoryID {get; set;}

        [Required]
        public required string Name { get; set; }
    }
}