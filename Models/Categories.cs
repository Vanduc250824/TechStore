using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechStore.Models;

namespace TechStore.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }
}
